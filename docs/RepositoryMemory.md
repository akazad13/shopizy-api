# 🧠 Shopizy — Repository Memory & AI Architecture Reference

> **Purpose:** High-density architecture map and reference manual for AI agents and developers. Consult this memory file to understand codebase structure, entity graphs, handler conventions, message routing, security mechanisms, and data access patterns without re-scanning the workspace.  
> **Last Updated:** August 2026  
> **Framework:** .NET 10 (C# 13) | **Architecture:** Clean Architecture & Domain-Driven Design (DDD)

---

## 1. Solution & Project Topology

```
shopizy/
├── src/
│   ├── Shopizy.Domain/           # Enterprise logic: Aggregates, Value Objects, Domain Events, Invariants (Zero external deps)
│   ├── Shopizy.Application/      # Business use-cases: CQRS Handlers, Validators, Interfaces, DTO Mapping
│   ├── Shopizy.Infrastructure/   # Tech implementations: EF Core 10, Redis, SignalR, Outbox, Stripe, Cloudinary
│   ├── Shopizy.Api/              # Presentation layer: Minimal APIs, Endpoints, Middleware, Swagger, Hubs
│   ├── Shopizy.Contracts/        # Transport models: Request/Response DTO records (Shared with client SDKs)
│   └── Shopizy.SharedKernel/     # Shared primitives: Entity, AggregateRoot, ValueObject, IDispatcher, Result/Error
├── tests/
│   ├── Shopizy.Domain.UnitTests/         # Domain invariant unit tests
│   ├── Shopizy.Application.UnitTests/    # Handler unit tests (xUnit + Shouldly + Moq)
│   ├── Shopizy.Infrastructure.UnitTests/ # Infra service unit tests
│   ├── Shopizy.Contracts.UnitTests/      # DTO serialization & validation tests
│   ├── Shopizy.Architecture.Tests/       # NetArchTest layer boundary rules & Mapster compile checks
│   └── Shopizy.Api.IntegrationTests/     # End-to-end API tests (Testcontainers MsSql 2022 + WebApplicationFactory)
├── docs/                         # Project documentation suite
├── Bruno/                        # Bruno API collections for endpoint testing
├── docker-compose.yml            # Local development stack (SQL Server 2022, Redis 7, Mailpit, API)
├── Directory.Build.props         # Central MSBuild & compiler settings (TreatWarningsAsErrors=true)
└── Directory.Packages.props      # Centralized NuGet Package Versioning
```

---

## 2. Core Architectural Patterns & Abstractions

### 2.1 Custom CQRS & Messaging Pipeline
- **Handrolled Mediator (`IDispatcher`):** Defined in `Shopizy.SharedKernel.Application.Messaging`. Replaces MediatR. Uses reflection lookup + DI assembly scanning via **Scrutor**.
- **Interfaces:**
  - `ICommand<TResult>` / `ICommandHandler<TCommand, TResult>` (Returns `ErrorOr<TResult>`)
  - `IQuery<TResult>` / `IQueryHandler<TQuery, TResult>` (Returns DTOs / read models)
  - `IDomainEvent` / `IDomainEventHandler<TEvent>`
- **Scrutor Decorators (`Shopizy.Application/DependencyInjectionRegister.cs`):**
  1. `ValidationBehavior<TCommand, TResult>` (FluentValidation pipeline)
  2. `UnitOfWorkBehavior<TCommand, TResult>` (Auto-commits `SaveChangesAsync`)
  3. `CachingBehavior<TQuery, TResult>` (Redis cache-aside for queries)
  4. `CacheInvalidationBehavior<TCommand, TResult>` (Removes keys marked with `IInvalidateCache`)

### 2.2 Outbox Pattern & Eventual Consistency
- **In-Transaction Dispatch:** Domain events raised via `Entity.AddDomainEvent` are committed atomically into the `OutboxMessages` table in `AppDbContext.SaveChangesAsync`.
- **Pipeline Execution (`EventualConsistencyMiddleware`):**
  - Intercepts requests, opens an EF Core transaction strategy execution wrapper (`EnableRetryOnFailure`).
  - Dispatches domain event handlers **inside** the open transaction.
  - If any handler throws, the transaction rolls back completely (aggregates + outbox + handler side-effects).
  - Outbox rows are marked `ProcessedOn` immediately upon success.
- **Background Backstop (`OutboxProcessor`):**
  - Hosted background service running every 30 seconds.
  - Sweeps unprocessed rows older than 5 minutes to recover from rare crash-mid-commit scenarios.
  - Poison messages exceeding retry threshold route to `OutboxDeadLetters` and increment `shopizy.outbox.dead_lettered` OTel counters.
- **Test Synchronizer (`IOutboxDrainer`):** Synchronously drains pending outbox messages in integration tests (`BaseIntegrationTest.DrainOutboxAsync()`).

### 2.3 Data Access, CQRS Readers & EF Core Architecture
- **Database Engine:** Microsoft SQL Server 2022 (canonical provider).
- **CQRS Read-Side Readers (`IProductReader`, `IOrderReader`):** Dedicated read-only query abstractions (`ProductReader`, `OrderReader`) encapsulate reporting, aggregations (`GetTotalRevenueAsync`, `GetTopProductsByRevenueAsync`), and read models, keeping repositories focused on aggregate persistence.
- **Concurrency Tokens:** `RowVersion` (`byte[]`) shadow tokens on `Product`, `Cart`, `Order`, `User`. Unhandled write collisions raise `DbUpdateConcurrencyException`, caught and converted to HTTP 409 Conflict.
- **Value Object Converters & Command Boundaries:** Strongly-typed IDs (`ProductId`, `OrderId`, `UserId`, etc.) and `Price` structs mapped via `HasConversion`. Commands (`CreateProductCommand`, `UpdateProductCommand`) carry domain Value Objects directly; Mapster converts contract DTO primitives at API boundary.
- **Encapsulation:** Private backing fields (e.g. `_orderItems`, `_imageUrls`) enforce aggregate invariants.

### 2.4 Caching, Idempotency & Distributed State
- **Redis Services (`Shopizy.Infrastructure`):**
  - `ICacheHelper` (`RedisCacheHelper`): Cache-aside with configurable TTLs.
  - `IIdempotencyStore` (`RedisIdempotencyStore`): Stores request hashes and response payloads (24h TTL) for `X-Idempotency-Key` headers on order/payment endpoints.
  - `IRefreshTokenStore` (`RedisRefreshTokenStore`): Opaque sliding refresh tokens with rotation and per-user revocation indexes.

---

## 3. Domain Aggregates & Invariants Map

| Aggregate Root | Primary Responsibilities & Key Methods | Domain Events Raised |
|---|---|---|
| **`User`** (`Users/User.cs`) | Identity, password hash (PBKDF2-HMACSHA512), role assignment (`Customer`, `Admin`), permissions, user addresses. | `UserRegisteredDomainEvent` |
| **`Product`** (`Products/Product.cs`) | Catalog title, price, SKU, tags, image galleries, variants, stock management (`ReduceStock`, `IncreaseStock`), rating aggregations. | `ProductPriceDroppedDomainEvent`, `ProductBackInStockDomainEvent` |
| **`Order`** (`Orders/Order.cs`) | Line items, shipping address, status state machine, cancellation reason, delivery method, payment references. | `OrderCreatedDomainEvent`, `OrderCancelledDomainEvent`, `PaymentCompletedDomainEvent` |
| **`Cart`** (`Carts/Cart.cs`) | Line item collection, variant options, pricing snapshot, inactivity timestamp (`LastAbandonedReminderSentOn`). | `CartItemAddedDomainEvent` |
| **`PromoCode`** (`PromoCodes/PromoCode.cs`) | Percentage/fixed discounts, BOGO rules, tiered minimum spend, target category filters, usage limits. | — |
| **`LoyaltyAccount`** (`LoyaltyAccounts/LoyaltyAccount.cs`) | Customer point balances, earning tier rules, append-only `LoyaltyTransaction` history (`Earn`, `Redeem`, `Expire`). | — |
| **`GiftCard`** (`GiftCards/GiftCard.cs`) | Unique voucher code, initial/remaining balance, redemption logic (`Redeem(amount)`). | — |
| **`ProductReview`** (`ProductReviews/ProductReview.cs`) | Star ratings (1–5), verified purchase badge verification, review photos, community upvotes (`HelpfulVotesCount`). | — |
| **`Wishlist`** (`Wishlists/Wishlist.cs`) | Saved product favorites per user; triggers price drop and restock notifications. | — |
| **`Category`** (`Categories/Category.cs`) | Self-referencing hierarchical category tree. | — |
| **`Brand`** (`Brands/Brand.cs`) | Manufacturer lookup with logo and description. | — |
| **`Payment`** (`Payments/Payment.cs`) | Stripe payment intent tracking, payment status (`Pending`, `Payed`, `Cancelled`, `Refunded`). | — |
| **`AuditLog`** (`AuditLogs/AuditLog.cs`) | Append-only system audit log tracking resource mutations (`EntityName`, `EntityId`, `Action`, `UserId`). | — |

---

## 4. API Endpoints & Request Routing Summary

All endpoints follow Minimal API pattern (`IEndpoint` implementations registered via `AddEndpoints`) and route under `/api/v1.0/...`:

### 🔑 Authentication (`/api/v1.0/auth`)
- `POST /register`: Account registration.
- `POST /login`: Validates credentials, returns JWT access token + refresh token.
- `POST /refresh-token`: Rotates sliding refresh token and returns new access/refresh pair.

### 🛍️ Product Catalog & Search (`/api/v1.0/products`)
- `POST /faceted-search`: Multi-token search with Levenshtein typo tolerance ("Did You Mean?"), category/brand/price/rating facets.
- `GET /`, `GET /{id}`: Catalog browsing.
- `POST /`, `PUT /{id}`, `DELETE /{id}`: Admin product management.
- `GET /{id}/variants`, `POST /{id}/variants`: Variant management.

### 🛒 Shopping Cart (`/api/v1.0/users/{userId}/cart`)
- `GET /`: Retrieve active cart.
- `POST /items`, `PUT /items/{itemId}`, `DELETE /items/{itemId}`: Cart mutations.

### 📦 Orders & Payments (`/api/v1.0/orders` & `/api/v1.0/users/{userId}/orders`)
- `POST /users/{userId}/orders`: Submit order (Requires `X-Idempotency-Key` header).
- `GET /orders/{orderId}/tracking`: Live shipping tracking scan log.
- `POST /webhooks/stripe`: Idempotent Stripe webhook receiver (`payment_intent.succeeded`, `payment_intent.payment_failed`).

### 🚚 Shipping Rates (`/api/v1.0/shipping`)
- `POST /estimate-rates`: Live carrier rate comparisons (USPS, UPS, FedEx, DHL) with automated free-shipping threshold qualification.

### 🎯 Promotions, Loyalty, Reviews, Wishlists
- `POST /users/{userId}/orders/validate-promo`: Coupon validation.
- `GET /users/{userId}/loyalty`, `POST /users/{userId}/loyalty/redeem`: Points management.
- `GET/POST /products/{productId}/reviews`: Review submission & upvoting.
- `GET/POST/DELETE /users/{userId}/wishlist`: Wishlist management.

---

## 5. Security & Authorization Architecture

1. **Owner Authorization (`AuthorizeOwner`):** Endpoint extension method `ClaimsPrincipalExtensions.AuthorizeOwner(userId)` verifies the caller's JWT `sub` matches the route `userId` parameter, throwing 403 Forbidden on cross-tenant access attempts.
2. **Role-Based Access Control (RBAC):** Admin endpoints enforce `[Authorize(Roles = "Admin")]` and permission claims. Permission GUIDs are resolved by name via cached `IPermissionLookup`.
3. **Security Headers (`SecurityHeadersMiddleware`):** Injects HSTS, CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, COOP, and CORP headers.
4. **Rate Limiting:** Auth endpoints capped at 5 req/min/IP; General API endpoints capped at 100 req/min/user.
5. **Zero-Allocation Logging & PII Masking:** Compile-time `LoggerMessage` delegates with `LogSanitizer` regex masking for emails, credit cards, and tokens.
6. **Password Policy & Dictionary Check:** `PasswordRules.StrongPassword()` enforces minimum length ($\ge 12$), uppercase, lowercase, digit, special character, and a compromised password dictionary check.

---

## 6. Background Workers & Hosted Services

- `OutboxProcessor`: Sweeps uncommitted/failed outbox events every 30s.
- `AbandonedCartReminderWorker`: Scans inactive carts ($\ge 2$ hours) and dispatches recovery emails.
- `PendingOrderExpirationWorker`: Cancels unpaid pending orders ($\ge 15$ mins) and restores inventory stock.
- `DbMigrationsHostedService`: Manages automatic EF Core migration execution at application startup.

---

## 7. Developer Quick Reference & Commands

### Build & Run
```powershell
# Restore dependencies
dotnet restore

# Run API locally
dotnet run --project src/Shopizy.Api

# Run local infrastructure dependencies via Docker
docker-compose up -d
```

### Run Test Suite
```powershell
# Run all unit, architecture, and integration tests
dotnet test

# Run architecture tests specifically
dotnet test tests/Shopizy.Architecture.Tests

# Run integration tests (Requires Docker running for Testcontainers)
dotnet test tests/Shopizy.Api.IntegrationTests
```

---

## 8. Documentation Index

- **[README.md](../README.md)**: Main getting started guide and project overview.
- **[Feature Documentation](FeatureDocumentation.md)**: Business features and domain capabilities.
- **[Technical Documentation](TechnicalDocumentation.md)**: Deep dive into Clean Architecture and DDD design.
- **[API Documentation](Api.md)**: OpenAPI & Swagger guide.
- **[Domain Models](Domain.md)**: Aggregate details and state transition machines.
- **[Eventual Consistency](EventualConsistency.md)**: Outbox pattern and event handler rules.
- **[Frontend Handoff Guide](FrontendHandoffDoc.md)**: TypeScript interfaces, endpoints, and SignalR setup for frontend clients.
- **[Project Structure](ProjectStructure.md)**: Map of repository files and layers.
- **[Threat Model](ThreatModel.md)**: Security STRIDE analysis.
- **[Improvement Scope](ImprovementScope.md)**: Production audit roadmap.
