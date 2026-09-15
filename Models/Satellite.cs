using System.ComponentModel.DataAnnotations;

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
    public DateTime LaunchDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string OrbitType { get; set; } = string.Empty;

    public int MissionId { get; set; }

    public Mission Mission { get; set; } = null!;

    public ICollection<TrajectoryRecord> TrajectoryRecords { get; set; }
    = new List<TrajectoryRecord>();
}
