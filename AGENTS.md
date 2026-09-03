# Repository Guidelines

## Project Structure & Module Organization

`RouteFlow.slnx` is the solution entry point. Production code lives under `src/`: `Bootstrapper/RouteFlow.Api` hosts the HTTP API, `Orchestration/RouteFlow.AppHost` provides .NET Aspire orchestration, and `Shared` plus `SharedKernel` contain cross-cutting primitives. Business capabilities are grouped in `src/Modules`; the current `Deliveries` module is split into `Domain`, `Application`, `Contracts`, and `Infrastructure` projects. Tests mirror these boundaries under `tests/`. Architectural and business decisions belong in `docs/`.

Keep domain rules in aggregates and value objects, application workflows in command handlers, contracts free of implementation details, and infrastructure concerns out of the domain project.

## Build, Test, and Development Commands

- `dotnet restore RouteFlow.slnx` restores NuGet dependencies.
- `dotnet build RouteFlow.slnx` compiles the full solution; resolve warnings, especially in projects that treat them as errors.
- `dotnet test RouteFlow.slnx` runs all xUnit unit and architecture tests.
- `dotnet test tests/RouteFlow.Deliveries.UnitTests --collect:"XPlat Code Coverage"` collects coverage for domain tests.
- `dotnet run --project src/Bootstrapper/RouteFlow.Api` starts the API directly.
- `dotnet run --project src/Orchestration/RouteFlow.AppHost` starts the Aspire host.

## Coding Style & Naming Conventions

Use standard C# formatting with four-space indentation, file-scoped namespaces, nullable reference types, and implicit usings. Use `PascalCase` for types, methods, and public members; `camelCase` for parameters and locals; and `_camelCase` for private fields. Keep one primary type per file and match its filename. Prefer domain-specific types such as `DeliveryId` over primitive identifiers. Run `dotnet format RouteFlow.slnx --verify-no-changes` before submitting formatting-sensitive changes.

## Testing Guidelines

Tests use xUnit; application tests also use NSubstitute, while architecture tests use ArchUnitNET. Name test classes after the behavior under test and methods with the pattern `Method_Scenario_ExpectedResult`. Follow Arrange/Act/Assert where useful. Add tests for successful transitions, invalid states, emitted domain events, and dependency-boundary changes. No numeric coverage threshold is configured; cover every changed rule and regression.

## Commit & Pull Request Guidelines

Recent history uses concise, lowercase imperative subjects, for example `add delivery status management...`. Keep each commit focused and explain non-obvious design decisions in its body. Pull requests should summarize behavior, list validation commands, link relevant issues, and call out architecture or domain-rule changes. Include screenshots or request/response examples when API-visible behavior changes. Never commit secrets; use user secrets or environment-specific configuration instead of values in `appsettings*.json`.
