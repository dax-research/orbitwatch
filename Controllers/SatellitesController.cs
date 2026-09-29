using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrbitWatch.Models;
using OrbitWatch.Repositories;
using OrbitWatch.Services;
using OrbitWatch.ViewModels;

namespace OrbitWatch.Controllers
{
    [Authorize]
    public class SatellitesController : Controller
    {
        private readonly ISatelliteRepository _repository;
        private readonly ISatelliteApiService _satelliteApiService;

        public SatellitesController(
            ISatelliteRepository repository,
            ISatelliteApiService satelliteApiService)
        {
            _repository = repository;
            _satelliteApiService = satelliteApiService;
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

            var viewModel = new SatelliteDetailsViewModel
            {
                Satellite = satellite
            };

            if (!string.IsNullOrWhiteSpace(satellite.NoradId))
            {
                try
                {
                    viewModel.CelesTrakData =
                        await _satelliteApiService.GetSatelliteDataAsync(
                            satellite.NoradId);
                }
                catch (HttpRequestException)
                {
                    viewModel.CelesTrakError =
                        "CelesTrak data is temporarily unavailable.";
                }
            }

            return View(viewModel);
        }

        // GET: Satellites/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: Satellites/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Satellite satellite)
        {
            ModelState.Remove(nameof(satellite.Mission));
            ModelState.Remove(nameof(satellite.Country));
            ModelState.Remove(nameof(satellite.Agency));

            if (await _repository.NoradIdExistsAsync(satellite.NoradId))
            {
                ModelState.AddModelError(nameof(satellite.NoradId), "A satellite with this NORAD ID already exists.");
            }

            if (!await _repository.MissionExistsAsync(satellite.MissionId))
            {
                ModelState.AddModelError(nameof(satellite.MissionId), "Select an existing mission.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(
                    satellite.MissionId,
                    satellite.CountryId,
                    satellite.AgencyId);

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

            await PopulateDropdowns(
                satellite.MissionId,
                satellite.CountryId,
                satellite.AgencyId);

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

            ModelState.Remove(nameof(satellite.Mission));
            ModelState.Remove(nameof(satellite.Country));
            ModelState.Remove(nameof(satellite.Agency));

            if (await _repository.NoradIdExistsAsync(satellite.NoradId, satellite.Id))
            {
                ModelState.AddModelError(nameof(satellite.NoradId), "A satellite with this NORAD ID already exists.");
            }

            if (!await _repository.MissionExistsAsync(satellite.MissionId))
            {
                ModelState.AddModelError(nameof(satellite.MissionId), "Select an existing mission.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(
                    satellite.MissionId,
                    satellite.CountryId,
                    satellite.AgencyId);

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

        private async Task PopulateDropdowns(
            int? missionId = null,
            int? countryId = null,
            int? agencyId = null)
        {
            ViewBag.Missions = new SelectList(
                await _repository.GetAllMissionsAsync(),
                "Id",
                "Name",
                missionId);

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

