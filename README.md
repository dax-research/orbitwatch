# 🛰️ OrbitWatch

### Satellite Mission, Trajectory & Incident Management System

OrbitWatch is an **ASP.NET Core MVC web application** for managing satellite missions, satellite information, orbital trajectory records, ground stations, observations, and satellite-related incidents.

The project combines **C#**, **ASP.NET Core MVC**, **Entity Framework Core**, **Microsoft SQL Server**, **Authentication & Authorization**, **Middleware**, and **external space-data APIs** into a single web application.

---

## 🚀 Features

### 🛰️ Satellite Management

* Add new satellites
* View satellite information
* Update satellite details
* Delete satellites
* Search and filter satellites
* Track satellite status and orbit type

### 🚀 Mission Management

* Create and manage space missions
* Associate satellites with missions
* Store mission details such as launch date, launch vehicle, launch site, and mission type
* Track mission status

### 🌍 Trajectory Records

* Store satellite trajectory observations
* Record altitude, velocity, latitude, longitude, and inclination
* View historical trajectory records for individual satellites

### ⚠️ Incident & Anomaly Management

* Record satellite incidents and anomalies
* Track incident severity
* Maintain incident descriptions and resolutions
* Track whether an incident is resolved or under investigation

Examples of incidents include:

* Communication Failure
* Power Failure
* Orbital Anomaly
* Loss of Control
* Collision
* Debris Event
* Mission Failure

### 🔭 Satellite Observations

* Record satellite observations
* Store observation date and observer information
* Maintain observation notes and visibility information

### 📡 Ground Station Management

* Manage satellite ground stations
* Store station location and coordinates
* Track ground station status
* Associate ground stations with satellite operations

### 📊 Dashboard

The dashboard provides an overview of the system, including:

* Total satellites
* Active and inactive satellites
* Total missions
* Recent incidents
* Mission statistics
* Satellite information
* External space-data information

### 🔐 Authentication

The application uses **ASP.NET Core Identity** for:

* User registration
* User login
* User logout
* Password management
* Secure password storage

### 🛡️ Authorization

Role-based authorization is implemented using different user roles:

| Role               | Access                                                  |
| ------------------ | ------------------------------------------------------- |
| **Admin**          | Full system access                                      |
| **MissionManager** | Manage missions, satellites, trajectories and incidents |
| **Viewer**         | View system information without modifying data          |

### ⚙️ Middleware

The application demonstrates ASP.NET Core middleware through:

* Custom exception-handling middleware
* Request logging middleware
* Built-in authentication and authorization middleware
* Routing middleware
* Static file middleware

The request pipeline follows the general flow:

```text
HTTP Request
     ↓
Exception Middleware
     ↓
Request Logging Middleware
     ↓
Authentication
     ↓
Authorization
     ↓
Routing / MVC
     ↓
Controller
     ↓
Service / EF Core
     ↓
SQL Server / External API
     ↓
HTTP Response
```

### 🌐 External APIs

OrbitWatch can integrate external space-data APIs to supplement locally stored information.

Potential integrations include:

* **CelesTrak** for satellite orbital data
* **NASA APIs** for astronomy and space-related information

External API data is separated from the application's core database using a service layer.

---

## 🏗️ Architecture

OrbitWatch follows the **Model-View-Controller (MVC)** architecture.

```text
                    ┌─────────────┐
                    │    User     │
                    └──────┬──────┘
                           │
                           ▼
                    ┌─────────────┐
                    │    Views    │
                    │   Razor     │
                    └──────┬──────┘
                           │
                           ▼
                    ┌─────────────┐
                    │ Controllers │
                    └──────┬──────┘
                           │
                ┌──────────┴──────────┐
                ▼                     ▼
          ┌───────────┐        ┌───────────┐
          │ Services  │        │  EF Core  │
          └─────┬─────┘        └─────┬─────┘
                │                    │
                ▼                    ▼
        External APIs            SQL Server
```

---

## 🗂️ Project Structure

```text
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
├── Views/
│   ├── Home/
│   ├── Satellites/
│   ├── Missions/
│   ├── TrajectoryRecords/
│   ├── Incidents/
│   ├── SatelliteObservations/
│   └── GroundStations/
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
└── OrbitWatch.csproj
```

---

## 🗄️ Database Design

The application uses **Microsoft SQL Server** with **Entity Framework Core**.

### Main entities

```text
Mission
   │
   │ 1 : Many
   ▼
Satellite
   │
   ├──── 1 : Many ──── TrajectoryRecord
   │
   ├──── 1 : Many ──── Incident
   │
   └──── 1 : Many ──── SatelliteObservation

GroundStation
```

ASP.NET Core Identity also provides tables for users, roles, and authentication-related data.

---

## 🛠️ Technology Stack

| Technology                   | Purpose                    |
| ---------------------------- | -------------------------- |
| **C#**                       | Programming language       |
| **ASP.NET Core MVC**         | Web application framework  |
| **Razor Views**              | Frontend/UI                |
| **Bootstrap**                | UI styling                 |
| **Entity Framework Core**    | ORM and database access    |
| **Microsoft SQL Server**     | Database                   |
| **ASP.NET Core Identity**    | Authentication             |
| **Role-Based Authorization** | Access control             |
| **HttpClient**               | External API communication |
| **LINQ**                     | Queries and filtering      |
| **Git**                      | Version control            |
| **GitHub**                   | Source code management     |

---

## 📋 Core Models

### Satellite

Stores information about satellites.

```text
Id
Name
NoradId
Country
Operator
LaunchDate
Status
OrbitType
MissionId
```

### Mission

Stores information about space missions.

```text
Id
Name
Description
MissionType
LaunchDate
LaunchVehicle
LaunchSite
Status
```

### TrajectoryRecord

Stores orbital trajectory information.

```text
Id
SatelliteId
RecordedAt
Altitude
Velocity
Latitude
Longitude
Inclination
```

### Incident

Stores satellite failures and anomalies.

```text
Id
SatelliteId
Title
IncidentType
Severity
IncidentDate
Description
Resolution
Status
```

### SatelliteObservation

Stores satellite observation records.

```text
Id
SatelliteId
ObservationDate
Observer
ObservationType
Notes
Visibility
```

### GroundStation

Stores ground station information.

```text
Id
Name
Location
Country
Latitude
Longitude
Contact
Status
```

---

## 🔐 Security

OrbitWatch uses ASP.NET Core Identity and role-based authorization.

Example:

```csharp
[Authorize]
public class SatellitesController : Controller
{
}
```

Only administrators can perform certain sensitive operations:

```csharp
[Authorize(Roles = "Admin")]
public IActionResult Delete(int id)
{
    // Delete satellite
}
```

Multiple roles can also be authorized:

```csharp
[Authorize(Roles = "Admin,MissionManager")]
public IActionResult Create()
{
    // Create satellite
}
```

---

## ⚙️ Middleware

Custom middleware is used to demonstrate the ASP.NET Core request pipeline.

### Exception Middleware

Handles unexpected application errors and provides a user-friendly error response.

### Request Logging Middleware

Records information such as:

```text
HTTP Method
Request Path
User
Timestamp
Response Status Code
Request Execution Time
```

---

## 🌐 API Integration

OrbitWatch can retrieve external satellite and astronomy information through REST APIs.

The external API functionality is implemented through dedicated service classes instead of placing API logic directly inside controllers.

```text
Controller
     ↓
API Service
     ↓
HttpClient
     ↓
External API
     ↓
Response
```

---

## 💻 Getting Started

### Prerequisites

Make sure the following are installed:

* Visual Studio
* .NET SDK
* Microsoft SQL Server
* SQL Server Management Studio (SSMS)
* Git

### Clone the repository

```bash
git clone https://github.com/YOUR-USERNAME/orbitwatch.git
```

### Open the project

Open:

```text
OrbitWatch.sln
```

in Visual Studio.

### Configure SQL Server

Update the connection string in:

```text
appsettings.json
```

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=OrbitWatchDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### Apply migrations

Run in the Package Manager Console:

```powershell
Update-Database
```

Or using the .NET CLI:

```bash
dotnet ef database update
```

### Run the application

Run the project from Visual Studio or use:

```bash
dotnet run
```

---

## 📌 Project Goals

The main goals of OrbitWatch are to demonstrate practical implementation of:

* ASP.NET Core MVC
* Model-View-Controller architecture
* CRUD operations
* Entity Framework Core
* SQL Server database management
* One-to-many and many-to-many relationships
* Authentication
* Role-based authorization
* Middleware
* Dependency Injection
* REST API integration
* LINQ queries
* Form validation
* Git and GitHub workflow

---

## 🎓 Academic Project

This project is developed as part of a **Web Application Development / ASP.NET Core MVC academic assignment**.

The project focuses on applying web development concepts to a space and satellite management domain.

---

## 👨‍💻 Author

**Dax Patel**

---

## 📄 License

This project is developed for educational purposes.
