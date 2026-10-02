using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrbitWatch.Models;

namespace OrbitWatch.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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

        public DbSet<Country> Countries { get; set; }

        public DbSet<Agency> Agencies { get; set; }

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

            modelBuilder.Entity<Satellite>()
                .HasIndex(s => s.NoradId)
                .IsUnique();

            modelBuilder.Entity<GroundStation>()
                .HasIndex(g => g.Name)
                .IsUnique();

            // Restrict Country/Agency deletes to avoid SQL Server multiple cascade paths
            // (Agency -> Mission -> Satellite and Agency -> Satellite).
            modelBuilder.Entity<Mission>()
                .HasOne(m => m.Agency)
                .WithMany(a => a.Missions)
                .HasForeignKey(m => m.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Satellite>()
                .HasOne(s => s.Agency)
                .WithMany(a => a.Satellites)
                .HasForeignKey(s => s.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Satellite>()
                .HasOne(s => s.Country)
                .WithMany(c => c.Satellites)
                .HasForeignKey(s => s.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroundStation>()
                .HasOne(g => g.Country)
                .WithMany(c => c.GroundStations)
                .HasForeignKey(g => g.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroundStation>()
                .HasOne(g => g.Agency)
                .WithMany(a => a.GroundStations)
                .HasForeignKey(g => g.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);
            // Development Seed Data
            modelBuilder.Entity<Mission>().HasData(
                new Mission 
                { 
                    Id = 1, 
                    Name = "ISS Operations", 
                    Status = "Active", 
                    AgencyId = 2, 
                    LaunchDate = new DateTime(1998, 11, 20, 0, 0, 0, DateTimeKind.Utc) 
                }
            );

            modelBuilder.Entity<Satellite>().HasData(
                new Satellite 
                { 
                    Id = 1, 
                    Name = "ISS (ZARYA)", 
                    NoradId = "25544", 
                    Status = "Active", 
                    OrbitType = "LEO", 
                    CountryId = 2, 
                    AgencyId = 2, 
                    MissionId = 1, 
                    LaunchDate = new DateTime(1998, 11, 20, 0, 0, 0, DateTimeKind.Utc) 
                }
            );
        }
    }
}