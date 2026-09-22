using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public class SatelliteRepository : ISatelliteRepository
    {
        private readonly ApplicationDbContext _context;

        public SatelliteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Satellite>> GetAllAsync()
        {
            return await _context.Satellites
                .Include(s => s.Mission)
                .ToListAsync();
        }

        public async Task<Satellite?> GetByIdAsync(int id)
        {
            return await _context.Satellites
                .Include(s => s.Mission)
                .Include(s => s.TrajectoryRecords)
                .Include(s => s.Incidents)
                .Include(s => s.SatelliteObservations)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Satellite satellite)
        {
            await _context.Satellites.AddAsync(satellite);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Satellite satellite)
        {
            _context.Satellites.Update(satellite);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var satellite = await _context.Satellites.FindAsync(id);

            if (satellite != null)
            {
                _context.Satellites.Remove(satellite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Satellites.AnyAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Mission>> GetAllMissionsAsync()
        {
            return await _context.Missions.ToListAsync();
        }

        public async Task<bool> MissionExistsAsync(int missionId)
        {
            return await _context.Missions.AnyAsync(m => m.Id == missionId);
        }

        public async Task<bool> NoradIdExistsAsync(int noradId, int? excludeSatelliteId = null)
        {
            return await _context.Satellites.AnyAsync(s =>
                s.NoradId == noradId
                && (!excludeSatelliteId.HasValue || s.Id != excludeSatelliteId.Value));
        }
    }
}
