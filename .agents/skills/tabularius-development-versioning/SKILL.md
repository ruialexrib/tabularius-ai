---
name: tabularius-development-versioning
description: Assign a distinct TabulariusAI development version when beginning repository changes and preserve stable release versions until publication.
---

# TabulariusAI development versioning

Use this skill whenever beginning work that modifies the TabulariusAI repository. Read-only investigation does not require a version change.

## Development versions

- Inspect the stable version in `Directory.Build.props` and any project-level development override before implementation edits.
- Give each new change set a distinct prerelease version using `MAJOR.MINOR.PATCH-dev.N`.
- Increment `N` for each new feature, correction, refactor, configuration change, documentation change, or other repository modification within the planned release.
- Never reuse or decrement a development version.
- When development starts from a stable release, choose the next release candidate according to semantic versioning: normally increment `MINOR` for functionality and `PATCH` for corrections.
- Keep `Directory.Build.props` as the last stable/released version during ordinary development when a project-level development override is available.
- Ensure diagnostic or UI version surfaces display the complete prerelease version, including `-dev.N`.

A development version change does not authorize a commit, tag, release, container publication, or deployment.

## Release transition

When publishing a release, choose the final semantic version consistently, remove the temporary development override, build and test the release configuration, and verify that the application reports the final stable version.
