# Tabularius AI

**SAF-T analytics with AI-powered insights**

Tabularius AI is a local-first application for importing, analysing and exploring Portuguese SAF-T accounting data, with AI-assisted insights planned as part of the product roadmap.

## Technology

- ASP.NET Core MVC
- .NET 9
- SQL Server LocalDB (planned)
- Entity Framework Core (planned)
- Mistral AI integration (planned)

## Current status

The project is in its initial bootstrap phase. This first version establishes the ASP.NET Core MVC application and shared project structure.

## Run locally

Requirements: .NET 9 SDK.

```powershell
dotnet restore
dotnet run --project src/TabulariusAI.Web
```

Open the local address shown by ASP.NET Core in the terminal.

## Roadmap

1. Application foundation and interface
2. Entity Framework Core and SQL Server LocalDB
3. SAF-T PT import and validation
4. Accounting analytics and dashboards
5. AI service abstraction and Mistral integration
6. AI assistant for SAF-T data
7. Windows packaging and installer
