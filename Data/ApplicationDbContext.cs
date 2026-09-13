using Microsoft.EntityFrameworkCore;
using OrbitWatch.Models;

namespace OrbitWatch.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Mission> Missions { get; set; } = null!;

    public DbSet<Satellite> Satellites { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Satellite>()
            .HasOne(satellite => satellite.Mission)
            .WithMany(mission => mission.Satellites)
            .HasForeignKey(satellite => satellite.MissionId);
    }
}
