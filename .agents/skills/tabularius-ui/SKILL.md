---
name: tabularius-ui
description: Implement Tabularius AI Razor and CSS interfaces using the established Denarius-inspired visual direction and consistent analytical UI patterns.
---

# Tabularius AI UI

Use this skill for Razor views, layouts, navigation, dashboards, tables, filters, forms and responsive behavior.

## Denarius visual reference

Tabularius AI must remain visually consistent with Denarius AI while keeping its own SAF-T (PT) accounting-analysis identity. When visual details are uncertain, inspect the current Denarius AI UI and shared CSS before inventing a new pattern.

Preserve these established Denarius characteristics:

- Application shell with a dark navy top bar and sidebar, a light content workspace and a restrained footer/context bar.
- Core palette: navy `#111b2e`, secondary navy `#19263e`, emerald `#34d399`, primary action green `#159a70`, green text `#087f5b`, ink `#172033`, muted text `#687386`, borders `#e5e9f0`, and page background `#f5f7fb`.
- Header treatment based on the Denarius dark gradient `linear-gradient(110deg,#101b2f 0%,#12243a 58%,#102c38 100%)`, subtle light borders and low-noise shadows.
- White panels/cards with subtle borders, approximately 14-16 px radii and restrained shadows rather than heavy elevation.
- Compact analytical page headers: small uppercase emerald eyebrow, clear page title, concise supporting text and optional status/context pill.
- Forms and filters follow the Denarius control pattern: clear compact labels, 8-9 px rounded controls, neutral `#cfd6e2` borders, white backgrounds, consistent heights and emerald focus rings.
- Primary actions use `#159a70`; secondary actions use neutral light surfaces. Destructive actions use soft red semantics.
- Tables use compact uppercase muted headers, comfortable accounting-data rows, subtle separators, clear hover states and right-aligned action areas where appropriate.
- Status pills use soft semantic backgrounds. Green indicates valid/positive states, red destructive/error states, and amber warning/transfer states, never relying on colour alone.
- Keep the interface visually quiet: no unnecessary gradients, oversized decoration, excessive shadows, or competing accent colours.

## SAF-T source selection

- Every page whose values can differ between SAF-T (PT) imports in the same dossier must expose the shared SAF-T source selector. This includes the SAF-T summary and all source-specific master data, transactions, documents, taxes and future source-dependent analyses.
- Do not create page-specific source selectors. Reuse the shared selector partial/component so appearance and behavior remain consistent.
- The selector follows the Denarius filter-bar visual language: compact label, neutral bordered select, emerald focus state, restrained filter surface and professional information density.
- Always show source traceability next to the selector: original filename, accounting period and SAF-T version.
- If no source is explicitly selected, default to the latest accounting period, ordered by `EndDate`, then `StartDate`, then import timestamp and identifier. Do not define “current” merely as the last file imported.
- Preserve the selected `importId` when navigating between SAF-T source-dependent pages.
- Never silently combine values from multiple SAF-T files. A consolidated dossier view must be explicitly identified and implemented with deterministic accounting rules.
- Pages that are genuinely dossier-level and independent of a specific SAF-T source do not require this selector.

## Workspace and available width

- Forms, editing screens, lists, tables, filters, analytical grids and other working surfaces must use the full useful width of the content area by default.
- Do not impose arbitrary `max-width` constraints on operational screens merely for aesthetics.
- The main content area may have normal outer page padding, but the form/list/panel inside it should normally expand to the available width.
- Use narrow panels only when the task genuinely benefits from a constrained reading width, such as authentication, a short confirmation or a small single-purpose dialog-like form.
- Data tables should be allowed to use the full workspace and use horizontal scrolling only when their real column requirements exceed the available width.
- Multi-field forms should exploit horizontal space with responsive grids/columns where this improves scanning and editing, collapsing naturally on narrower screens.
- Avoid large unused blank areas beside forms or lists on desktop displays.

## Design direction

- Prefer a clean analytical dashboard language with dark navy structure, restrained emerald accents, subtle borders and low-noise shadows.
- Reuse shared CSS, layouts, partials and components before adding page-specific styles.
- Keep typography, spacing, control height, border radius, hover, focus, disabled and loading states consistent.
- Prioritise readability for dense accounting and analytical information.
- Use semantic visual cues without relying on color alone.
- Preserve information density appropriate to professional accounting software; modern does not mean oversized.

## Interaction

- User-facing text is European Portuguese unless a surface is explicitly English.
- Tables must remain readable with large datasets and expose clear filtering/sorting states when those capabilities exist.
- File-upload workflows must clearly show accepted formats, validation failures, processing state and successful import state.
- Destructive actions require clear wording and proportional confirmation.
- Validation and import errors should identify the actionable problem without exposing technical internals unnecessarily.
- AI-generated insights must be visually distinguishable from deterministic application metrics.
- AI suggestions should expose uncertainty when relevant and must not replace deterministic accounting calculations.

## Verification

- Render the affected page and inspect the changed state rather than relying only on compilation whenever a rendering environment is available.
- Compare related controls for consistency and verify empty, populated, loading, validation, error, focus and responsive states when applicable.
- Verify specifically that desktop forms and lists make effective use of the available content width and do not leave avoidable dead space.
- Run the relevant automated tests and confirm repository CI for the completed change set. Do not block every non-critical intermediate UI commit waiting for CI; stop immediately when a critical change or an already-failing CI requires investigation.
