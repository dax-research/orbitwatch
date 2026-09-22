using OrbitWatch.ViewModels;

namespace OrbitWatch.Repositories
{
    public interface IDashboardRepository
    {
        Task<DashboardViewModel> GetStatisticsAsync();
    }
}
