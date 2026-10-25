# AGENTS.md — AI Agent Guidance & Repository Rules

This document provides mandatory guidance for autonomous AI coding agents (Antigravity, Claude Code, GitHub Copilot, Cursor, Windsurf, Devin) operating in `akazad13/shopizy`.

---

## 1. System Identity & Architecture

**Product**: `Shopizy API` — Production-grade digital store management platform built with .NET 10, Clean Architecture, and Domain-Driven Design (DDD).
- **Backend**: .NET 10, C# 14, ASP.NET Core Minimal APIs, Entity Framework Core, SQL Server / PostgreSQL, Redis, SignalR.
- **Patterns**: CQRS via custom lightweight mediator (`IDispatcher`), Scrutor pipeline behaviors, NetArchTest architectural fitness tests, Testcontainers.
- **Solution File**: `Shopizy.sln`.

### Project Layout (`src/`):
- `src/Shopizy.Domain/`: Core domain entities (`Order`, `Product`, `Cart`, `Customer`), value objects, domain events, domain exceptions. Zero external dependencies.
- `src/Shopizy.Application/`: CQRS command & query handlers, validation behaviors (FluentValidation), event handlers.
- `src/Shopizy.Infrastructure/`: EF Core `ApplicationDbContext`, Redis caching layer, Stripe payment client, email/shipping integrations.
- `src/Shopizy.Api/`: Minimal API endpoint modules, middleware, SignalR hubs (`/hubs/orders`, `/hubs/admin-dashboard`).
- `tests/`: 1,100+ tests including `Shopizy.Domain.UnitTests`, `Shopizy.Application.UnitTests`, `Shopizy.Architecture.Tests` (NetArchTest), and `Shopizy.Integration.Tests` (Testcontainers).

---

## 2. Inviolable Architectural Invariants

1. **Clean Architecture Layering (Enforced by NetArchTest)**:
   - `Domain` must NOT reference `Application`, `Infrastructure`, or `Api`.
   - `Application` depends ONLY on `Domain`.
   - Any violation of layer dependency boundaries will fail `Shopizy.Architecture.Tests`.
2. **Banned Packages Policy**:
   - `Moq` is prohibited (use `NSubstitute`).
   - `AutoMapper` is prohibited (use explicit mapping or compile-time generators).
3. **Idempotency & Concurrency**:
   - Mutating API requests (`POST /api/orders`, payments) must honor the `X-Idempotency-Key` header.
   - Stock deduction must use atomic database operations or row-level locking to prevent overselling.
4. **No Secrets in Tracked Code**:
   - Database connection strings, Stripe API keys, and JWT secrets must be loaded from user-secrets or environment variables.

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
