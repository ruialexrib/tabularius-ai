---
name: tabularius-git
description: Manage Tabularius AI branches, commits and pull requests while enforcing the repository quality gate.
---

# Tabularius AI Git workflow

Use this skill whenever creating branches, commits or pull requests. Quality and CI behavior is defined centrally by `tabularius-quality`; do not duplicate its detailed test policy here.

## Branches and pull requests

- Keep `main` stable and normally develop changes on focused branches.
- Use descriptive branch names such as `feat/saft-import`, `feat/localdb`, `style/dashboard` or `fix/import-validation`.
- Keep each pull request focused on one coherent change set.
- Do not merge a pull request unless the user explicitly asks for or approves the merge.
- Prefer squash merge after review so feature history remains concise.
- When the user has explicitly requested direct development on `main`, follow that instruction until revoked, but the mandatory CI gate still applies to every pushed commit.

## Commit messages

Write commit messages in English using Conventional Commits:

```text
type(optional-scope): concise imperative summary
```

Use `feat`, `fix`, `style`, `refactor`, `test`, `docs`, `build`, `ci` or `chore` as appropriate.

- Use imperative wording, lowercase after the colon and no final period.
- Describe the outcome rather than implementation trivia.
- Do not mention AI authorship, Codex or generated-by attribution.
- Never commit secrets, API keys, real SAF-T files, LocalDB data files, database backups or sensitive accounting data.
- A request for a commit message does not authorize staging, committing, pushing, tagging or publishing.
- Never invent issue or pull request references.

## After every commit

Apply `tabularius-quality`. Inspect the GitHub Actions CI run corresponding to the latest relevant commit. A successful GitHub write is not evidence that the code builds or tests pass.

If CI fails, investigate the failed step and logs, correct the root cause, commit the correction and inspect the replacement CI run before continuing normal feature development. If a run was cancelled because a newer commit superseded it, verify the latest final commit instead.

## Before a pull request or merge

- Review the actual changed files and diff.
- Preserve unrelated changes and separate independent work when appropriate.
- Verify C# documentation and architecture rules through `tabularius-coding`.
- Apply the full `tabularius-quality` gate and require a passing CI result.
- State clearly what was verified and what remains unverified.
