using Microsoft.EntityFrameworkCore;
using OrbitWatch.Data;
using OrbitWatch.Repositories;
using OrbitWatch.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<ISatelliteRepository, SatelliteRepository>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
builder.Services.AddScoped<ITrajectoryRecordRepository, TrajectoryRecordRepository>();
builder.Services.AddScoped<ISatelliteObservationRepository, SatelliteObservationRepository>();
builder.Services.AddScoped<IGroundStationRepository, GroundStationRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

builder.Services.AddHttpClient<ISatelliteApiService, SatelliteApiService>(client =>
{
    client.BaseAddress = new Uri("https://celestrak.org/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
