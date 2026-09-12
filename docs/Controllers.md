# Controllers

## What are Controllers?

Controllers handle HTTP requests coming from users or clients and determine what the application should do in response.

They act as the main connection between the user interface, application logic, database, and external services.

For example, when a user requests the list of satellites, the `SatellitesController` receives the request, retrieves the required satellite data, and sends that data to the appropriate View.

## Why are Controllers used?

Controllers are used to:

- Receive HTTP requests.
- Process user actions.
- Retrieve and modify data.
- Perform CRUD operations.
- Validate incoming data.
- Call application services.
- Select the appropriate View.
- Redirect users after operations.
- Apply authentication and authorization rules.

## Controllers in OrbitWatch

OrbitWatch uses controllers to manage different parts of the application.

### HomeController

Handles the main pages of the application, including the dashboard and general navigation.

It can display summary information such as:

- Total satellites.
- Total missions.
- Active satellites.
- Open incidents.
- Recent incidents.

### SatellitesController

Manages satellite-related operations.

It handles:

- Displaying all satellites.
- Viewing satellite details.
- Adding a satellite.
- Editing satellite information.
- Deleting a satellite.
- Searching and filtering satellites.

### MissionsController

Manages space mission information.

It handles:

- Displaying missions.
- Viewing mission details.
- Creating missions.
- Editing missions.
- Deleting missions.
- Managing mission status.

### TrajectoryRecordsController

Manages satellite trajectory records.

It handles:

- Adding trajectory records.
- Viewing trajectory history.
- Editing records.
- Deleting records.
- Displaying orbital information for satellites.

### IncidentsController

Manages satellite incidents and anomalies.

It handles:

- Recording incidents.
- Viewing incidents.
- Updating incident status.
- Editing incident information.
- Deleting incidents.
- Tracking incident severity and resolution.

### SatelliteObservationsController

Manages satellite observations.

It handles:

- Creating observations.
- Viewing observations.
- Editing observations.
- Deleting observations.
- Displaying observation history.

### GroundStationsController

Manages ground station information.

It handles:

- Adding ground stations.
- Viewing ground stations.
- Editing ground station information.
- Deleting ground stations.
- Managing station status.

## CRUD Operations

Controllers are responsible for implementing the main CRUD operations:

- **Create** – Add new data.
- **Read** – Display existing data.
- **Update** – Modify existing data.
- **Delete** – Remove data.

For example, `SatellitesController` can use different actions to create, read, update, and delete satellite records.

## HTTP Methods

Controllers commonly use HTTP methods such as:

- `GET` – Retrieve or display information.
- `POST` – Submit or create information.
- `PUT` – Update information in APIs.
- `DELETE` – Delete information in APIs.

In ASP.NET Core MVC, actions can be explicitly associated with HTTP methods using attributes such as `[HttpGet]` and `[HttpPost]`.

## Authentication and Authorization

Controllers can also control access to different operations.

For example, an action can require an authenticated user:

`[Authorize]`

An action can also be restricted to a specific role:

`[Authorize(Roles = "Admin")]`

This allows OrbitWatch to provide different permissions to administrators, mission managers, and viewers.

## Role in MVC

Controllers are the **request-handling layer** of MVC.

The general flow is:

Browser → Controller → Model/Service → Database/API → Controller → View → Browser

Controllers should coordinate these operations rather than containing all database or external API logic themselves.