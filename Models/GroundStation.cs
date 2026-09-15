using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class GroundStation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [StringLength(100)]
        public string Operator { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }
}