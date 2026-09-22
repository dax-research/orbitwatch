using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public class TrajectoryRecordRepository : ITrajectoryRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public TrajectoryRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TrajectoryRecord>> GetAllAsync()
        {
            return await _context.TrajectoryRecords
                .Include(t => t.Satellite)
                .ToListAsync();
        }

        public async Task<TrajectoryRecord?> GetByIdAsync(int id)
        {
            return await _context.TrajectoryRecords
                .Include(t => t.Satellite)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(TrajectoryRecord record)
        {
            await _context.TrajectoryRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TrajectoryRecord record)
        {
            _context.TrajectoryRecords.Update(record);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var record = await _context.TrajectoryRecords.FindAsync(id);
            if (record != null)
            {
                _context.TrajectoryRecords.Remove(record);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TrajectoryRecords.AnyAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Satellite>> GetAllSatellitesAsync()
        {
            return await _context.Satellites.ToListAsync();
        }

        public async Task<bool> SatelliteExistsAsync(int satelliteId)
        {
            return await _context.Satellites.AnyAsync(s => s.Id == satelliteId);
        }
    }
}
