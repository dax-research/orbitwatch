using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrbitWatch.Models;
using OrbitWatch.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrbitWatch.Controllers
{
    [Authorize]
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
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: GroundStations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(GroundStation station)
        {
            ModelState.Remove(nameof(station.Country));
            ModelState.Remove(nameof(station.Agency));

            if (await _repository.NameExistsAsync(station.Name))
            {
                ModelState.AddModelError(nameof(station.Name), "A ground station with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(
                    station.CountryId,
                    station.AgencyId);

                return View(station);
            }

            await _repository.AddAsync(station);
            return RedirectToAction(nameof(Index));
        }

        // GET: GroundStations/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var station = await _repository.GetByIdAsync(id);

            if (station == null)
            {
                return NotFound();
            }

            await PopulateDropdowns(
                station.CountryId,
                station.AgencyId);

            return View(station);
        }

        // POST: GroundStations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, GroundStation station)
        {
            if (id != station.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(station.Country));
            ModelState.Remove(nameof(station.Agency));

            if (await _repository.NameExistsAsync(station.Name, station.Id))
            {
                ModelState.AddModelError(nameof(station.Name), "A ground station with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(
                    station.CountryId,
                    station.AgencyId);

                return View(station);
            }

            await _repository.UpdateAsync(station);
            return RedirectToAction(nameof(Index));
        }

        // GET: GroundStations/Delete/5
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(
            int? countryId = null,
            int? agencyId = null)
        {
            ViewBag.Countries = new SelectList(
                await _repository.GetAllCountriesAsync(),
                "Id",
                "Name",
                countryId);

            ViewBag.Agencies = new SelectList(
                await _repository.GetAllAgenciesAsync(),
                "Id",
                "Name",
                agencyId);
        }
    }
}

