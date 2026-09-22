using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public interface ISatelliteObservationRepository
    {
        Task<IEnumerable<SatelliteObservation>> GetAllAsync();
        Task<SatelliteObservation?> GetByIdAsync(int id);
        Task AddAsync(SatelliteObservation observation);
        Task UpdateAsync(SatelliteObservation observation);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Satellite>> GetAllSatellitesAsync();
    }
}
