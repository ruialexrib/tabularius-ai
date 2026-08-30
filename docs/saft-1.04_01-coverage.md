# SAF-T (PT) 1.04_01 coverage audit

This document records the current persistence and application coverage against the repository's authoritative `assets/schema/saftpt1.04_01.xsd`.

## Current application coverage

| SAF-T area | Current status | Persisted / exposed |
| --- | --- | --- |
| Header | Partial | Version, tax registration number, company name, fiscal year and import period metadata used by the application. The full Header structure is not persisted. |
| MasterFiles / GeneralLedgerAccounts | Supported core | Account identifier, description, opening/closing debit and credit balances, taxonomy reference. |
| MasterFiles / Customer | Supported core | Customer identifier, account, tax identifier and company name. Full addresses, contacts, self-billing and other optional metadata are not persisted. |
| MasterFiles / Supplier | Supported core | Supplier identifier, account, tax identifier and company name. Full addresses, contacts, self-billing and other optional metadata are not persisted. |
| MasterFiles / Product | Supported core | Type, code, group, description and product number code. |
| MasterFiles / TaxTable | Supported core | Tax type, country/region, code, description, percentage and amount. |
| GeneralLedgerEntries | Supported core | Journals, transactions and debit/credit lines required by current accounting reports. Several optional transaction/line references are not persisted. |
| SourceDocuments / SalesInvoices | Supported core | Invoice identity/status/date/type/source/customer, totals, product lines and core tax data. Optional references, settlement, withholding and richer tax/exemption metadata are not fully persisted. |
| SourceDocuments / MovementOfGoods | Supported core | Document identity/status/date/type/source/customer/supplier and product lines. Addresses, movement times and other logistics metadata are not fully persisted. |
| SourceDocuments / WorkingDocuments | Supported core | Document identity/status/date/type/source/customer, totals and product lines with core tax data. Optional references and richer metadata are not fully persisted. |
| SourceDocuments / Payments | Supported core | Payment reference/status/date/type/source/customer, totals and settlement lines with originating document/date and debit/credit amount. Payment mechanisms, settlement details, withholding and other optional payment metadata are not yet fully persisted. |

## Structural conclusion

All major top-level accounting datasets currently targeted by Tabularius AI are represented: master data, general ledger and the four SourceDocuments groups (SalesInvoices, MovementOfGoods, WorkingDocuments and Payments).

This is **core analytical coverage**, not full-field XSD fidelity. The next SAF-T hardening phase should therefore be field-level rather than adding another major top-level section.

## Recommended field-level backlog

1. Header: persist additional audit/software/company metadata required for traceability.
2. Customers and suppliers: addresses, contacts and relevant billing flags.
3. General ledger: source-document references and optional line metadata useful for audit trails.
4. Sales invoices: document references, settlement, withholding, tax exemptions and richer status metadata.
5. Movement of goods: movement timestamps, delivery/loading addresses and logistics references.
6. Working documents: document references and optional tax/exemption metadata.
7. Payments: payment mechanisms, settlement information, withholding tax and richer source-document references.
8. Add representative XSD-valid fixtures for each supported SourceDocuments group and regression tests for precision, optional values and rollback behavior.

## Definition of support

A section is marked `Supported core` only when Tabularius AI parses the section, persists the application's analytical subset, scopes it to a SAF-T import and exposes it through the dossier workspace. This document must not be interpreted as a claim that every optional element defined by SAF-T (PT) 1.04_01 is persisted.
