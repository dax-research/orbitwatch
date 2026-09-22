using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrbitWatch.Models;
using OrbitWatch.Repositories;

namespace OrbitWatch.Controllers
{
    public class TrajectoryRecordsController : Controller
    {
        private readonly ITrajectoryRecordRepository _repository;

        public TrajectoryRecordsController(ITrajectoryRecordRepository repository)
        {
            _repository = repository;
        }

        // GET: TrajectoryRecords
        public async Task<IActionResult> Index()
        {
            var records = await _repository.GetAllAsync();
            return View(records);
        }

        // GET: TrajectoryRecords/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }
            return View(record);
        }

        // GET: TrajectoryRecords/Create
        public async Task<IActionResult> Create()
        {
            await PopulateSatellitesDropDownList();
            return View();
        }

        // POST: TrajectoryRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrajectoryRecord record)
        {
            ModelState.Remove(nameof(record.Satellite));

            if (!await _repository.SatelliteExistsAsync(record.SatelliteId))
            {
                ModelState.AddModelError(nameof(record.SatelliteId), "Select an existing satellite.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSatellitesDropDownList(record.SatelliteId);
                return View(record);
            }

            await _repository.AddAsync(record);
            return RedirectToAction(nameof(Index));
        }

        // GET: TrajectoryRecords/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            await PopulateSatellitesDropDownList(record.SatelliteId);
            return View(record);
        }

        // POST: TrajectoryRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrajectoryRecord record)
        {
            if (id != record.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(record.Satellite));

            if (!await _repository.SatelliteExistsAsync(record.SatelliteId))
            {
                ModelState.AddModelError(nameof(record.SatelliteId), "Select an existing satellite.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSatellitesDropDownList(record.SatelliteId);
                return View(record);
            }

            await _repository.UpdateAsync(record);
            return RedirectToAction(nameof(Index));
        }

        // GET: TrajectoryRecords/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }
            return View(record);
        }

        // POST: TrajectoryRecords/Delete/5
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
