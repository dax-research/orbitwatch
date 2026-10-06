using Microsoft.AspNetCore.Mvc;
using OrbitWatch.Repositories;
using OrbitWatch.Services;
using OrbitWatch.Services.Orbit;
using System.Net.Http;

namespace OrbitWatch.Controllers.Api
{
    [ApiController]
    [Route("api/orbit")]
    public class OrbitController : ControllerBase
    {
        private readonly ISatelliteRepository _repository;
        private readonly ISatelliteApiService _satelliteApiService;
        private readonly IOrbitPropagationService _orbitPropagationService;

        public OrbitController(
            ISatelliteRepository repository,
            ISatelliteApiService satelliteApiService,
            IOrbitPropagationService orbitPropagationService)
        {
            _repository = repository;
            _satelliteApiService = satelliteApiService;
            _orbitPropagationService = orbitPropagationService;
        }

        [HttpGet("satellites/{id:int}/position")]
        public async Task<IActionResult> GetPosition(int id, [FromQuery] DateTime? timestamp)
        {
            var satellite = await _repository.GetByIdAsync(id);
            if (satellite == null)
            {
                return NotFound(new { error = "Satellite not found." });
            }

            if (string.IsNullOrWhiteSpace(satellite.NoradId))
            {
                return BadRequest(new { error = "Satellite does not have a valid NORAD ID." });
            }

            try
            {
                var result = await _satelliteApiService.GetSatelliteDataAsync(satellite.NoradId);
                if (result.Data == null)
                {
                    return StatusCode(502, new { error = "Orbital data unavailable from CelesTrak." });
                }

                var targetTime = timestamp ?? DateTime.UtcNow;
                var position = _orbitPropagationService.GetPosition(result.Data, targetTime);

                if (position == null)
                {
                    return StatusCode(500, new { error = "Failed to propagate orbit for the given timestamp." });
                }

                position.IsStaleOrbitalData = result.IsStale;

                return Ok(position);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred during orbit propagation." });
            }
        }

        [HttpGet("satellites/{id:int}/trajectory")]
        public async Task<IActionResult> GetTrajectory(int id)
        {
            var satellite = await _repository.GetByIdAsync(id);
            if (satellite == null)
            {
                return NotFound(new { error = "Satellite not found." });
            }

            if (string.IsNullOrWhiteSpace(satellite.NoradId))
            {
                return BadRequest(new { error = "Satellite does not have a valid NORAD ID." });
            }

            try
            {
                var result = await _satelliteApiService.GetSatelliteDataAsync(satellite.NoradId);
                if (result.Data == null)
                {
                    return StatusCode(502, new { error = "Orbital data unavailable from CelesTrak." });
                }

                var targetTime = DateTime.UtcNow;
                var gpData = result.Data;

                // Estimate orbital period in minutes (MeanMotion = revs per day)
                double orbitalPeriodMinutes = 90; // Default LEO fallback
                if (gpData.MeanMotion.HasValue && gpData.MeanMotion.Value > 0)
                {
                    orbitalPeriodMinutes = (24 * 60) / gpData.MeanMotion.Value;
                }

                // Dynamic step size based on orbital period to avoid excessive points
                int stepMinutes = orbitalPeriodMinutes > 200 ? 5 : 2; 

                var pathTimestamps = new List<DateTime>();
                for (double t = 0; t <= orbitalPeriodMinutes; t += stepMinutes)
                {
                    pathTimestamps.Add(targetTime.AddMinutes(t));
                }

                var fiveMinTimestamps = new List<DateTime>();
                for (int s = 0; s <= 300; s += 30)
                {
                    fiveMinTimestamps.Add(targetTime.AddSeconds(s));
                }

                var currentPosition = _orbitPropagationService.GetPosition(gpData, targetTime);
                if (currentPosition == null)
                {
                    return StatusCode(500, new { error = "Failed to propagate current orbit." });
                }
                currentPosition.IsStaleOrbitalData = result.IsStale;

                var orbitPath = _orbitPropagationService.GetPositions(gpData, pathTimestamps);
                var next5Minutes = _orbitPropagationService.GetPositions(gpData, fiveMinTimestamps);

                return Ok(new
                {
                    currentPosition = currentPosition,
                    orbitPath = orbitPath,
                    next5Minutes = next5Minutes
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred during orbit propagation." });
            }
        }
    }
}
