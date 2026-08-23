# Shopizy API

[⬅️ Back to README](../README.md) · [Repository Memory](RepositoryMemory.md) · [Domain Models](Domain.md) · [Eventual Consistency](EventualConsistency.md)

The hand-maintained route reference that previously lived here drifted from the implementation. The authoritative source for the API surface is the OpenAPI document generated at runtime.

## Where to look

- **Swagger UI** — start the API (`dotnet run --project src/Shopizy.Api`) and browse `https://localhost:5001/swagger`. Available in `Development` and `Production` environments by design (gated in `Program.cs`).
- **OpenAPI JSON** — `https://localhost:5001/swagger/v1/swagger.json` for tooling.
- **Bruno collection** — `Bruno/Shopizy/` has a working request collection for manual exercise.
- **Versioning** — endpoints are routed under `api/v1.0/...`. The version is also accepted via the `api-version` query string or header.

## Auth shape

- `POST /api/v1.0/auth/register` — create an account.
- `POST /api/v1.0/auth/login` — returns `{ token, refreshToken, refreshTokenExpiresAt }`.
- `POST /api/v1.0/auth/refresh` — exchange a refresh token for a new access + refresh pair (rotated).
- All other endpoints expect `Authorization: Bearer <token>`.

## Idempotency

`POST /api/v1.0/orders/checkout` and `POST /api/v1.0/users/{userId}/payments` require an `Idempotency-Key` header. Replays with the same key + body return the original response; reusing the key with a different body returns 409.

## Contract versioning

The current contracts live in `Shopizy.Contracts` and map to v1 routes. The `Shopizy.Contracts.V1` namespace is reserved (see `V1/PlaceholderV1.cs`) so future breaking changes can introduce a sibling `Shopizy.Contracts.V2` namespace cleanly. Rules:

- **Non-breaking** (additive optional fields, new endpoints) → no version bump.
- **Breaking** (remove/rename a field, change semantics, tighten validation) → add a `V2` contract sibling alongside the v1 one and route the new endpoint under `/api/v2.0/...`. v1 stays available until its deprecation window closes.
- ASP.NET API versioning is already configured (`AddApiVersioning` in `Shopizy.Api/DependencyInjectionRegister.cs`) — endpoints declare their version via the route prefix; the version reader also accepts an `api-version` query/header.
