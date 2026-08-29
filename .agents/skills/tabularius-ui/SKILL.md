---
name: tabularius-ui
description: Implement Tabularius AI Razor and CSS interfaces using the established Denarius-inspired visual direction and consistent analytical UI patterns.
---

# Tabularius AI UI

Use this skill for Razor views, layouts, navigation, dashboards, tables, filters, forms and responsive behavior.

## Denarius visual reference

Tabularius AI must remain visually consistent with Denarius AI while keeping its own SAF-T (PT) accounting-analysis identity. When visual details are uncertain, inspect the current Denarius AI UI and shared CSS before inventing a new pattern.

Preserve these established Denarius characteristics:
- Application shell with dark navy top bar/sidebar and light content workspace.
- Core palette: navy `#111b2e`, secondary navy `#19263e`, emerald `#34d399`, primary green `#159a70`, green text `#087f5b`, ink `#172033`, muted `#687386`, borders `#e5e9f0`, background `#f5f7fb`.
- White panels with subtle borders, 14-16 px radii and restrained shadows.
- Compact analytical headers, controls with emerald focus rings, compact tables and semantic status states.
- Keep the interface visually quiet and appropriate to professional accounting software.

## Standard list workspace

All current and future list pages must use the shared Denarius-inspired list language implemented by `lists.css` and the paginated list view models.

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
- New list areas such as Products, Movements, Documents and Taxes must adopt this standard rather than inventing page-specific list structures.

## SAF-T source selection

- Every page whose values can differ between SAF-T (PT) imports in the same dossier must expose the shared SAF-T source selector.
- Reuse the shared selector partial/component; always show filename, accounting period and SAF-T version.
- Default to the latest accounting period ordered by `EndDate`, `StartDate`, import timestamp and identifier.
- Preserve selected `importId` between source-dependent pages and never silently combine multiple SAF-T sources.

## Workspace and available width

- Forms, editing screens, lists, tables, filters and analytical grids use the full useful content width by default.
- Do not impose arbitrary `max-width` constraints on operational screens.
- Narrow panels are reserved for tasks that genuinely benefit from constrained reading width.
- Avoid large unused blank areas beside forms or lists on desktop displays.

## Interaction

- User-facing text is European Portuguese unless explicitly English.
- File-upload workflows clearly show selected file, processing state, failures and success.
- Destructive actions require clear wording and proportional confirmation.
- Validation/import errors identify the actionable problem without exposing unnecessary technical internals.
- AI-generated insights are visually distinguishable from deterministic metrics.

## Verification

- Render and inspect affected pages when a rendering environment is available.
- Verify empty, populated, filtering, pagination, focus and responsive states where applicable.
- Verify desktop lists use available width and preserve query state across pagination.
- Run relevant automated tests and confirm repository CI for the completed change set. Do not block every non-critical intermediate UI commit waiting for CI; stop immediately when a critical change or an already-failing CI requires investigation.
