# TabulariusAI agent instructions

These instructions apply to all AI-assisted development in this repository.

## Project skills

Before modifying the repository, use the relevant skills under `.agents/skills/`:

- `tabularius-code-quality` for C# implementation, documentation, testing, and verification.
- `tabularius-git` for commits and repository change handling.
- `tabularius-ui` for Razor, CSS, dashboards, forms, tables, and interactions.
- `tabularius-development-versioning` when beginning repository modifications.
- `tabularius-saft` for SAF-T ingestion, parsing, accounting data, analytics, and AI interpretation.

## Mandatory C# documentation

All maintained C# classes, records, structs, interfaces, enums, delegates, methods, constructors, properties, and public/protected members must use XML documentation comments in English as defined by the `tabularius-code-quality` skill.

Documentation must describe purpose and contract rather than merely repeat the symbol name. Parameters, return values, and relevant contract exceptions must be documented where applicable.

## General principles

- Preserve deterministic accounting calculations independently from generative AI output.
- Treat uploaded SAF-T files and extracted accounting information as sensitive data.
- Do not commit credentials, secrets, real SAF-T files, personal data, database backups, or generated sensitive exports.
- Prefer maintainable architecture and shared components over feature-specific duplication.
- Do not claim tests, CI, browser verification, Docker health, security validation, or deployment success unless they were actually performed.
