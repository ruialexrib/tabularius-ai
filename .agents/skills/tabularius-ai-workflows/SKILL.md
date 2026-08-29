---
name: tabularius-ai-workflows
description: Build and review Tabularius AI Mistral integration, prompts, structured responses and SAF-T analytical assistant workflows.
---

# Tabularius AI AI workflows

Use this skill for Mistral integration, prompts, assistant behavior, generated insights and structured model responses.

## Provider boundary

- Access model functionality through an application abstraction such as `IAIService`.
- Keep the first provider implementation Mistral-specific without coupling controllers, analytics or SAF-T domain services directly to Mistral APIs.
- Keep model name, endpoint behavior and operational prompts configurable.
- Never commit API keys or expose credentials in prompts, logs or user-visible errors.

## Data and calculation boundary

- Calculate accounting totals, tax totals, document counts, period comparisons, ratios and other deterministic metrics in application code or database queries before model invocation.
- Use the model to explain, summarise, classify, identify patterns or propose interpretations.
- Supply bounded and explicit SAF-T context rather than unrestricted database content.
- Do not send more accounting data to an external provider than is required for the requested operation.

## Structured output

- Require structured JSON when the application needs machine-readable model output and validate it before use.
- Reject malformed, incomplete or inconsistent responses safely.
- Do not invent missing SAF-T facts to make an AI response appear complete.
- Distinguish deterministic metrics from AI-generated interpretations in the UI.

## Language

Use European Portuguese for prompts and model responses intended for Portuguese-speaking users unless a specific workflow requires another language. Keep developer-facing code and XML documentation in English.

## Verification

Test success, malformed responses, timeout/cancellation, provider errors, missing configuration and representative context sizes. Compare any numerical facts supplied to the model with deterministic application results.
