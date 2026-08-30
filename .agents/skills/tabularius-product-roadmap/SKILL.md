# Tabularius AI Product Roadmap

Use this skill whenever continuing product design or implementation work on Tabularius AI. It records the intended functional structure, domain model, implementation sequence, UX principles, current delivery priorities and AI boundary so work can be resumed without reconstructing the plan.

## Product purpose

Tabularius AI is a local-first application for Portuguese accountants to analyse SAF-T (PT) data, compare it with accounting and external evidence, identify discrepancies, perform deterministic accounting checks and use AI to interpret already-structured results.

The product must evolve from a SAF-T reader into an accounting analysis, reconciliation and audit workspace. The central hierarchy is:

`Accounting Entity -> Analysis Dossier / Fiscal Period -> Imports -> Analyses -> Reconciliations -> Findings -> Reports`

A dossier normally represents one entity and one fiscal year or accounting period. It can contain several SAF-T (PT) imports and, later, external evidence such as bank statements or tax declarations.

## Core principles

- Use SAF-T (PT) terminology in user-facing text.
- Keep the application local-first and suitable for sensitive accounting information.
- Preserve traceability from every calculated result or finding back to source records.
- Deterministic .NET code performs accounting calculations, reconciliations, validations and rule evaluation.
- Generative AI never becomes the source of accounting truth. AI explains, summarises and helps investigate deterministic results.
- Keep the AI provider behind an abstraction such as `IAIService`.
- Do not send accounting data to an external AI provider without an explicit product decision and appropriate user control.
- Use `decimal` for monetary values and treat imported files as untrusted input.
- Parse large SAF-T sections with secure streaming XML processing where practical.
- User-facing language is Portuguese (Portugal). Code, identifiers, developer documentation, commit messages and XML documentation are English.
- Every C# class, interface, record, enum, method and function must have English XML documentation. Document public domain/API properties where appropriate.
- Keep visual consistency with Denarius AI: navy application shell, green accent, light content surfaces, restrained cards and the same general visual language.

## Navigation model

Navigation follows the accounting hierarchy rather than exposing every future feature globally.

### Home

The application home page is a presentation and orientation page. It must not be the SAF-T upload form. It explains what Tabularius AI does and the major capability areas without duplicating detailed dossier workflow guidance. It may provide clear entry actions to entities and SAF-T import.

### Global sidebar

Keep global navigation intentionally small. It should expose only destinations that make sense without a selected dossier, currently:

- Home
- Entities and fiscal years
- Import SAF-T (PT)

Do not fill the global sidebar with disabled links for source-specific or dossier-specific functionality. Features such as accounts, customers, suppliers, movements, accounting analysis, reconciliation, tests, analytics, AI and reports belong to the selected dossier workspace when they become available.

### Entity and dossier workspace

The normal navigation path is:

`Entities -> Entity -> Fiscal year / Dossier -> Workspace`

Once a dossier is selected, the workspace exposes its own contextual navigation. SAF-T source-dependent pages preserve the selected import and use the shared source selector. Future accounting analysis, reconciliation, tests, analytics, AI and reporting should also be reached in dossier context rather than as context-free global destinations.

### SAF-T (PT) exploration

Provide structured exploration of the selected imported source data: summary, general ledger accounts, customers, suppliers, products/services, accounting transactions, sales documents, movement of goods documents, working/conference documents, payments/receipts and taxes. Support filtering, search and drill-down, preserving source traceability.

### Accounting analysis, reconciliation and audit

Build deterministic trial balance, general ledger, journal, balance sheet, income statement, monthly analysis, account evolution, customer/supplier/VAT analysis and indicators. Reconciliation is a central capability and should compare SAF-T/accounting data with independent evidence using explicit states such as `Matched | Difference | Not found | Requires review`. Tests and audit use transparent deterministic rules with traceable evidence.

### Analytics, AI and reports

Analytics provides focused exploratory analysis rather than reproducing a complete BI platform. The AI assistant operates over structured application data and deterministic findings and must identify the evidence used whenever possible. Reports are accountant-controlled outputs built only after underlying analyses and findings have stable models.

## Domain model direction

The persistence model starts with `AccountingEntity`, `AnalysisDossier` and `SaftImport`, and evolves with imported master data/transactions, external evidence, reconciliation runs/items, audit rules/findings, review annotations and report metadata. Do not persist only dashboard aggregates if doing so destroys traceability.

## Import workflow

The current first-import workflow may infer the entity from NIF and the dossier from fiscal year. The import operation lives on a dedicated import page, not on Home. As the dossier workflow matures, prefer importing additional sources from within the selected dossier and validate that they belong to that entity/exercise.

Target flow:

`Select/create entity -> Select/create dossier -> Import SAF-T (PT) -> Validate -> Parse -> Persist source/provenance -> Calculate deterministic summaries -> Show findings/analysis`

Do not use file name as a trusted business identifier.

## SAF-T source behavior

A dossier can contain multiple SAF-T (PT) imports. Source-specific pages must expose the shared source selector and preserve `importId` between related pages. If no source is selected, default to the latest accounting period by `EndDate`, then `StartDate`, then import timestamp and identifier. Never silently sum or merge multiple files. Consolidated dossier views require explicit deterministic accounting rules.

## SAF-T parsing direction

Continue using namespace-aware, secure streaming parsing. Avoid loading large XML files into a full in-memory DOM solely for convenience. Parse elements only in their correct structural path. Important areas include `Header`, `MasterFiles/GeneralLedgerAccounts/Account`, `MasterFiles/Customer`, `MasterFiles/Supplier`, `MasterFiles/Product`, `GeneralLedgerEntries/Journal/Transaction`, and the relevant `SourceDocuments` sections. Support legitimate SAF-T (PT) versions deliberately.

## Current implementation baseline

The application already has the entity/dossier/import structure, persisted accounts, customers, suppliers and products, accounting transactions, sales documents and movement of goods exploration. Deterministic accounting views already include a trial balance, income statement and a synthetic balance-sheet view.

Treat these capabilities as an implementation baseline rather than as completed areas. Existing functionality must remain source-aware, traceable and covered progressively by regression tests as the product evolves.

## Immediate technical completion

Before adding the next SAF-T functional area, close technical debt introduced by the latest persistence changes:

1. Complete cookie-consent persistence by keeping the Entity Framework model snapshot synchronized with the `CookieConsentAcceptedAt` user property and validating the migration path.
2. Complete movement-of-goods persistence by ensuring the Entity Framework migration and model snapshot accurately represent `SaftStockMovement` and `SaftStockMovementLine`.
3. Validate pending migrations against both the SQLite development path and SQL Server deployment path.
4. Add focused parser, persistence and controller tests for movement-of-goods data, including source/import isolation and line detail.
5. Confirm that existing imported SAF-T data requiring newly persisted sections has an explicit re-import or backfill strategy; never silently present incomplete persisted data as complete.
6. Keep GitHub Actions green before starting a new coherent feature set. Investigate and correct CI failures before continuing.

## Functional delivery sequence

After the immediate technical completion work, continue SAF-T coverage in this order.

### 1. Working and conference documents

Implement the SAF-T `WorkingDocuments` area with:

- secure structural parsing;
- persistence associated with `SaftImport`;
- document list and line-level drill-down;
- date, type, number and relevant party/source information;
- search, filtering and pagination consistent with existing dossier lists;
- shared source selection preserving `importId`;
- synthetic parser/persistence tests and source-isolation tests.

### 2. Payments and receipts

Implement the SAF-T `Payments` area with:

- payment/receipt document metadata;
- customer/supplier and source identifiers where supplied by SAF-T;
- payment dates, settlement values and document totals;
- line-level detail and references to settled source documents when available;
- source-aware list/detail views, search and filtering;
- parser, persistence and deterministic total tests.

### 3. Taxes

Expose and persist the SAF-T tax table and tax information required for deterministic analysis:

- tax type, code, description, country/region and rates where applicable;
- traceability between document lines and tax codes;
- VAT-focused exploration;
- validation of unknown or inconsistent tax codes without inventing classifications;
- tests for tax-table parsing and document/tax relationships.

### 4. Complete deterministic accounting analysis

Once source records are reliable, expand accounting analysis beyond the current views:

- general ledger account drill-down;
- journal exploration;
- monthly debit/credit/balance evolution;
- account evolution and comparative periods where valid;
- customer and supplier analysis;
- VAT analysis based on persisted tax/document data;
- accounting indicators with transparent formulas;
- formalize the balance sheet only when a reliable taxonomy/classification mapping is available; until then keep synthetic views explicitly labelled as such.

### 5. Reconciliation

Reconciliation becomes the next major product capability after deterministic source and accounting coverage is stable.

Introduce explicit reconciliation models for runs, evidence and items. Reconciliations should support deterministic comparisons between accounting/SAF-T data and independent evidence, initially prioritising high-value cases such as:

- accounting entries versus sales documents;
- customer/supplier balances versus document evidence;
- VAT/tax data versus document totals;
- later, bank statements and tax declarations as external evidence.

Every reconciliation item uses explicit states:

`Matched | Difference | Not found | Requires review`

A difference must retain both sides of the comparison and enough provenance for an accountant to reproduce the result.

### 6. Audit rules and findings

Build a transparent deterministic rule engine after reconciliation models are stable. Initial rules may detect:

- unbalanced or structurally inconsistent accounting entries;
- duplicate or suspicious document identifiers;
- unexpected gaps or sequence anomalies where the SAF-T semantics support the check;
- document/accounting total differences;
- inconsistent tax usage;
- unusual dates, values or counterparties using explicit deterministic criteria.

Rules produce traceable findings, severity, status and evidence. Findings must be reviewable and must not be generated solely from LLM output.

### 7. Analytics

Add focused analytics over stable persisted data and findings:

- period trends;
- account concentration and evolution;
- customer/supplier concentration;
- tax/VAT patterns;
- document and transaction distributions;
- anomaly/finding summaries.

Analytics should support investigation and should not attempt to reproduce a full BI platform.

### 8. AI assistant

Introduce the AI assistant only after deterministic analyses, reconciliations and findings provide a reliable structured context.

The assistant should:

- explain deterministic results rather than recalculate accounting truth;
- summarise findings and help prioritise investigation;
- answer questions over selected dossier/source context;
- cite or identify application evidence used whenever possible;
- remain behind a provider abstraction;
- require an explicit product decision before confidential accounting data can leave the local environment.

### 9. Reports

Build reports last, over stable analysis and finding models. Reports should support accountant-controlled selection of content, provenance and review status. Generated narrative may be AI-assisted, but reported figures and findings must come from deterministic application data.

## Cross-cutting requirements

Apply these requirements throughout the roadmap:

- Preserve `AccountingEntity -> AnalysisDossier -> SaftImport` provenance for every source-derived record.
- Never silently merge multiple SAF-T imports.
- Use the shared source selector consistently on source-dependent pages.
- Reuse the established list/detail UX, compact table actions, filtering and pagination patterns.
- Keep user-facing language in Portuguese (Portugal).
- Keep code, identifiers, commits, technical documentation and XML documentation in English.
- Add English XML documentation to C# classes, interfaces, records, enums, methods and functions.
- Keep Entity Framework migrations and `TabulariusDbContextModelSnapshot` synchronized for every schema change.
- Consider SQLite and SQL Server for every persistence change.
- Use synthetic or anonymised SAF-T data in automated tests and documentation.
- Treat SAF-T/XML, imported text and future external evidence as untrusted input.
- Do not expose secrets or confidential accounting information in logs, fixtures, screenshots or AI prompts.
- Keep CI green and add regression tests as each functional area becomes stable.

## Testing expectations

Use synthetic SAF-T fixtures by default. The repository reference SAF-T may be used according to the SAF-T skill for structural confirmation and appropriate regression/integration checks. Tests should progressively cover encodings, malformed XML, DTD rejection, namespace/version handling, header extraction, structural paths, entity/dossier matching, repeated imports, source selection, deterministic accounting calculations, reconciliation and audit rules.

For each new persisted SAF-T section, test at minimum: successful parsing, empty/missing optional sections, malformed relevant values, persistence, source isolation between multiple imports, list/detail retrieval and deterministic totals where applicable.

For every database model change, validate that the migration and Entity Framework snapshot describe the same model and that application startup does not trigger pending-model-change failures.

## Development/versioning reminder

Before each coherent modification set, inspect the repository's development-versioning skill and current project version. Increment the development suffix without reusing or decrementing a previous development version. Follow the repository skills for SAF-T, coding, data, UI, Git and quality.

Normal contributions should use a dedicated branch and pull request according to `CONTRIBUTING.md`. Only continue directly on `main` when the user has explicitly instructed that workflow for the relevant work.

## Definition of product success

Tabularius AI succeeds when an accountant can open a dossier, import SAF-T (PT) and external accounting evidence, identify and investigate differences with traceable deterministic evidence, review anomalies efficiently, and use AI to explain or summarise those results without delegating accounting truth to the language model.
