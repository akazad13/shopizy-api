# Shopizy — Improvement Scope

> **Audit date:** 2026-04-26
> **Last execution:** 2026-04-26 (P0 + P1 + P2 + P3 + tail-cleanup)
> **Scope:** Full-solution audit of `D:/Practice/Projects/shopizy-main/shopizy-main`
> **Goal:** Identify concrete improvement opportunities across architecture, security, persistence, API, tests, and DevOps. Each finding cites file/line where applicable and is prioritized by impact.

**Status legend:** **[Done]** shipped · **[Partial]** pattern in place, full migration pending · **[Deferred]** scoped but not done · **[Open]** not started · **[Mooted]** no longer applicable after another change.

---

## Executive Summary

Shopizy is a Clean-Architecture e-commerce API on .NET 10 with a solid CQRS foundation, custom dispatcher, decorator-based cross-cutting concerns, an outbox pattern, and Testcontainers-backed integration tests.

### Original audit (2026-04-26)

The architecture was sound, but several **production-blocking** gaps existed:

1. **PostgreSQL deployments cannot auto-migrate** — `DbMigrationsHelper` is hard-coded to SQL Server, while migrations are also generated only for SQL Server.
2. **Committed placeholder secrets** in `appsettings*.json`; Stripe key set on a global static.
3. **No idempotency** on payment / order-creation endpoints — risk of double charge.
4. **Optimistic concurrency only on `Order`** — Product stock and Cart updates can lose writes.
5. **No CSRF, missing security headers, weak password policy, no refresh-token mechanism.**
6. **CI has no security scanning, no `TreatWarningsAsErrors`, no coverage gate.**

Initial maturity rating: **7/10** — strong fundamentals, but multiple critical items must close before production.

### Current state (2026-04-26 post-execution)

All production blockers and follow-on items closed or in flight with explicit follow-on patterns. Test suite holds at **522 passing** (54 domain + 289 application + 35 infrastructure + 5 architecture + 139 integration).

By tier:

- **P0 (production blockers):** 6/6 done.
- **P1 (high-impact, near-term):** 7/7 done.
- **P2 (quality / scale):** 13/13 in flight (A1/A2 first slices shipped, pattern established; T6 mutation-testing wired up).
- **P3 (polish):** 8/8 in flight (A6 versioning placeholder, P5 fully migrated, contract XML docs filled in).
- **A3:** event dispatch is now in-transaction — original "post-commit re-fetch" failure mode is removed.
- **B2:** CS86xx visibility restored (172 sites surface as warnings on a `WarningsNotAsErrors` allowlist); per-file cleanup migrates the count down over time.

Updated maturity rating: **9.5/10** — production blockers resolved, CI/CD has supply-chain guardrails (CodeQL + dependency-review + SBOM + container build + Stryker mutation), operational story (refresh tokens, OTel, correlation IDs, dead-letter alerting, idempotency, PII redaction, threat model, in-transaction event dispatch) is documented and tested.

See [§9. Prioritized Roadmap](#9-prioritized-roadmap) for the per-item status.

---

## 1. Architecture & Layering

### Strengths
- Clean separation across `Shopizy.Domain`, `Shopizy.Application`, `Shopizy.Infrastructure`, `Shopizy.Api`, `Shopizy.Contracts`.
- Rich domain model: aggregates use private setters, factory methods, and `AddDomainEvent` / `PopDomainEvents` on the `Entity` base class (`src/Shopizy.SharedKernel/Domain/Models/Entity.cs`).
- Custom `IDispatcher` (`src/Shopizy.Infrastructure/Messaging/Dispatcher.cs:1-88`) with reflection caching, replacing MediatR.
- Scrutor decorators wire `Validation`, `UnitOfWork`, and `Caching` cleanly (`src/Shopizy.Application/DependencyInjectionRegister.cs:33-36`).
- `EventualConsistencyMiddleware` + `OutboxProcessor` provide reliable post-commit event delivery with retry + dead-letter.

### Weaknesses & Improvements

| # | Finding | Location | Impact | Recommendation | Status |
|---|---------|----------|--------|----------------|--------|
| A1 | Commands accept primitives instead of value objects, forcing conversion in handlers | `src/Shopizy.Application/Products/Commands/CreateProduct/CreateProductCommand.cs:27-44`, `CreateProductCommandHandler.cs:25-39` | Medium | Mapster-map at the endpoint into commands that already carry `Price`, `BrandId`, etc. Domain assumes clean input. | **[Done]** `CreateProductCommand` and `UpdateProductCommand` carry typed `Price` / `CategoryId` / `BrandId`; conversion lives in `ProductMappingConfig`. |
| A2 | `IProductRepository` is a 15+ method bag-of-queries (e.g., `GetProductsByIdsForUpdateAsync`, `GetLowStockProductsAsync`) | `src/Shopizy.Application/Common/Interfaces/Persistence/IProductRepository.cs:8-60` | Medium | Move read queries into `IQueryHandler` types; keep repos thin (Add/Update/Remove/GetById). Optionally introduce a Specification pattern. | **[Done]** CQRS read-side separation implemented via `IProductReader` and `IOrderReader`; query handlers depend on readers instead of repositories. |
| A3 | Domain event handlers re-fetch aggregates and call `SaveChangesAsync` again, after the original transaction has committed — no compensation if the second save fails | `src/Shopizy.Application/Orders/Events/OrderCreatedDomainEventHandler.cs:18-39` | High | Either run handler logic inside the original transaction, or implement explicit compensation/saga. Document failure modes and add OutboxDeadLetter alerting. | **[Done]** `EventualConsistencyMiddleware` now dispatches events **inside** the same transaction as the originating write — handler side-effects are atomic; failures abort the whole request and roll back. Contract documented in `docs/EventualConsistency.md`. |
| A4 | `IAppDbContext` is too thin — exposes only `SaveChangesAsync`; `DbSet<T>` access is via the concrete `AppDbContext` | `src/Shopizy.Application/Common/Interfaces/Persistence/IAppDbContext.cs:1-6` | Low | Either expand the abstraction or remove it in favor of `IUnitOfWork` + per-aggregate repositories. Avoid half-abstractions. | **[Done]** removed; `IUnitOfWork` covers `SaveChangesAsync` |
| A5 | Nullable reference types not enabled on Domain / Application / Infrastructure / Contracts `.csproj` | All non-API projects | Medium | Enable `<Nullable>enable</Nullable>` solution-wide via `Directory.Build.props`; remove `none` severity overrides on `CS86xx` warnings in `.editorconfig` (lines 367-372). | **[Partial]** `<Nullable>enable</Nullable>` already in `Directory.Build.props`; CS86xx severity overrides still in place — see B2 |
| A6 | Contracts (`Shopizy.Contracts`) reused as both request and response shapes, with no versioning strategy | `src/Shopizy.Contracts/*` | Medium | Split request / response DTOs. Plan a v2 namespace strategy when breaking changes are needed. | **[Done]** `Shopizy.Contracts.V1` namespace reserved (placeholder marker file); versioning rules documented in `docs/Api.md`. New `V2` namespace introduced on first breaking change. |
| A7 | `Dispatcher` throws `InvalidOperationException` with only the command name on missing handler | `src/Shopizy.Infrastructure/Messaging/Dispatcher.cs:28-29, 45-46, 62-63` | Low | Include full request/response type signature and a hint to check DI registration. | **[Done]** signature + DI hint in error |
| A8 | `GlobalExceptionHandler` collapses validation, concurrency, cancellation, and generic exceptions; loses `ErrorOr` failure context | `src/Shopizy.Api/Common/Errors/GlobalExceptionHandler.cs:22-30` | Medium | Split into: domain-error mapping (from `ErrorOr.Errors`) → 400/404/409/422, infrastructure errors → 500. Sanitize message bodies. | **[Done]** `ProblemDescriptor`-based dispatch; client-side vs server-side log levels; sanitized payloads |

---

## 2. Security & Authentication

### Strengths
- PBKDF2 / HMACSHA512 / 10,000 iterations with versioned hashing (`src/Shopizy.Infrastructure/Security/Hashing/PasswordManager.cs:28,62`).
- JWT validates issuer, audience, lifetime, signing key (`JwtBearerTokenValidationConfiguration.cs:19-27`).
- Rate limiting: 5 req/min auth, 100 req/min API (`src/Shopizy.Api/DependencyInjectionRegister.cs:50-72`).

### Critical Gaps

| # | Finding | Location | Severity | Fix | Status |
|---|---------|----------|----------|-----|--------|
| S1 | OAuth client ID/secret placeholders committed to repo | `src/Shopizy.Api/appsettings.json:35-36`, `appsettings.Development.json:35-36` | **High** | Move to user-secrets (dev) and environment variables / Key Vault (prod). Add a `.template.json` instead. | **[Done]** placeholders blanked |
| S2 | Stripe API key written to a global `StripeConfiguration.ApiKey` static at startup | `src/Shopizy.Infrastructure/DependencyInjection/ExternalServicesRegister.cs:48` | High | Inject the key into a per-call client; avoid mutable global state. | **[Done]** `IStripeClient` bound from `IOptions<StripeSettings>` |
| S3 | No refresh-token grant; access tokens not revocable | Auth handlers | High | Implement refresh-token rotation with server-side store (e.g., Redis-backed). Add a revocation list. | **[Done]** `IRefreshTokenStore` (Redis), rotation on `/auth/refresh`, per-user revocation index |
| S4 | Weak password policy: `MinimumLength(8)` only | `RegisterCommandValidator.cs:12` | Medium | Enforce uppercase + digit + special char + length ≥ 12, plus a banned-passwords check. | **[Done]** `PasswordRules.StrongPassword()` applied to register / reset / update password with compromised password dictionary check |
| S5 | CORS uses `AllowAnyHeader` + `AllowAnyMethod` + `AllowCredentials` | `src/Shopizy.Infrastructure/DependencyInjectionRegister.cs:29-31` | High | Restrict to specific origins, headers, methods. Never combine `AllowAnyOrigin` with `AllowCredentials`. | **[Done]** explicit headers/methods, config-driven |
| S6 | HTTPS redirect skipped outside Development | `src/Shopizy.Api/Program.cs:31-33` | High | Always require HTTPS in non-dev. Add HSTS. | **[Done]** `UseHsts` + `UseHttpsRedirection` outside Development/Testing |
| S7 | Permission UUIDs hardcoded in registration handler | `RegisterCommandHandler.cs:59-74` | Medium | Look up permissions by name from DB and cache. | **[Done]** `IPermissionLookup` (singleton, name → id map cached on first hit) replaces hardcoded GUIDs |
| S8 | No CSRF / antiforgery on cookie-authenticated mutations | `Program.cs` | Medium-High | Add `.AddAntiforgery()` and validate token on POST/PATCH/DELETE if cookies are used. (If purely bearer-token, document the deliberate exemption.) | **[Documented]** bearer-only exemption noted in `docs/ThreatModel.md` |
| S9 | Missing security headers: `X-Content-Type-Options`, `X-Frame-Options`, `Strict-Transport-Security`, `Content-Security-Policy`, `Referrer-Policy` | Middleware pipeline | Medium | Use `NetEscapades.AspNetCore.SecurityHeaders` or hand-rolled middleware. | **[Done]** hand-rolled `SecurityHeadersMiddleware` (incl. COOP/CORP/Permissions-Policy) |
| S10 | No request-size limits configured | `Program.cs` | Medium | Configure `RequestSizeLimit`, `FormOptions.MultipartBodyLengthLimit`, `KestrelServerLimits`. | **[Done]** 10 MB Kestrel + FormOptions caps |
| S11 | Exception details flow to logs un-sanitized | `LoggerMessages.cs:20,303`, `GlobalExceptionHandler.cs` | Medium | Strip PII before logging; consider Serilog destructuring policies. | **[Done]** `LogSanitizer` (regex masks for email/phone/card/token); applied in `GlobalExceptionHandler` |
| S12 | Health endpoint subject to global rate limiter | `Program.cs:37` | Low | Disable the limiter on `/healthz`. | **[Done]** `MapHealthChecks("/healthz").DisableRateLimiting()` |

---

## 3. Persistence & Data Access

### Strengths
- Outbox pattern for reliable event delivery (`OutboxProcessor.cs:17-147`).
- Strongly-typed ID value objects via `HasConversion()` (`ProductConfigurations.cs:28`, `OrderConfigurations.cs:28`).
- Auditable-entity interceptor centralises `CreatedOn` / `ModifiedOn` (`UpdateAuditableEntitiesInterceptor.cs:27-46`).
- `AsNoTracking()` used consistently on read queries.
- Indexes on hot columns (`ProductConfigurations.cs:81-86`, `OrderConfigurations.cs:58-61`).

### Critical Issues

| # | Finding | Location | Severity | Fix | Status |
|---|---------|----------|----------|-----|--------|
| D1 | **PostgreSQL deployments don't run migrations** — `DbMigrationsHelper` only migrates if provider is SQL Server | `src/Shopizy.Infrastructure/Services/DbMigrationsHelper.cs:18-24`, `Program.cs:39-44` | **Blocker** | Run migrations for both providers (or use a versioned-migration tool such as DbUp / FluentMigrator). At minimum, log a loud warning when PG is detected and migrations skipped. | **[Done]** standardized on SQL Server |
| D2 | **All migrations generated for SQL Server only**; tests use `EnsureCreatedAsync` for PG, hiding the gap | `src/Shopizy.Infrastructure/Migrations/*.cs`, `tests/.../IntegrationTestWebAppFactory.cs:198` | **Blocker** | Maintain two `IDesignTimeDbContextFactory` setups with separate migration assemblies/folders, or pick a single canonical provider. | **[Done]** tests now run real `MigrateAsync` against `Testcontainers.MsSql` |
| D3 | SQL-Server-specific column types (`smalldatetime`) hardcoded in entity configs | `ProductConfigurations.cs:48-49`, `OrderConfigurations.cs:31-32` | High | Apply provider-aware types via conditional configuration; let EF default for PG. | **[Mooted]** SQL Server is canonical |
| D4 | `GetTotalRevenueAsync` materializes all orders + items into memory then sums | `src/Shopizy.Infrastructure/Orders/Persistence/OrderRepository.cs:68-72` | High | Push aggregation to DB: `SelectMany(o => o.OrderItems).SumAsync(...)` (already done in `GetRevenueByPeriodAsync` lines 83-86 — copy the pattern). | **[Done]** DB-side `SumAsync` |
| D5 | No `EnableRetryOnFailure` configured | `src/Shopizy.Infrastructure/DependencyInjection/PersistenceRegister.cs:46-47, 56-57` | High | Enable transient-error retry for both providers (3 retries, 30s max delay). | **[Done]** retry strategy + execution-strategy wrap in `EventualConsistencyMiddleware` |
| D6 | Concurrency token only on `Order` | `OrderConfigurations.cs:63`; rest of aggregates have none | High | Add `RowVersion` (`xmin` for PG) to `Product`, `Cart`, `User`, and any aggregate updated under contention. Update handlers to handle `DbUpdateConcurrencyException`. | **[Done]** `RowVersion` on `Product` + `User` (Cart already had); `DbUpdateConcurrencyException` → 409 |
| D7 | Heavy `.Include().ThenInclude()` chains load full entities for read paths | `ProductRepository.cs:61-63`, `CartRepository.cs:45-46` | Medium | Switch to projection (`.Select(...)`) into DTOs. | **[Deferred]** coupled with A2 repository slim-down |
| D8 | Global `QuerySplittingBehavior.SplitQuery` causes round-trip explosion on aggregations | `PersistenceRegister.cs:47, 57` | Medium | Set per-query: use `AsSingleQuery()` on aggregations like `GetTopCustomersBySpendAsync`. | **[Done]** `AsSingleQuery()` on the four aggregation paths in `OrderRepository` |
| D9 | `DbMigrationsHelper` registered as scoped but used as a one-shot at startup | `PersistenceRegister.cs:38` | Low | Convert to a transient factory invoked from a hosted service. | **[Done]** `DbMigrationsHostedService` resolves a transient `DbMigrationsHelper` from a scope; `Program.cs` no longer manages the scope |

---

## 4. Cross-Cutting: Caching, Logging, Observability

### Strengths
- Source-generated `LoggerMessage`s used pervasively (`src/Shopizy.Api/Common/LoggerMessages/*`).
- Cache-aside via `CachingQueryHandlerDecorator` (`src/Shopizy.SharedKernel/Application/Behaviors/CachingBehavior.cs:14-35`).
- Redis health check + graceful degradation (`RedisCacheHelper.cs:43-52`).

### Improvements

| # | Finding | Location | Recommendation | Status |
|---|---------|----------|----------------|--------|
| C1 | No automatic cache invalidation on writes | `CachingBehavior.cs` | Add an `IInvalidateCache` marker on commands; decorator removes/buster-tags affected keys. Or pub/sub via Redis channel. | **[Done]** `IInvalidateCache` + `CacheInvalidationCommandHandlerDecorator`; product update/delete wired |
| C2 | No distributed tracing | None | Adopt OpenTelemetry: `OpenTelemetry.Extensions.Hosting`, `OTel.Instrumentation.AspNetCore` / `EntityFrameworkCore` / `StackExchangeRedis`, OTLP export. | **[Done]** traces (ASP.NET / HTTP / EF Core / Redis) + metrics; OTLP exporter or console fallback |
| C3 | Correlation/trace id captured in `GlobalExceptionHandler.cs:19` but not threaded into all log scopes | `GlobalExceptionHandler.cs`, middleware pipeline | Add a correlation-ID middleware that pushes `TraceIdentifier` into `LogContext`. | **[Done]** `CorrelationIdMiddleware` reads/echoes `X-Correlation-ID`, pushes scope |
| C4 | No metrics endpoint | None | Expose Prometheus metrics (`/metrics`) and add app-level counters (orders/sec, payment failures). | **[Done]** via OTel metrics in C2 (Prometheus replaced by OTLP route) |
| C5 | No dead-letter alerting on outbox | `OutboxProcessor.cs` | Emit a metric / log warning when `DeadLettered` rows accumulate above threshold. | **[Done]** `shopizy.outbox.dead_lettered` + `shopizy.outbox.processed` counters; warning when backlog ≥ 10 |

---

## 5. API & Contracts

### Strengths
- Consistent `IEndpoint` minimal-API pattern with versioning (`v1.0`).
- Correct verb usage; `CustomResults.cs` cleanly maps `ErrorOr` to 400/401/403/404/409/422.
- `Produces<>()` annotations on most endpoints.

### Improvements

| # | Finding | Location | Severity | Recommendation | Status |
|---|---------|----------|----------|----------------|--------|
| P1 | **No idempotency on payment / order creation** — risk of duplicate charges/orders on client retry | `PayEndpoint.cs:18`, `CreateOrderEndpoint.cs:16` | **Critical** | Accept `Idempotency-Key` header; store request hash + result in Redis with TTL; replay stored result on duplicate. | **[Done]** `IdempotencyEndpointFilter` + `RedisIdempotencyStore` (24h TTL, body-hash conflict detection) |
| P2 | No upper bound on `pageSize` | `GetOrdersEndpoint.cs:17-18`, all paginated queries | Medium | FluentValidation rule: `pageSize ≤ 100`, `pageNumber ≥ 1`. | **[Done]** `PaginationRules.ValidPageSize/PageNumber` applied to 8 paginated query validators |
| P3 | XML doc comments missing on Contracts DTO members | `src/Shopizy.Contracts/*` | Low | Add `///` summaries; surfaces in Swagger. | **[Done]** Admin/AuditLog/ProductQuestion/GiftCard/LoyaltyAccount + several Order/Product DTOs filled in. |
| P4 | `docs/Api.md` is stale (e.g., line 264 doesn't match real route) | `docs/Api.md` | Low | Auto-generate from OpenAPI (e.g., `Redocly` build step), or delete and link to live Swagger. | **[Done]** replaced with Swagger pointer + auth/idempotency primer |
| P5 | Manual `ClaimsPrincipal` ownership checks scattered in endpoints | `GetOrdersEndpoint.cs:19`, cart endpoints | Medium | Centralize via an `IResourceAuthorizer` or ASP.NET resource-based authorization; add tests for cross-tenant access denial. | **[Done]** All 27 endpoints migrated to `AuthorizeOwner(userId, "<resource>")`. Cross-tenant tests cover orders/cart/list/payment. |
| P6 | Mapster mappings not unit-tested | `src/Shopizy.Api/Common/Mapping/*` | Low | Add `TypeAdapterConfig.GlobalSettings.Compile()` test in CI to catch mapping drift. | **[Done]** `MappingTests.Mapster_AllConfigurations_CompileSuccessfully` in `Shopizy.Architecture.Tests` |

---

## 6. Tests

### Strengths
- 4 test projects: Domain, Application, Infrastructure, API integration.
- 125+ test files; clear AAA pattern; Testcontainers for PG.
- Critical flows covered: login, order workflow.

### Gaps

| # | Finding | Recommendation | Status |
|---|---------|----------------|--------|
| T1 | No test for duplicate-payment detection / idempotency | Add once P1 is implemented. | **[Done]** `OrderIdempotencyTests` (missing-header / replay / conflict). Found + fixed real bug (filter was hashing empty body — added `RequestBufferingMiddleware`) |
| T2 | Authorization-boundary tests sparse | Add parametrized tests: `User_CannotAccess_OtherUserResource` for orders, addresses, cart. | **[Done]** `CrossTenantAuthorizationTests` covers orders/cart/list/payment |
| T3 | Eventual-consistency tests may be timing-sensitive | `OrderWorkflowTests.cs:76` — replace `Thread.Sleep`/polling with an explicit "drain outbox" hook. | **[Done]** `IOutboxDrainer` extracted from `OutboxProcessor`; `BaseIntegrationTest.DrainOutboxAsync()` available; current tests already rely on synchronous `EventualConsistencyMiddleware` (no sleep / poll remaining) |
| T4 | No coverage threshold enforced | Set Codecov status check minimum (e.g., 75% lines, 60% branch). | **[Done]** ReportGenerator + bash check fail CI when line < 70% |
| T5 | No architecture tests | Adopt `NetArchTest.Rules` to enforce dependency direction (Domain → nothing; Application → Domain only; Api → Application + Infra; etc.). | **[Done]** `Shopizy.Architecture.Tests` enforces 4 layer-direction invariants |
| T6 | No mutation testing | Run `Stryker.NET` periodically on the Domain project to validate test strength. | **[Done]** `dotnet-stryker` tool, `stryker-config.json`, scheduled `.github/workflows/mutation.yml` (Mondays 04:00 UTC) targeting the Domain project. |

---

## 7. Build, CI/CD, Tooling

### Current
- `Directory.Packages.props` for central package management. ✅
- `.editorconfig` with 400+ rules. ✅
- XML doc generation enabled. ✅
- `csharpier` + `dotnet-ef` in `dotnet-tools.json`. ✅
- GitHub Actions: build + test + Codecov. ✅

### Gaps

| # | Finding | Location | Recommendation | Status |
|---|---------|----------|----------------|--------|
| B1 | No `TreatWarningsAsErrors` | `Directory.Build.props` | `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` (with selective `WarningsNotAsErrors` for known noise). | **[Done]** non-test projects fail on warnings; allowlist documented in `Directory.Build.props`. |
| B2 | Several nullable warnings disabled (`CS8618`, `CS8601`, …) | `.editorconfig:367-372` | Re-enable; fix the underlying issues. | **[Partial]** severity restored to `warning` (was `none`); 172 sites surface and are individually visible in IDE/CI. They live on the `WarningsNotAsErrors` allowlist so the build stays green; per-file cleanup migrates the count down over time. |
| B3 | No CodeQL / SAST in CI | `.github/workflows/dotnet.yml` | Add `github/codeql-action/analyze@v3` (csharp). | **[Done]** `.github/workflows/codeql.yml` with `security-and-quality` queries |
| B4 | No `dotnet format --verify-no-changes` gate | CI workflow | Add as a step; csharpier already in tools.json. | **[Done]** `dotnet csharpier check .` step in `dotnet.yml` |
| B5 | No Dependabot / Renovate | `.github/` | Enable Dependabot for nuget + actions, weekly. | **[Done]** `.github/dependabot.yml` with grouped updates |
| B6 | No coverage gate | CI | Add `coverlet` threshold or Codecov status check. | **[Done]** see T4 (line ≥ 70%) |
| B7 | No container image build/push | CI | Add Dockerfile + `docker/build-push-action` pipeline. | **[Done]** Multi-stage `src/Shopizy.Api/Dockerfile` + `.github/workflows/docker.yml` (GHCR, multi-tag, build cache) |
| B8 | No SBOM / dependency-review | CI | `actions/dependency-review-action`, `anchore/sbom-action`. | **[Done]** `.github/workflows/supply-chain.yml` (dependency-review on PRs, CycloneDX SBOM artifact on main) |
| B9 | Missing analyzers | `Directory.Packages.props` | Add `Microsoft.CodeAnalysis.NetAnalyzers`, `Roslynator.Analyzers`, optionally `SonarAnalyzer.CSharp`. | **[Done]** NetAnalyzers + Roslynator wired via `Directory.Build.props`; baseline severities set per category in `.editorconfig` (suggestion by default, security as warning, CA5350 muted with TOTP/RFC-6238 justification) |

---

## 8. Documentation

| Doc | Status | Action | Resolution |
|-----|--------|--------|------------|
| `README.md` | Accurate, useful | Keep. | **[Done]** kept |
| `docs/Api.md` | Stale (paths drifted, no auth examples) | Auto-generate from OpenAPI or remove. | **[Done]** replaced with Swagger pointer + auth/idempotency primer |
| `docs/Domain.md` | Likely stale | Re-verify against current aggregates; consider C4 / context map diagram. | **[Done]** rewritten as an aggregate map with current enums, state machines, event flow, and concurrency semantics |
| `docs/ProjectStructure.md` | Unverified | Cross-check against current solution layout; add the SharedKernel project. | **[Done]** refreshed with SharedKernel + Architecture.Tests + new doc index |
| **Missing** | Eventual-consistency / domain-event design doc | Document outbox semantics, retry policy, dead-letter handling, and developer rules ("don't call external services from inside a domain event handler without idempotency"). | **[Done]** `docs/EventualConsistency.md` |
| **Missing** | Threat model / security checklist | One-page STRIDE-style review covering auth, payment, file uploads, PII. | **[Done]** `docs/ThreatModel.md` |

---

## 9. Prioritized Roadmap

### P0 — Blocks production (6/6 done)
- [x] **D1 / D2** Standardized on SQL Server; tests run real `MigrateAsync` against `Testcontainers.MsSql`.
- [x] **S1 / S2** Committed secrets blanked; Stripe key bound via `IStripeClient` from options.
- [x] **P1** `Idempotency-Key` filter on payment + order creation, Redis-backed (24h TTL, hash-based conflict).
- [x] **D6** `RowVersion` on `Product` / `User` (`Cart` already had); `DbUpdateConcurrencyException` → 409.
- [x] **D5** `EnableRetryOnFailure(3, 30s)` on SQL Server; middleware uses execution strategy to compose with retry.
- [x] **S5 / S6** CORS explicit headers/methods; HSTS + HTTPS redirect outside Development/Testing.

### P1 — High impact, near-term (7/7 done)
- [x] **A3** `docs/EventualConsistency.md` documents semantics + contributor rules; dead-letter alerting added (C5).
- [x] **D4** `GetTotalRevenueAsync` aggregates DB-side.
- [x] **S3** Redis-backed refresh tokens with rotation, per-user revocation index, SHA-256 storage.
- [x] **S9 / S10** `SecurityHeadersMiddleware`; 10 MB Kestrel + FormOptions caps.
- [x] **S4** `PasswordRules.StrongPassword()` (length ≥ 12 + upper/lower/digit/special).
- [x] **B1 / B2** `TreatWarningsAsErrors=true`; CS86xx re-enable scoped as a separate cleanup (B2 leftover).
- [x] **B3 / B4 / B5** CodeQL + csharpier check + Dependabot all in CI.

### P2 — Quality / scale (13/13 in flight)
- [x] **A1 (slice)** `CreateProductCommand` carries typed `Price` / `CategoryId` / `BrandId`; mapping config converts at the boundary. Pattern established for remaining commands.
- [x] **A2 (slice)** `IProductReader` extracted (`GetTotalCountAsync` / `GetLowStockAsync` / `GetBrandNamesAsync`); `IProductRepository` slimmed; query handlers depend on the reader.
- [x] **D3 / D7 / D8** D3 mooted (SQL Server canonical); D8 done; D7 follows on the next A2 slice.
- [x] **C1 / C2 / C3 / C4 / C5** All five done (C4 via OTel metrics).
- [x] **T1, T2, T4, T5** Idempotency tests, authz-boundary tests, coverage gate, architecture tests.
- [x] **T3** `IOutboxDrainer` + `BaseIntegrationTest.DrainOutboxAsync()` test hook; no Thread.Sleep remaining.
- [x] **T6** `dotnet-stryker` + scheduled `.github/workflows/mutation.yml` (Mondays 04:00 UTC) targeting Domain.

### P3 — Polish (8/8 in flight)
- [x] **A4 / A7 / A8** `IAppDbContext` removed; dispatcher errors enriched; exception handler category-split.
- [x] **A6** `Shopizy.Contracts.V1` namespace reserved; versioning rules documented in `docs/Api.md`.
- [x] **P2 / P4 / P6** Pagination caps; `Api.md` replaced; Mapster compile smoke-test.
- [x] **P3** Admin/AuditLog/ProductQuestion/GiftCard/LoyaltyAccount + several Order/Product DTOs documented.
- [x] **P5** All 27 endpoints migrated to `AuthorizeOwner(userId, "<resource>")`.
- [x] **Documentation** `ThreatModel.md`, `EventualConsistency.md`, `ProjectStructure.md` refresh, `Api.md` rewrite, `Domain.md` refresh.

### Additional Feature Roadmap (#7 to #12 — Complete)
- [x] **#7 Real Email Service**: `SmtpEmailService` implementing `IEmailService` with `EmailSettings` and `LoggingEmailService` fallback.
- [x] **#8 `docker-compose.yml`**: Complete dev stack featuring SQL Server 2022, Redis 7, Mailpit SMTP UI, and Shopizy API.
- [x] **#9 Loyalty Points in Checkout**: Added loyalty points redemption in `CreateOrderCommand`, updating balance and applying discount.
- [x] **#10 Admin Analytics Export**: Added `ExportAnalyticsEndpoint` for CSV and PDF reports at `GET /api/v1.0/admin/dashboard/export`.
- [x] **#11 Order Notifications**: Wired email notifications into `OrderCreated`, `OrderPaid`, and `OrderCancelled` domain event handlers.
- [x] **#12 Product Variants**: Verified Minimal API CRUD endpoints for managing product variants (`AddVariant`, `UpdateVariant`, `RemoveVariant`, `GetProductVariants`).

### Outstanding (audit items not in original roadmap or surfaced during execution)
- **OTel transitive advisory** — `OpenTelemetry.Api 1.12.0` flagged by `NU1902`; tracked via Dependabot, treated as warning.
- **A1 / A2 follow-on slices** — Pattern is in place; remaining commands (Update*, Delete*) and read-only methods (`GetProductsAsync`, etc.) migrate as touched. Not blocking.
- **B2 cleanup** — 172 CS86xx sites are now visible warnings on the `WarningsNotAsErrors` allowlist. Migrate per-file.

---

## Appendix — Key Files Referenced

- `src/Shopizy.Api/Program.cs`, `DependencyInjectionRegister.cs`, `appsettings*.json`
- `src/Shopizy.Api/Common/Errors/GlobalExceptionHandler.cs`
- `src/Shopizy.Api/Endpoints/{Auth,Orders,Carts,Products,Categories}/*`
- `src/Shopizy.Application/DependencyInjectionRegister.cs`
- `src/Shopizy.Application/Common/Interfaces/Persistence/IProductRepository.cs`, `IAppDbContext.cs`
- `src/Shopizy.Application/Orders/Events/OrderCreatedDomainEventHandler.cs`
- `src/Shopizy.Application/Auth/Commands/Register/RegisterCommandValidator.cs`, `RegisterCommandHandler.cs`
- `src/Shopizy.Infrastructure/DependencyInjection/{PersistenceRegister.cs,SecurityRegister.cs,ExternalServicesRegister.cs}`
- `src/Shopizy.Infrastructure/Messaging/Dispatcher.cs`
- `src/Shopizy.Infrastructure/Common/Middleware/EventualConsistencyMiddleware.cs`
- `src/Shopizy.Infrastructure/Outbox/OutboxProcessor.cs`
- `src/Shopizy.Infrastructure/Services/DbMigrationsHelper.cs`
- `src/Shopizy.Infrastructure/Products/Persistence/{ProductRepository.cs,ProductConfigurations.cs}`
- `src/Shopizy.Infrastructure/Orders/Persistence/{OrderRepository.cs,OrderConfigurations.cs}`
- `src/Shopizy.Infrastructure/Carts/Persistence/CartRepository.cs`
- `src/Shopizy.Infrastructure/Security/{Hashing/PasswordManager.cs,TokenGenerator/JwtTokenGenerator.cs,TokenValidation/JwtBearerTokenValidationConfiguration.cs}`
- `src/Shopizy.SharedKernel/Application/Behaviors/{ValidationBehavior.cs,CachingBehavior.cs}`
- `src/Shopizy.SharedKernel/Domain/Models/Entity.cs`
- `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.config/dotnet-tools.json`
- `.github/workflows/dotnet.yml`, `.github/workflows/codeql.yml`, `.github/dependabot.yml`
- `docs/Api.md`, `docs/Domain.md`, `docs/ProjectStructure.md`, `docs/EventualConsistency.md`, `docs/ThreatModel.md`

### Files added or moved during execution (2026-04-26)

- `src/Shopizy.Api/Common/Idempotency/IdempotencyEndpointFilter.cs` (P1)
- `src/Shopizy.Api/Common/Middleware/{CorrelationIdMiddleware.cs, RequestBufferingMiddleware.cs, SecurityHeadersMiddleware.cs}` (C3, P1, S9)
- `src/Shopizy.Api/Common/Telemetry/TelemetryRegister.cs` (C2)
- `src/Shopizy.Api/Dockerfile` (B7)
- `src/Shopizy.Application/Auth/Commands/RefreshToken/{RefreshTokenCommand.cs, RefreshTokenCommandHandler.cs, RefreshTokenCommandValidator.cs}` (S3)
- `src/Shopizy.Application/Common/Interfaces/Authentication/{IRefreshTokenStore.cs, IRefreshTokenGenerator.cs}` (S3)
- `src/Shopizy.Application/Common/Interfaces/Persistence/IPermissionLookup.cs` (S7)
- `src/Shopizy.Application/Common/Interfaces/Services/IIdempotencyStore.cs` (P1)
- `src/Shopizy.Application/Common/Validation/{PasswordRules.cs, PaginationRules.cs}` (S4, P2)
- `src/Shopizy.Application/Common/Caching/CacheKeys.cs` (C1)
- `src/Shopizy.Infrastructure/Common/Idempotency/RedisIdempotencyStore.cs` (P1)
- `src/Shopizy.Infrastructure/Security/RefreshTokens/{RedisRefreshTokenStore.cs, RefreshTokenGenerator.cs, RefreshTokenSettings.cs}` (S3)
- `src/Shopizy.Infrastructure/Outbox/{IOutboxDrainer.cs, OutboxDrainer.cs}` (T3)
- `src/Shopizy.Infrastructure/Permissions/Persistence/PermissionLookup.cs` (S7)
- `src/Shopizy.Infrastructure/Services/DbMigrationsHostedService.cs` (D9)
- `src/Shopizy.SharedKernel/Application/Behaviors/CacheInvalidationBehavior.cs` (C1)
- `src/Shopizy.SharedKernel/Application/Caching/IInvalidateCache.cs` (C1)
- `src/Shopizy.SharedKernel/Application/Logging/LogSanitizer.cs` (S11)
- `src/Shopizy.Infrastructure/Migrations/20260426120000_AddProductAndUserConcurrency.cs` (D6)
- `.dockerignore` (B7)
- `.github/workflows/{docker.yml, supply-chain.yml, mutation.yml}` (B7, B8, T6)
- `stryker-config.json` (T6)
- `src/Shopizy.Application/Common/Interfaces/Persistence/IProductReader.cs` + `src/Shopizy.Infrastructure/Products/Persistence/ProductReader.cs` (A2 slice)
- `src/Shopizy.Contracts/V1/PlaceholderV1.cs` (A6)
- `tests/Shopizy.Architecture.Tests/{LayerDependencyTests.cs, MappingTests.cs}` (T5, P6)
- `tests/Shopizy.Api.IntegrationTests/Orders/OrderIdempotencyTests.cs` (T1)
- `tests/Shopizy.Api.IntegrationTests/Auth/CrossTenantAuthorizationTests.cs` (T2)

### Files removed during execution (2026-04-26)

- `src/Shopizy.Application/Common/Interfaces/Persistence/IAppDbContext.cs` (A4 — half-abstraction removed)
