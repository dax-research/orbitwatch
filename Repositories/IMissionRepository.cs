using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public interface IMissionRepository
    {
        Task<IEnumerable<Mission>> GetAllAsync();
        Task<Mission?> GetByIdAsync(int id);
        Task AddAsync(Mission mission);
        Task UpdateAsync(Mission mission);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}