# HospitalManagement

HospitalManagement is a modern hospital management application.
It uses .NET 10, Clean Architecture, and CQRS on the backend, with React + Vite on the frontend.

## Features

- Patient and doctor management
- Appointment creation, completion, cancellation, and rescheduling
- Examination, prescription, and lab request/result workflows
- OpenAPI + Scalar API documentation
- Unit, integration, functional, and acceptance test layers

## Technology Stack

- .NET 10
- ASP.NET Core Web API
- MediatR + FluentValidation
- Entity Framework Core + PostgreSQL
- React 19 + Vite
- .NET Aspire (AppHost)

## Architecture

The project is structured with the following layers:

- `src/Domain`: Business rules, entities, value objects, and domain events
- `src/Application`: Use cases, CQRS handlers, validation, and pipeline behaviors
- `src/Infrastructure`: EF Core, Identity, persistence, and external integrations
- `src/Web`: HTTP endpoints, authentication, OpenAPI, and static file hosting
- `src/AppHost`: Service orchestration with Aspire (db + api + frontend)
- `tests/*`: Unit, integration, functional, and acceptance test projects

## Requirements

- .NET SDK 10.0.101 or later
- Node.js 22 or later
- Docker (optional but recommended)

## Quick Start (with Aspire)

1. Clone the repository.
2. Install dependencies:

```bash
dotnet restore
cd src/Web/ClientApp
npm install
cd ../../..
```

3. Start the application:

```bash
dotnet run --project src/AppHost/AppHost.csproj
```

Once running, use the Aspire dashboard to view API and frontend endpoints.

## Run with Docker

The following command starts PostgreSQL and web services with docker compose:

```bash
docker compose up --build
```

Default ports:

- Web: `http://localhost:5001`
- PostgreSQL: `localhost:5432`

## API Documentation

When the Web API is running, Scalar is available at:

- `/scalar`

## Tests

Run all tests:

```bash
dotnet test
```

Run only domain unit tests:

```bash
dotnet test tests/Domain.UnitTests/Domain.UnitTests.csproj
```

## Contributing

Feel free to open an issue or submit a pull request.

## License

No license has been specified for this project yet.