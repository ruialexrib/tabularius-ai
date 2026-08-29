---
name: tabularius-saft
description: Implement and review Portuguese SAF-T import, validation, mapping and analytical processing in Tabularius AI.
---

# Tabularius AI SAF-T processing

Use this skill whenever work affects SAF-T XML ingestion, validation, domain mapping, accounting data or derived analytics.

## Import boundary

- Treat every SAF-T file as untrusted external input and as sensitive business and accounting data.
- Detect and validate the supported SAF-T namespace/schema version before processing domain data.
- Reject malformed XML and unsupported structures with an actionable user-facing error.
- Never invent values for missing SAF-T fields. Preserve nullability or report validation failure according to the field contract.
- Preserve original source identifiers required to trace imported records back to the SAF-T dataset.
- Protect XML processing against external entity resolution and similar unsafe parser behavior.

## Parsing and persistence

- Keep XML parsing separate from persistence and analytics.
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

Cover representative supported SAF-T versions and namespaces, malformed XML, unsupported versions, missing mandatory fields, duplicate data, invalid values, decimal precision, date parsing, boundary dates, transactional rollback, empty collections and large-file behavior as those capabilities are implemented. Never commit real taxpayer SAF-T data as a test fixture.
