# Shopizy — Technical Architecture & Implementation Reference

[⬅️ Back to README](../README.md) · [Repository Memory](RepositoryMemory.md) · [Feature Docs](FeatureDocumentation.md) · [API](Api.md)

This document serves as the comprehensive technical guide for developers, architects, and DevOps engineers working on the **Shopizy** codebase.

---

## 🏛️ 1. Architecture Overview

Shopizy is designed around **Clean Architecture** and **Domain-Driven Design (DDD)** principles, separating business logic from infrastructure and UI concerns.

```
┌────────────────────────────────────────────────────────┐
│                      Shopizy.Api                       │
│      (Minimal Endpoints, Middleware, Swagger, Hubs)    │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│                  Shopizy.Application                   │
│   (CQRS Commands, Queries, Domain Event Handlers, DTOs)│
└─────────────┬───────────────────────────┬──────────────┘
              │                           │
┌─────────────▼───────────────┐ ┌─────────▼──────────────┐
│       Shopizy.Domain        │ │  Shopizy.Infrastructure│
│(Aggregates, Value Objects,  │ │ (EF Core, Redis, Hubs, │
│ Domain Events, Invariants)  │ │  External Services, DI)│
└─────────────────────────────┘ └────────────────────────┘
              │                           │
┌─────────────▼───────────────────────────▼──────────────┐
│                  Shopizy.SharedKernel                  │
│  (Base Entity, AggregateRoot, IDispatcher, Result/Error)│
└────────────────────────────────────────────────────────┘
```

---

## 📦 2. Project & Layer Structure

| Project | Purpose & Responsibilities | Key Technologies |
|---|---|---|
| **`Shopizy.Domain`** | Enterprise business rules, aggregate roots, domain events, entities, value objects, domain errors. Pure C# with zero external dependencies. | C# 13 / .NET 10 |
| **`Shopizy.Application`** | Application business rules, CQRS command & query handlers, validation, domain event handlers, external service interfaces. | FluentValidation, ErrorOr, Mapster |
| **`Shopizy.Infrastructure`** | Database persistence (EF Core), Redis caching & idempotency, SignalR hubs, Stripe, Cloudinary, background workers, outbox processor. | EF Core 10, StackExchange.Redis, SignalR, Stripe.net |
| **`Shopizy.Api`** | Presentation layer hosting REST minimal API endpoints, SignalR hubs, authentication/authorization middleware, OpenTelemetry, Swagger. | ASP.NET Core Minimal APIs |
| **`Shopizy.Contracts`** | Data contract models for HTTP requests and responses. Shared with client SDKs. | C# Records |
| **`Shopizy.SharedKernel`** | Base primitives (`AggregateRoot`, `Entity`, `ValueObject`, `IDomainEvent`, `IDispatcher`, `IUnitOfWork`). | Reusable kernel |

---

## 🎯 3. Domain-Driven Design (DDD) Patterns

### Aggregate Roots
All core domain models are encapsulated as Aggregate Roots to safeguard domain invariants:
- **`Product`**: Controls catalog data, stock levels (`ReduceStock`, `IncreaseStock`), rating aggregations, and raises `ProductPriceDroppedDomainEvent` and `ProductBackInStockDomainEvent`.
- **`Order`**: Manages order lifecycle transitions, line items, shipments, payment associations, cancellation reasons, and raises `OrderCreatedDomainEvent` and `OrderCancelledDomainEvent`.
- **`Cart`**: Enforces cart modifications, quantity limits, and tracks inactivity timestamps (`LastAbandonedReminderSentOn`).
- **`PromoCode`**: Evaluates discount eligibility, BOGO algorithms, tiered minimum spend checks, category restrictions, and usage limits.
- **`ProductReview`**: Validates customer purchase verification, maintains helpful upvote tallies, and stores review photo URLs.
- **`LoyaltyAccount`**, **`GiftCard`**, **`User`**, **`Wishlist`**, **`AuditLog`**.

### Value Objects
Immutable value objects with structural equality:
- Strongly typed IDs: `ProductId`, `OrderId`, `UserId`, `CategoryId`, `BrandId`, `ShipmentId`, `PaymentId`.
- Complex types: `Price` (Amount + Currency enum), `Address` (Street, City, State, Country, ZipCode), `AverageRating`.

---

## ⚡ 4. CQRS & Messaging Pipeline

Shopizy utilizes a lightweight, handrolled mediator and dispatching pipeline (`Shopizy.SharedKernel.Application.Messaging`):

### Components
1. **`ICommand<TResult>` / `ICommandHandler<TCommand, TResult>`**: State-modifying operations returning `ErrorOr<TResult>`.
2. **`IQuery<TResult>` / `IQueryHandler<TQuery, TResult>`**: Read-only operations retrieving data transfer objects.
3. **`IDomainEvent` / `IDomainEventHandler<TEvent>`**: Internal notification handlers triggered on entity state changes.
4. **`IDispatcher`**: Automatically dispatches commands, queries, and domain events to their respective registered handlers using DI assembly scanning via **Scrutor**.

---

## 🗄️ 5. Data Persistence & EF Core Architecture

### Persistence Highlights
- **Entity Framework Core 10:** Configured for Microsoft SQL Server with strict migrations.
- **Optimistic Concurrency:** Protects high-concurrency entities (`Product`, `User`, `Cart`) against race conditions using row-version byte tokens (`RowVersion`).
- **Backing Field Encapsulation:** Private collection backing fields (e.g. `_imageUrls`, `_productImages`, `_cartItems`) ensure entity mutators are never bypassed.
- **Automatic Snapshot Synchronization:** Every schema modification is recorded in migrations and synchronized with `AppDbContextModelSnapshot.cs`.

### Outbox Pattern & Eventual Consistency
- State modifications and domain events are committed atomically within a single SQL transaction to the `OutboxMessages` table.
- A dedicated background worker (`OutboxProcessor`) periodically processes pending outbox messages, guaranteeing at-least-once event delivery.
- Poison messages exceeding retry thresholds are routed to `OutboxDeadLetters` with full error stack traces.

---

## ⚡ 6. Caching, Idempotency & Distributed State

### Redis Integration
- **`ICacheHelper` (`RedisCacheHelper`):** Distributed caching for hot catalog reads and search results with configurable TTLs.
- **`IIdempotencyStore` (`RedisIdempotencyStore`):** Prevents duplicate API executions on payment processing and order submissions.
- **`IRefreshTokenStore` (`RedisRefreshTokenStore`):** Stores sliding refresh tokens with automatic revocation upon token rotation.

---

## 🔄 7. Real-Time Operations (SignalR)

Shopizy hosts two SignalR hubs mapped to dedicated endpoint routes:

1. **`OrderStatusHub` (`/hubs/orders`):**
   - Secured with JWT authorization.
   - On connection, clients join private user groups: `user-{userId}`.
   - Client event: `ReceiveOrderStatusUpdate` (`OrderId`, `Status`, `TimestampUtc`).

2. **`AdminDashboardHub` (`/hubs/admin-dashboard`):**
   - Secured with `[Authorize(Roles = "Admin")]`.
   - On connection, administrators join the `Admins` group.
   - Client event: `ReceiveMetricUpdate` (`MetricType`, `Data`, `TimestampUtc`).

---

## 🔍 8. Product Search Engine

The `ProductSearchEngine` service implements a multi-stage search and ranking algorithm:
1. **Token Extraction:** Tokenizes user queries into distinct keywords.
2. **Synonym Expansion:** Resolves e-commerce synonyms (e.g. `phone` $\to$ `smartphone`, `mobile`, `iphone`, `android`).
3. **Scoring & Fuzzy Distance:** Scores candidate products using exact matches, partial matches, and Levenshtein distance ($dist \le 2$).
4. **Facet Computation:** Generates dynamic facet counts for Categories, Brands, Price Tiers, and Rating Tiers across matching results.
5. **Keyword Suggestions:** Generates "Did You Mean?" keyword recommendations for misspelled queries.

---

## 🛡️ 9. Performance, Telemetry & Code Quality Standards

### Zero-Allocation Logging (CA1848 Compliant)
All logging across the Application, Infrastructure, and API layers uses high-performance compile-time source-generated logging delegates:
```csharp
[LoggerMessage(
    EventId = 1050,
    Level = LogLevel.Error,
    Message = "An error occurred while estimating shipping rates."
)]
public static partial void ShippingRateEstimationError(this ILogger logger, Exception ex);
```

### OpenTelemetry Distributed Tracing
- Full OpenTelemetry integration instrumenting **ASP.NET Core HTTP**, **HttpClient**, **EF Core SQL Queries**, and **StackExchange.Redis**.
- Exporters configured for OpenTelemetry Protocol (OTLP) and Console logging.

---

## 🧪 10. Testing Strategy & CI/CD Verification

Shopizy maintains a comprehensive test pyramid with **1,173+ automated tests**:

```
                  ┌──────────────────────┐
                  │ Integration Tests    │  (ASP.NET Core TestHost +
                  │ (Shopizy.Api.Tests)  │   Testcontainers MsSql & Docker)
                  └──────────┬───────────┘
                             │
                  ┌──────────▼───────────┐
                  │ Architecture Tests   │  (NetArchTest Layer Isolation,
                  │ (Architecture.Tests) │   Naming & Dependency Rules)
                  └──────────┬───────────┘
                             │
                  ┌──────────▼───────────┐
                  │ Unit Test Suites     │  (Domain, Application,
                  │ (xUnit + Shouldly)   │   Infrastructure, Contracts)
                  └──────────────────────┘
```

### Running the Test Suite
```powershell
dotnet test
```
All tests execute with **0 compiler warnings** and **100% pass rate**.
