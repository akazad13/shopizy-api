# ADR-0002: Testing Paradigm and Agentic Verification

## Status
Accepted

## Date
2026-11-07

## Context
Shopizy maintains a comprehensive test suite of 1,164+ automated tests spanning Domain entities, Application CQRS handlers, Infrastructure integrations, API endpoints, and architectural fitness. As autonomous AI coding agents (Antigravity, Claude Code, Cursor, Copilot) increasingly contribute code, refactor components, and fix defects, an ambiguous or fragile testing strategy creates severe risks:
1. **Brittle Mock Verification**: Interaction-based tests that verify internal method call counts (`Verify(x => x.Method(), Times.Once)`) break during harmless refactoring even when behavioral correctness is preserved.
2. **Slow Agent Feedback Loops**: Over-reliance on heavy integration or container-based tests stalls fast agent iteration cycles and increases token and compute costs.
3. **Architectural Drift**: Inadvertent layer leaks (e.g., Domain referencing Application or Infrastructure) can slip in silently without automated boundary enforcement.
4. **Untracked Defect Regressions**: Fixing bugs without first proving their reproduction leads to phantom fixes and recurring regressions.
5. **Legacy Testing Dependencies**: Historical usage of `Moq` introduces supply-chain friction, while reflection-heavy automappers (`AutoMapper`) obscure mapping failures until runtime.

We need a codified testing paradigm that guarantees rapid developer/agent feedback, maximum refactoring freedom, architectural fitness, and zero regression.

## Decision
We formally adopt a standardized Testing Paradigm and Agentic Verification Framework across Shopizy, guided by Test-Driven Development (TDD) best practices.

### 1. The Test Pyramid Distribution

The test suite is structured into three tiers optimized for fast feedback and deterministic confidence:

```text
          ╱╲
         ╱  ╲         ~5% Integration Tests (Medium/Large)
        ╱────╲        Testcontainers MS SQL Server, Redis, Real WebApplicationFactory
       ╱      ╲
      ╱────────╲      ~10% Architecture Tests (Small)
     ╱          ╲     NetArchTest rules, Mapster config compilation
    ╱────────────╲
   ╱              ╲   ~85% Fast Unit Tests (Small)
  ╱                ╲  Domain aggregates & value objects, Application CQRS handlers,
 ╱──────────────────╲ in-memory test doubles (NSubstitute), milliseconds execution
```

- **~85% Unit Tests (`Shopizy.Domain.UnitTests`, `Shopizy.Application.UnitTests`, `Shopizy.Contracts.UnitTests`, `Shopizy.Infrastructure.UnitTests`)**:
  - Focus on pure domain invariants, business calculations, CQRS commands/queries, and validation logic.
  - Zero I/O, no network, no database, no Docker dependencies.
  - Execution time: Milliseconds per test.
- **~10% Architecture Tests (`Shopizy.Architecture.Tests`)**:
  - NetArchTest rules enforcing Clean Architecture layer boundaries:
    - `Domain` has no dependencies on `Application`, `Infrastructure`, `Api`, or `Contracts`.
    - `Application` depends strictly on `Domain` and `SharedKernel` (no `Infrastructure` or `Api`).
    - `Infrastructure` does not reference `Api`.
    - `Contracts` has no dependency on internal application layers.
  - Compile-time verification of mapping registrations (`Mapster.TypeAdapterConfig.Compile()`).
- **~5% Integration Tests (`Shopizy.Api.IntegrationTests`)**:
  - Spin up disposable MS SQL Server 2022 containers via Testcontainers.
  - Verify database transactions, EF Core migrations, concurrency conflicts (`RowVersion`), in-transaction domain event dispatching, and critical end-to-end API workflows.

### 2. Core Testing Invariants

Autonomous agents and human contributors must uphold the following four invariants:

1. **State Over Interaction Verification**:
   - Assert on resulting state and returned outputs (e.g., entity properties, returned `ErrorOr<T>` values, database rows, generated domain events), **never** on private invocation sequences or method call counts.
   - Interaction testing (`mock.Received().SomeInternalCall()`) binds tests to implementation details and penalizes valid refactorings. Use mocks/test doubles only at outer system boundaries (external payment gateways, email dispatchers).
2. **DAMP Over DRY Test Design**:
   - Tests favor **DAMP (Descriptive And Meaningful Phrases)** over DRY (Don't Repeat Yourself).
   - Each test must tell an explicit, self-contained story following Arrange-Act-Assert (AAA).
   - Avoid deep inheritance hierarchies or obscured shared test setups where readers must navigate across multiple files to understand the test scenario.
3. **The Prove-It Bug Reproduction Pattern**:
   - When addressing a bug report or regression:
     1. **RED**: Write a focused test that reproduces the bug. The test must fail, confirming the defect.
     2. **GREEN**: Implement the minimal production fix to make the test pass.
     3. **REFACTOR**: Clean up implementation without altering behavior, ensuring the entire test suite remains green.
   - Code fixes without a reproduction test are strictly unacceptable.
4. **Zero Moq / Zero AutoMapper Policy**:
   - `Moq` is prohibited for all new tests. In-memory test doubles must use `NSubstitute`.
   - `AutoMapper` is prohibited across the solution. Use explicit mapping or `Mapster` with validated configurations.

### 3. Fast Agentic Test Loops

To maintain fast agentic iteration without executing the full 1,164-test suite on every single prompt turn, agents must utilize layered verification loops:

1. **Inner Loop (Micro Iteration — 1 to 3 seconds)**:
   - Run only the single test or class under active modification:
     ```bash
     dotnet test tests/Shopizy.Domain.UnitTests --filter "FullyQualifiedName~OrderTests"
     dotnet test tests/Shopizy.Application.UnitTests --filter "FullyQualifiedName~CreateOrderCommandHandlerTests"
     ```
2. **Sub-Suite Verification (3 to 6 seconds)**:
   - Run the target project's unit tests:
     ```bash
     dotnet test tests/Shopizy.Domain.UnitTests
     dotnet test tests/Shopizy.Application.UnitTests
     ```
3. **Architecture Fitness Gate (2 to 3 seconds)**:
   - Validate that no layer violations or invalid references were introduced:
     ```bash
     dotnet test tests/Shopizy.Architecture.Tests
     ```
4. **Pre-Completion Full Gate**:
   - Before completing tasks, verify all unit and architecture suites pass cleanly:
     ```bash
     dotnet test Shopizy.sln --filter "FullyQualifiedName!~IntegrationTests"
     ```
   - Run `tests/Shopizy.Api.IntegrationTests` only when touching database schemas, migrations, or end-to-end HTTP pipeline behaviors (requires Docker).

## Alternatives Considered

- **Interaction-Heavy Mock Verification**:
  - *Pros*: Tests can be written without setting up real domain aggregates.
  - *Cons*: Mocks frequently pass while runtime code fails; brittle tests break whenever internal methods are refactored or renamed.
  - *Rejected*: Incompatible with long-term maintainability and autonomous agent refactoring.
- **Integration-Heavy Test Pyramid**:
  - *Pros*: Exercises the real database and full ASP.NET Core pipeline.
  - *Cons*: Extremely slow (>5 minutes), high compute/resource consumption, dependent on Docker daemon, flaky network/container initialization.
  - *Rejected*: Stifles agent responsiveness and rapid feedback cycles.
- **Relying Only on Code Reviews for Architecture Boundaries**:
  - *Pros*: No need to write or maintain architecture test projects.
  - *Cons*: Humans and AI agents inevitably make mistakes; layer leaks slip into PRs.
  - *Rejected*: Automated NetArchTest enforcement provides a deterministic, zero-cost guarantee on every build.

## Consequences

- **Positive**:
  - AI agents can autonomously develop and refactor features with 2–5 second feedback loops.
  - Zero accidental Clean Architecture layer violations due to automated fitness gates.
  - Bug fixes are permanently guarded against regression via the Prove-It pattern.
  - Tests serve as live, readable executable specifications of domain invariants.
- **Negative / Neutral**:
  - Testcontainers-based integration tests require Docker running on the host machine and are reserved for CI and targeted end-to-end validation.
  - Existing legacy tests using Moq will be phased out incrementally in favor of NSubstitute.
