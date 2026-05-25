# HospitalManagement

HospitalManagement is a hospital operations app built with .NET 10, Clean Architecture, and CQRS. It provides a Web API and a React UI with a PostgreSQL backend.

## Stack

- .NET 10 / ASP.NET Core
- EF Core + PostgreSQL
- MediatR + FluentValidation
- React 19 + Vite
- .NET Aspire (AppHost)

## Structure

- `src/Domain`: Domain model and rules
- `src/Application`: CQRS handlers and validation
- `src/Infrastructure`: Persistence, Identity, external services
- `src/Web`: API and web host
- `src/AppHost`: Aspire orchestration
- `tests/*`: Test projects

## Run (Docker)

```bash
cd HospitalManagement
docker compose up --build
```

- UI: `http://localhost:5173`
- API: `http://localhost:5001`
- API docs: `http://localhost:5001/scalar`

Seeded dev users:

- Administrator: `administrator@localhost` / `Administrator1!`
- Doctor: `doctor@localhost` / `Doctor1!`
- Patient: `patient@localhost` / `Patient1!`

Docker starts three services:

- `frontend`: React/Vite development server on port `5173`
- `web`: ASP.NET Core API on port `5001`
- `db`: PostgreSQL on port `5432`

## Run (Local)

Start the database:

```bash
cd HospitalManagement
docker compose up db
```

Start the API:

```bash
cd HospitalManagement
dotnet run --project src/Web/Web.csproj --launch-profile http
```

Start the UI:

```bash
cd HospitalManagement/src/Web/ClientApp
npm install
HOSPITALMANAGEMENT_API_URL=http://localhost:5276 npm run start
```

## Tests

```bash
dotnet test
```
