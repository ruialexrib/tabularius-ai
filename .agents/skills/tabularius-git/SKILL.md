---
name: tabularius-git
description: Inspect TabulariusAI changes, propose consistent Git commit messages, separate unrelated work, and commit only when explicitly requested.
---

# TabulariusAI Git workflow

Use this skill when preparing commits, reviewing repository changes, or proposing commit messages.

## Commit scope

- Inspect repository status and relevant diffs before proposing or creating a commit.
- Base commit messages only on changes that will actually be committed.
- Preserve unrelated changes and separate independent work into different commits where appropriate.
- Never commit `.env`, credentials, API keys, uploaded SAF-T files containing real accounting data, database backups, or other sensitive material.
- A request for a commit message does not authorize staging, committing, pushing, tagging, or publishing.

## Commit messages

Write commit messages in English using Conventional Commits:

```text
type(optional-scope): concise imperative summary
```

Use the narrowest accurate type: `feat`, `fix`, `style`, `refactor`, `test`, `docs`, `build`, `ci`, or `chore`.

Useful TabulariusAI scopes may include `saft`, `analytics`, `ai`, `ui`, `docker`, `security`, or `release`.

- Use imperative mood and lowercase after the colon.
- Do not end the subject with a period.
- Normally keep the subject at 72 characters or fewer.
- Describe the outcome rather than file-level implementation details.
- Do not mention AI authorship, Codex, or generated-by attribution.
- Never invent issue or PR references.

## Before committing

1. Confirm the intended files and changes.
2. Check that no sensitive files or data are included.
3. Run verification appropriate to the change or clearly report what was not run.
4. Commit without rewriting existing history unless explicitly requested.
5. Report the resulting commit hash and subject.
6. Do not push unless explicitly requested.
