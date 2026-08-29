---
name: tabularius-saft
description: Guide safe and deterministic SAF-T ingestion, validation, parsing, accounting calculations, and AI-assisted analysis in TabulariusAI.
---

# TabulariusAI SAF-T domain

Use this skill whenever working with SAF-T uploads, XML parsing, validation, accounting records, analytics, or AI-generated interpretations of SAF-T data.

## Source-of-truth rules

- Treat the uploaded SAF-T document as the source of truth for imported accounting data.
- Never invent missing values or silently repair ambiguous accounting records.
- Preserve original identifiers, document references, dates, tax identifiers, amounts, debit/credit semantics, and source relationships unless an explicit normalized representation is required.
- Keep deterministic extraction and calculations separate from AI-generated interpretation.
- AI must not calculate authoritative totals when the same value can be derived deterministically from structured SAF-T data.

## Parsing and validation

- Validate file type, size, XML readability, expected SAF-T structure, and supported schema/version before processing domain data.
- Use namespace-aware XML parsing and do not assume a single namespace when supporting multiple SAF-T versions.
- Handle malformed or unsupported files with clear user-facing errors rather than partial silent imports.
- Prefer streaming XML processing for potentially large files when full document loading is unnecessary.
- Use `decimal` for monetary values and preserve accounting precision until presentation formatting.
- Parse dates and numeric values using invariant/source-defined formats rather than machine-local culture assumptions.

## Security and privacy

- Treat SAF-T files as sensitive business and accounting data.
- Do not log complete SAF-T contents, taxpayer identifiers, addresses, customer/supplier details, or transaction descriptions unless explicitly required and appropriately protected.
- Protect XML processing against external entity resolution and similar unsafe parser behavior.
- Do not send SAF-T contents to an external AI provider implicitly. Any external model integration must make the data boundary explicit and minimize transmitted data.

## Analytics and AI

- Build KPIs, totals, reconciliations, tax summaries, customer/supplier statistics, and document counts deterministically from parsed data.
- Feed AI structured, minimized, purpose-specific context rather than raw XML whenever possible.
- Clearly distinguish facts calculated from SAF-T from AI summaries, classifications, explanations, anomalies, or recommendations.
- AI output must not be treated as accounting, tax, legal, or audit evidence without independent validation.

## Testing

- Cover supported SAF-T versions and namespaces with representative fixtures.
- Include malformed XML, missing mandatory sections, invalid values, large files, empty collections, decimal precision, and boundary dates.
- Use synthetic or anonymized fixtures in the repository; never commit real taxpayer SAF-T data.
