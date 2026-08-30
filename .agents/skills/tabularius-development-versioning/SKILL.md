---
name: tabularius-development-versioning
description: Require a sequential development version based on the current Tabularius AI release for every repository modification.
---

# Tabularius AI development versioning

Use this skill whenever work modifies the Tabularius AI repository. Read-only investigation does not require a version change.

## Mandatory rule

Every repository modification must result in a new application development version. Do not complete a code, UI, configuration, documentation, skill or other repository change without updating the version.

## Development versions

- Determine the current stable/released version before implementation edits.
- Development versions must keep that exact stable release as their base version. Do not increment PATCH, MINOR or MAJOR merely because development work has started.
- Use the format `MAJOR.MINOR.PATCH-dev.NNN`.
- Example: if the current release is `0.2.2`, development builds are `0.2.2-dev.001`, `0.2.2-dev.002`, `0.2.2-dev.003`, and so on.
- Increment the three-digit development sequence for every repository modification/change set.
- Never reuse or decrement a development sequence.
- Determine the next sequence from the latest development version for the current stable release.
- Ensure diagnostic and UI version surfaces display the complete development version.
- Keep the version source used by `ApplicationInfo` and the built assembly aligned.

A development version change does not by itself authorize a tag, stable release, container publication or deployment.

## Release transition

When publishing a release, choose the final semantic version consistently and remove the `-dev.NNN` suffix. Build and test the release configuration and verify that the application reports the final stable version.

After a stable release, reset the development sequence for that new release base: the first subsequent development version is `MAJOR.MINOR.PATCH-dev.001`.
