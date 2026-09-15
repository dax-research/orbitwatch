using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public class MissionRepository : IMissionRepository
    {
        private readonly ApplicationDbContext _context;

        public MissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Mission>> GetAllAsync()
        {
            return await _context.Missions
                .ToListAsync();
        }

        public async Task<Mission?> GetByIdAsync(int id)
        {
            return await _context.Missions
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddAsync(Mission mission)
        {
            await _context.Missions.AddAsync(mission);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Mission mission)
        {
            _context.Missions.Update(mission);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var mission = await GetByIdAsync(id);

            if (mission != null)
            {
                _context.Missions.Remove(mission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Missions
                .AnyAsync(m => m.Id == id);
        }
    }
}