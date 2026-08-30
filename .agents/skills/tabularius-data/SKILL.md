---
name: tabularius-data
description: Implement and review Tabularius AI Entity Framework Core, SQL Server LocalDB, migrations and local data lifecycle.
---

# Tabularius AI data lifecycle

Use this skill for Entity Framework Core, SQL Server LocalDB, schema changes, migrations and stored imported data.

## Persistence model

- Use SQL Server LocalDB for the Windows local-first application unless requirements explicitly change.
- Keep EF Core entities and configurations aligned with the SAF-T/domain model without leaking database concerns into parsing code.
- Add named EF Core migrations for schema changes and review generated operations for destructive changes, unsafe defaults and missing indexes.
- Preserve imported source identifiers needed for traceability.
- Use appropriate indexes for common analytical and lookup paths once query patterns are established.

## EF Core migration consistency

Whenever an EF Core entity, `DbContext` configuration, relationship, index, constraint or persisted property is created, changed or removed:

1. Determine whether the change affects the EF Core model or database schema.
2. If it changes the schema, create a descriptive EF Core migration in the same change set. Do not postpone the migration to a later task.
3. Let EF Core update `TabulariusDbContextModelSnapshot.cs` as part of migration generation.
4. Do not manually edit the model snapshot except when repairing a verified migration inconsistency. Any repair must be followed by the consistency checks below.
5. Do not delete, rewrite or regenerate migrations that have already been published in a release or may have been applied to user databases. Add a new forward migration instead.
6. Run `dotnet ef migrations has-pending-model-changes --project src/TabulariusAI.Web --startup-project src/TabulariusAI.Web` after model-related work.
7. Run the relevant tests after migration generation and verify that the solution builds.
8. Do not consider the task complete while EF Core reports pending model changes.

The expected invariant before commit is:

`EF Core model == ModelSnapshot == committed migrations`

The database used by an installed application can still be behind the committed migrations, so application startup/upgrade behavior must apply pending migrations safely where that lifecycle is responsible for upgrades.

## Integrity

- Import persistence should be transactional where practical.
- A failed SAF-T import must not leave a dataset in an ambiguous partially imported state.
- Define ownership and deletion behavior for an imported dataset and its dependent records explicitly.
- Do not persist secrets or Mistral API keys in ordinary application tables unless an explicitly designed secure mechanism is introduced.

## Local application behavior

- Database creation and migration must be suitable for a non-technical Windows user and later installer/bootstrap workflows.
- Do not require Docker for the end-user runtime.
- Keep development and test databases separate from real imported accounting data.

## Verification

Test migrations against the intended LocalDB path, verify representative relationships and counts after import, and test rollback on invalid input when persistence is changed.

Before completing persistence work, verify all of the following:

- The model snapshot represents the current EF Core model.
- Every real schema change has a committed migration.
- `dotnet ef migrations has-pending-model-changes` succeeds.
- Relevant migration, persistence and application tests succeed.
- No published migration was altered retroactively.
