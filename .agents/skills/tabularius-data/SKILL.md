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
