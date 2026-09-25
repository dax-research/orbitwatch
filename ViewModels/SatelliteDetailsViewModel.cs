using OrbitWatch.Models;
using OrbitWatch.Models.Api;

namespace OrbitWatch.ViewModels
{
    public class SatelliteDetailsViewModel
    {
        public Satellite Satellite { get; set; } = null!;

        public CelesTrakGpData? CelesTrakData { get; set; }

        public string? CelesTrakError { get; set; }
    }
}