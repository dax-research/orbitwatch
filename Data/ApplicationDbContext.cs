using Microsoft.EntityFrameworkCore;
using OrbitWatch.Models;

namespace OrbitWatch.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Mission> Missions { get; set; }

        public DbSet<Satellite> Satellites { get; set; }

        public DbSet<TrajectoryRecord> TrajectoryRecords { get; set; }

        public DbSet<Incident> Incidents { get; set; }

        public DbSet<SatelliteObservation> SatelliteObservations { get; set; }

        public DbSet<GroundStation> GroundStations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mission 1 : Many Satellites
            modelBuilder.Entity<Satellite>()
                .HasOne(s => s.Mission)
                .WithMany(m => m.Satellites)
                .HasForeignKey(s => s.MissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Satellite 1 : Many TrajectoryRecords
            modelBuilder.Entity<TrajectoryRecord>()
                .HasOne(t => t.Satellite)
                .WithMany(s => s.TrajectoryRecords)
                .HasForeignKey(t => t.SatelliteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Satellite 1 : Many Incidents
            modelBuilder.Entity<Incident>()
                .HasOne(i => i.Satellite)
                .WithMany(s => s.Incidents)
                .HasForeignKey(i => i.SatelliteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Satellite 1 : Many SatelliteObservations
            modelBuilder.Entity<SatelliteObservation>()
                .HasOne(o => o.Satellite)
                .WithMany(s => s.SatelliteObservations)
                .HasForeignKey(o => o.SatelliteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}