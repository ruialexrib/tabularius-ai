---
name: tabularius-development-versioning
description: Require a sequential NuGet-compatible development version based on the current Tabularius AI release for every repository modification and define release progression.
---

# Tabularius AI development versioning

Use this skill whenever work modifies the Tabularius AI repository. Read-only investigation does not require a version change.

## Mandatory rule

Every repository modification must result in a new application development version. Do not complete a code, UI, configuration, documentation, skill or other repository change without updating the version.

## Development versions

- Determine the current stable/released version before implementation edits.
- Development versions must keep that exact stable release as their base version. Do not increment PATCH, MINOR or MAJOR merely because development work has started.
- Use the NuGet/SemVer-compatible format `MAJOR.MINOR.PATCH-dev.N`.
- Example: if the current release is `0.2.2`, development builds are `0.2.2-dev.1`, `0.2.2-dev.2`, `0.2.2-dev.3`, and so on.
- Numeric prerelease identifiers must not contain leading zeroes; therefore do not use `dev.001`, `dev.002`, etc.
- Increment the development sequence for every repository modification/change set.
- Never reuse or decrement a development sequence.
- Determine the next sequence from the latest development version for the current stable release.
- Ensure diagnostic and UI version surfaces display the complete development version.
- Keep the version source used by `ApplicationInfo` and the built assembly aligned.

A development version change does not by itself authorize a tag, stable release, container publication or deployment.

## Release transition

Choose the next stable release according to the scope accumulated since the current release:

- Corrections, maintenance and a small number of incremental functionalities: increment PATCH. Example: `0.2.2` becomes `0.2.3`.
- Large functional additions, substantial product evolution or broad changes: increment MINOR and reset PATCH. Example: `0.2.2` becomes `0.3.0`.
- Do not decide the release number solely from the development sequence; assess the actual scope of the changes included in the release.
- Remove the `-dev.N` suffix for the stable release, build and test the release configuration, and verify that the application reports the final stable version.

After a stable release, reset the development sequence for that new release base. For example, after releasing `0.2.3`, the first subsequent development version is `0.2.3-dev.1`; after releasing `0.3.0`, it is `0.3.0-dev.1`.
