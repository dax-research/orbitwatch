using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.ViewModels;

namespace OrbitWatch.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetStatisticsAsync()
        {
            return new DashboardViewModel
            {
                TrackedSatellites = await _context.Satellites.CountAsync(),
                ActiveMissions = await _context.Missions
                    .CountAsync(mission => mission.Status.ToLower() == "active"),
                OpenIncidents = await _context.Incidents
                    .CountAsync(incident =>
                        incident.Status.ToLower() == "open"
                        || incident.Status.ToLower() == "investigating"),
                GroundStations = await _context.GroundStations.CountAsync()
            };
        }
    }
}
