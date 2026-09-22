using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public class SatelliteObservationRepository : ISatelliteObservationRepository
    {
        private readonly ApplicationDbContext _context;

        public SatelliteObservationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SatelliteObservation>> GetAllAsync()
        {
            return await _context.SatelliteObservations
                .Include(o => o.Satellite)
                .ToListAsync();
        }

        public async Task<SatelliteObservation?> GetByIdAsync(int id)
        {
            return await _context.SatelliteObservations
                .Include(o => o.Satellite)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(SatelliteObservation observation)
        {
            await _context.SatelliteObservations.AddAsync(observation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SatelliteObservation observation)
        {
            _context.SatelliteObservations.Update(observation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var observation = await _context.SatelliteObservations.FindAsync(id);
            if (observation != null)
            {
                _context.SatelliteObservations.Remove(observation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.SatelliteObservations.AnyAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Satellite>> GetAllSatellitesAsync()
        {
            return await _context.Satellites.ToListAsync();
        }
    }
}
