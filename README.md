<div align="center">

# Tabularius AI

### Accounting data. Analysis with context.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/)
[![SQLite](https://img.shields.io/badge/SQLite-Local-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

</div>

Tabularius AI is an application for importing, exploring, and analysing accounting data from Portuguese **SAF-T (PT)** files. It organises the workspace around **Entity → Dossier / fiscal year → SAF-T import**, preserving the source of the data used in each analysis.

Accounting calculations are deterministic. Artificial Intelligence is optional and adds interpretation and context to the results without replacing values calculated by the application.

![Tabularius AI](assets/tabularius-ai-hero.jpg)

<div align="center">

### Get Tabularius AI for Windows

For normal desktop use, download the latest Windows installer. No .NET SDK, SQL Server, or Docker setup is required.

[**Download Windows Installer (.exe)**](https://github.com/ruialexrib/tabularius-ai/releases/latest/download/TabulariusAI-Setup-0.2.2.exe) · [View all releases](https://github.com/ruialexrib/tabularius-ai/releases)

</div>

## Key features

- Management of accounting entities, dossiers, and fiscal years.
- Structured import and persistence of SAF-T (PT) files.
- Multiple imports per dossier with explicit source selection and traceability.
- Exploration of accounts, customers, suppliers, products, and tax data.
- General ledger exploration with detailed debit and credit lines.
- Sales invoices, working documents, stock movements, and payment documents.
- Deterministic **Trial Balance**, **Income Statement**, and **Balance Sheet**.
- Analytical workspace with overview, anomaly detection, account analysis, and VAT analysis.
- Detailed account and transaction investigation.
- Optional AI assistant and AI analytical reports with configurable providers.
- Dossier backup and restore.
- Authentication, user profiles, and role-based administration.
- Local single-user mode with SQLite.
- Multi-user server mode with SQL Server and Docker.

> The current implementation does not claim formal validation of imported files against the official SAF-T (PT) XSD schema.

## Using Tabularius AI

Tabularius AI can run locally for individual use or on a server for shared multi-user access.

### Local mode — single user

For most Windows users, the recommended option is the installer available from the [latest release](https://github.com/ruialexrib/tabularius-ai/releases/latest). It provides a self-contained application and uses **SQLite** locally, with no SQL Server or Docker infrastructure required.

Developers can run the application directly from source with the **.NET 9 SDK**:

```powershell
git clone https://github.com/ruialexrib/tabularius-ai.git
cd tabularius-ai
dotnet restore
dotnet run --project src/TabulariusAI.Web
```

The application stores local data in the SQLite database at `data/tabularius.db`.

A self-contained Windows build can also be created with:

```bat
publish-local.bat
```

The published application is created in:

```text
artifacts\publish\win-x64\
```

Run `TabulariusAI.Web.exe`. The application starts the local web server and opens the default browser automatically.

### Server mode — multi-user with Docker

For shared use, Tabularius AI can run with **Docker Compose**. This deployment uses the ASP.NET Core application and SQL Server 2022 Express as separate containers.

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

Clone the repository and create the environment configuration:

```powershell
git clone https://github.com/ruialexrib/tabularius-ai.git
cd tabularius-ai
Copy-Item .env.example .env
```

Set a strong SQL Server password in `.env`:

```text
TABULARIUS_DB_PASSWORD=replace-with-a-strong-private-password
```

Start the application:

```powershell
docker compose up -d --build
docker compose ps
```

Open:

```text
http://localhost:8080
```

On Windows, the stack can also be started with:

```bat
start-docker.bat
```

SQL Server data and application logs are stored in persistent Docker volumes.

### Default credentials

On first startup, Tabularius AI automatically creates an administrator account:

| Field | Value |
| --- | --- |
| Username | `admin` |
| Email | `admin@tabularius.local` |
| Temporary password | `LetMeIn` |
| Role | Administrator |

The initial password is temporary. The application requires it to be changed before normal use of the account.

## Artificial Intelligence

AI functionality is optional. When configured, it can be used to interact with dossier data and generate contextual interpretations of indicators presented throughout the analytical areas.

The architecture keeps a clear separation between deterministic accounting calculations and generative AI:

```text
SAF-T data → deterministic rules and calculations → accounting results
                                                    ↓
                                         optional AI interpretation
```

The language model is not the source of accounting totals. Values calculated deterministically by Tabularius AI take precedence over any interpretation generated by the model.

## Contributing

Contributions should be submitted through **pull requests** and remain focused on a clearly identifiable change.

Before submitting a contribution:

- create a branch from the latest `main`;
- keep the change small and focused;
- preserve traceability to the selected SAF-T import;
- never silently aggregate data from different imports;
- keep deterministic accounting calculations separate from generative AI;
- add tests when introducing behaviour that can reasonably be tested;
- ensure the project builds and existing tests continue to pass;
- use Portuguese from Portugal for the application UI and English for code, technical documentation, and commit messages;
- never commit passwords, API keys, tokens, `.env` files, or real confidential accounting data.

Significant changes involving architecture, database schema, SAF-T parsing, security, deployment, or AI integrations should be discussed before implementation.

See [CONTRIBUTING.md](CONTRIBUTING.md) for the complete contribution guidelines.

## License

Distributed under the [MIT License](LICENSE).

Copyright © 2026 [Rui Ribeiro](https://github.com/ruialexrib).
