OrbitWatch/
│
├── Controllers/
│   ├── HomeController.cs
│   ├── SatellitesController.cs
│   ├── MissionsController.cs
│   ├── TrajectoryRecordsController.cs
│   ├── IncidentsController.cs
│   ├── SatelliteObservationsController.cs
│   └── GroundStationsController.cs
│
├── Models/
│   ├── Satellite.cs
│   ├── Mission.cs
│   ├── TrajectoryRecord.cs
│   ├── Incident.cs
│   ├── SatelliteObservation.cs
│   └── GroundStation.cs
│
├── Views/
│   ├── Home/
│   │   └── Index.cshtml
│   │
│   ├── Satellites/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   ├── Missions/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   ├── TrajectoryRecords/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   ├── Incidents/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   ├── SatelliteObservations/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   ├── GroundStations/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   └── Shared/
│       ├── _Layout.cshtml
│       ├── _ValidationScriptsPartial.cshtml
│       └── Error.cshtml
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Services/
│   ├── ISatelliteApiService.cs
│   ├── SatelliteApiService.cs
│   ├── INasaApiService.cs
│   └── NasaApiService.cs
│
├── Middleware/
│   ├── ExceptionMiddleware.cs
│   └── RequestLoggingMiddleware.cs
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
│
├── Migrations/
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── OrbitWatch.csproj
└── README.md