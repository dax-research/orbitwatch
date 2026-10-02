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
    }
}
