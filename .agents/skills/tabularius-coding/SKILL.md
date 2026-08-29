---
name: tabularius-coding
description: Apply Tabularius AI C# coding, architecture and documentation standards to every implementation change.
---

# Tabularius AI coding standards

Use this skill whenever creating, modifying or reviewing C# code in Tabularius AI.

## XML documentation

- Write XML documentation comments in English.
- Every class, interface, record and enum must have a `/// <summary>` describing its responsibility.
- Every method or function must have a `/// <summary>` describing what it does, including private methods when they are explicitly declared.
- Add `<param>` for every parameter when the method has parameters.
- Add `<returns>` when a method returns a value or task result whose meaning is not self-evident.
- Add `<typeparam>` for generic type parameters.
- Add `<exception>` when an exception is intentionally part of the method contract.
- Document public properties when they expose domain, configuration, persistence or API meaning.
- Keep comments useful and concise. Explain intent and contract rather than repeating the identifier name.

## C# conventions

- Enable nullable reference types and implicit usings.
- Treat compiler and analyzer warnings as errors.
- Prefer asynchronous I/O APIs and propagate `CancellationToken` through database, file and AI operations.
- Use dependency injection and constructor injection for services.
- Keep controllers thin; business logic belongs in application/domain services.
- Use `decimal` for monetary calculations.
- Prefer immutable DTOs/records when mutation is not required.
- Validate arguments and external data at system boundaries.

## Architecture

- Keep SAF-T parsing, persistence, analytics, presentation and AI integration separated.
- Do not couple accounting calculations to Mistral or another model provider.
- Access AI through an abstraction such as `IAIService`.
- Keep configurable operational prompts and model settings outside hard-coded business logic.
- Avoid introducing dependencies without a clear project-level benefit.

## Review

Before considering C# work complete, verify that every new or modified class and method satisfies the XML documentation requirement and that the solution builds without warnings.
