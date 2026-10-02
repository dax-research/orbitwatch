using OrbitWatch.Models.Api;

namespace OrbitWatch.Services
{
    public interface ISatelliteApiService
    {
        Task<CelesTrakResult> GetSatelliteDataAsync(string noradId);
    }
}