# Contributing to Tabularius AI

Thank you for your interest in contributing to Tabularius AI.

This document defines the basic rules for proposing changes to the project. The goal is to keep contributions focused, reviewable, secure, and consistent with the existing architecture and SAF-T (PT) domain.

## Before You Start

For significant features, architectural changes, database changes, changes to SAF-T parsing or persistence, or changes that affect deployment, security, or AI integrations, please open an issue or start a discussion before investing substantial development effort.

Small bug fixes, documentation improvements, tests, and clearly scoped maintenance changes can normally be submitted directly as a pull request.

## Development Workflow

1. Fork the repository or create a dedicated branch if you have write access.
2. Create your branch from the latest `main` branch.
3. Keep the branch focused on a single change or closely related set of changes.
4. Implement and test the change locally.
5. Push the branch and open a pull request against `main`.

Use descriptive branch names, for example:

```text
feature/saft-payments
fix/stock-movement-import
docs/update-deployment-guide
refactor/accounting-analysis
```

Do not commit directly to `main` for normal contribution work.

## Pull Request Rules

Pull requests should be small enough to review effectively and should have a clear purpose.

A pull request should:

- explain what was changed and why;
- describe how the change was tested;
- reference a related issue when applicable;
- avoid unrelated formatting, refactoring, or dependency changes;
- update documentation when behaviour, configuration, deployment, or public interfaces change;
- include tests when the change introduces behaviour that can reasonably be tested;
- keep existing tests passing;
- compile successfully before being submitted for review;
- preserve the existing user-facing language conventions (Portuguese from Portugal);
- use English for code identifiers, technical documentation, and commit messages.

Large changes may be requested to be split into smaller pull requests.

## Code and Architecture

Follow the existing project structure, naming conventions, architectural patterns, and repository development skills under `.agents/skills/`.

Avoid introducing new frameworks, major dependencies, architectural layers, or infrastructure components unless they provide a clear benefit and have been discussed beforehand.

Do not combine a large refactoring with a new feature unless the refactoring is necessary to implement that feature.

Public C# classes, interfaces, records, enums, methods, and functions should include appropriate English XML documentation in accordance with the repository coding rules.

## SAF-T (PT) Changes

Changes involving SAF-T parsing, persistence, navigation, or analysis must preserve source-file traceability. Data from multiple imports must not be silently merged.

When adding support for a SAF-T area:

- preserve the selected import/source context;
- keep parsed and persisted data associated with its source import;
- use deterministic calculations for accounting views unless the feature is explicitly AI-assisted;
- avoid inventing or inferring accounting data that is not supported by the source file;
- add or update tests for parsing and accounting behaviour where practical;
- document compatibility or re-import requirements when existing imports are affected.

## Database Changes

Database changes must be explicit and reviewable.

When a contribution changes the database schema or persistence behaviour:

- include the required Entity Framework migration and update the model snapshot;
- explain the impact in the pull request;
- consider both SQLite development databases and SQL Server deployments;
- avoid destructive migrations unless clearly justified;
- preserve existing data whenever reasonably possible;
- document any manual deployment, migration, or re-import step that may be required.

## Configuration

Never commit secrets or environment-specific credentials.

This includes, but is not limited to:

- passwords;
- API keys;
- OAuth client secrets;
- access tokens;
- private keys;
- production connection strings;
- confidential accounting or business information.

If a new environment variable or configuration value is required, update `.env.example` and the relevant documentation using safe placeholder values.

## Privacy and Accounting Data

Tabularius AI processes accounting and SAF-T data. Contributions must not contain real confidential SAF-T files, taxpayer information, customer or supplier personal data, authentication data, or other private business information.

Use synthetic or properly anonymised data in tests, examples, screenshots, fixtures, and documentation.

## AI and LLM Integrations

Changes involving AI providers, prompts, tool calling, MCP, or LLM integrations should:

- keep provider-specific configuration externalised where practical;
- avoid hard-coding API credentials;
- document new configuration requirements;
- handle model output as untrusted input where appropriate;
- keep deterministic accounting calculations separate from generative AI output;
- avoid sending accounting, SAF-T, personal, or confidential business data to external services without an explicit and documented reason.

## Dependencies

New dependencies should be necessary, actively maintained, and appropriate for the project.

Avoid adding a dependency when the same result can reasonably be achieved using the existing stack or platform libraries.

Dependency upgrades should preferably be submitted separately from unrelated feature changes.

## Commit Messages

Use concise commit messages that describe the change clearly.

Examples:

```text
feat: add SAF-T payment documents
fix: preserve selected import in sales documents
docs: update contribution guidelines
test: add stock movement parser tests
refactor: simplify trial balance calculation
```

## Review, CI, and Merge

Submitting a pull request does not guarantee that it will be merged.

A pull request may require changes before approval. Review may consider correctness, accounting integrity, security, maintainability, architecture, test coverage, documentation, and consistency with the direction of the project.

Pull requests should only be merged after review and after required automated checks have passed. A failing CI run must be investigated and corrected before merge.

## License

By submitting a contribution, you agree that your contribution will be licensed under the same license as the project.
