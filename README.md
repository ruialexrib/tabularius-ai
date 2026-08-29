# Tabularius AI

**SAF-T (PT) analytics with AI-powered insights**

Tabularius AI is a local-first accounting analysis workspace for importing, exploring and analysing Portuguese SAF-T (PT) data. It supports a lightweight local Windows deployment and a multi-user server deployment, while keeping deterministic accounting processing separate from optional AI-assisted interpretation.

## Technology

- ASP.NET Core MVC
- .NET 9
- Entity Framework Core
- SQLite for local Windows deployments
- SQL Server for server deployments
- Docker for the server stack
- AI provider abstraction planned for assisted analysis

## Deployment modes

Tabularius AI supports two deployment profiles.

### Local Windows

The local profile is designed for an individual user and does not require Docker, SQL Server or a separately installed .NET runtime when using the self-contained publication.

Architecture:

```text
Windows
  TabulariusAI.Web.exe
        |
        +-- ASP.NET Core on localhost
        |
        +-- SQLite
             data/tabularius.db
```

To create the self-contained Windows x64 publication from a development machine with the .NET 9 SDK installed:

```bat
publish-local.bat
```

The output is created in:

```text
artifacts/publish/win-x64/
```

Run `TabulariusAI.Web.exe` from that directory. The SQLite database is created under the local `data` directory on first run.

### Docker server

The server profile is intended for shared browser access and uses SQL Server 2022 Express.

Create a `.env` file containing a strong SQL Server administrator password:

```text
TABULARIUS_DB_PASSWORD=replace-with-a-strong-password
```

Then start the stack:

```bash
docker compose up -d --build
```

The application is exposed on port `8080` by the current development deployment definition. SQL Server data is stored in a named Docker volume.

Do not commit the `.env` file or production credentials.

## Development

Requirements: .NET 9 SDK.

```powershell
dotnet restore
dotnet run --project src/TabulariusAI.Web
```

Development uses the local SQLite profile by default. The database is stored in `data/tabularius.db`.

## Persistence strategy

Local and server deployments use different relational database providers. SQLite is optimized for a simple local installation, while SQL Server is used for the shared server deployment.

The current local bootstrap uses schema creation for SQLite. A provider-specific migration strategy is required before local releases can safely upgrade an existing SQLite database between application versions. Until that migration path is implemented and tested, local self-contained publications should be treated as development/pre-release builds rather than production upgrade-safe installers.

## Current direction

The product hierarchy is:

```text
Entity -> Accounting dossier / fiscal year -> SAF-T (PT) imports -> Analysis
```

The roadmap includes accounting analytics, reconciliation, tests and optional AI-assisted interpretation.
