# 🧠 Shopizy — Workspace Repository Memory & AI Architecture Reference

> **Purpose:** Workspace rule and memory file for AI agents working in Shopizy. Consult this memory file to understand codebase structure, entity graphs, handler conventions, message routing, security mechanisms, and data access patterns without re-scanning the workspace.  
> **Last Updated:** August 2026  
> **Framework:** .NET 10 (C# 13) | **Architecture:** Clean Architecture & Domain-Driven Design (DDD)

---

## 1. Solution Topology & Layer Mapping

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
- **In-Transaction Dispatch:** Domain events raised via `Entity.AddDomainEvent` are committed atomically into `OutboxMessages` table in `AppDbContext.SaveChangesAsync`.
- **Pipeline Execution (`EventualConsistencyMiddleware`):**
  - Opens EF Core transaction strategy execution wrapper (`EnableRetryOnFailure`).
  - Dispatches domain event handlers **inside** the open transaction.
  - Handler failures roll back transaction completely (aggregates + outbox + handler side-effects).
  - Outbox rows are marked `ProcessedOn` immediately upon success.
- **Background Worker (`OutboxProcessor`):** Sweeps unprocessed rows older than 5 minutes every 30s.

### 2.3 Data Access & EF Core Architecture
- **Database Engine:** Microsoft SQL Server 2022 (canonical provider).
- **CQRS Read-Side Readers (`IProductReader`, `IOrderReader`):** Dedicated read-only query abstractions (`ProductReader`, `OrderReader`) encapsulate reporting and aggregations, keeping repositories focused on aggregate persistence.
- **Concurrency Tokens:** `RowVersion` (`byte[]`) shadow tokens on `Product`, `Cart`, `Order`, `User`. Writes collisions throw `DbUpdateConcurrencyException`, returning HTTP 409 Conflict.
- **Value Object Converters & Command Boundaries:** Strongly-typed IDs (`ProductId`, `OrderId`, `UserId`, etc.) and `Price` structs mapped via `HasConversion`. Commands (`CreateProductCommand`, `UpdateProductCommand`) carry domain Value Objects directly; Mapster converts contract DTO primitives at API boundary.

---

## 3. Domain Aggregates & Invariants Map

| Aggregate Root | Primary Responsibilities & Key Methods | Domain Events Raised |
|---|---|---|
| **`User`** (`Users/User.cs`) | Identity, password hash (PBKDF2-HMACSHA512), roles (`Customer`, `Admin`), user addresses. | `UserRegisteredDomainEvent` |
| **`Product`** (`Products/Product.cs`) | Catalog title, price, SKU, tags, image galleries, variants, stock (`ReduceStock`, `IncreaseStock`), rating stats. | `ProductPriceDroppedDomainEvent`, `ProductBackInStockDomainEvent` |
| **`Order`** (`Orders/Order.cs`) | Line items, shipping address, status state machine, cancellation reason, delivery method. | `OrderCreatedDomainEvent`, `OrderCancelledDomainEvent`, `PaymentCompletedDomainEvent` |
| **`Cart`** (`Carts/Cart.cs`) | Line items, variant options, pricing snapshot, inactivity timestamp (`LastAbandonedReminderSentOn`). | `CartItemAddedDomainEvent` |
| **`PromoCode`** (`PromoCodes/PromoCode.cs`) | Discounts (Percentage, Fixed, BOGO, Tiered Minimum Spend), category targets, usage limits. | — |
| **`LoyaltyAccount`** (`LoyaltyAccounts/LoyaltyAccount.cs`) | Point balances, tier rules, append-only `LoyaltyTransaction` history (`Earn`, `Redeem`, `Expire`). | — |
| **`GiftCard`** (`GiftCards/GiftCard.cs`) | Unique voucher code, initial/remaining balance, redemption logic (`Redeem(amount)`). | — |
| **`ProductReview`** (`ProductReviews/ProductReview.cs`) | Ratings (1–5), verified purchase badge, photos, upvotes (`HelpfulVotesCount`). | — |
| **`Wishlist`** (`Wishlists/Wishlist.cs`) | Saved product favorites per user; triggers price drop and restock notifications. | — |
| **`Category`** (`Categories/Category.cs`) | Hierarchical category tree. | — |
| **`Brand`** (`Brands/Brand.cs`) | Manufacturer lookup with logo and description. | — |
| **`Payment`** (`Payments/Payment.cs`) | Stripe payment intent tracking, payment status (`Pending`, `Payed`, `Cancelled`, `Refunded`). | — |
| **`AuditLog`** (`AuditLogs/AuditLog.cs`) | Audit log tracking resource mutations (`EntityName`, `EntityId`, `Action`, `UserId`). | — |

---

## 4. Key Rules for Workspace Edits

1. **Domain Isolation:** Never add external dependencies or infra frameworks to `Shopizy.Domain` or `Shopizy.SharedKernel`.
2. **Commands & Value Objects:** Pass strongly-typed value objects in commands where established (`Price`, `CategoryId`, `BrandId`), using Mapster configs at API boundary.
3. **Idempotency Header:** Mutation endpoints like `POST /users/{userId}/orders` require `X-Idempotency-Key` header.
4. **Owner Authorization:** Always use `ClaimsPrincipalExtensions.AuthorizeOwner(userId)` on user-scoped endpoints.
5. **LoggerMessage Usage:** Use source-generated `[LoggerMessage]` partial methods in `LoggerMessages.cs` instead of string-interpolated logger calls.
6. **Password Policy:** Validate passwords using `PasswordRules.StrongPassword()` including complexity and dictionary checks.
7. **Tests:** Keep architecture invariants intact in `Shopizy.Architecture.Tests` when adding new projects or layers.
