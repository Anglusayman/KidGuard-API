# KeyGuard (Kid Guard) – Backend API

KeyGuard is a child-care application. This repository contains the **backend (Web API)** that I built as my **graduation project**.

## Tech Stack

- **Language:** C#
- **Framework:** ASP.NET Core Web API
- **Database:** SQL (relational database)
- **Authentication:** JWT (JSON Web Tokens)
- **IDE:** Visual Studio

## Project Structure

```
├── Controllers/     # API endpoints (handle HTTP requests)
├── DTO/             # Data Transfer Objects (what the API sends and receives)
├── Model/           # Entity classes that represent database tables
├── Db_context/      # Database context and database access setup
├── Properties/      # Launch settings
├── Program.cs       # App startup, services, and middleware configuration
└── Kid_Guard_part1.http   # Sample requests to test the API
```

## My Role

I was responsible for the **entire backend**: designing the database, building the API endpoints, and securing them with JWT authentication.

## Getting Started

### Requirements

- Visual Studio (with the ASP.NET workload) or the .NET SDK
- A SQL database

### Configuration

For security reasons, the real `appsettings.json` file is **not included** in this repository because it contains private data (database connection string and the JWT secret key).

To run the project, create an `appsettings.json` file in the project root with the following structure and your own values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER,1433;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  },
  "Jwt": {
    "Key": "YOUR_LONG_SECRET_KEY_AT_LEAST_64_CHARACTERS",
    "Issuer": "ChildCareApp",
    "Audience": "ChildCareUsers",
    "DurationInMinutes": 60
  }
}
```

### Run

1. Open the project in Visual Studio.
2. Add your `appsettings.json` as shown above.
3. Press **F5** to run, or use `dotnet run` from the terminal.
4. Test the endpoints using the included `Kid_Guard_part1.http` file.

## Author

Backend Developer – C#, C++, SQL
