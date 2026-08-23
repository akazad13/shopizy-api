# Eventual Consistency & Domain Events

[⬅️ Back to README](../README.md) · [Repository Memory](RepositoryMemory.md) · [Domain Models](Domain.md) · [Threat Model](ThreatModel.md)

This doc describes how Shopizy delivers domain events reliably and the rules contributors must follow when writing event handlers.

## Goals

- Domain events fire **inside the same transaction** as the aggregate change, so handler side-effects are atomic with the originating write.
- If an in-process dispatch fails or the process crashes, the event is retried by the outbox processor.
- Repeated failures are surfaced as dead-letters, not silently dropped.

## Components

| Component | Responsibility | Source |
|-----------|----------------|--------|
| `Entity.AddDomainEvent` / `PopDomainEvents` | Aggregates collect events while behaviour mutates state. | `src/Shopizy.SharedKernel/Domain/Models/Entity.cs` |
| `AppDbContext.SaveChangesAsync` | Pops events from tracked aggregates, writes one `OutboxMessage` row per event in the same transaction, and queues `(event, outboxId)` tuples on `HttpContext.Items`. | `src/Shopizy.Infrastructure/Common/Persistence/AppDbContext.cs` |
| `EventualConsistencyMiddleware` | Wraps each non-GET request in a transaction (using the EF Core execution strategy so it composes with `EnableRetryOnFailure`). After `Next(context)` and **before commit**, drains the queue and calls in-process handlers. Successful dispatches mark the matching outbox row processed; a handler exception aborts the whole transaction. | `src/Shopizy.Infrastructure/Common/Middleware/EventualConsistencyMiddleware.cs` |
| `OutboxProcessor` | Background service. Every 30s, picks up outbox rows older than 5 minutes that are neither processed nor dead-lettered, deserialises them, and republishes via the dispatcher. Acts as a backstop for crash-after-dispatch / crash-mid-commit cases. | `src/Shopizy.Infrastructure/Outbox/OutboxProcessor.cs` |
| `OutboxDrainer` (`IOutboxDrainer`) | Synchronous drain of all pending outbox messages. Tests use it to flush the outbox without sleeping. | `src/Shopizy.Infrastructure/Outbox/OutboxDrainer.cs` |
| `OutboxMessage` | Persistent envelope: `{ Id, Type, Content (JSON), OccurredOn, ProcessedOn?, DeadLetteredOn?, DeadLetterReason? }`. Indexed on `ProcessedOn` and `DeadLetteredOn`. | `src/Shopizy.Infrastructure/Outbox/OutboxMessage.cs` |

## End-to-end flow

1. **Command** mutates aggregates and calls `AddDomainEvent`.
2. **`SaveChangesAsync`** writes aggregate changes + `OutboxMessage` rows in one transaction; events are also queued on `HttpContext.Items`.
3. **Middleware** drains the queue inside the still-open transaction, dispatching each event in turn. Each handler runs against the same scoped `AppDbContext`, so its `SaveChangesAsync` writes flush into the same transaction. The matching outbox row is marked `ProcessedOn = now` immediately on success.
4. **Commit.** If any handler threw, the whole transaction rolls back — the original aggregate change, its outbox rows, and any handler-side writes are all undone. The caller sees a 500.
5. **Background worker** (`OutboxProcessor`) sweeps stale-pending rows every 30s. With the inline-dispatch model, this is mostly a backstop for crash-mid-commit scenarios; in normal operation rows are already `ProcessedOn`-marked at commit time.

## Why in-transaction dispatch

Earlier the middleware committed first and dispatched after. That model created a window where the order would be created but stock-reduction (handler side) could fail, leaving phantom orders against unreserved inventory. Inline dispatch closes that window: a handler failure aborts the order entirely, and operations are re-tried by retrying the request.

The trade-off is that handler errors become user-facing 500s. In practice this is the *right* failure for stock/cart/payment-completion handlers — better to fail fast than to ship inconsistent state.

## Retry boundaries

- **In-transaction retry:** none on the dispatch path itself. The EF Core execution strategy (`EnableRetryOnFailure(3, 30s)`) wraps the whole `strategy.ExecuteAsync(...)` block, so transient SQL failures (deadlocks, connection drops) cause the entire request body — handlers and all — to re-run. Handlers must therefore be safe to re-execute idempotently against the same starting state.
- **Background retry:** every 30s, runs only against rows older than 5 minutes (so the in-process path gets a chance first).
- **Dead-letter triggers:** unresolvable `Type`, undeserialisable `Content`. Runtime exceptions during dispatch are surfaced via `DomainEventDeadLettered` log + the metric counter; the row stays unprocessed and the request fails.

## Rules for contributors

1. **Handlers must be DB-only by default.** External calls (HTTP, email, payment processor) inside a handler are not transactional and may execute multiple times under EF retry. If you must do external work, write a follow-up outbox row inside the handler and process it from a dedicated worker with its own idempotency key.
2. **Idempotency under retry.** Because the EF execution strategy can re-run the request body, a handler that, say, decrements stock should be safe against re-execution against the same starting aggregate. Today's handlers operate on aggregates fetched fresh inside the handler, so the EF concurrency tokens (`RowVersion` on `Product`, `Cart`, `User`, `Order`) detect the second pass and either succeed or 409 cleanly.
3. **Don't begin or commit transactions inside event handlers.** The middleware owns the transaction; opening another inside a handler can deadlock or break atomicity.
4. **Don't swallow exceptions.** If your handler can't complete, throw — that's the signal to roll back the whole request. Returning silently means the order ships without its side-effects.
5. **Keep event payloads serialisable with `System.Text.Json`.** The processor deserialises with the assembly-qualified type name from the `Type` column, so renaming or moving an event class breaks pending rows. Coordinate with a rename migration if needed.

## Operational notes

- **Dead-letter alerting:** `OutboxProcessor` emits the `shopizy.outbox.dead_lettered` counter (OTel metrics) and logs a warning when the dead-letter backlog reaches 10. Wire that into your alerting stack.
- **Replaying a dead-letter:** `UPDATE OutboxMessages SET DeadLetteredOn = NULL, DeadLetterReason = NULL WHERE Id = @Id;` puts it back in the queue. Don't blanket-replay — investigate the root cause first.
- **Outbox table growth:** processed rows are not pruned automatically. Plan a retention job (e.g., delete `ProcessedOn < now - 30 days`) once the volume justifies it.
- **Test drain hook:** `BaseIntegrationTest.DrainOutboxAsync()` (resolves `IOutboxDrainer`) flushes any pending messages synchronously. Use it instead of polling/sleep in tests that assert on event-driven side-effects.
