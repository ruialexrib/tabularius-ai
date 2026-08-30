---
name: tabularius-development-versioning
description: Require a new Tabularius AI development version for every repository modification and preserve stable release versions until publication.
---

# Tabularius AI development versioning

Use this skill whenever work modifies the Tabularius AI repository. Read-only investigation does not require a version change.

## Mandatory rule

Every repository modification must result in a new application version. Do not complete a code, UI, configuration, documentation, skill or other repository change without updating the version.

## Development versions

- Inspect the current application version before implementation edits.
- Development builds must always carry the `-dev` prerelease suffix.
- Use sequential semantic versions such as `0.2.3-dev`, `0.2.4-dev`, `0.2.5-dev` for successive development change sets.
- Increment the version for every new feature, correction, refactor, configuration change, documentation change, skill update or other repository modification.
- Never reuse or decrement a development version.
- Ensure diagnostic and UI version surfaces display the complete prerelease version, including `-dev`.
- Keep the version source used by `ApplicationInfo` and the built assembly aligned.

A development version change does not by itself authorize a tag, stable release, container publication or deployment.

## Release transition

When publishing a release, choose the final semantic version consistently and remove the `-dev` suffix. Build and test the release configuration and verify that the application reports the final stable version.

After a stable release, the next repository modification must immediately move to a new `-dev` version.
