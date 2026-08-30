# Tabularius AI Product Roadmap

Use this skill whenever continuing product design or implementation work on Tabularius AI. It records the intended functional structure, domain model, implementation sequence, UX principles, current delivery priorities and AI boundary so work can be resumed without reconstructing the plan.

## Product purpose

Tabularius AI is a local-first application for Portuguese accountants to analyse SAF-T (PT) data, compare it with accounting and external evidence, identify discrepancies, perform deterministic accounting checks and use AI to interpret already-structured results.

The product must evolve from a SAF-T reader into an accounting analysis, reconciliation and audit workspace. The central hierarchy is:

`Accounting Entity -> Analysis Dossier / Fiscal Period -> Imports -> Analyses -> Reconciliations -> Findings -> Reports`

## Core analytical principle

The application must remain useful when AI is disabled. Accounting figures, totals, balances, ratios, comparisons, anomaly rules and findings are calculated deterministically by application code or database queries. AI is optional and may explain, summarise, prioritise and investigate those structured results; it must not become the source of accounting truth.

Every analytical result must preserve the selected dossier and SAF-T import provenance. Never silently aggregate multiple imports.

## Current product baseline

The application provides entity/dossier/import management, SAF-T master data, general-ledger transactions, sales documents, stock movements, working documents, payments and tax-table exploration. Deterministic accounting views include trial balance, income statement and balance-sheet analysis. The analytical workspace includes `Visão geral`, `Anomalias` and `Análise de contas`. Optional provider-neutral AI reports are available in every accounting-analysis and analytical subarea, using dedicated administrator-configurable prompts and bounded deterministic context.

## Current priority — deepen the accountant analytical workspace

The first analytical workspace is established. The next delivery cycle must turn it from summary views into an investigation workflow for day-to-day accounting review.

### 1. Account investigation — next functional priority

Extend `Análise de contas` with:

- account detail/drill-down page;
- monthly debit and credit evolution;
- opening, period movement and closing position where deterministic source semantics support them;
- links to the underlying accounting entries;
- largest movements and concentration indicators;
- counterpart-account relationships derived from traceable accounting entries;
- optional AI action to explain the selected account using only the deterministic metrics and bounded evidence shown by the application.

### 2. Anomalies and controls — hardening

Move anomaly detection out of controllers into dedicated deterministic services/rules. Add explicit rule identifiers and tests. Expand progressively beyond the initial rules to document sequence gaps and duplicates where reliable, dates outside the selected accounting period, unusual/extreme values using transparent criteria, tax-code/rate inconsistencies and accounting/document reconciliation differences when the required relationships are available.

Every finding must expose severity, rule, reference, explanation and traceable evidence. AI may explain and prioritise findings but cannot create an accounting finding without deterministic evidence.

### 3. Analytical overview — richer accountant indicators

Extend the current transaction/account KPIs with deterministic indicators from the already-persisted SAF-T areas: sales and document activity, customer concentration, receipts/payments, tax distribution and period comparisons where comparable source periods are explicitly selected. The overview AI report remains an interpretation of calculated indicators, never their source.

## Accounting analysis + AI reports

`Balancete`, `Demonstração de Resultados` and `Balanço` remain deterministic accounting reports. When AI is enabled, each page may display a separate interpretative AI report based on the already-calculated report context. Each subarea has its own configurable prompt in AI Settings. The UI must clearly distinguish deterministic accounting results from AI interpretation.

## Analytical area + AI reports

`Visão geral`, `Anomalias` and `Análise de contas` provide deterministic investigation and control. When AI is enabled, each page may display a separate interpretative AI report based on bounded deterministic context. Each subarea has its own configurable prompt in AI Settings. AI output must never overwrite, alter or silently supplement deterministic findings.

## Subsequent analytical expansion

After account investigation and anomaly hardening are stable and tested, continue with:

1. Customer and receivables analysis — concentration, activity, receipts and deterministic open-item indicators where source relationships permit them.
2. Fiscal/VAT analysis — tax-code usage, rates, exemptions where available, document/tax consistency and period trends.
3. Sales and document analytics — turnover, document distributions, customer/product concentration and period comparisons.
4. Dossier health indicators — transparent composite dimensions based only on deterministic controls, with drill-down to the evidence behind every score component.
5. Cross-area AI briefing — optional dossier-level summary composing already-calculated results from individual analytical areas without replacing their dedicated reports.

## Reconciliation

Reconciliation remains a major product capability after deterministic source and analytical coverage is stable. Introduce explicit reconciliation models for runs, evidence and items, initially prioritising accounting entries versus documents, customer/supplier evidence, VAT/tax totals and later external bank/tax evidence.

Every reconciliation item uses explicit states:

`Matched | Difference | Not found | Requires review`

A difference must retain both sides and enough provenance for an accountant to reproduce it.

## AI boundary

The assistant and analytical reports should explain deterministic results rather than recalculate accounting truth, summarise findings, help prioritise investigation, answer questions over the selected dossier/source context and identify application evidence used whenever possible. Provider-specific behavior remains behind `IAIService`. Confidential accounting data must not leave the local environment without explicit product/user control.

## Reports

Reports are built over stable analysis and finding models. Accountant-controlled selection, provenance and review status are mandatory. Narrative may be AI-assisted, but figures and findings come from deterministic application data.

## UX and navigation

Keep global navigation small. Dossier-specific navigation is grouped by functional area. `Dados SAF-T (PT)` only shows source-data exploration pages; `Análise contabilística` only shows `Balancete | Demonstração de Resultados | Balanço`; `Área analítica` only shows `Visão geral | Anomalias | Análise de contas`. Preserve `importId` consistently when navigating within an area.

User-facing language is Portuguese (Portugal). Code, identifiers, developer documentation, commit messages and XML documentation are English. Use the application purple/violet identity and established modern list/detail/edit patterns.

## Cross-cutting requirements

- Preserve `AccountingEntity -> AnalysisDossier -> SaftImport` provenance.
- Never silently merge multiple SAF-T imports.
- Use the shared source selector on source-dependent pages.
- Use `decimal` for monetary calculations.
- Treat SAF-T/XML and future external evidence as untrusted input.
- Keep deterministic calculations independent from generative AI.
- Keep AI prompts for each analytical/accounting-analysis subarea configurable in AI Settings.
- Render AI Markdown safely and tolerate common model wrappers such as Markdown code fences.
- Add English XML documentation to C# classes, records, methods and public domain/API properties where appropriate.
- Keep EF migrations and model snapshot synchronized for schema changes.
- Support SQLite and SQL Server deployment paths.
- Use synthetic/anonymised accounting data in tests.
- Keep CI green and add regression tests for every analytical rule and calculation.

## Immediate next engineering steps

1. Stabilise the current AI analytical-report implementation, migration/model snapshot and regression tests.
2. Add account drill-down with monthly evolution and links to underlying accounting entries.
3. Add largest-movement and counterpart-account analysis to the account detail.
4. Move anomaly calculations into dedicated deterministic services/rules with unit tests.
5. Add source-isolation and empty-source controller tests for analytics.
6. Expand deterministic anomaly rules with accounting/document/tax controls whose semantics can be demonstrated from SAF-T evidence.
7. Enrich the analytical overview with sales, customer, payment and tax indicators.
8. Continue SAF-T 1.04_01 field-level hardening and full XSD validation strategy in parallel with analytics.

## Definition of product success

Tabularius AI succeeds when an accountant can open a dossier, select a SAF-T source, understand the accounting position, identify and investigate relevant differences with traceable deterministic evidence, review anomalies efficiently, and optionally use AI to explain or summarise those results without delegating accounting truth to the language model.
