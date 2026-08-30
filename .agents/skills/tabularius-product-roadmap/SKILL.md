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

The application now provides:

- entity, dossier and SAF-T import management;
- SAF-T master-data exploration for accounts, customers, suppliers, products and taxes;
- general-ledger transactions with entry detail;
- sales documents, stock movements, working documents and payments;
- deterministic `Balancete`, `Demonstração de Resultados` and `Balanço`;
- an analytical workspace with `Visão geral`, `Anomalias`, `Análise de contas` and `Análise de IVA`;
- account investigation with monthly debit/credit evolution, opening/period/closing positions, traceable links to accounting entries, movement detail and counterpart-account analysis;
- VAT analysis with deterministic totals by rate and document plus document/customer/type/rate/date filtering;
- optional provider-neutral AI reports across accounting-analysis and analytical subareas, using administrator-configurable prompts and bounded deterministic context;
- shared Markdown rendering for AI analytical reports;
- local SQLite and server SQL Server/Docker deployment paths;
- authentication, user administration, first-use credential change and cookie-consent handling;
- dossier backup and restore;
- Windows installer/release path for normal local users.

## Delivered analytical milestones

### Account investigation — delivered baseline

The first account drill-down is implemented with:

- [x] account detail/investigation page;
- [x] monthly debit and credit evolution;
- [x] opening, period movement and closing positions;
- [x] traceable links to underlying accounting entries;
- [x] movement-level detail;
- [x] counterpart-account relationships derived from shared accounting entries;
- [x] bounded optional AI interpretation for the selected account;
- [x] largest movements included in the bounded AI context.

Further account-analysis work should now focus on richer deterministic concentration indicators, usability and regression coverage rather than recreating the drill-down foundation.

### VAT analysis — delivered baseline

`Análise de IVA` is now part of `Área analítica` and includes:

- [x] deterministic net taxable base, VAT and gross totals;
- [x] aggregation by VAT rate;
- [x] traceable effect by sales document and VAT rate;
- [x] filters for document/customer search, document type, VAT rate and date interval;
- [x] optional bounded AI interpretation;
- [x] source-aware navigation preserving the selected SAF-T import.

This is an analytical baseline, not a substitute for formal Portuguese VAT compliance validation.

### AI analytical reports — delivered baseline

- [x] separate AI interpretation from deterministic accounting results;
- [x] shared analytical-report component;
- [x] reports across accounting and analytical subareas;
- [x] configurable prompts per supported subarea;
- [x] shared Markdown presentation for model output;
- [x] provider-neutral AI boundary supporting local and remote model providers.

AI report hardening, regression tests and safe rendering remain continuous requirements.

### Navigation and workspace structure — delivered baseline

Dossier navigation is grouped by functional area:

- `Dados SAF-T (PT)`: `Resumo | Contas | Clientes | Fornecedores | Produtos | Impostos | Lançamentos | Documentos de vendas | Documentos de conferência | Movimentação de mercadorias | Recibos`;
- `Análise contabilística`: `Balancete | Demonstração de Resultados | Balanço`;
- `Área analítica`: `Visão geral | Anomalias | Análise de contas | Análise de IVA`.

The selected `importId` must continue to be preserved consistently when navigating inside source-dependent areas.

## Current priority — harden deterministic controls and enrich analysis

The account-investigation and VAT-analysis foundations are now present. The next cycle should strengthen deterministic controls, test coverage and the analytical overview before adding more large analytical areas.

### 1. Anomalies and controls — current functional priority

Move anomaly detection out of controllers into dedicated deterministic services/rules. Add explicit rule identifiers and tests. Expand progressively beyond the initial rules to:

- document sequence gaps and duplicates where reliable;
- dates outside the selected accounting period;
- unusual/extreme values using transparent criteria;
- tax-code/rate inconsistencies;
- accounting/document reconciliation differences when the required relationships are available.

Every finding must expose severity, rule, reference, explanation and traceable evidence. AI may explain and prioritise findings but cannot create an accounting finding without deterministic evidence.

### 2. Analytical overview — richer accountant indicators

Extend the current transaction/account KPIs with deterministic indicators from the already-persisted SAF-T areas:

- sales and document activity;
- customer concentration;
- receipts/payments;
- tax distribution;
- period comparisons where comparable source periods are explicitly selected.

The overview AI report remains an interpretation of calculated indicators, never their source.

### 3. Regression and source-isolation hardening

Increase automated coverage for analytical behavior, especially:

- selected-import isolation;
- empty-source behavior;
- account-investigation calculations;
- VAT calculations and sign handling;
- anomaly rules and evidence;
- AI analytical-report boundaries where practical.

## Accounting analysis + AI reports

`Balancete`, `Demonstração de Resultados` and `Balanço` remain deterministic accounting reports. When AI is enabled, each page may display a separate interpretative AI report based on the already-calculated report context. Each subarea has its own configurable prompt in AI Settings. The UI must clearly distinguish deterministic accounting results from AI interpretation.

## Analytical area + AI reports

`Visão geral`, `Anomalias`, `Análise de contas` and `Análise de IVA` provide deterministic investigation and control. When AI is enabled, each page may display a separate interpretative AI report based on bounded deterministic context. Each supported subarea has its own configurable prompt in AI Settings. AI output must never overwrite, alter or silently supplement deterministic findings.

## Subsequent analytical expansion

After anomaly hardening, overview enrichment and regression coverage are stable:

1. Customer and receivables analysis — concentration, activity, receipts and deterministic open-item indicators where source relationships permit them.
2. Sales and document analytics — turnover, document distributions, customer/product concentration and period comparisons.
3. Deeper VAT/fiscal controls — exemptions where available, tax-code consistency, document/tax reconciliation and period trends beyond the current VAT baseline.
4. Dossier health indicators — transparent composite dimensions based only on deterministic controls, with drill-down to the evidence behind every score component.
5. Cross-area AI briefing — optional dossier-level summary composing already-calculated results from individual analytical areas without replacing their dedicated reports.

## Reconciliation

Reconciliation remains a major product capability after deterministic source and analytical coverage is stable. Introduce explicit reconciliation models for runs, evidence and items, initially prioritising accounting entries versus documents, customer/supplier evidence, VAT/tax totals and later external bank/tax evidence.

Every reconciliation item uses explicit states:

`Matched | Difference | Not found | Requires review`

A difference must retain both sides and enough provenance for an accountant to reproduce it.

## AI boundary

The assistant and analytical reports should explain deterministic results rather than recalculate accounting truth, summarise findings, help prioritise investigation, answer questions over the selected dossier/source context and identify application evidence used whenever possible. Provider-specific behavior remains behind `IAIService`. Local models may run through Ollama; remote providers may also be configured. Confidential accounting data must not leave the local environment without explicit product/user control.

## Reports

Reports are built over stable analysis and finding models. Accountant-controlled selection, provenance and review status are mandatory. Narrative may be AI-assisted, but figures and findings come from deterministic application data.

## UX and navigation

Keep global navigation small. Dossier-specific navigation is grouped by functional area. Preserve the established functional navigation structure documented above and preserve `importId` consistently when navigating within an area.

User-facing language is Portuguese (Portugal). Code, identifiers, developer documentation, commit messages and XML documentation are English. Use the application purple/violet identity and established modern list/detail/edit patterns. Keep terminology consistent around `Área de trabalho` in the UI.

## Cross-cutting requirements

- Preserve `AccountingEntity -> AnalysisDossier -> SaftImport` provenance.
- Never silently merge multiple SAF-T imports.
- Use the shared source selector on source-dependent pages.
- Use `decimal` for monetary calculations.
- Treat SAF-T/XML and future external evidence as untrusted input.
- Keep deterministic calculations independent from generative AI.
- Keep AI prompts for each analytical/accounting-analysis subarea configurable in AI Settings.
- Render AI Markdown safely and idempotently and tolerate common model wrappers such as Markdown code fences.
- Add English XML documentation to C# classes, records, methods and public domain/API properties where appropriate.
- Keep EF migrations and model snapshot synchronized for schema changes.
- Support SQLite and SQL Server deployment paths.
- Use synthetic/anonymised accounting data in tests.
- Keep CI green and add regression tests for every analytical rule and calculation.
- Keep the README focused on essential capabilities and normal-user installation, with developer/server instructions kept concise.

## Immediate next engineering steps

1. Move anomaly calculations into dedicated deterministic services/rules with explicit rule identifiers and unit tests.
2. Add source-isolation and empty-source controller tests for analytics.
3. Add regression tests for account investigation and VAT calculations, including debit/credit and opposite-sign document behavior.
4. Expand deterministic anomaly rules with accounting/document/tax controls whose semantics can be demonstrated from SAF-T evidence.
5. Enrich the analytical overview with sales, customer, payment and tax indicators.
6. Harden the current VAT analysis, including filtering/binding edge cases and additional tax consistency controls.
7. Continue AI analytical-report hardening and regression coverage without changing the deterministic/AI boundary.
8. Continue SAF-T 1.04_01 field-level hardening and define the full XSD validation strategy in parallel with analytics.
9. After the above is stable, start customer/receivables analysis and then sales/document analytics.

## Definition of product success

Tabularius AI succeeds when an accountant can open a dossier, select a SAF-T source, understand the accounting position, identify and investigate relevant differences with traceable deterministic evidence, review anomalies efficiently, and optionally use AI to explain or summarise those results without delegating accounting truth to the language model.
