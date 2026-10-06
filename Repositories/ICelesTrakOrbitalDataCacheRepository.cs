using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public interface ICelesTrakOrbitalDataCacheRepository
    {
        Task<CelesTrakOrbitalDataCache?> GetByNoradIdAsync(string noradId);
        Task UpsertAsync(CelesTrakOrbitalDataCache cacheRecord);
    }
}
