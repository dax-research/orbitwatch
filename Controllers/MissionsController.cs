using Microsoft.AspNetCore.Mvc;
using OrbitWatch.Models;
using OrbitWatch.Repositories;

namespace OrbitWatch.Controllers
{
    public class MissionsController : Controller
    {
        private readonly IMissionRepository _repository;

        public MissionsController(IMissionRepository repository)
        {
            _repository = repository;
        }

        // GET: Missions
        public async Task<IActionResult> Index()
        {
            var missions = await _repository.GetAllAsync();

            return View(missions);
        }

        // GET: Missions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var mission = await _repository.GetByIdAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            return View(mission);
        }

        // GET: Missions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Missions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mission mission)
        {
            if (!ModelState.IsValid)
            {
                return View(mission);
            }

            await _repository.AddAsync(mission);

            return RedirectToAction(nameof(Index));
        }

        // GET: Missions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var mission = await _repository.GetByIdAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            return View(mission);
        }

        // POST: Missions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mission mission)
        {
            if (id != mission.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(mission);
            }

            await _repository.UpdateAsync(mission);

            return RedirectToAction(nameof(Index));
        }

        // GET: Missions/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var mission = await _repository.GetByIdAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            return View(mission);
        }

        // POST: Missions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}