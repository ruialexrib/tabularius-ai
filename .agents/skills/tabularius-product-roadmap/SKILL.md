# Tabularius AI Product Roadmap

Use this skill whenever continuing product design or implementation work on Tabularius AI. It records the intended functional structure, domain model, implementation sequence, UX principles and AI boundary so work can be resumed without reconstructing the plan.

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

The application home page is a presentation and orientation page. It must not be the SAF-T upload form. It explains what Tabularius AI does, the major capability areas and the workflow from entity to analysis. It may provide clear entry actions to entities and SAF-T import.

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

## Implementation sequence

Continue from the established entity/dossier/import model and persisted accounts/customers/suppliers. Next priorities are products, then accounting movements with a parser/service architecture suitable for larger datasets, followed by documents and taxes. Build deterministic accounting views after the underlying records are reliable, then reconciliation, audit rules, analytics, AI and reports.

Before expanding source data, keep the entity -> dossier -> source navigation coherent and preserve provenance. New dossier-specific functionality must be integrated into the contextual workspace rather than added automatically to the global sidebar.

## Testing expectations

Use synthetic SAF-T fixtures by default. The repository reference SAF-T may be used according to the SAF-T skill for structural confirmation and appropriate regression/integration checks. Tests should progressively cover encodings, malformed XML, DTD rejection, namespace/version handling, header extraction, structural paths, entity/dossier matching, repeated imports, source selection, deterministic accounting calculations, reconciliation and audit rules.

## Development/versioning reminder

Before each coherent modification set, inspect the repository's development-versioning skill and current project version. Increment the development suffix without reusing or decrementing a previous development version. Follow the repository skills for SAF-T, coding, data, UI, Git and quality. If the user has explicitly instructed work to continue directly on `main`, follow that instruction until changed.

## Definition of product success

Tabularius AI succeeds when an accountant can open a dossier, import SAF-T (PT) and external accounting evidence, identify and investigate differences with traceable deterministic evidence, review anomalies efficiently, and use AI to explain or summarise those results without delegating accounting truth to the language model.
