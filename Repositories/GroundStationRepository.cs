using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public class GroundStationRepository : IGroundStationRepository
    {
        private readonly ApplicationDbContext _context;

        public GroundStationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GroundStation>> GetAllAsync()
        {
            return await _context.GroundStations.ToListAsync();
        }

        public async Task<GroundStation?> GetByIdAsync(int id)
        {
            return await _context.GroundStations.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task AddAsync(GroundStation station)
        {
            await _context.GroundStations.AddAsync(station);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GroundStation station)
        {
            _context.GroundStations.Update(station);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var station = await _context.GroundStations.FindAsync(id);
            if (station != null)
            {
                _context.GroundStations.Remove(station);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.GroundStations.AnyAsync(g => g.Id == id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeStationId = null)
        {
            return await _context.GroundStations.AnyAsync(g =>
                g.Name == name
                && (!excludeStationId.HasValue || g.Id != excludeStationId.Value));
        }
    }
}
