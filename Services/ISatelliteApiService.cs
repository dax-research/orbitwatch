using OrbitWatch.Models.Api;

namespace OrbitWatch.Services
{
    public interface ISatelliteApiService
    {
        Task<CelesTrakGpData?> GetSatelliteDataAsync(string noradId);
    }
}