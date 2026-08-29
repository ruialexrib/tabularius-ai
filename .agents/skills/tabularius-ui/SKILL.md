---
name: tabularius-ui
description: Implement Tabularius AI Razor and CSS interfaces using the established Denarius-inspired visual direction and consistent analytical UI patterns.
---

# Tabularius AI UI

Use this skill for Razor views, layouts, navigation, dashboards, tables, filters, forms and responsive behavior.

## Design direction

- Use Denarius AI as the visual reference while keeping Tabularius AI a distinct SAF-T analytics product.
- Prefer a clean analytical dashboard language with dark navy structure, restrained emerald accents, subtle borders and low-noise shadows.
- Reuse shared CSS, layouts, partials and components before adding page-specific styles.
- Keep typography, spacing, control height, border radius, hover, focus, disabled and loading states consistent.
- Prioritise readability for dense accounting and analytical information.

## Interaction

- User-facing text is European Portuguese unless a surface is explicitly English.
- Tables must remain readable with large datasets and expose clear filtering/sorting states when those capabilities exist.
- Destructive actions require clear wording and proportional confirmation.
- Validation and import errors should identify the actionable problem without exposing technical internals unnecessarily.
- AI-generated insights must be visually distinguishable from deterministic application metrics.

## Verification

Render the affected page and inspect the changed state rather than relying only on compilation. Compare related controls for consistency and verify responsive behavior when applicable.
