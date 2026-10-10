# Shopizy Domain

[⬅️ Back to README](../README.md) · [Product Requirements Document (PRD)](ProductRequirementsDocument.md) · [Repository Memory](RepositoryMemory.md) · [Technical Docs](TechnicalDocumentation.md) · [Feature Docs](FeatureDocumentation.md) · [API](Api.md) · [Eventual Consistency](EventualConsistency.md) · [Threat Model](ThreatModel.md)

This document is a current map of the domain layer (`src/Shopizy.Domain`). The authoritative source of truth is the code — when in doubt, read the aggregate. Treat this as a navigation aid, not a specification.

## Layout

All aggregates live in `src/Shopizy.Domain/<AggregateName>/` and inherit from `AggregateRoot<TId, Guid>` (defined in `Shopizy.SharedKernel.Domain.Models`). Each aggregate has:

- a strongly-typed `*Id` value object under `ValueObjects/`
- factory `Create(...)` and a private constructor (no public mutation)
- domain events under `Events/`
- a private parameterless constructor for EF Core hydration

Cross-cutting types live in `Shopizy.Domain/Common/`:

| Type | Purpose |
|------|---------|
| `Price` | Money value object — `Amount` + `Currency` |
| `Address` | Shipping/billing address |
| `Currency` | enum (`USD = 1`, `EUR = 2`, `GBP = 3`, `BDT = 4`) |
| `OrderType` | enum used by reporting paths |
| `CustomErrors` | static factories for `ErrorOr.Error` failures |

## Aggregates

| Aggregate | Root file | Notes |
|-----------|-----------|-------|
| User | `Users/User.cs` | Identity, role, permissions, addresses, hashed password. `RowVersion` for concurrency. |
| Cart | `Carts/Cart.cs` | One per user. `RowVersion` concurrency, raises `CartItemAddedDomainEvent` on adds. |
| Category | `Categories/Category.cs` | Self-referencing parent; supports breadcrumb traversal. |
| Brand | `Brands/Brand.cs` | Simple lookup; products reference by `BrandId`. |
| Product | `Products/Product.cs` | Stock, price, images, specifications, reviews summary. `RowVersion` for stock concurrency. |
| ProductReview | `ProductReviews/ProductReview.cs` | Rating + comment per (`ProductId`, `UserId`). |
| ProductQuestion | `ProductQuestions/ProductQuestion.cs` | Q&A thread on a product. |
| Order | `Orders/Order.cs` | Items, shipment, status machine (`Pending → Processing → Shipping → Delivered`). Raises `OrderCreatedDomainEvent` and `PaymentCompletedDomainEvent`. |
| Payment | `Payments/Payment.cs` | Per-order; tracks Stripe transaction id, status, total. |
| PromoCode | `PromoCodes/PromoCode.cs` | Percentage- or amount-based discount; `IsActive` flag. |
| GiftCard | `GiftCards/GiftCard.cs` | Redeemable balance; `Redeem(amount)` mutates remaining balance. |
| LoyaltyAccount | `LoyaltyAccounts/LoyaltyAccount.cs` | Points balance with append-only `LoyaltyTransaction` history (`LoyaltyTransactionType`). |
| Wishlist | `Wishlists/Wishlist.cs` | Per-user, list of `(ProductId, …)` items. |
| Permission | `Permissions/Permission.cs` | Lookup table; users hold `IList<PermissionId>`. Seeded in `PermissionConfigurations`. |
| AuditLog | `AuditLogs/AuditLog.cs` | Append-only audit trail written by command handlers. |

## Key enums

| Enum | Values |
|------|--------|
| `Orders.Enums.OrderStatus` | `Pending = 1`, `Processing = 2`, `Shipping = 3`, `Delivered = 4`, `Cancelled = 5`, `Refunded = 6` |
| `Orders.Enums.DeliveryMethods` | `Free = 0`, `Standard = 1`, `Express = 2`, `Premium = 3` |
| `Orders.Enums.ShipmentStatus` | shipment lifecycle (preparing → in transit → delivered) |
| `Payments.Enums.PaymentStatus` | `Pending = 1`, `Cancelled = 2`, `Payed = 3`, `Refunded = 4` |
| `Users.Enums.UserRole` | `Customer`, `Admin` |
| `LoyaltyAccounts.Enums.LoyaltyTransactionType` | `Earn`, `Redeem`, `Expire`, `Adjust` |

## Order state transitions

```
Pending ──CompletePayment──▶ Processing
   │                              │
   │                              └─AddShipment / UpdateShipment──▶ Shipping ──▶ Delivered
   │
   └─CancelOrder──▶ Cancelled
```

`Order.CompletePayment(stripeCustomerId)` is the only way to leave `Pending`. It raises `PaymentCompletedDomainEvent`. Cancellation (`CancelOrder(reason)`) raises `OrderCancelledDomainEvent` and is idempotent only at the API layer.

## Domain events

Every aggregate inherits `AddDomainEvent` / `PopDomainEvents` from the `Entity` base. Events are collected during command handling, persisted to the `OutboxMessages` table inside the same transaction, and dispatched after commit by `EventualConsistencyMiddleware`. The `OutboxProcessor` background service retries any messages older than 5 minutes that the synchronous path missed (e.g., due to a crash between commit and dispatch).

Notable events:

- `OrderCreatedDomainEvent` — clears the user's cart, decrements product stock.
- `PaymentCompletedDomainEvent` — updates `Order.OrderStatus = Processing`, awards loyalty points if applicable.
- `OrderCancelledDomainEvent` — restores stock.
- `CartItemAddedDomainEvent` — used by recommendation/analytics surfaces (currently no-op handler).
- `UserRegisteredDomainEvent` — provisions a starter `Cart` for the new user.

For the rules a contributor must follow when authoring a handler (idempotency, no out-of-band side effects without retry, dead-letter expectations), see [`docs/EventualConsistency.md`](EventualConsistency.md).

## Concurrency

`Product`, `Cart`, `Order`, and `User` carry a `RowVersion` (`byte[]`). Updates that race lose with `DbUpdateConcurrencyException`, mapped by `GlobalExceptionHandler` to HTTP 409. Other aggregates (Category, Brand, PromoCode, …) have no token and are last-write-wins by design — they're rarely contended and not safety-critical.

## Identity

User passwords are PBKDF2/HMACSHA512 with 10 000 iterations, versioned via `PasswordManager.HashVersion`. JWT tokens are short-lived; refresh tokens are SHA-256-hashed and stored in Redis with rotation on every refresh (see `Shopizy.Infrastructure.Security.RefreshTokens`). Permissions are seeded once in `PermissionConfigurations` and resolved at registration via `IPermissionLookup` (singleton-cached name → id map).
