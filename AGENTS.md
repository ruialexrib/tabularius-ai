# Tabularius AI project guidance

These instructions apply to every task in this repository.

## Product and language

- Treat Tabularius AI as a local-first Windows application for importing, validating, analysing and exploring Portuguese SAF-T accounting data with optional AI-powered insights.
- Write all user-facing application text in European Portuguese unless an existing surface is explicitly English.
- Keep source-code identifiers, technical documentation, XML documentation comments, commit messages and developer-facing guidance in English.
- Never commit `.env`, credentials, API keys, database files containing real accounting data, real SAF-T files, or other sensitive business data.

## Implementation

- Preserve .NET 9 and ASP.NET Core MVC unless the user explicitly requests a framework change.
- Keep the .NET SDK aligned with Denarius AI so both projects can be developed and tested on the same machine.
- Use SQL Server LocalDB and Entity Framework Core for local persistence unless requirements explicitly change.
- Add English XML documentation comments to every C# class, interface, record, enum and method/function. Document public properties when they carry domain or API meaning. Add `<param>`, `<returns>`, `<exception>` and `<typeparam>` elements when applicable.
- Prefer clear domain services and deterministic calculations over placing business logic in controllers or Razor views.
- Keep Mistral behind an application abstraction such as `IAIService` so the provider can be replaced without changing domain logic.
- AI may explain, summarise or propose insights, but accounting totals and analytical metrics must be calculated deterministically by the application.

## SAF-T and data

- Treat imported SAF-T files as untrusted input. Validate structure, required fields and supported schema/version before persistence or analysis.
- Preserve source identifiers and accounting meaning from the SAF-T file; do not invent missing accounting data.
- Use `decimal` for monetary values and explicit date/time types for accounting periods.
- Keep import, persistence, analytics and AI-context generation separated so imported data can be tested independently of model output.

## Verification

- Build the solution after C# changes when practical and treat warnings as failures.
- Add focused tests for parsing, validation, persistence and financial calculations as those areas are introduced.
- For visual changes, inspect the rendered page and responsive behavior rather than relying only on compilation.
- Do not claim that a build, test, installer, database migration or UI state works unless it was actually verified.

## Repository workflows

- Work through feature branches and pull requests. Keep `main` stable.
- Use the project skills under `.agents/skills/` for coding standards, Git workflow, quality checks, UI work, SAF-T processing, persistence, AI workflows and development versioning.
- Apply `tabularius-development-versioning` whenever repository modifications begin so each change set receives a distinct prerelease development version.
- Write Git commit messages in English using Conventional Commits.
- Do not merge a pull request unless the user explicitly asks for or approves the merge.
- Prefer squash merge for completed feature pull requests.
