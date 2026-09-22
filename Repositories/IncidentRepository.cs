using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Incident>> GetAllAsync()
        {
            return await _context.Incidents
                .Include(i => i.Satellite)
                .ToListAsync();
        }

        public async Task<Incident?> GetByIdAsync(int id)
        {
            return await _context.Incidents
                .Include(i => i.Satellite)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task AddAsync(Incident incident)
        {
            await _context.Incidents.AddAsync(incident);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Incident incident)
        {
            _context.Incidents.Update(incident);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);

            if (incident != null)
            {
                _context.Incidents.Remove(incident);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Incidents.AnyAsync(i => i.Id == id);
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
