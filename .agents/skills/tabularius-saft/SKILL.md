---
name: tabularius-saft
description: Implement and review Portuguese SAF-T import, validation, mapping and analytical processing in Tabularius AI.
---

# Tabularius AI SAF-T processing

Use this skill whenever work affects SAF-T XML ingestion, validation, domain mapping, accounting data or derived analytics.

## Import boundary

- Treat every SAF-T file as untrusted external input.
- Detect and validate the supported SAF-T namespace/schema version before processing domain data.
- Reject malformed XML and unsupported structures with an actionable user-facing error.
- Never invent values for missing SAF-T fields. Preserve nullability or report validation failure according to the field contract.
- Preserve original source identifiers required to trace imported records back to the SAF-T dataset.

## Parsing and persistence

- Keep XML parsing separate from persistence and analytics.
- Prefer streaming parsing for sections that may become large rather than loading an entire large file into memory without need.
- Make imports transactional where persistence is involved so a failed import does not leave a partially imported dataset.
- Define duplicate-import behavior explicitly before persisting repeated files or source records.
- Use synthetic or anonymised SAF-T fixtures in tests and documentation.

## Accounting data

- Use `decimal` for monetary values and preserve source precision until a presentation or business rule requires rounding.
- Preserve debit/credit meaning, tax information, document dates, fiscal periods and source document relationships.
- Calculate KPIs and totals deterministically from imported data. Do not ask an LLM to calculate accounting totals that application code can calculate.

## Testing

Cover representative valid files, malformed XML, unsupported versions, missing fields, duplicate data, decimal/date parsing, transactional rollback and large-file behavior as those capabilities are implemented.
