# Tabularius AI Product Roadmap

Use this skill whenever continuing product design or implementation work on Tabularius AI. It records the intended functional structure, domain model, implementation sequence, UX principles and AI boundary so work can be resumed without reconstructing the plan.

## Product purpose

Tabularius AI is a local-first application for Portuguese accountants to analyse SAF-T (PT) data, compare it with accounting and external evidence, identify discrepancies, perform deterministic accounting checks and use AI to interpret already-structured results.

The product must evolve from a SAF-T reader into an accounting analysis, reconciliation and audit workspace.

The central concept is not the XML file. The central hierarchy is:

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
- Use `decimal` for monetary values.
- Treat imported XML and external files as untrusted input.
- Parse large SAF-T sections with secure streaming XML processing where practical.
- User-facing language is Portuguese (Portugal). Code, identifiers, developer documentation, commit messages and XML documentation are English.
- Every C# class, interface, record, enum, method and function must have English XML documentation. Document public domain/API properties where appropriate.
- Keep visual consistency with Denarius AI: navy application shell, green accent, light content surfaces, restrained cards and the same general visual language.

## Target navigation

### Dashboard

The dossier overview should answer: what requires the accountant's attention?

Show entity, NIF, fiscal year/period, SAF-T version, source software, import status, relevant document/movement totals, reconciliation status, detected findings and high-level accounting indicators.

### Dossier

#### Entities
Manage companies or organisations being analysed. The Portuguese tax registration number is the principal natural identifier for matching imported SAF-T data to an existing entity.

#### Dossiers
Manage fiscal-year or accounting-period workspaces for each entity.

#### Imports
Show imported SAF-T (PT) files and, later, other evidence sources. Preserve import metadata and provenance.

### SAF-T (PT)

Provide direct structured exploration of imported source data:

- Summary
- General ledger accounts
- Customers
- Suppliers
- Products/services
- Accounting transactions
- Sales documents
- Movement of goods documents
- Working/conference documents
- Payments/receipts
- Taxes

Support filtering, search and drill-down. A user must be able to navigate from an analytical result to the relevant SAF-T records.

### Accounting Analysis

Build deterministic accounting views from imported data:

- Trial balance
- General ledger
- Journal
- Balance sheet
- Income statement
- Monthly analysis
- Account evolution
- Customer analysis
- Supplier analysis
- VAT analysis
- Accounting indicators and ratios

### Reconciliation

This is a central product capability. Compare SAF-T/accounting data with independent sources.

Target reconciliations include:

- Bank statements vs bank ledger accounts
- VAT declarations vs recorded VAT
- Issued invoices vs sales/accounting records
- Supplier invoices vs purchases/accounting records
- Customer/supplier current accounts vs documents
- Fixed assets vs accounting balances
- Payroll/Social Security evidence vs accounting records

Each reconciliation item should use explicit deterministic statuses such as:

`Matched | Difference | Not found | Requires review`

Store matching evidence, differences and review status so the accountant can work through exceptions.

### Tests and Audit

Implement a reusable deterministic rule engine. Candidate tests include:

- Duplicate documents
- Missing or broken document sequences
- Unbalanced accounting entries
- Transactions outside the expected period
- Accounts with suspicious or abnormal balances
- Invalid tax identifiers
- Inconsistent VAT rates or tax treatment
- Documents without expected accounting correspondence
- Differences between declared and accounted totals
- Statistical or rule-based unusual transactions

A finding should expose:

`Rule -> Result -> Severity -> Evidence -> Affected records -> Review state`

Eventually allow configurable test libraries, but begin with built-in rules that are transparent and testable.

### Analytics

Provide exploratory analysis without trying to reproduce a complete BI platform. Candidate analyses:

- Sales evolution
- Expense evolution
- Margins
- Customer concentration
- Supplier concentration
- Aging
- Seasonality
- Account distributions
- Tax evolution
- Monthly/yearly comparisons
- Statistical outliers

### AI Assistant

The assistant operates over structured application data and deterministic findings. Example questions:

- Which anomalies are most relevant in this fiscal year?
- Explain the unusual balance in a specific account.
- Which customers had the largest revenue decrease?
- Summarise bank reconciliation differences.
- Prepare discussion points for the accountant's client.

AI output must be distinguishable from deterministic results and should cite/identify the application evidence used whenever possible.

### Reports

Support accountant-controlled report composition and export. Candidate reports:

- Accounting analysis report
- Differences report
- Reconciliation report
- Audit/findings report
- Tax analysis report
- Executive report

Allow selected findings and evidence to be included. Target PDF and Excel export after the underlying analysis model is stable.

## Domain model direction

The initial persistence model already starts with:

- `AccountingEntity`
- `AnalysisDossier`
- `SaftImport`

Expected evolution may add concepts such as:

- imported SAF-T master data and transaction records
- external evidence/import sources
- reconciliation runs and reconciliation items
- audit rules and findings
- review/status annotations
- report definitions or generated report metadata

Do not persist only dashboard aggregates if doing so destroys traceability. Design storage so findings can point to the relevant imported records.

## Current implementation state

At the time this roadmap was created:

- ASP.NET Core MVC on .NET 9 is established.
- The application accepts SAF-T (PT) XML files up to 100 MB.
- Windows-1252 SAF-T input is supported through the code pages provider.
- SAF-T parsing uses secure `XmlReader` settings and streams the document.
- Header data is extracted and displayed.
- A structural summary counts accounts, customers, suppliers, products/services, accounting transactions, sales invoices, movement-of-goods documents, working documents and payments.
- The UI uses a Denarius-style navy/green application shell and sidebar.
- The navigation shell includes Dashboard, Dossier, SAF-T (PT), Accounting Analysis, Reconciliation, Tests and Audit, Analytics, AI Assistant and Reports. Most destinations are intentionally disabled until implemented.
- Entity Framework Core SQL Server support has been added.
- SQL Server LocalDB is configured as the initial local persistence provider.
- `TabulariusDbContext`, `AccountingEntity`, `AnalysisDossier` and `SaftImport` exist.
- Upload processing attempts to create/reuse an entity by NIF, create/reuse a fiscal-year dossier and persist SAF-T import metadata.
- The project development version was `0.1.0-dev.2` when this roadmap was recorded.

## Immediate next steps

Resume implementation in this order unless a later product decision changes priorities:

1. Create and verify the initial EF Core migration for the current entity/dossier/import model.
2. Ensure local database creation/update has a deliberate development workflow; do not silently hide migration failures.
3. Verify the project builds and the current SAF-T upload persists entity, dossier and import metadata correctly.
4. Implement real Entities, Dossiers and Imports pages and activate those sidebar links.
5. After upload, provide navigation to the created/reused entity and dossier instead of treating the result as a temporary upload-only screen.
6. Decide how complete SAF-T source data should be persisted. Preserve source traceability and avoid storing only summary counts.
7. Implement SAF-T exploration pages beginning with accounts, customers, suppliers and accounting transactions.
8. Build deterministic accounting views such as trial balance, journal and general ledger.
9. Introduce the reconciliation model and implement bank reconciliation as the first external comparison workflow.
10. Introduce deterministic audit rules/findings.
11. Add analytics on top of validated structured data.
12. Add the AI abstraction and assistant only after sufficient deterministic domain data and findings are available.
13. Add accountant-controlled reports/export after analyses and findings have stable models.

## Suggested import workflow

The intended user flow is:

`Select/create entity -> Select/create dossier -> Import SAF-T (PT) -> Validate -> Parse -> Persist source/provenance -> Calculate deterministic summaries -> Show findings/analysis`

For convenience, the first import may infer the entity from NIF and the dossier from fiscal year, but the UI should make the resulting entity and dossier explicit and allow the accountant to control the workspace.

Do not use file name as a trusted business identifier.

## SAF-T parsing direction

Continue using namespace-aware, secure streaming parsing. Avoid loading 100 MB XML files into a full in-memory DOM solely for convenience.

When expanding extraction, count or parse elements only in their correct SAF-T structural path. Do not rely only on `LocalName` if the same element name could occur in another context.

Important source areas include:

- `Header`
- `MasterFiles/GeneralLedgerAccounts/Account`
- `MasterFiles/Customer`
- `MasterFiles/Supplier`
- `MasterFiles/Product`
- `GeneralLedgerEntries/Journal/Transaction`
- `SourceDocuments/SalesInvoices/Invoice`
- `SourceDocuments/MovementOfGoods/StockMovement`
- `SourceDocuments/WorkingDocuments/WorkDocument`
- `SourceDocuments/Payments/Payment`

Support multiple legitimate SAF-T (PT) versions deliberately. Use `AuditFileVersion` as the user-facing SAF-T version and validate the Portuguese SAF-T namespace separately.

## Testing expectations

Use synthetic SAF-T fixtures only. Never commit real client/accounting SAF-T data.

Tests should progressively cover:

- supported encodings including Windows-1252
- malformed XML
- DTD/external entity rejection
- SAF-T (PT) namespace/version handling
- header extraction
- exact structural-path counts
- entity matching by NIF
- dossier matching by fiscal year
- duplicate/repeated import behaviour
- deterministic accounting calculations
- reconciliation matching rules
- audit rule outcomes

## Development/versioning reminder

Before each coherent modification set, inspect the repository's development-versioning skill and current project version. Increment the development suffix without reusing or decrementing a previous development version.

Follow the repository's other skills for SAF-T, coding, data, UI, Git and quality. If the user has explicitly instructed work to continue directly on `main`, follow that instruction until the user changes it; otherwise follow the repository's normal Git workflow.

## Definition of product success

Tabularius AI succeeds when an accountant can open a dossier, import SAF-T (PT) and external accounting evidence, identify and investigate differences with traceable deterministic evidence, review anomalies efficiently, and use AI to explain or summarise those results without delegating accounting truth to the language model.
