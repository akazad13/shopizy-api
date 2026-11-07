# AGENTS.md — AI Agent Guidance & Repository Rules

This document provides mandatory guidance for autonomous AI coding agents (Antigravity, Claude Code, GitHub Copilot, Cursor, Windsurf, Devin) operating in `akazad13/shopizy`.

All architectural decisions and evolutionary records are formally indexed in [docs/adr/README.md](docs/adr/README.md).

---

## 1. System Identity & Architecture

**Product**: `Shopizy API` — Production-grade digital store management platform built with .NET 10, Clean Architecture, and Domain-Driven Design (DDD).
- **Backend**: .NET 10, C# 14, ASP.NET Core Minimal APIs, Entity Framework Core, SQL Server / PostgreSQL, Redis, SignalR.
- **Patterns**: CQRS via custom lightweight mediator (`IDispatcher`), Scrutor pipeline behaviors, NetArchTest architectural fitness tests, Testcontainers.
- **Solution File**: `Shopizy.sln`.
- **Decision Records**: [docs/adr/README.md](docs/adr/README.md) (ADR-0000, ADR-0001, ADR-0002).

### Project Layout (`src/`):
- `src/Shopizy.Domain/`: Core domain entities (`Order`, `Product`, `Cart`, `Customer`), value objects, domain events, domain exceptions. Zero external dependencies.
- `src/Shopizy.Application/`: CQRS command & query handlers, validation behaviors (FluentValidation), event handlers. Depends strictly on `Shopizy.Domain` and `Shopizy.SharedKernel`.
- `src/Shopizy.Infrastructure/`: EF Core `ApplicationDbContext`, Redis caching layer, Stripe payment client, email/shipping integrations, background Outbox processors.
- `src/Shopizy.Api/`: Minimal API endpoint modules, middleware, SignalR hubs (`/hubs/orders`, `/hubs/admin-dashboard`).
- `src/Shopizy.Contracts/`: Transport DTO records shared with client consumers.
- `src/Shopizy.SharedKernel/`: Core domain and application primitives (`Entity`, `AggregateRoot`, `ValueObject`, `IDispatcher`, `ErrorOr`).
- `tests/`: 1,164+ tests including `Shopizy.Domain.UnitTests`, `Shopizy.Application.UnitTests`, `Shopizy.Contracts.UnitTests`, `Shopizy.Infrastructure.UnitTests`, `Shopizy.Architecture.Tests` (NetArchTest), and `Shopizy.Api.IntegrationTests` (Testcontainers).

---

## 2. Inviolable Architectural Invariants

Agents must adhere to all architectural invariants. Any modification violating these rules will break architectural fitness tests (`Shopizy.Architecture.Tests`) or transactional safety:

1. **Architecture Decision Records (ADR-0000)**:
   - Significant architectural choices, framework/library upgrades, and structural patterns must be documented via Architecture Decision Records in [docs/adr/README.md](docs/adr/README.md).
2. **Clean Architecture Layering (Enforced by NetArchTest)**:
   - `Domain` must NOT reference `Application`, `Infrastructure`, or `Api`.
   - `Application` depends ONLY on `Domain` and `SharedKernel`.
   - `Infrastructure` implements ports/interfaces defined in `Application` or `Domain`.
   - Layer boundary rules are enforced automatically by `Shopizy.Architecture.Tests`.
3. **CQRS & Messaging Pipeline (`IDispatcher`)**:
   - The solution uses a lightweight custom mediator (`IDispatcher`) instead of MediatR.
   - Commands implement `ICommand<TResult>` and return `ErrorOr<TResult>`.
   - Queries implement `IQuery<TResult>` and return DTO read models.
   - Handlers are decorated via Scrutor in order: `ValidationBehavior`, `UnitOfWorkBehavior` (auto-commits `SaveChangesAsync`), `CachingBehavior`, and `CacheInvalidationBehavior`.
4. **Transactional Outbox & In-Transaction Event Dispatching**:
   - Domain events raised via `Entity.AddDomainEvent` are persisted atomically into `OutboxMessages` within `AppDbContext.SaveChangesAsync`.
   - Domain event handlers are dispatched **inside** the open EF transaction via `EventualConsistencyMiddleware`. If any handler fails, the entire transaction rolls back.
   - Background `OutboxProcessor` acts as a backstop for rare crash-mid-commit scenarios, sweeping rows older than 5 minutes.
5. **Two-Tiered Caching Invariants**:
   - **Pipeline Cache**: Queries implement `ICachableQuery` handled by `CachingBehavior` via Redis `ICacheHelper`. Commands mutating state implement `IInvalidateCache` for post-commit eviction via `CacheInvalidationBehavior`.
   - **Second-Level Cache**: EF Core queries on read-heavy entities (`Brand`, `Category`, `ProductReader`) use `.Cacheable()` with automatic table-level invalidation.
6. **Concurrency & Optimistic Locking**:
   - Concurrency tokens (`RowVersion` `byte[]`) protect against race conditions on `Product`, `Cart`, `Order`, and `User`. Collisions throw `DbUpdateConcurrencyException`, returning HTTP 409 Conflict.
   - Stock deduction must use atomic database operations or row-level locking to prevent overselling.
7. **Idempotency Invariants**:
   - Mutating API endpoints (`POST /api/orders`, payments) must honor the `X-Idempotency-Key` header, stored with 24-hour TTL in Redis via `IIdempotencyStore`.
8. **Banned Packages Policy (ADR-0002)**:
   - `Moq` is strictly prohibited in new unit tests (use `NSubstitute`).
   - `AutoMapper` is prohibited (use explicit mapping or Mapster).
9. **Secrets & Configuration Security**:
   - Zero secrets in source control. Connection strings, Stripe API keys, and JWT secrets must be loaded from user-secrets or environment variables.
10. **Stripe.net Modernization Convention (ADR-0001)**:
    - When configuring Stripe `PaymentIntentCreateOptions`, use `AllowedPaymentMethodTypes` instead of the obsolete `PaymentMethodTypes` property.

---

## 3. Testing Paradigm & Agentic Verification (ADR-0002)

To maintain rapid developer/agent velocity, high confidence, and zero regression, testing adheres to [ADR-0002](docs/adr/0002-testing-paradigm-and-agentic-verification.md):

### Test Pyramid Distribution
- **~85% Fast Unit Tests**: Domain entities/value objects, Application CQRS handlers, and validation rules (`Shopizy.Domain.UnitTests`, `Shopizy.Application.UnitTests`, `Shopizy.Contracts.UnitTests`, `Shopizy.Infrastructure.UnitTests`). In-memory only, no I/O, test doubles via NSubstitute.
- **~10% Architecture Tests**: Automated Clean Architecture layer boundary and dependency enforcement via NetArchTest (`Shopizy.Architecture.Tests`) plus compile-time Mapster config verification.
- **~5% Integration Tests**: ASP.NET Core `WebApplicationFactory` + Testcontainers (real SQL Server 2022 and Redis in `Shopizy.Api.IntegrationTests`). Reserved for EF migrations, transactions, concurrency, and end-to-end workflows.

### Testing Invariants
- **State Over Interaction Verification**: Assert on resulting state and returned outputs (`ErrorOr<T>`, entity properties, domain events), not internal mock method call counts.
- **DAMP Over DRY**: Tests favor Descriptive And Meaningful Phrases. Every test is a self-contained Arrange-Act-Assert specification.
- **Prove-It Bug Reproduction Pattern**: Always write a failing reproduction test (RED) before writing code to fix a bug (GREEN), followed by clean refactoring (REFACTOR).
- **Zero Moq / Zero AutoMapper**: New test doubles must use NSubstitute; mappings must use Mapster or explicit code.

### Fast Agentic Test Loops
Agents must use focused test execution during inner development loops rather than waiting on the full suite:
```bash
# 1. Inner loop: Single focused test or test class (~1-3s)
dotnet test tests/Shopizy.Domain.UnitTests --filter "FullyQualifiedName~OrderTests"
dotnet test tests/Shopizy.Application.UnitTests --filter "FullyQualifiedName~CreateOrderCommandHandlerTests"

# 2. Sub-suite loop: Target project unit tests (~3-5s)
dotnet test tests/Shopizy.Domain.UnitTests
dotnet test tests/Shopizy.Application.UnitTests

# 3. Architecture fitness gate (~2s)
dotnet test tests/Shopizy.Architecture.Tests

# 4. Pre-completion unit & architecture gate (excludes heavy Testcontainers)
dotnet test Shopizy.sln --filter "FullyQualifiedName!~IntegrationTests"
```

---

## 4. Commands Cheat Sheet

```bash
# Build solution
dotnet build Shopizy.sln

# Run all 1,164 tests (requires Docker for integration tests)
dotnet test Shopizy.sln

# Run all unit and architecture tests (fast, no Docker needed)
dotnet test Shopizy.sln --filter "FullyQualifiedName!~IntegrationTests"

# Run architectural fitness tests only
dotnet test tests/Shopizy.Architecture.Tests

# Run API locally
dotnet run --project src/Shopizy.Api/Shopizy.Api.csproj

# Run Stryker mutation tests
dotnet stryker
```

---

## 5. Verification Gates for AI Agents

Before submitting code changes or finishing a turn:
1. **Inner Loop**: Run focused unit tests for the affected project (`dotnet test tests/<Project>.UnitTests`).
2. **Architecture Gate**: Run `dotnet test tests/Shopizy.Architecture.Tests` — must pass with zero layer boundary violations.
3. **Pre-Completion Gate**: Run `dotnet test Shopizy.sln --filter "FullyQualifiedName!~IntegrationTests"` to ensure no regression across unit and architecture suites.
4. If modifying database schema, migrations, or HTTP endpoints, verify with `dotnet test tests/Shopizy.Api.IntegrationTests`.
5. If introducing architectural changes, consult and update [docs/adr/README.md](docs/adr/README.md).
