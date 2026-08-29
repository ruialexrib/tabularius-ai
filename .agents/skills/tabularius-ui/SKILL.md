---
name: tabularius-ui
description: Implement and review TabulariusAI Razor views, forms, dashboards, tables, navigation, feedback, and responsive layouts consistently.
---

# TabulariusAI UI

Use this skill for visual or interaction changes in the TabulariusAI web application.

## Design direction

- Maintain a clean professional analytics interface suitable for accounting and SAF-T analysis.
- Prefer shared CSS and reusable components over page-specific duplication.
- Keep typography, spacing, control height, borders, focus, hover, disabled, loading, empty, and validation states consistent.
- Prioritize readability of dense accounting and analytical information.
- Use semantic visual cues without relying on colour alone.
- Keep dashboards information-dense but visually restrained; avoid decorative elements that compete with the data.

## Interaction rules

- File-upload workflows must clearly show accepted formats, validation failures, processing state, and successful import state.
- Uploaded SAF-T data must not be silently modified or inferred when parsing fails.
- Destructive or irreversible actions require explicit confirmation proportional to their impact.
- AI-generated interpretations must be distinguishable from deterministic values extracted or calculated from SAF-T data.
- AI suggestions should expose uncertainty when relevant and must not replace deterministic accounting calculations.
- Tables and dashboards should remain usable on narrower screens where practical.

## Verification

- Render and inspect the affected page and the state changed by the implementation, not only the initial state.
- Verify validation, empty, populated, loading, error, focus, and responsive states when applicable.
- Run the relevant tests and a Release build after changes.
