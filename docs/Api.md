# Shopizy API

[⬅️ Back to README](../README.md) · [Product Requirements Document (PRD)](ProductRequirementsDocument.md) · [Repository Memory](RepositoryMemory.md) · [Feature Docs](FeatureDocumentation.md) · [Technical Docs](TechnicalDocumentation.md) · [Domain Models](Domain.md) · [Eventual Consistency](EventualConsistency.md)

The authoritative source for the API surface is the OpenAPI document generated at runtime.

## Where to look

- **Swagger UI** — start the API (`dotnet run --project src/Shopizy.Api`) and browse `https://localhost:7172/swagger` (or `http://127.0.0.1:18080/swagger`). Available in `Development` and `Production` environments by design (configured in `Program.cs`).
- **OpenAPI JSON** — `https://localhost:7172/swagger/v1/swagger.json` for tooling and SDK generators.
- **Bruno collection** — `Bruno/Shopizy/` has a comprehensive, working request collection for manual testing.
- **Versioning** — endpoints are routed under `api/v1.0/...`. The version is also accepted via the `api-version` query string or header.

## Auth shape

- `POST /api/v1.0/auth/register` — create an account (assigns `Customer` role and default permissions).
- `POST /api/v1.0/auth/login` — returns `{ token, refreshToken, refreshTokenExpiresAt }`.
- `POST /api/v1.0/auth/refresh-token` — exchange a sliding refresh token for a new access + refresh pair (rotated and tracked in Redis).
- Protected endpoints expect `Authorization: Bearer <token>`.
- Cross-tenant requests enforce ownership matching (`userId` path parameter vs. token `sub` claim).

## Real-Time WebSockets (SignalR)

- `/hubs/orders` — Customer order status updates (`ReceiveOrderStatusUpdate`), scoped to `user-{userId}` groups. Requires JWT Bearer token.
- `/hubs/admin-dashboard` — Merchant live metrics and events (`ReceiveMetricUpdate`). Enforces `Admin` role.

## Idempotency

`POST /api/v1.0/users/{userId}/orders`, `POST /api/v1.0/orders/checkout`, and payment endpoints require an `X-Idempotency-Key` (or `Idempotency-Key`) header. Replays with the same key + payload return the original response from Redis cache; reusing the key with a different body returns HTTP 409 Conflict.

## Contract versioning

The current contracts live in `Shopizy.Contracts` and map to v1 routes. The `Shopizy.Contracts.V1` namespace is reserved (see `V1/PlaceholderV1.cs`) so future breaking changes can introduce a sibling `Shopizy.Contracts.V2` namespace cleanly. Rules:

- **Non-breaking** (additive optional fields, new endpoints) → no version bump.
- **Breaking** (remove/rename a field, change semantics, tighten validation) → add a `V2` contract sibling alongside the v1 one and route the new endpoint under `/api/v2.0/...`. v1 stays available until its deprecation window closes.
- ASP.NET API versioning is already configured (`AddApiVersioning` in `Shopizy.Api/DependencyInjectionRegister.cs`) — endpoints declare their version via the route prefix; the version reader also accepts an `api-version` query/header.
