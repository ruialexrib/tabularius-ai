---
name: tabularius-visual-identity
description: Define the reusable Tabularius AI visual design system and adaptation rules for applying its modern professional interface language to Tabularius AI and new projects without copying project-specific branding.
---

# Tabularius Visual Identity

Use this skill when designing, reviewing or adapting application shells, dashboards, analytical pages, forms, lists, detail pages, authentication surfaces, navigation, cards, tabs, tables, filters, buttons, alerts, empty states and responsive layouts.

This skill separates the reusable design language from Tabularius-specific branding. New projects may inherit the layout, proportions, hierarchy, component language and interaction principles while replacing brand colours, logo, product name and domain-specific semantics.

## 1. Design character

The interface must feel:

- modern but restrained;
- professional and suitable for business, accounting and data-intensive software;
- visually quiet rather than decorative;
- dense enough for operational work without feeling cramped;
- consistent across equivalent workflows;
- structured around clear hierarchy, alignment and whitespace;
- trustworthy, deterministic and precise;
- desktop-efficient while remaining responsive.

Avoid fashionable effects that reduce clarity. The design should age well and should not resemble a marketing landing page inside operational screens.

## 2. Core composition

Use a stable application shell with:

- a dark brand header/top bar;
- a dark sidebar for primary navigation;
- a light neutral workspace for operational content;
- a discreet context/footer bar when persistent context is useful;
- full-width content inside sensible page margins.

For Tabularius AI, the shell is dark purple and the accent is violet. In another project, preserve the tonal relationship but map the accent to that project's identity.

Do not arbitrarily constrain data-heavy pages with narrow `max-width` containers. Lists, forms and analytical surfaces should normally use the useful width available between navigation and page margins.

## 3. Brand token architecture

Never scatter identity colours through component CSS when a semantic token can be used. A reusable implementation should expose at least:

```css
:root {
  --brand-shell: #241a35;
  --brand-accent: #6d28d9;
  --brand-focus: #7c3aed;
  --brand-soft: #a78bfa;
  --brand-softest: #c4b5fd;
  --ink: #172033;
  --muted: #687386;
  --surface: #ffffff;
  --background: #f5f7fb;
  --line: #e5e9f0;
  --line-strong: #cfd6e2;
  --danger: #b42318;
  --success: #159a70;
}
```

These are reference values for Tabularius AI. A new project should replace the `--brand-*` tokens while normally retaining the neutral and semantic structure.

Identity colour and semantic colour are different concepts:

- brand/accent colour: navigation, active states, primary actions, links, focus, selected tabs, interactive borders and identity decoration;
- green: success, positive status or explicitly positive domain meaning;
- red: destructive action, error, invalid state or explicitly negative domain meaning;
- amber/orange: warning or attention when required;
- neutral grey: secondary information, disabled states and supporting text.

Never use success green merely because it looks attractive. Never use the brand accent to disguise errors or destructive actions.

## 4. Typography

Use a clean sans-serif UI stack, preferably `Inter`, with system fallbacks.

Recommended hierarchy:

- page title: strong weight, compact line-height, high contrast;
- section title: approximately 18-24 px depending on context;
- body/lead: 14-18 px with comfortable line-height;
- operational table/form text: approximately 12-14 px;
- eyebrow/kicker/metadata: approximately 10-11 px, bold, optionally uppercase with increased tracking;
- muted supporting text: smaller and lower contrast, but always readable.

Use font weight and spacing before introducing extra colours. Avoid excessive heading sizes on operational pages.

## 5. Spacing system

Prefer a consistent spacing rhythm based on roughly 4 px increments. Typical values are 4, 8, 10, 12, 14, 16, 18, 20, 24, 28, 32 and 38 px.

Key rules:

- page content should have generous outer padding on desktop;
- related controls stay visually grouped;
- unrelated groups require visible separation;
- cards inside tabbed/analytical areas must not touch the navigation tabs;
- forms should not contain large accidental blank columns;
- button groups use consistent gaps rather than margins added individually;
- vertical rhythm must remain consistent between equivalent pages.

When one page already implements the intended spacing pattern correctly, reuse it instead of inventing a local variation.

## 6. Surfaces and cards

Primary surfaces use:

- white or near-white backgrounds;
- subtle neutral borders;
- rounded corners, normally 14-18 px for major surfaces and 8-12 px for controls/smaller cards;
- restrained, diffuse shadows;
- sufficient internal padding, commonly 18-28 px.

Cards should communicate grouping, not decoration. Avoid heavy shadows, thick borders, coloured bottom strips, excessive gradients or multiple competing accent lines.

Use gradients sparingly. They are appropriate for controlled brand surfaces such as the authentication brand panel, not as a default treatment for every card.

## 7. Page headers

Operational pages should begin with a compact, predictable header containing:

- optional eyebrow/context label;
- clear page title;
- concise supporting description only when useful;
- page-level actions aligned consistently, normally to the right on desktop.

Do not repeat information already obvious from navigation. Keep action placement stable between similar pages.

## 8. Navigation shell

Sidebar navigation must be compact and scan-friendly.

- Use small, consistently aligned icons.
- Keep icon boxes and label baselines aligned.
- Use section headings sparingly.
- Active navigation uses the project accent and/or a tinted accent surface.
- Hover states are subtle and must not cause layout movement.
- Keep labels concise.
- Do not use unrelated semantic colours for navigation.

The top bar should prioritise brand recognition and global context rather than becoming a second crowded navigation bar.

## 9. Tabs and analytical navigation

Tabbed analytical areas are a signature pattern.

- Tabs must form a coherent navigation band.
- Active state uses the brand accent clearly but without oversized decoration.
- Cards/content below tabs require deliberate vertical separation.
- Equivalent analytical pages must use the same tab height, spacing, typography and active-state treatment.
- Do not create page-specific tab systems unless the interaction is genuinely different.
- Preserve selected source/context when moving between related analytical tabs.

## 10. Dashboard and metric cards

Metric cards should optimise comparison.

- Use a regular grid.
- Keep labels muted and values visually dominant.
- Align values consistently.
- Use accent colour selectively; semantic colours only when the metric meaning justifies them.
- Avoid icons unless they improve scanning or classification.
- Do not place every KPI in a different colour.
- Keep card heights consistent within the same grid.

A dashboard should communicate hierarchy: primary metrics first, secondary detail afterwards.

## 11. Forms

Creation and editing forms should look like intentional work surfaces, not raw Bootstrap scaffolding.

Use:

- a clear page or card header;
- logical sections for related fields;
- labels above controls;
- concise helper text below labels or controls where necessary;
- consistent input heights, normally around 42-46 px;
- neutral borders at rest;
- brand-coloured border and soft focus ring on focus;
- clear validation states;
- a dedicated, consistently aligned action area.

Form layout rules:

- use available width intelligently;
- group short related fields in columns where this improves scanning;
- let long text/select/file fields use more width;
- avoid a narrow form floating beside a large empty desktop area without reason;
- collapse columns cleanly on smaller screens;
- maintain identical visual language between create and edit versions of the same entity.

Primary submit actions use the brand accent. Secondary actions are neutral outlined/white. Destructive actions are red and visually separated when practical.

## 12. Buttons and actions

Buttons share a common geometry across the product:

- consistent height;
- medium radius, generally 8-10 px;
- compact horizontal padding;
- strong but not oversized label weight;
- icon and label alignment when icons are present.

Use hierarchy:

- primary: filled brand accent;
- secondary: neutral surface with border;
- tertiary/link: low-emphasis text action;
- destructive: red treatment;
- disabled: visibly unavailable and non-interactive.

Row actions such as `Ver linhas`, edit and delete must use the same sizing and spacing pattern across all tables.

## 13. Tables and operational lists

Tables are compact, neutral and information-first.

- Use full available width.
- Header background is a very light neutral.
- Header labels are small, muted, bold and may use uppercase tracking.
- Rows use subtle separators.
- Hover is a very light neutral/accent tint.
- Numeric and monetary columns align consistently, normally right-aligned.
- Actions remain compact and do not dominate the row.
- Horizontal scrolling is acceptable only when column requirements genuinely exceed the viewport.

Persisted-data lists should normally include a filter toolbar and server-side pagination.

Standard pagination expectations:

- page sizes 10, 25, 50 and 100;
- 25 as default;
- visible range and total count;
- previous/next controls;
- current page/total pages;
- active filters retained during navigation;
- clear-filter action when relevant;
- clearly disabled controls when unavailable.

## 14. Filter bars and selectors

Filters should appear as part of the list surface, not as disconnected forms.

- Align labels and controls consistently.
- Keep common filters on one row where space permits.
- Allow wrapping responsively.
- Use the brand focus state.
- Keep search as the baseline filter for generic lists.
- Add domain filters only when they materially improve navigation.
- Use clear reset/clear actions.

Context selectors that affect all values on a page belong above page-specific filters. Their current selection must be obvious.

## 15. Status, alerts and toasts

Status styling is semantic and restrained.

- success: green;
- warning: amber/orange;
- error/destructive: red;
- informational: brand accent or neutral blue only if the project's palette defines it.

Alerts and toasts should share border radius, typography, padding and icon alignment with the rest of the system. Do not introduce a separate visual library appearance.

Success messages may auto-dismiss when appropriate; errors requiring action should remain visible long enough to be understood.

## 16. Empty states

Distinguish two cases:

1. Dataset genuinely empty: a richer empty state may explain what to do next and provide a primary action.
2. Filters return no matches: use a compact message explaining that no records match the selected filters and offer to clear them.

Do not show onboarding illustrations or large empty-state cards for routine filtered-zero results.

## 17. Authentication surfaces

Authentication may be more brand-forward than operational screens while remaining restrained.

For Tabularius AI:

- split layout;
- purple gradient on the left brand panel;
- neutral/light form panel on the right;
- strong logo/product name/slogan hierarchy;
- form controls use the same focus and validation language as the application.

For a new project, preserve the split-layout logic when appropriate but replace colours, logo, product name, slogan and brand imagery.

## 18. Icons

Icons are functional, compact and secondary to labels.

- Use one icon family consistently.
- Match stroke/fill style across the application.
- Keep navigation and button icons aligned to a common size.
- Do not mix emoji with interface iconography.
- Avoid decorative icons in dense tables unless they encode useful state.

## 19. Motion and interaction feedback

Motion should be minimal:

- short hover/focus transitions;
- at most a subtle 1 px lift on interactive cards where appropriate;
- no large transforms, bouncing or decorative animation in operational views;
- loading states should indicate real work without blocking unrelated interaction unnecessarily.

Respect reduced-motion preferences where custom animation exists.

## 20. Responsive behaviour

Desktop efficiency is important, but responsive layouts must degrade predictably.

- Multi-column card grids reduce columns progressively.
- Form columns stack without losing logical order.
- Toolbars wrap while preserving action grouping.
- Tables may scroll horizontally when necessary.
- Sidebar/navigation should have an intentional smaller-screen strategy.
- Never solve responsiveness by shrinking text below comfortable reading sizes.

Verify at representative desktop, tablet and narrow/mobile widths when the surface is expected to support them.

## 21. Accessibility

Visual consistency must not reduce accessibility.

- Maintain sufficient text/background contrast.
- Never encode state by colour alone.
- Provide visible keyboard focus.
- Use semantic HTML and associated form labels.
- Ensure buttons and links have adequate hit areas.
- Disabled state must be both visual and functional.
- Preserve logical keyboard order when layouts reflow.
- Use meaningful accessible names for icon-only actions.

## 22. AI-specific presentation

AI output must be visually distinguishable from deterministic application results without appearing untrustworthy or experimental.

- Deterministic KPIs and accounting values retain the standard analytical styling.
- AI interpretation may use a dedicated report/chat surface with a subtle brand treatment.
- Clearly label AI-generated interpretation when context requires it.
- Do not use AI styling to imply that deterministic figures were generated by a model.
- Markdown output should inherit application typography, table and callout styles rather than browser defaults.

## 23. Domain semantics

When adapting this identity to another domain, preserve visual grammar but replace domain semantics.

Do not copy:

- Tabularius product name;
- Tabularius logo;
- accounting-specific labels;
- SAF-T-specific selectors;
- violet colour values when the target project has another established identity.

Do copy/adapt:

- shell proportions;
- hierarchy;
- spacing rhythm;
- card geometry;
- form language;
- list/filter/pagination patterns;
- button hierarchy;
- restrained shadow/border treatment;
- semantic separation of identity, success, warning and danger colours;
- responsive and accessibility rules.

## 24. Reuse-first rule

Before creating a new component or visual pattern:

1. Find an existing page/component implementing the same interaction.
2. Reuse its shared partial, CSS class, layout or component where practical.
3. If the pattern is duplicated in multiple places, prefer extracting or extending a shared implementation.
4. Only introduce a new pattern when existing ones do not satisfy the interaction.
5. When changing a transversal pattern, inspect other pages using it for regressions and consistency.

A local CSS override should not become the default solution for a systemic inconsistency.

## 25. Anti-patterns

Avoid:

- arbitrary page-specific colours;
- green identity controls in Tabularius AI;
- inconsistent radii between equivalent cards;
- heavy box shadows;
- excessive gradients;
- coloured borders used decoratively on every surface;
- giant titles on data-entry pages;
- narrow operational content with unused desktop space;
- raw framework-default forms beside custom modern forms;
- different button geometry per page;
- different tab systems for equivalent analytical areas;
- browser-only filtering of unbounded persisted datasets;
- destructive actions styled as normal primary actions;
- success/error communicated by colour alone;
- duplicated component CSS that drifts over time.

## 26. Visual review checklist

Before completing a visual change, verify:

- Does it use the target project's identity tokens rather than copied brand colours?
- Is the page hierarchy consistent with equivalent pages?
- Are outer margins and vertical spacing intentional?
- Are cards separated from tabs and neighbouring surfaces correctly?
- Are radii, borders and shadows consistent?
- Are primary, secondary and destructive actions visually distinct?
- Do inputs have correct focus and validation states?
- Does the layout use available desktop width sensibly?
- Are tables, filters and pagination consistent with shared patterns?
- Are empty and error states appropriate to the situation?
- Are semantic colours used only for semantic meaning?
- Is keyboard focus visible?
- Does the layout remain usable at smaller widths?
- Have shared/transversal components been checked on other pages?

## 27. Adaptation procedure for a new project

When asked to apply the Tabularius visual language to a new application:

1. Inspect the target project's current visual identity, logo, palette, typography and existing shared components.
2. Identify its brand accent and semantic colours.
3. Map the target identity to semantic design tokens; do not paste Tabularius violet values blindly.
4. Inventory the target application's layouts, forms, lists, tables, tabs, cards, buttons, alerts and authentication pages.
5. Choose one representative existing target page for each major pattern.
6. Apply shared component/style changes first so multiple pages improve together.
7. Modernise page-specific markup only where shared changes are insufficient.
8. Preserve target-project terminology and domain semantics.
9. Compare equivalent create/edit/detail/list pages for consistency.
10. Inspect rendered results and responsive behaviour.
11. Search for legacy hardcoded identity colours and framework-default styling that conflict with the new system.
12. Run the target project's required build, tests and visual verification workflow.

The goal is not to make every project look purple or to clone Tabularius. The goal is to transfer a coherent design system: quiet professional surfaces, disciplined spacing, modern forms, compact analytical components, consistent interaction hierarchy and strong reuse.