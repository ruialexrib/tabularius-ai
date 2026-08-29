---
name: tabularius-quality
description: Validate Tabularius AI changes with .NET builds, tests, SAF-T checks, persistence verification and browser review.
---

# Tabularius AI quality checks

Use this skill when verifying implementation work or reviewing a pull request.

## Standard verification

Use the project .NET 9 SDK and run:

```powershell
dotnet restore TabulariusAI.sln
dotnet build TabulariusAI.sln --configuration Release --no-restore
```

Run the full test suite once test projects exist:

```powershell
dotnet test TabulariusAI.sln --configuration Release --no-build
```

Treat warnings as failures because the repository enables `TreatWarningsAsErrors`.

## Code quality

- Verify every new or modified C# class and method has English XML documentation according to `tabularius-coding`.
- Prefer focused tests for changed behavior, then run the full suite before a release or broad refactor.
- Persistence and SAF-T parser changes require integration or representative parser tests.
- Monetary calculations require deterministic expected-value tests using `decimal`.

## SAF-T verification

- Test valid files, malformed XML, unsupported namespaces/versions, missing required fields and large-file behavior when applicable.
- Do not use real confidential SAF-T data as committed fixtures. Use synthetic or anonymised fixtures.
- Verify that failed imports do not leave partially persisted datasets.

## Browser review

For visual changes, inspect the rendered page and the state affected by the change, including empty, loading, validation and populated states when applicable. Check a narrow viewport when the layout is responsive.

## Reporting

Report exactly which commands and observable checks were completed. Distinguish passed, skipped and environment-blocked checks. Do not claim success for builds, tests, database migrations, installers or UI states that were not directly verified.
