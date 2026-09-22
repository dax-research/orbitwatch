using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrbitWatch.Models;
using OrbitWatch.Repositories;

namespace OrbitWatch.Controllers
{
    public class SatellitesController : Controller
    {
        private readonly ISatelliteRepository _repository;

        public SatellitesController(ISatelliteRepository repository)
        {
            _repository = repository;
        }

        // GET: Satellites
        public async Task<IActionResult> Index()
        {
            var satellites = await _repository.GetAllAsync();
            return View(satellites);
        }

        // GET: Satellites/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var satellite = await _repository.GetByIdAsync(id);

            if (satellite == null)
            {
                return NotFound();
            }

            return View(satellite);
        }

        // GET: Satellites/Create
        public async Task<IActionResult> Create()
        {
            await PopulateMissionsDropDownList();
            return View();
        }

        // POST: Satellites/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Satellite satellite)
        {
            if (!ModelState.IsValid)
            {
                await PopulateMissionsDropDownList(satellite.MissionId);
                return View(satellite);
            }

            await _repository.AddAsync(satellite);
            return RedirectToAction(nameof(Index));
        }

        // GET: Satellites/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var satellite = await _repository.GetByIdAsync(id);

            if (satellite == null)
            {
                return NotFound();
            }

            await PopulateMissionsDropDownList(satellite.MissionId);
            return View(satellite);
        }

        // POST: Satellites/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Satellite satellite)
        {
            if (id != satellite.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateMissionsDropDownList(satellite.MissionId);
                return View(satellite);
            }

            await _repository.UpdateAsync(satellite);
            return RedirectToAction(nameof(Index));
        }

        // GET: Satellites/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var satellite = await _repository.GetByIdAsync(id);

            if (satellite == null)
            {
                return NotFound();
            }

            return View(satellite);
        }

        // POST: Satellites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateMissionsDropDownList(int? selectedMissionId = null)
        {
            var missions = await _repository.GetAllMissionsAsync();
            ViewBag.MissionId = new SelectList(missions, "Id", "Name", selectedMissionId);
        }
    }
}
