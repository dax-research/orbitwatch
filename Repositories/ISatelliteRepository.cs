using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public interface ISatelliteRepository
    {
        Task<IEnumerable<Satellite>> GetAllAsync();
        Task<Satellite?> GetByIdAsync(int id);
        Task AddAsync(Satellite satellite);
        Task UpdateAsync(Satellite satellite);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Mission>> GetAllMissionsAsync();
    }
}
