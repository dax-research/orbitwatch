using Microsoft.AspNetCore.Mvc;
using OrbitWatch.Models;
using OrbitWatch.Repositories;

namespace OrbitWatch.Controllers
{
    public class GroundStationsController : Controller
    {
        private readonly IGroundStationRepository _repository;

        public GroundStationsController(IGroundStationRepository repository)
        {
            _repository = repository;
        }

        // GET: GroundStations
        public async Task<IActionResult> Index()
        {
            var stations = await _repository.GetAllAsync();
            return View(stations);
        }

        // GET: GroundStations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var station = await _repository.GetByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }
            return View(station);
        }

        // GET: GroundStations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GroundStations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroundStation station)
        {
            if (await _repository.NameExistsAsync(station.Name))
            {
                ModelState.AddModelError(nameof(station.Name), "A ground station with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(station);
            }

            await _repository.AddAsync(station);
            return RedirectToAction(nameof(Index));
        }

        // GET: GroundStations/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var station = await _repository.GetByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }
            return View(station);
        }

        // POST: GroundStations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GroundStation station)
        {
            if (id != station.Id)
            {
                return NotFound();
            }

            if (await _repository.NameExistsAsync(station.Name, station.Id))
            {
                ModelState.AddModelError(nameof(station.Name), "A ground station with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(station);
            }

            await _repository.UpdateAsync(station);
            return RedirectToAction(nameof(Index));
        }

        // GET: GroundStations/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var station = await _repository.GetByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }
            return View(station);
        }

        // POST: GroundStations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
