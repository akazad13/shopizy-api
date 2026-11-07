# AGENTS.md — AI Agent Guidance & Repository Rules

This document provides mandatory guidance for autonomous AI coding agents (Antigravity, Claude Code, GitHub Copilot, Cursor, Windsurf, Devin) operating in `akazad13/shopizy`.

All architectural decisions and evolutionary records are formally indexed in [docs/adr/README.md](docs/adr/README.md).

---

## 1. System Identity & Architecture

**Product**: `Shopizy API` — Production-grade digital store management platform built with .NET 10, Clean Architecture, and Domain-Driven Design (DDD).
- **Backend**: .NET 10, C# 14, ASP.NET Core Minimal APIs, Entity Framework Core, SQL Server / PostgreSQL, Redis, SignalR.
- **Patterns**: CQRS via custom lightweight mediator (`IDispatcher`), Scrutor pipeline behaviors, NetArchTest architectural fitness tests, Testcontainers.
- **Solution File**: `Shopizy.sln`.
- **Decision Records**: [docs/adr/README.md](docs/adr/README.md) (ADR-0000, ADR-0001).

### Project Layout (`src/`):
- `src/Shopizy.Domain/`: Core domain entities (`Order`, `Product`, `Cart`, `Customer`), value objects, domain events, domain exceptions. Zero external dependencies.
- `src/Shopizy.Application/`: CQRS command & query handlers, validation behaviors (FluentValidation), event handlers. Depends strictly on `Shopizy.Domain` and `Shopizy.SharedKernel`.
- `src/Shopizy.Infrastructure/`: EF Core `ApplicationDbContext`, Redis caching layer, Stripe payment client, email/shipping integrations, background Outbox processors.
- `src/Shopizy.Api/`: Minimal API endpoint modules, middleware, SignalR hubs (`/hubs/orders`, `/hubs/admin-dashboard`).
- `src/Shopizy.Contracts/`: Transport DTO records shared with client consumers.
- `src/Shopizy.SharedKernel/`: Core domain and application primitives (`Entity`, `AggregateRoot`, `ValueObject`, `IDispatcher`, `ErrorOr`).
- `tests/`: 1,164+ tests including `Shopizy.Domain.UnitTests`, `Shopizy.Application.UnitTests`, `Shopizy.Architecture.Tests` (NetArchTest), and `Shopizy.Integration.Tests` (Testcontainers).

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
8. **Banned Packages Policy**:
   - `Moq` is strictly prohibited in new unit tests (use `NSubstitute`).
   - `AutoMapper` is prohibited (use explicit mapping or Mapster).
9. **Secrets & Configuration Security**:
   - Zero secrets in source control. Connection strings, Stripe API keys, and JWT secrets must be loaded from user-secrets or environment variables.
10. **Stripe.net Modernization Convention (ADR-0001)**:
    - When configuring Stripe `PaymentIntentCreateOptions`, use `AllowedPaymentMethodTypes` instead of the obsolete `PaymentMethodTypes` property.

---

## 3. Commands Cheat Sheet

```bash
# Build solution
dotnet build Shopizy.sln

# Run all 1,164 tests
dotnet test Shopizy.sln

# Run architectural fitness tests only
dotnet test tests/Shopizy.Architecture.Tests

# Run API locally
dotnet run --project src/Shopizy.Api/Shopizy.Api.csproj

# Run Stryker mutation tests
dotnet stryker
```

---

## 4. Verification Gates for AI Agents

Before submitting code changes or finishing a turn:
1. Run `dotnet test Shopizy.sln` — all tests must pass.
2. Ensure `Shopizy.Architecture.Tests` pass with zero layer boundary violations.
3. If introducing architectural changes, consult and update [docs/adr/README.md](docs/adr/README.md).
