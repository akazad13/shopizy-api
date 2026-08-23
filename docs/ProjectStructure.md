# Shopizy Project Structure Reference

Last updated: 2026-08-23

This document is a quick map of the repository to guide safe, targeted changes.
Use it before editing to choose the right layer and keep boundaries clear.

## Solution Overview

```
shopizy/
├─ Shopizy.sln
├─ Directory.Build.props
├─ Directory.Packages.props
├─ README.md
├─ docs/
├─ src/
├─ tests/
└─ Bruno/
```

## Root-Level Files and Folders

- `Shopizy.sln`: Solution entry point.
- `Directory.Build.props`: Shared MSBuild settings.
- `Directory.Packages.props`: Centralized NuGet package versions.
- `README.md`: General setup and usage.
- `docs/`: Project documentation.
- `src/`: Production code.
- `tests/`: Unit and integration tests.
- `Bruno/`: API collection assets for manual testing.

## Source Projects (`src/`)

### `src/Shopizy.Api`

API host and HTTP edge of the system.

- `Program.cs`: App startup and middleware wiring.
- `DependencyInjectionRegister.cs`: API-level service registrations.
- `Endpoints/`: HTTP endpoints grouped by feature.
- `Common/`: Shared API concerns.
- `appsettings*.json`: Environment-specific settings.

### `src/Shopizy.Application`

Application layer use cases and orchestration.

- Feature folders: `Admin/`, `Auth/`, `Products/`, `Orders/`, `Users/`, etc.
- `Common/`: Shared application patterns/utilities.
- `DependencyInjectionRegister.cs`: Application service wiring.

### `src/Shopizy.Contracts`

DTOs/contracts shared across boundaries.

- Feature folders mirror business modules (`Product/`, `Order/`, `User/`, etc.).
- Keep transport models here, not domain behavior.

### `src/Shopizy.Domain`

Core business model and domain rules.

- Feature folders: `Products/`, `Orders/`, `PromoCodes/`, etc.
- `Common/`: Domain base abstractions/shared primitives.
- `Permissions/`: Authorization-related domain concepts.

### `src/Shopizy.Infrastructure`

Technical implementations for external concerns.

- `DependencyInjection/` and `DependencyInjectionRegister.cs`: Infra wiring.
- `Migrations/`: Database migrations.
- `ExternalServices/`: Third-party integrations.
- `Messaging/` and `Outbox/`: Messaging and reliability patterns.
- `Security/`, `Services/`, `Common/`: Cross-cutting infrastructure code.
- `RequestPipeline.cs`, `LoggerMessages.cs`: Pipeline/logging helpers.

### `src/Shopizy.SharedKernel`

Cross-cutting shared abstractions used by multiple projects.

- `Application/`: Shared application-level primitives.
- `Domain/`: Shared domain-level primitives.

## Tests (`tests/`)

### `tests/Shopizy.Api.IntegrationTests`

End-to-end API behavior using `WebApplicationFactory<Program>` and a Testcontainers-managed SQL Server 2022 container. `IntegrationTestWebAppFactory` runs real EF migrations on startup, mocks `IPaymentService` / `IMediaUploader`, and registers in-memory replacements for the Redis-backed cache, idempotency store, and refresh-token store.

- `BaseIntegrationTest.cs` — auth helpers, default `Idempotency-Key` header
- `IntegrationTestWebAppFactory.cs` — container + DI overrides
- Feature-oriented test folders (`Products/`, `Orders/`, `Auth/`, etc.)

### `tests/Shopizy.Application.UnitTests`

Unit tests for application handlers/services by feature.

### `tests/Shopizy.Domain.UnitTests`

Unit tests for domain behavior and invariants.

### `tests/Shopizy.Infrastructure.UnitTests`

Unit tests for infrastructure services, security, and integrations.

### `tests/Shopizy.Architecture.Tests`

Architecture/dependency-direction tests via `NetArchTest.Rules`, plus a Mapster compile smoke-test. Asserts: Domain depends on no outer layer; Application stays out of Infrastructure/Api; Infrastructure stays out of Api; Contracts stays standalone.

## API Collections (`Bruno/`)

### `Bruno/Shopizy`

- `collection.bru`: Main API collection.
- `environments/`: Environment configurations.
- Feature folders (for example `Product/`, `User/`) for endpoint request sets.

## Existing Docs (`docs/`)

- `RepositoryMemory.md`: High-density map of domain aggregates, messaging pipelines, security, and developer quick reference.
- `Api.md`: Pointer to the live Swagger UI, OpenAPI spec, and contract versioning rules.
- `Domain.md`: Aggregate map, state machines, domain events, and concurrency tokens.
- `EventualConsistency.md`: Outbox / domain-event design and contributor rules.
- `FeatureDocumentation.md`: End-to-end guide of all platform features, workflows, and business logic.
- `FrontendHandoffDoc.md`: Frontend client specification, TypeScript interfaces, and SignalR WebSocket setup.
- `ImprovementScope.md`: Full-solution architectural audit and completed roadmap.
- `ProjectStructure.md`: This structure reference guide.
- `TechnicalDocumentation.md`: Architectural patterns, DDD design, CQRS messaging, caching, and infrastructure.
- `ThreatModel.md`: STRIDE-style review of the API surface.

## Change Routing Checklist

When making a change, route work using this quick mapping:

1. API route/contract handling -> `Shopizy.Api` and possibly `Shopizy.Contracts`.
2. Use case/business flow -> `Shopizy.Application`.
3. Business rules/invariants/entities -> `Shopizy.Domain`.
4. Persistence/external systems/security plumbing -> `Shopizy.Infrastructure`.
5. Shared abstractions -> `Shopizy.SharedKernel`.
6. Add or update tests under matching project in `tests/`.

## Notes for Ongoing Maintenance

- Keep this file updated when adding/removing major feature folders or projects.
- Prefer feature-aligned changes across `Api`, `Application`, `Domain`, `Infrastructure`, and tests.
- Avoid adding behavior to `Contracts`; keep it for transport models only.