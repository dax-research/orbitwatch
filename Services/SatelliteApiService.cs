using System.Net;
using System.Net.Http.Json;
using OrbitWatch.Models.Api;

namespace OrbitWatch.Services
{
    public class SatelliteApiService : ISatelliteApiService
    {
        private readonly HttpClient _httpClient;

        public SatelliteApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CelesTrakGpData?> GetSatelliteDataAsync(string noradId)
        {
            if (string.IsNullOrWhiteSpace(noradId))
            {
                return null;
            }

            var url =
                $"NORAD/elements/gp.php?CATNR={Uri.EscapeDataString(noradId)}&FORMAT=JSON";

            using var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var data =
                await response.Content.ReadFromJsonAsync<List<CelesTrakGpData>>();

            return data?.FirstOrDefault();
        }
    }
}