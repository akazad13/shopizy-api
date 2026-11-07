# ADR-0001: Stripe.net v53 Migration and Package Modernization

## Status
Accepted

## Date
2026-11-07

## Context
During the repository-wide dependency upgrade, NuGet packages in Shopizy (ASP.NET Core / Clean Architecture e-commerce monolith) were updated to their latest stable releases, including Stripe.net v53.

Stripe.net v53 deprecated and removed `PaymentMethodTypes` on `PaymentIntentCreateOptions`, replacing it with `AllowedPaymentMethodTypes` to align with Stripe's modern Payment Methods API. This was a breaking compilation error across payment intent creation handlers in `Shopizy.Application`.

## Decision
1. **Adopt `AllowedPaymentMethodTypes`**: Update payment intent generation across `Shopizy.Application` and `Shopizy.Infrastructure` to use `PaymentIntentCreateOptions.AllowedPaymentMethodTypes = new List<string> { "card" }`.
2. **Modernize Solution Packages**: Upgrade EF Core, MediatR, FluentValidation, and Mapster to ensure complete alignment with .NET LTS/current.
3. **Document Invariant**: Invariant documented in AGENTS.md so future contributors and AI agents do not revert to deprecated `PaymentMethodTypes`.

## Alternatives Considered
- **Pin Stripe.net to v45**: Pinning delays required security updates and keeps the payment gateway tied to legacy Stripe API versions that will eventually be retired.
- **Switch to Hosted Checkout Sessions**: Significantly higher refactoring cost for the application compared to adapting `AllowedPaymentMethodTypes`.

## Consequences
- Clean compilation without deprecation warnings.
- Continuous access to Stripe API security updates and enhanced SCA (Strong Customer Authentication) compliance.
