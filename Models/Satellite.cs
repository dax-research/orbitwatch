using System.ComponentModel.DataAnnotations;
using OrbitWatch.Validation;

namespace OrbitWatch.Models;

public class Satellite
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int NoradId { get; set; }

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Operator { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [NotFuture(DateOnly = true, ErrorMessage = "Satellite launch date cannot be in the future.")]
    public DateTime LaunchDate { get; set; }

    [Required]
    [StringLength(50)]
    [AllowedValues("Active", "Inactive", "Decommissioned", "In Testing",
        ErrorMessage = "Select a valid satellite status.")]
    public string Status { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [AllowedValues("LEO", "MEO", "GEO", "HEO", "SSO",
        ErrorMessage = "Select a valid orbit type.")]
    public string OrbitType { get; set; } = string.Empty;

    public int MissionId { get; set; }

    public Mission Mission { get; set; } = null!;

    public ICollection<TrajectoryRecord> TrajectoryRecords { get; set; }
    = new List<TrajectoryRecord>();

    public ICollection<Incident> Incidents { get; set; }
    = new List<Incident>();

    public ICollection<SatelliteObservation> SatelliteObservations { get; set; }
    = new List<SatelliteObservation>();
}
