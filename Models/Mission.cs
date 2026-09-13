using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models;

public class Mission
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime LaunchDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Required]
    [StringLength(150)]
    public string Agency { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    public List<Satellite> Satellites { get; set; } = new();
}
