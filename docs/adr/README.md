# Architecture Decision Records (ADRs)

Architecture Decision Records capture significant technical, structural, and infrastructure decisions made in **Shopizy**. They provide the rationale, trade-offs, and invariants behind our architectural choices to guide contributors and autonomous AI coding agents.

---

## 📑 ADR Index

| ADR | Title | Status | Date | Area |
| :--- | :--- | :--- | :--- | :--- |
| [ADR-0000](0000-record-architecture-decisions.md) | Record Architecture Decisions | Accepted | 2026-11-07 | Architecture & Governance |
| [ADR-0001](0001-stripe-v53-and-package-modernization.md) | Stripe.net v53 Migration and Package Modernization | Accepted | 2026-11-07 | Payments & Dependencies |
| [ADR-0002](0002-testing-paradigm-and-agentic-verification.md) | Testing Paradigm and Agentic Verification | Accepted | 2026-11-07 | Testing & Quality Assurance |

---

## 🧭 How to Create a New ADR

1. **Numbering and Filename**: Identify the next sequential 4-digit number. Name the file `NNNN-descriptive-title.md` inside `docs/adr/` (e.g., `0003-redis-distributed-caching.md`).
2. **Template Structure**:
   ```markdown
   # ADR-NNNN: Title

   ## Status
   Proposed | Accepted | Superseded by ADR-XXXX | Deprecated

   ## Date
   YYYY-MM-DD

   ## Context
   [Describe the problem, context, and forces influencing the decision]

   ## Decision
   [Describe the selected option, design choices, and architectural invariants]

   ## Alternatives Considered
   - **Alternative 1**: Pros, cons, reason for rejection
   - **Alternative 2**: Pros, cons, reason for rejection

   ## Consequences
   - Positive, negative, and neutral impacts
   ```
3. **Lifecycle**:
   ```text
   PROPOSED ──> ACCEPTED ──> (SUPERSEDED by ADR-XXXX or DEPRECATED)
   ```
   *Never delete old ADRs.* When an architectural approach changes, record a new ADR that references and supersedes the predecessor.
4. **Update Index**: Add the new record to the index table in this file (`docs/adr/README.md`).
