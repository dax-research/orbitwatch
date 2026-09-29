using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class Country
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        public ICollection<Satellite> Satellites { get; set; }
            = new List<Satellite>();

        public ICollection<GroundStation> GroundStations { get; set; }
            = new List<GroundStation>();
    }
}