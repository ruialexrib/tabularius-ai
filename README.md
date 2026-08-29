<div align="center">

# Tabularius AI

### Accounting analysis and control from SAF-T (PT) data.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/Entity_Framework-Core-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQLite](https://img.shields.io/badge/SQLite-Local-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![SQL Server 2022](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Developed by [Rui Ribeiro](https://github.com/ruialexrib)

</div>

---

## About

Tabularius AI is a local-first accounting analysis workspace for importing, exploring and analysing Portuguese **SAF-T (PT)** data. It is designed around the accounting workflow **Entity → Accounting dossier / fiscal year → SAF-T (PT) imports → Analysis**, providing a structured foundation for accounting control, reconciliation, testing and assisted interpretation.

The application supports both a lightweight Windows deployment using SQLite and a shared server deployment using SQL Server and Docker. Deterministic accounting processing remains separate from optional AI-assisted features.

## Highlights

- Import and structured exploration of Portuguese SAF-T (PT) files
- Entity and accounting dossier / fiscal year organisation
- Multiple SAF-T (PT) imports with explicit source and accounting-period selection
- Chart of accounts, customers, suppliers and products
- General ledger transactions and accounting entry details
- Source traceability across SAF-T (PT) views
- Authentication, user roles and administration
- Local Windows mode with SQLite
- Multi-user server mode with SQL Server 2022 Express and Docker Compose
- Foundation for trial balance, general ledger, journal, reconciliation and accounting tests
- Provider-neutral architecture for future AI-assisted accounting analysis

> SAF-T (PT) XSD validation is currently deferred. Imported files are parsed and processed by the application, but the current release does not claim validation against the official XSD schema.

## Technology

| Technology | Role |
| --- | --- |
| **.NET 9 / ASP.NET Core MVC** | Web application and accounting workflows |
| **Entity Framework Core** | Persistence and database access |
| **SQLite** | Lightweight local Windows persistence |
| **SQL Server 2022 Express** | Shared server and multi-user persistence |
| **Docker Compose** | Reproducible server deployment |
| **ASP.NET Core Identity** | Authentication, roles and user administration |
| **xUnit** | Automated tests |

## Deployment modes

Tabularius AI provides two deployment profiles for different usage scenarios.

### Local Windows

The local profile is intended for individual use. It runs directly on Windows, stores its data in SQLite and does not require Docker, SQL Server or a separately installed .NET runtime when using the self-contained publication.

```text
Windows
  TabulariusAI.Web.exe
        │
        ├── ASP.NET Core on localhost
        │
        └── SQLite
             data/tabularius.db
```

To create a self-contained Windows x64 build from a machine with the .NET 9 SDK:

```bat
publish-local.bat
```

The publication is created in:

```text
artifacts\publish\win-x64\
```

Run `TabulariusAI.Web.exe`. The application selects an available local port and opens automatically in the default browser.

The current SQLite bootstrap creates the schema for a new database. A provider-specific migration strategy is still required before local releases can safely upgrade existing SQLite databases between application versions. For that reason, self-contained local builds should currently be treated as development/pre-release distributions rather than upgrade-safe production installers.

### Docker server

The server profile is intended for shared browser access. It runs the ASP.NET Core application and SQL Server 2022 Express as separate containers, with persistent database and log volumes.

```text
Browser
   │
   │ :8080
   ▼
Tabularius AI
ASP.NET Core
   │
   ▼
SQL Server 2022 Express
```

On Windows, the simplest way to start the stack is:

```bat
start-docker.bat
```

On the first run, the script creates `.env` from `.env.example`. Replace the example SQL Server password with a strong private password and run the script again. It builds the images, starts the containers, displays their status and opens the application in the default browser.

Alternatively, start Docker Compose manually:

```powershell
git clone https://github.com/ruialexrib/tabularius-ai.git
cd tabularius-ai
Copy-Item .env.example .env
```

Configure the database password in `.env`:

```text
TABULARIUS_DB_PASSWORD=replace-with-a-strong-private-password
```

Then start the stack:

```powershell
docker compose up -d --build
docker compose ps
```

Open `http://localhost:8080`.

Useful commands:

```powershell
docker compose logs -f tabularius-ai-web
docker compose down
```

SQL Server data is stored in a named Docker volume and is preserved when the containers are recreated. Never commit `.env`, credentials or real SAF-T (PT) files containing accounting data.

## Development

Requirements: .NET 9 SDK.

```powershell
git clone https://github.com/ruialexrib/tabularius-ai.git
cd tabularius-ai
dotnet restore
dotnet run --project src/TabulariusAI.Web
```

Development uses the SQLite profile by default.

On Windows, `start.bat` can also be used during development to monitor the `main` branch, update the local checkout and restart the application after new commits.

## Product direction

The application follows a dossier-centred accounting model:

```text
Entity
  └── Accounting dossier / fiscal year
        └── SAF-T (PT) imports
              ├── Accounts
              ├── Customers
              ├── Suppliers
              ├── Products
              ├── Accounting entries
              └── Analysis and controls
```

The next product areas include trial balance, general ledger, journal views, reconciliation, accounting tests and optional AI-assisted interpretation. AI functionality is intended to assist analysis rather than replace deterministic accounting calculations.

## Security and data

SAF-T (PT) files can contain sensitive accounting, customer, supplier and transaction information. Real company files, credentials, `.env` files and other confidential data must not be committed to the repository.

The application uses ASP.NET Core Identity for authentication and role-based administration. Server deployments should be placed behind HTTPS before being exposed outside a trusted local network.

## License

Distributed under the [MIT License](LICENSE). Copyright © 2026 [Rui Ribeiro](https://github.com/ruialexrib).
