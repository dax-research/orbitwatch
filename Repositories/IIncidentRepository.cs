using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public interface IIncidentRepository
    {
        Task<IEnumerable<Incident>> GetAllAsync();
        Task<Incident?> GetByIdAsync(int id);
        Task AddAsync(Incident incident);
        Task UpdateAsync(Incident incident);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Satellite>> GetAllSatellitesAsync();
    }
}
