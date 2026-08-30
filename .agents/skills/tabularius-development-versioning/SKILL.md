---
name: tabularius-development-versioning
description: Require a timestamped development version based on the current Tabularius AI release for every repository modification.
---

# Tabularius AI development versioning

Use this skill whenever work modifies the Tabularius AI repository. Read-only investigation does not require a version change.

## Mandatory rule

Every repository modification must result in a new application development version. Do not complete a code, UI, configuration, documentation, skill or other repository change without updating the version.

## Development versions

- Determine the current stable/released version before implementation edits.
- Development versions must keep that exact stable release as their base version. Do not increment PATCH, MINOR or MAJOR merely because development work has started.
- Use the format `MAJOR.MINOR.PATCH-dev.YYYYMMDDHHMMSS`.
- Example: if the current release is `0.2.2`, a development build created on 30 August 2026 at 13:41:50 is `0.2.2-dev.20260830134150`.
- Generate a fresh timestamp for every repository modification/change set so each development build is uniquely identifiable.
- The timestamp uses the local development time in compact `YYYYMMDDHHMMSS` form.
- Never reuse a development version.
- Ensure diagnostic and UI version surfaces display the complete development version.
- Keep the version source used by `ApplicationInfo` and the built assembly aligned.

A development version change does not by itself authorize a tag, stable release, container publication or deployment.

## Release transition

When publishing a release, choose the final semantic version consistently and remove the `-dev.YYYYMMDDHHMMSS` suffix. Build and test the release configuration and verify that the application reports the final stable version.

After a stable release, every subsequent repository modification must use that new stable release as the base for timestamped development versions until the next release is published.
