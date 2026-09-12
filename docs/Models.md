# Models

## What are Models?

Models represent the data and main entities used in the OrbitWatch application.

They define the information that the application works with and are also used by Entity Framework Core to create and interact with database tables in SQL Server.

For example, a satellite has information such as its name, NORAD ID, country, launch date, and status. This information is represented by the `Satellite` model.

## Why are Models used?

Models are used to:

- Represent real-world entities in the application.
- Define the properties and data types of those entities.
- Represent relationships between different entities.
- Apply validation rules to input data.
- Work with Entity Framework Core and SQL Server.
- Transfer data between Controllers and Views.

## Models in OrbitWatch

OrbitWatch uses models for important entities such as:

### Satellite

Represents a satellite being monitored by the system.

It contains information such as the satellite name, NORAD ID, country, operator, launch date, orbit type, and current status.

### Mission

Represents a space mission associated with satellites.

It stores information such as mission name, mission type, launch date, launch vehicle, launch site, and mission status.

### TrajectoryRecord

Stores recorded orbital information of a satellite.

It can contain altitude, velocity, latitude, longitude, inclination, and the time at which the observation was recorded.

### Incident

Represents an incident or anomaly related to a satellite.

Examples include communication failure, power failure, orbital anomaly, collision, or loss of control.

It stores information such as incident type, severity, date, description, resolution, and status.

### SatelliteObservation

Represents an observation made about a satellite.

It stores information such as the observation date, observer, observation type, visibility, and notes.

### GroundStation

Represents a ground station used for satellite communication and monitoring.

It contains information such as the station name, location, country, coordinates, contact information, and operational status.

## Model Relationships

Models can have relationships with other models.

For example:

- A Mission can have multiple Satellites.
- A Satellite can have multiple Trajectory Records.
- A Satellite can have multiple Incidents.
- A Satellite can have multiple Observations.
- Ground Stations can be associated with satellites.

These relationships are represented using primary keys and foreign keys.

## Role in MVC

Models are the **data and domain layer** of the MVC architecture.

The general flow is:

Browser → Controller → Model/Database → Controller → View → Browser

The Model does not directly display the user interface. It represents and manages the data used by the application.