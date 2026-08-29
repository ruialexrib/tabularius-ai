---
name: tabularius-saft
description: Implement and review Portuguese SAF-T import, validation, mapping and analytical processing in Tabularius AI.
---

# Tabularius AI SAF-T processing

Use this skill whenever work affects SAF-T XML ingestion, validation, domain mapping, accounting data or derived analytics.

## Official SAF-T (PT) schema

- The authoritative schema for SAF-T (PT) version `1.04_01` is published by the Portuguese Tax and Customs Authority (Autoridade Tributária e Aduaneira) at `https://info.portaldasfinancas.gov.pt/apps/saft-pt04/saftpt1.04_01.xsd`.
- Keep a repository-local copy at `assets/saftpt1.04_01.xsd`. The local asset is the schema used by development, automated tests and application validation; the Portal das Finanças URL remains the authoritative upstream source.
- Every SAF-T (PT) `1.04_01` file accepted by Tabularius AI must be validated against this XSD before its accounting data is persisted or analysed. Namespace/header checks alone are not sufficient validation.
- XSD validation failure must reject the import and return an actionable user-facing validation error. A failed validation must never leave partially persisted data.
- Tests and synthetic SAF-T assets must also be validated against the repository XSD. Do not knowingly keep invalid synthetic SAF-T fixtures merely because the current parser can read them.
- When the official XSD changes or another SAF-T version is supported, preserve the schemas by version and select validation according to the SAF-T namespace/version. Never silently validate one SAF-T version against a different schema.
- Treat the repository XSD as an external specification asset: do not casually edit it to make an invalid SAF-T pass. If a compatibility issue is discovered between an XML/XSD engine and the official schema, document and test the compatibility handling separately while preserving the official schema unchanged.

## Repository reference SAF-T

- The repository contains the demonstration file `assets/saft_idemo599999999.xml` as a development reference for the concrete SAF-T (PT) structure used by the project.
- Consult this file whenever implementation work requires confirmation of actual element hierarchy, section placement, field names, namespaces, representative values or relationships in a SAF-T (PT) document instead of relying on memory or assumptions.
- Use the demonstration file as a structural and exploratory reference when implementing or reviewing parsers, mappings, persistence, navigation, accounting analyses and SAF-T-derived tests.
- Do not assume that the demonstration file exhaustively represents every valid SAF-T (PT) file, optional field, schema version or edge case. Implementation must remain namespace-aware and follow the supported SAF-T (PT) contract.
- Do not copy identifying or accounting values from the demonstration file into production defaults, logs, documentation examples or application behavior.
- Prefer small synthetic fixtures for focused automated unit tests. The demonstration file may be used for repository-level integration or regression tests when testing the real document structure materially improves coverage and its demonstrative status has been confirmed.
- If the demonstration asset is renamed, replaced or supplemented, inspect the current `assets/` directory before assuming its path or contents.

## Import boundary

- Treat every SAF-T file as untrusted external input and as sensitive business and accounting data.
- Detect the SAF-T namespace/schema version, select the matching repository XSD and validate the complete XML document before processing domain data.
- Reject malformed XML, XSD-invalid documents and unsupported structures with an actionable user-facing error.
- Never invent values for missing SAF-T fields. Preserve nullability or report validation failure according to the field contract.
- Preserve original source identifiers required to trace imported records back to the SAF-T dataset.
- Protect XML processing against external entity resolution and similar unsafe parser behavior.

## Parsing and persistence

- Keep XML/XSD validation, XML parsing, persistence and analytics as distinct responsibilities.
- Use namespace-aware XML parsing and do not assume a single namespace when supporting multiple SAF-T versions.
- Prefer streaming parsing for sections that may become large rather than loading an entire large file into memory without need.
- Parse dates and numeric values using invariant or source-defined formats rather than machine-local culture assumptions.
- Make imports transactional where persistence is involved so a failed import does not leave a partially imported dataset.
- Define duplicate-import behavior explicitly before persisting repeated files or source records.
- Use synthetic or anonymised SAF-T fixtures in tests and documentation.

## Accounting data

- Use `decimal` for monetary values and preserve source precision until a presentation or business rule requires rounding.
- Preserve debit/credit meaning, tax information, document dates, fiscal periods and source document relationships.
- Calculate KPIs and totals deterministically from imported data. Do not ask an LLM to calculate accounting totals that application code can calculate.

## Security and privacy

- Do not log complete SAF-T contents, taxpayer identifiers, addresses, customer or supplier details, or transaction descriptions unless explicitly required and appropriately protected.
- Do not send SAF-T contents to an external AI provider implicitly. Any external model integration must make the data boundary explicit and minimise transmitted data.
- Feed AI structured, minimised and purpose-specific context rather than raw SAF-T XML whenever possible.
- Clearly distinguish deterministic SAF-T facts and calculations from AI-generated summaries, classifications, explanations, anomalies or recommendations.

## Testing

- Validate the demonstration SAF-T and all synthetic SAF-T fixtures against `assets/saftpt1.04_01.xsd` as part of the relevant automated quality checks.
- Cover representative supported SAF-T versions and namespaces, malformed XML, XSD-invalid XML, unsupported versions, missing mandatory fields, duplicate data, invalid values, decimal precision, date parsing, boundary dates, transactional rollback, empty collections and large-file behavior as those capabilities are implemented.
- Never commit real taxpayer SAF-T data as a test fixture.
