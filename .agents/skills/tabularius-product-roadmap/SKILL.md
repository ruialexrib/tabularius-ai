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

The application provides entity/dossier/import management, SAF-T master data, general-ledger transactions, sales documents, stock movements, working documents, payments and tax-table exploration. Deterministic accounting views include trial balance, income statement and balance-sheet analysis. Optional provider-neutral AI workflows are already available over bounded accounting tools.

## Current priority — accountant analytical workspace

The next major product capability is a focused analytical area designed for day-to-day accounting review. Delivery starts with three connected views:

### 1. Analytical overview

Provide a source-aware overview with deterministic KPIs and trends, initially:

- accounting transaction count;
- total debit and credit movement;
- active accounts;
- deterministic anomaly count;
- monthly debit/credit evolution;
- accounts with the highest movement volume.

Later extend this overview with sales, customer, payment and tax indicators as their deterministic semantics are hardened. AI, when enabled, may provide an interpretation or period briefing based only on the calculated indicators.

### 2. Anomalies and controls

Build a transparent deterministic control centre. Initial rules cover:

- unbalanced accounting entries;
- negative line amounts where structurally unexpected;
- invalid debit/credit side values;
- duplicate accounting transaction identifiers.

Expand progressively to document sequences, tax consistency, unusual dates/values, reconciliation differences and other explicit accounting rules. Every finding must expose severity, rule, reference and traceable evidence. AI may explain or prioritise findings but cannot create an accounting finding without deterministic evidence.

### 3. Account analysis

Provide account-level investigation with:

- debit movement;
- credit movement;
- net movement;
- movement-line count;
- search by account or description;
- ranking/concentration views;
- future monthly evolution and drill-down to transactions;
- future counterparty/account relationship analysis where evidence supports it.

AI actions such as “Explain this variation” or “Investigate this account” should consume deterministic account metrics and bounded source evidence.

## Subsequent analytical expansion

After the first analytical workspace is stable and tested, continue with:

1. Customer and receivables analysis — concentration, activity, receipts and deterministic open-item indicators where source relationships permit them.
2. Fiscal/VAT analysis — tax-code usage, rates, exemptions where available, document/tax consistency and period trends.
3. Sales and document analytics — turnover, document distributions, customer/product concentration and period comparisons.
4. Dossier health indicators — transparent composite dimensions based only on deterministic controls, with drill-down to the evidence behind every score component.
5. AI analytical briefing — optional summaries, explanations and investigation prompts over the deterministic analytical context.

## Reconciliation

Reconciliation remains a major product capability after deterministic source and analytical coverage is stable. Introduce explicit reconciliation models for runs, evidence and items, initially prioritising accounting entries versus documents, customer/supplier evidence, VAT/tax totals and later external bank/tax evidence.

Every reconciliation item uses explicit states:

`Matched | Difference | Not found | Requires review`

A difference must retain both sides and enough provenance for an accountant to reproduce it.

## AI boundary

The assistant should explain deterministic results rather than recalculate accounting truth, summarise findings, help prioritise investigation, answer questions over the selected dossier/source context and identify application evidence used whenever possible. Provider-specific behavior remains behind `IAIService`. Confidential accounting data must not leave the local environment without explicit product/user control.

## Reports

Reports are built over stable analysis and finding models. Accountant-controlled selection, provenance and review status are mandatory. Narrative may be AI-assisted, but figures and findings come from deterministic application data.

## UX and navigation

Keep global navigation small. Dossier-specific data, accounting analysis, analytics, AI and future reconciliation/reporting belong to the selected dossier workspace. Analytical pages use a shared internal navigation for `Visão geral | Anomalias | Análise de contas` and preserve `importId` consistently.

User-facing language is Portuguese (Portugal). Code, identifiers, developer documentation, commit messages and XML documentation are English. Use the application purple/violet identity and established modern list/detail/edit patterns.

## Cross-cutting requirements

- Preserve `AccountingEntity -> AnalysisDossier -> SaftImport` provenance.
- Never silently merge multiple SAF-T imports.
- Use the shared source selector on source-dependent pages.
- Use `decimal` for monetary calculations.
- Treat SAF-T/XML and future external evidence as untrusted input.
- Keep deterministic calculations independent from generative AI.
- Add English XML documentation to C# classes, records, methods and public domain/API properties where appropriate.
- Keep EF migrations and model snapshot synchronized for schema changes.
- Support SQLite and SQL Server deployment paths.
- Use synthetic/anonymised accounting data in tests.
- Keep CI green and add regression tests for every analytical rule and calculation.

## Immediate next engineering steps

1. Stabilise and test the first `Visão geral + Anomalias + Análise de contas` implementation.
2. Add account drill-down with monthly evolution and links to underlying transactions.
3. Move anomaly calculations into dedicated deterministic services/rules with unit tests.
4. Add source-isolation and empty-source controller tests for analytics.
5. Add optional AI interpretation actions only after deterministic analytical contracts are tested.
6. Continue SAF-T 1.04_01 field-level hardening and full XSD validation strategy in parallel with analytics.

## Definition of product success

Tabularius AI succeeds when an accountant can open a dossier, select a SAF-T source, understand the accounting position, identify and investigate relevant differences with traceable deterministic evidence, review anomalies efficiently, and optionally use AI to explain or summarise those results without delegating accounting truth to the language model.
