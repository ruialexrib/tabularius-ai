---
name: tabularius-ui
description: Implement Tabularius AI Razor and CSS interfaces using the established professional visual direction and consistent analytical UI patterns.
---

# Tabularius AI UI

Use this skill for Razor views, layouts, navigation, dashboards, tables, filters, forms and responsive behavior.

## Visual identity

Tabularius AI may reuse proven interaction patterns from Denarius AI, but has its own visual identity. The primary application accent is purple/violet, not green.

Preserve these established characteristics:
- Application shell with dark purple top bar/sidebar and light content workspace.
- Primary palette: dark purple `#241a35`, violet `#6d28d9`, focus violet `#7c3aed`, light violet `#a78bfa` / `#c4b5fd`, ink `#172033`, muted `#687386`, light borders and neutral backgrounds.
- Green is reserved for semantic positive/success states. Never use green as a generic interaction, navigation, focus, action, form or identity color.
- White panels with subtle borders, 14-18 px radii and restrained shadows.
- Compact analytical headers, controls with violet focus rings, compact tables and semantic status states.
- Keep the interface visually quiet and appropriate to professional accounting software.
- The login page uses a purple gradient only on the left brand panel; the right form area remains neutral/light.
- Sidebar navigation options include compact icons aligned consistently with their labels.

## Standard list workspace

All current and future list pages must use the shared list language implemented by `lists.css`, `table-actions.css` and the paginated list view models.

- Lists occupy the full useful width between the sidebar and the content margin. Never constrain operational lists with an arbitrary `max-width`.
- Place a filter toolbar at the top of the list surface. Free-text search is the baseline; add domain-specific filters when useful.
- Filters are server-side for persisted data. Do not load an unbounded collection merely to filter it in the browser.
- Every persisted-data list uses server-side pagination by default. The standard page sizes are 10, 25, 50 and 100, with 25 as the default.
- Preserve active filters and page size while moving between pages. Reset to a valid page when filtering reduces the result count.
- Show total matching records and the visible range (`first-last of total`).
- Provide clear Previous/Next navigation and current page/total page context. Disabled pagination controls must be visibly disabled and non-interactive.
- Provide a clear-filter action when filters are active.
- Empty filtered results must say that no records match the selected filters; a genuinely empty dataset may use a richer onboarding empty state.
- SAF-T source-dependent lists place the shared source selector above the list filter toolbar and preserve `importId` across filtering and pagination.
- Table headers remain compact and muted; rows use subtle separators and hover states. Horizontal scrolling is allowed only when real column requirements exceed available width.
- Row actions must share the same visual pattern across all lists. Destructive actions remain semantically red, but use the same sizing, spacing and action structure as other row actions.
- `Ver linhas`, pagination, totals, user-menu interactions and generic list actions use the violet accent rather than green.

## Forms and detail pages

- All forms and detail/edit surfaces use the purple/violet accent for icons, focus states, helper panels, badges and interactive borders.
- Before completing a form-related change, check for page-specific CSS containing legacy hardcoded green values and replace identity greens with violet equivalents.
- Green may remain only where the UI explicitly communicates success, positive state or positive accounting meaning.
- Forms use the full useful content width unless the task genuinely benefits from a constrained reading width.
- Avoid large unused blank areas beside forms on desktop displays.

## SAF-T source selection

- Every page whose values can differ between SAF-T (PT) imports in the same dossier must expose the shared SAF-T source selector.
- Reuse the shared selector partial/component; always show filename, accounting period and SAF-T version.
- Default to the latest accounting period ordered by `EndDate`, `StartDate`, import timestamp and identifier.
- Preserve selected `importId` between source-dependent pages and never silently combine multiple SAF-T sources.

## Interaction

- User-facing text is European Portuguese unless explicitly English.
- File-upload workflows clearly show selected file, processing state, failures and success.
- Destructive actions require clear wording and proportional confirmation.
- Validation/import errors identify the actionable problem without exposing unnecessary technical internals.
- AI-generated insights are visually distinguishable from deterministic metrics.

## Verification

- Render and inspect affected pages when a rendering environment is available.
- Verify empty, populated, filtering, pagination, focus and responsive states where applicable.
- For visual changes, search all component-specific CSS for legacy green values instead of assuming theme variables cover every component.
- Verify desktop lists use available width and preserve query state across pagination.
- Run relevant automated tests and confirm repository CI for the completed change set. Do not block every non-critical intermediate UI commit waiting for CI; stop immediately when a critical change or an already-failing CI requires investigation.
