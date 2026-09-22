using Microsoft.AspNetCore.Mvc;
using OrbitWatch.Models;
using OrbitWatch.Repositories;
using System.Diagnostics;

namespace OrbitWatch.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;

        public HomeController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<IActionResult> Index()
        {
            var statistics = await _dashboardRepository.GetStatisticsAsync();

            return View(statistics);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
