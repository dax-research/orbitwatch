using System;
using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class CelesTrakOrbitalDataCache
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NoradId { get; set; } = string.Empty;

        [Required]
        public string PayloadJson { get; set; } = string.Empty;

        public DateTime FetchedAtUtc { get; set; }
    }
}
