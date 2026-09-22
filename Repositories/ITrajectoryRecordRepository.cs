using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public interface ITrajectoryRecordRepository
    {
        Task<IEnumerable<TrajectoryRecord>> GetAllAsync();
        Task<TrajectoryRecord?> GetByIdAsync(int id);
        Task AddAsync(TrajectoryRecord record);
        Task UpdateAsync(TrajectoryRecord record);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Satellite>> GetAllSatellitesAsync();
    }
}
