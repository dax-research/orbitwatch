using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Models;

namespace OrbitWatch.Controllers;

public class MissionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var missions = await _context.Missions
            .Include(mission => mission.Satellites)
            .OrderByDescending(mission => mission.LaunchDate)
            .ToListAsync();

        return View(missions);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var mission = await _context.Missions
            .Include(mission => mission.Satellites)
            .FirstOrDefaultAsync(mission => mission.Id == id);

        if (mission is null)
        {
            return NotFound();
        }

        return View(mission);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,LaunchDate,EndDate,Agency,Status")] Mission mission)
    {
        if (!ModelState.IsValid)
        {
            return View(mission);
        }

        _context.Add(mission);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Mission '{mission.Name}' is now in the mission register.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var mission = await _context.Missions.FindAsync(id);
        if (mission is null)
        {
            return NotFound();
        }

        return View(mission);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,LaunchDate,EndDate,Agency,Status")] Mission mission)
    {
        if (id != mission.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(mission);
        }

        try
        {
            _context.Update(mission);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MissionExists(mission.Id))
            {
                return NotFound();
            }

            throw;
        }

        TempData["SuccessMessage"] = $"Mission '{mission.Name}' was updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var mission = await _context.Missions
            .Include(mission => mission.Satellites)
            .FirstOrDefaultAsync(mission => mission.Id == id);

        if (mission is null)
        {
            return NotFound();
        }

        return View(mission);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var mission = await _context.Missions.FindAsync(id);
        if (mission is null)
        {
            return NotFound();
        }

        _context.Missions.Remove(mission);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Mission '{mission.Name}' was removed.";

        return RedirectToAction(nameof(Index));
    }

    private bool MissionExists(int id)
    {
        return _context.Missions.Any(mission => mission.Id == id);
    }
}
