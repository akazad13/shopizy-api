# Shopizy — Threat Model

> One-page STRIDE-style review of the API surface. Use this when adding endpoints or changing trust boundaries.

## Trust Boundaries

```
[Browser / native client] --(HTTPS)--> [API: Shopizy.Api]
                                          |
                       +------------------+-----------------+
                       |                  |                 |
              [SQL Server (EF Core)]  [Redis cache /    [Stripe / Cloudinary]
                                       refresh tokens /
                                       idempotency]
```

Tenants:
- Anonymous public (browse products, register, login)
- Authenticated **Customer** (own cart, orders, addresses, reviews)
- Authenticated **Admin** (catalog, users, reports)

## STRIDE per asset

### S — Spoofing identity

| Threat | Mitigation | Owner |
|---|---|---|
| Stolen JWT replays | Short-lived access token (60min default), opaque refresh token rotated on each `/auth/refresh` use, server-side store in Redis with revocation index per user. | `LoginQueryHandler`, `RefreshTokenCommandHandler`, `RedisRefreshTokenStore` |
| Brute-force login | `[RequireRateLimiting("auth")]` on `/auth/login` and `/auth/refresh` (5 req/min/IP). Hashing with PBKDF2-HMACSHA512 / 10k iterations. | `LoginEndpoint`, `PasswordManager` |
| Cross-tenant access via path-bound `userId` | `ClaimsPrincipalExtensions.AuthorizeOwner(userId)` returns 403 when caller's `sub` doesn't match. Covered by `CrossTenantAuthorizationTests`. | All endpoints accepting `{userId:guid}` path param |

### T — Tampering

| Threat | Mitigation |
|---|---|
| In-flight modification | HTTPS enforced outside `Development`/`Testing` (HSTS + redirect). |
| Replay of mutation requests (double-charge) | `Idempotency-Key` header on `POST /orders/checkout` and `POST /payments`, hashed-body match in Redis with 24h TTL. |
| Concurrent updates losing writes | `RowVersion` shadow concurrency token on `Order`, `Cart`, `Product`, `User`. `DbUpdateConcurrencyException` → 409. |
| Direct DB tampering through DTO bleed-through | Repositories return entities, but mapping is one-way (Domain → Contract). Domain models use private setters and factory methods. |

### R — Repudiation

| Threat | Mitigation |
|---|---|
| User claims they never made an order | `AuditLog` aggregate captures `EntityName, EntityId, Action, OldValues, NewValues, UserId, Timestamp`. Outbox pattern guarantees side-effects after the originating commit. |
| Admin actions disputed | All admin endpoints require `Admin` role + permission claim; correlation-ID middleware threads `X-Correlation-ID` into every log scope. |

### I — Information disclosure

| Threat | Mitigation |
|---|---|
| Stack traces / connection strings leaking via 500 responses | `GlobalExceptionHandler` returns sanitized `ProblemDetails` and only logs raw exception when `IsServerSide=true`. |
| Cache poisoning leaking other users' data | Cache keys include user-scoped identifiers where applicable; idempotency replay scope is `userId:path:idempotencyKey`. |
| API keys committed | Empty placeholders in `appsettings*.json`; secrets sourced from env vars / user-secrets. Stripe key bound via `IStripeClient` from options, not the global static. |
| Refresh tokens leaking from a Redis snapshot | Stored as SHA-256 hash, never plaintext. |
| PII in logs | Source-generated `LoggerMessage` definitions limit interpolation; no email/password values are logged. |

### D — Denial of service

| Threat | Mitigation |
|---|---|
| Auth flooding | 5 req/min/IP fixed window on `/auth/*`. |
| API flooding | 100 req/min/user sliding window on the rest. |
| Oversize uploads | Kestrel `MaxRequestBodySize` and `FormOptions.MultipartBodyLengthLimit` set to 10 MB. |
| Pagination abuse | `PaginationRules.ValidPageSize` caps at 100; applied to all paginated query validators. |
| Long-running aggregate scans | `OrderRepository.GetTotalRevenueAsync` (and similar) aggregate at the DB; `AsSingleQuery()` collapses split-query round-trips. |
| Unbounded retry on outbox failures | Background `OutboxProcessor` dead-letters parse-time errors; runtime errors keep retrying every 30s but emit a structured warning when dead-letter backlog ≥ 10. |

### E — Elevation of privilege

| Threat | Mitigation |
|---|---|
| Customer escalates to admin via direct request | Authorization policies check both role + permission claim (e.g. `Order.Create` requires `create:order`). Admin-only endpoints add `RequireRole("Admin")`. Tests cover the deny path. |
| Permissions registered with a non-existent UUID | Permissions are loaded from DB during register. (Hardcoded ID list in `RegisterCommandHandler` is a tracked weakness — see audit S7.) |

## Outstanding risks (acknowledged, not fixed)

- **CORS** allows credentials with explicit origins/methods/headers, but the allow-list is config-driven; misconfig in production is a deployment-time concern.
- **CSRF** — all mutation endpoints use Bearer tokens (not cookies), so antiforgery is not wired up. If cookie auth is ever added, antiforgery must be added with it.
- **Permission UUIDs** for the Customer role are hardcoded in `RegisterCommandHandler`. Replace with a name-based lookup + cache (audit S7).
- **CS86xx nullable warnings** are still suppressed in `.editorconfig`; ~164 sites need cleanup before strict null-safety is enforced.
