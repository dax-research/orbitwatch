using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrbitWatch.Models;
using OrbitWatch.Models.Api;
using OrbitWatch.Repositories;

namespace OrbitWatch.Services
{
    public class SatelliteApiService : ISatelliteApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ICelesTrakOrbitalDataCacheRepository _persistentCache;
        private readonly ILogger<SatelliteApiService> _logger;

        // Use a static concurrent dictionary to share locks across transient service instances
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        // Cache freshness/stale policy as per requirements
        private static readonly TimeSpan FreshDuration = TimeSpan.FromHours(2);
        private static readonly TimeSpan StaleDuration = TimeSpan.FromHours(24);

        private class CacheEntry
        {
            public CelesTrakGpData Data { get; set; } = null!;
            public DateTime FetchedAtUtc { get; set; }
        }

        public SatelliteApiService(
            HttpClient httpClient, 
            IMemoryCache cache, 
            ICelesTrakOrbitalDataCacheRepository persistentCache,
            ILogger<SatelliteApiService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _persistentCache = persistentCache;
            _logger = logger;
        }

        public async Task<CelesTrakResult> GetSatelliteDataAsync(string noradId)
        {
            if (string.IsNullOrWhiteSpace(noradId))
            {
                return new CelesTrakResult { Data = null };
            }

            var cacheKey = $"celestrak:{noradId}";
            var now = DateTime.UtcNow;

            // 1. Check IMemoryCache first
            if (_cache.TryGetValue(cacheKey, out CacheEntry? cachedEntry) && cachedEntry != null)
            {
                var memoryAge = now - cachedEntry.FetchedAtUtc;
                if (memoryAge <= FreshDuration)
                {
                    _logger.LogInformation("Memory cache hit for NORAD {NoradId}. Data is fresh.", noradId);
                    return new CelesTrakResult { Data = cachedEntry.Data, IsStale = false };
                }
            }

            // Acquire per-NORAD lock to prevent cache stampedes (request storms)
            var semaphore = _locks.GetOrAdd(noradId, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();

            try
            {
                // Double-check memory cache inside the lock
                if (_cache.TryGetValue(cacheKey, out cachedEntry) && cachedEntry != null)
                {
                    var memoryAge = now - cachedEntry.FetchedAtUtc;
                    if (memoryAge <= FreshDuration)
                    {
                        return new CelesTrakResult { Data = cachedEntry.Data, IsStale = false };
                    }
                }

                // 2. Memory cache missed or is stale. Check persistent cache.
                var persistentRecord = await _persistentCache.GetByNoradIdAsync(noradId);
                CelesTrakGpData? dbData = null;

                if (persistentRecord != null)
                {
                    try
                    {
                        var dataList = JsonSerializer.Deserialize<List<CelesTrakGpData>>(persistentRecord.PayloadJson);
                        dbData = dataList?.FirstOrDefault();
                        
                        if (dbData != null)
                        {
                            var dbAge = now - persistentRecord.FetchedAtUtc;
                            if (dbAge <= FreshDuration)
                            {
                                // Persistent cache is fresh. Update memory cache and return.
                                _logger.LogInformation("Persistent cache hit for NORAD {NoradId}. Data is fresh.", noradId);
                                UpdateMemoryCache(cacheKey, dbData, persistentRecord.FetchedAtUtc);
                                return new CelesTrakResult { Data = dbData, IsStale = false };
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize persistent cache for NORAD {NoradId}", noradId);
                        // Treat as unusable, continue to refresh
                    }
                }

                // 3. Persistent cache is stale or missing. Fetch from CelesTrak.
                _logger.LogInformation("Cache miss or stale for NORAD {NoradId}. Fetching from CelesTrak...", noradId);
                var fetchResult = await FetchFromApiAsync(noradId);

                if (fetchResult != null)
                {
                    _logger.LogInformation("Successfully fetched CelesTrak data for NORAD {NoradId}.", noradId);
                    var fetchTime = now;
                    
                    // Update persistent cache
                    var newRecord = new CelesTrakOrbitalDataCache
                    {
                        NoradId = noradId,
                        PayloadJson = JsonSerializer.Serialize(new List<CelesTrakGpData> { fetchResult }),
                        FetchedAtUtc = fetchTime
                    };
                    await _persistentCache.UpsertAsync(newRecord);

                    // Update memory cache
                    UpdateMemoryCache(cacheKey, fetchResult, fetchTime);

                    return new CelesTrakResult { Data = fetchResult, IsStale = false };
                }

                // 4. Fetch failed. Attempt fallback to stale data.
                if (dbData != null)
                {
                    var dbAge = now - persistentRecord!.FetchedAtUtc;
                    if (dbAge <= StaleDuration)
                    {
                        _logger.LogWarning("CelesTrak fetch failed for NORAD {NoradId}. Falling back to STALE persistent data.", noradId);
                        UpdateMemoryCache(cacheKey, dbData, persistentRecord.FetchedAtUtc);
                        return new CelesTrakResult { Data = dbData, IsStale = true };
                    }
                }

                // If memory cache has stale data that is still within StaleDuration (though we should have already checked db)
                if (cachedEntry != null)
                {
                     var memoryAge = now - cachedEntry.FetchedAtUtc;
                     if (memoryAge <= StaleDuration)
                     {
                         _logger.LogWarning("CelesTrak fetch failed for NORAD {NoradId}. Falling back to STALE memory data.", noradId);
                         return new CelesTrakResult { Data = cachedEntry.Data, IsStale = true };
                     }
                }

                _logger.LogError("CelesTrak fetch failed for NORAD {NoradId} and no valid stale data is available.", noradId);
                return new CelesTrakResult { Data = null };
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<CelesTrakGpData?> FetchFromApiAsync(string noradId)
        {
            try
            {
                var url = $"NORAD/elements/gp.php?CATNR={Uri.EscapeDataString(noradId)}&FORMAT=JSON";
                using var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadFromJsonAsync<List<CelesTrakGpData>>();
                return data?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching from CelesTrak for NORAD {NoradId}", noradId);
                return null;
            }
        }

        private void UpdateMemoryCache(string cacheKey, CelesTrakGpData data, DateTime fetchedAtUtc)
        {
            var entry = new CacheEntry { Data = data, FetchedAtUtc = fetchedAtUtc };
            _cache.Set(cacheKey, entry, StaleDuration);
        }
    }
}