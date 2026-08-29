---
name: tabularius-git
description: Manage Tabularius AI branches, commits and pull requests while keeping main stable and changes reviewable.
---

# Tabularius AI Git workflow

Use this skill whenever creating branches, commits or pull requests.

## Branches and pull requests

- Keep `main` stable and develop changes on focused branches.
- Use descriptive branch names such as `feat/saft-import`, `feat/localdb`, `style/dashboard` or `fix/import-validation`.
- Keep each pull request focused on one coherent change set.
- Do not merge a pull request unless the user explicitly asks for or approves the merge.
- Prefer squash merge after review so feature history remains concise.

## Commit messages

Write commit messages in English using Conventional Commits:

```text
type(optional-scope): concise imperative summary
```

Use `feat`, `fix`, `style`, `refactor`, `test`, `docs`, `build`, `ci` or `chore` as appropriate.

- Use imperative wording, lowercase after the colon and no final period.
- Describe the outcome rather than implementation trivia.
- Do not mention AI authorship, Codex or generated-by attribution.
- Never commit secrets, API keys, real SAF-T files, LocalDB data files or sensitive accounting data.

## Before a pull request

- Review the actual changed files and diff.
- Verify that C# changes comply with `tabularius-coding`, including English XML documentation on classes and methods.
- Run the appropriate build and tests when available.
- State clearly what was verified and what remains unverified.
