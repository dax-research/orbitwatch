using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Models;

namespace OrbitWatch.Repositories
{
    public class CelesTrakOrbitalDataCacheRepository : ICelesTrakOrbitalDataCacheRepository
    {
        private readonly ApplicationDbContext _context;

        public CelesTrakOrbitalDataCacheRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CelesTrakOrbitalDataCache?> GetByNoradIdAsync(string noradId)
        {
            return await _context.CelesTrakOrbitalDataCaches
                .FirstOrDefaultAsync(c => c.NoradId == noradId);
        }

        public async Task UpsertAsync(CelesTrakOrbitalDataCache cacheRecord)
        {
            var existing = await _context.CelesTrakOrbitalDataCaches
                .FirstOrDefaultAsync(c => c.NoradId == cacheRecord.NoradId);

            if (existing != null)
            {
                existing.PayloadJson = cacheRecord.PayloadJson;
                existing.FetchedAtUtc = cacheRecord.FetchedAtUtc;
                _context.CelesTrakOrbitalDataCaches.Update(existing);
            }
            else
            {
                await _context.CelesTrakOrbitalDataCaches.AddAsync(cacheRecord);
            }

            await _context.SaveChangesAsync();
        }
    }
}
