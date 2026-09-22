using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public interface IGroundStationRepository
    {
        Task<IEnumerable<GroundStation>> GetAllAsync();
        Task<GroundStation?> GetByIdAsync(int id);
        Task AddAsync(GroundStation station);
        Task UpdateAsync(GroundStation station);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeStationId = null);
    }
}
