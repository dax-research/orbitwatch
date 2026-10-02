using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrbitWatch.Repositories;

namespace OrbitWatch.Controllers
{
    [Authorize]
    public class TrackingController : Controller
    {
        private readonly ISatelliteRepository _repository;

        public TrackingController(ISatelliteRepository repository)
        {
            _repository = repository;
        }

        // GET: /Tracking
        public async Task<IActionResult> Index()
        {
            var satellites = await _repository.GetAllAsync();
            return View(satellites);
        }
    }
}
