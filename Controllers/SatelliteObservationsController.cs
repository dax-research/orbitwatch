using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrbitWatch.Models;
using OrbitWatch.Repositories;

namespace OrbitWatch.Controllers
{
    public class SatelliteObservationsController : Controller
    {
        private readonly ISatelliteObservationRepository _repository;

        public SatelliteObservationsController(ISatelliteObservationRepository repository)
        {
            _repository = repository;
        }

        // GET: SatelliteObservations
        public async Task<IActionResult> Index()
        {
            var observations = await _repository.GetAllAsync();
            return View(observations);
        }

        // GET: SatelliteObservations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var observation = await _repository.GetByIdAsync(id);
            if (observation == null)
            {
                return NotFound();
            }
            return View(observation);
        }

        // GET: SatelliteObservations/Create
        public async Task<IActionResult> Create()
        {
            await PopulateSatellitesDropDownList();
            return View();
        }

        // POST: SatelliteObservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SatelliteObservation observation)
        {
            ModelState.Remove(nameof(observation.Satellite));

            if (!ModelState.IsValid)
            {
                await PopulateSatellitesDropDownList(observation.SatelliteId);
                return View(observation);
            }

            await _repository.AddAsync(observation);
            return RedirectToAction(nameof(Index));
        }

        // GET: SatelliteObservations/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var observation = await _repository.GetByIdAsync(id);
            if (observation == null)
            {
                return NotFound();
            }

            await PopulateSatellitesDropDownList(observation.SatelliteId);
            return View(observation);
        }

        // POST: SatelliteObservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SatelliteObservation observation)
        {
            if (id != observation.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(observation.Satellite));

            if (!ModelState.IsValid)
            {
                await PopulateSatellitesDropDownList(observation.SatelliteId);
                return View(observation);
            }

            await _repository.UpdateAsync(observation);
            return RedirectToAction(nameof(Index));
        }

        // GET: SatelliteObservations/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var observation = await _repository.GetByIdAsync(id);
            if (observation == null)
            {
                return NotFound();
            }
            return View(observation);
        }

        // POST: SatelliteObservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSatellitesDropDownList(int? selectedSatelliteId = null)
        {
            var satellites = await _repository.GetAllSatellitesAsync();
            ViewBag.SatelliteId = new SelectList(satellites, "Id", "Name", selectedSatelliteId);
        }
    }
}
