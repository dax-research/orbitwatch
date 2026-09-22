using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrbitWatch.Models;
using OrbitWatch.Repositories;

namespace OrbitWatch.Controllers
{
    public class IncidentsController : Controller
    {
        private readonly IIncidentRepository _repository;

        public IncidentsController(IIncidentRepository repository)
        {
            _repository = repository;
        }

        // GET: Incidents
        public async Task<IActionResult> Index()
        {
            var incidents = await _repository.GetAllAsync();
            return View(incidents);
        }

        // GET: Incidents/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var incident = await _repository.GetByIdAsync(id);

            if (incident == null)
            {
                return NotFound();
            }

            return View(incident);
        }

        // GET: Incidents/Create
        public async Task<IActionResult> Create()
        {
            await PopulateSatellitesDropDownList();
            return View();
        }

        // POST: Incidents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Incident incident)
        {
            ModelState.Remove(nameof(incident.Satellite));

            if (!await _repository.SatelliteExistsAsync(incident.SatelliteId))
            {
                ModelState.AddModelError(nameof(incident.SatelliteId), "Select an existing satellite.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSatellitesDropDownList(incident.SatelliteId);
                return View(incident);
            }

            await _repository.AddAsync(incident);
            return RedirectToAction(nameof(Index));
        }

        // GET: Incidents/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var incident = await _repository.GetByIdAsync(id);

            if (incident == null)
            {
                return NotFound();
            }

            await PopulateSatellitesDropDownList(incident.SatelliteId);
            return View(incident);
        }

        // POST: Incidents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Incident incident)
        {
            if (id != incident.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(incident.Satellite));

            if (!await _repository.SatelliteExistsAsync(incident.SatelliteId))
            {
                ModelState.AddModelError(nameof(incident.SatelliteId), "Select an existing satellite.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSatellitesDropDownList(incident.SatelliteId);
                return View(incident);
            }

            await _repository.UpdateAsync(incident);
            return RedirectToAction(nameof(Index));
        }

        // GET: Incidents/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var incident = await _repository.GetByIdAsync(id);

            if (incident == null)
            {
                return NotFound();
            }

            return View(incident);
        }

        // POST: Incidents/Delete/5
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
