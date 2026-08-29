---
name: tabularius-quality
description: Validate Tabularius AI changes with local checks and the mandatory GitHub Actions CI quality gate.
---

# Tabularius AI quality checks

Use this skill for tests, build verification, CI status investigation, SAF-T checks, persistence verification and browser review. This is the single source of truth for repository quality gates; other skills should reference it rather than duplicate CI rules.

## Mandatory quality gate

Every commit pushed to `main` and every pull request targeting `main` must run `.github/workflows/ci.yml` in GitHub Actions.

The CI workflow must, at minimum:

1. restore the solution;
2. build the complete solution in Release configuration with warnings treated as errors;
3. run the complete automated test suite;
4. collect and retain test results and code coverage artifacts.

A repository change is not considered verified merely because the files were committed successfully. After a commit, inspect the corresponding GitHub Actions CI run. Report the CI result accurately.

If CI fails, do not continue feature development as though the change were healthy. Inspect the failed job and logs, identify the root cause, correct the code, tests or workflow as appropriate, commit the correction, and inspect the new CI run. Repeat until CI passes or an external/environmental blocker is established. Never weaken, remove or bypass a legitimate failing test simply to make CI green.

When multiple commits are made as part of one change set and GitHub concurrency cancels superseded runs, verify the latest run for the final commit.

## Local verification

When a suitable .NET 9 environment is available, run:

```powershell
dotnet restore TabulariusAI.sln
dotnet build TabulariusAI.sln --configuration Release --no-restore
dotnet test TabulariusAI.sln --configuration Release --no-build
```

Treat warnings as failures because the repository enables `TreatWarningsAsErrors`.

Local checks complement GitHub CI; they do not replace inspection of the CI run after a pushed commit.

## Test strategy

Add focused automated tests with each behavior that can be tested deterministically. Prioritise SAF-T parsing and validation, accounting calculations, persistence rules, duplicate handling, reconciliation rules and AI response validation boundaries.

Use synthetic or anonymised fixtures only. Never commit real taxpayer SAF-T data or confidential accounting records.

A bug correction should normally include a regression test that fails before the correction and passes afterwards.

## SAF-T verification

Test supported namespaces and versions, malformed XML, unsupported structures, missing mandatory fields, encoding, decimal precision, date parsing, duplicate data, empty collections and large-file behavior as those capabilities are introduced.

Persistence changes require tests for representative relationships and transactional behavior. Failed imports must not leave partially persisted datasets.

## Browser review

For visual changes, inspect the rendered page and the state affected by the change, including empty, loading, validation and populated states when applicable. Check a narrow viewport when the layout is responsive.

## Reporting

State exactly what was verified. Distinguish local checks, GitHub CI, browser checks and environment-blocked checks. Do not claim a build, test, migration or UI state passed unless the corresponding check was actually observed.
