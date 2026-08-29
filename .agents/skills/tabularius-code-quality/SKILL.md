---
name: tabularius-code-quality
description: Apply TabulariusAI C# coding, documentation, testing, and verification standards.
---

# TabulariusAI code quality

Use this skill whenever implementing, modifying, reviewing, or refactoring C# code in TabulariusAI.

## XML documentation

- All C# classes, records, structs, interfaces, enums, and delegates must have XML documentation comments written in English.
- All explicitly declared methods, constructors, properties, and public or protected members must have XML documentation comments written in English.
- Use `/// <summary>` to describe purpose rather than restating the member name.
- Document parameters with `/// <param>` and return values with `/// <returns>` when applicable.
- Document relevant exceptions with `/// <exception>` when the contract intentionally exposes them.
- Keep documentation concise, accurate, and updated when behavior changes.
- Do not add meaningless comments to generated code or framework-generated members that are not maintained by the project.

## C# quality rules

- Keep nullable reference types enabled and do not suppress nullable warnings without a justified reason.
- Treat compiler and analyzer warnings as failures because the repository enables `TreatWarningsAsErrors`.
- Prefer clear domain names and small focused methods over abbreviations or generic helper names.
- Use asynchronous APIs for I/O operations and propagate `CancellationToken` where appropriate.
- Validate external input, especially uploaded SAF-T files, before processing it.
- Do not expose secrets, connection strings, API keys, personal data, or uploaded accounting data in logs.

## Verification

Start with checks focused on the changed behavior and run the full solution checks for broad changes or before a release.

```powershell
dotnet restore TabulariusAI.sln
dotnet build TabulariusAI.sln --configuration Release --no-restore
dotnet test TabulariusAI.sln --configuration Release --no-build
```

If no tests exist yet, report that explicitly rather than claiming test coverage.

For visual changes, render and inspect the affected page in addition to compiling it. For Docker or runtime configuration changes, build the affected image and verify application health.

## Reporting

- State the exact verification performed.
- Distinguish passed, skipped, and environment-blocked checks.
- Do not claim CI, browser, container, security, or deployment success without directly verifying it.
