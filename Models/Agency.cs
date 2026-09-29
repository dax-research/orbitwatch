using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class Agency
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        public ICollection<Mission> Missions { get; set; }
            = new List<Mission>();

        public ICollection<Satellite> Satellites { get; set; }
            = new List<Satellite>();

        public ICollection<GroundStation> GroundStations { get; set; }
            = new List<GroundStation>();
    }
}