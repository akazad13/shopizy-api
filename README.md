# Shopizy - Online Store Management API

[![.NET](https://github.com/akazad13/shopizy/actions/workflows/dotnet.yml/badge.svg)](https://github.com/akazad13/shopizy/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/akazad13/shopizy-api/branch/main/graph/badge.svg)](https://codecov.io/gh/akazad13/shopizy-api)

Shopizy is a robust and scalable Online Store Management API built with .NET 10, following the principles of Clean Architecture. It provides a comprehensive set of endpoints for managing products, categories, carts, orders, and users, designed to support modern e-commerce applications.

## 🚀 About The Project

Shopizy aims to provide a solid foundation for e-commerce platforms. It separates concerns into distinct layers (Domain, Application, Infrastructure, API) to ensure maintainability, testability, and scalability.

Key features include:
-   **Product Catalog & Brand Management**: Hierarchical categories, brands, variants, barcode/SKU, and Cloudinary image galleries.
-   **Faceted Search & Typo Tolerance**: Multi-token search with Levenshtein fuzzy distance, synonym expansion, dynamic facet buckets (categories, brands, price tiers, ratings), and "Did You Mean?" suggestions.
-   **Shopping Cart & Recovery**: High-performance cart management with an automated background worker (`AbandonedCartReminderWorker`) for abandoned cart re-engagement.
-   **Order Lifecycle & Inventory Protection**: Resilient state machine (`Pending` -> `Processing` -> `Shipping` -> `Delivered`), atomic stock reservation, and automated 15-minute unpaid order expiration (`PendingOrderExpirationWorker`).
-   **Payments & Webhooks**: Stripe payment intent integration, idempotent webhook handling (`payment_intent.succeeded`, `payment_intent.payment_failed`), automatic refunds, and idempotency key protection (`X-Idempotency-Key`).
-   **Shipping Logistics & Live Tracking**: Multi-carrier rate estimation (USPS, UPS, FedEx, DHL), free-shipping thresholds, and package tracking checkpoints.
-   **Real-Time Operations**: SignalR WebSocket hubs for customer order status updates (`/hubs/orders`) and live merchant metrics (`/hubs/admin-dashboard`).
-   **Customer Engagement & Social Proof**: Verified-purchase product reviews, community upvotes, wishlists with automatic price-drop and back-in-stock alerts, loyalty points, and digital gift cards.
-   **Identity, RBAC & Security**: JWT authentication with sliding Redis refresh token rotation, granular permissions via cached `IPermissionLookup`, PBKDF2-HMACSHA512 password hashing, rate limiting, and security headers.
-   **Clean Architecture & Eventual Consistency**: Strict layer boundaries, custom mediator pipeline with Scrutor decorators, in-transaction domain event dispatching, and background outbox processing.

## 🛠️ Built With

*   [ASP.NET Core 10](https://dotnet.microsoft.com/en-us/apps/aspnet) - The web framework used (Minimal APIs).
*   **Custom Messaging System** - Handrolled mediator implementation for Commands, Queries, and Domain Events (`IDispatcher`).
*   [Scrutor](https://github.com/khellang/Scrutor) - Assembly scanning and decorator support for dependency injection pipelines.
*   [Mapster](https://github.com/MapsterMapper/Mapster) - High-performance object-to-object mapper.
*   [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - Object-Relational Mapper (ORM) with migrations and optimistic concurrency (`RowVersion`).
*   [Microsoft SQL Server](https://www.microsoft.com/sql-server) - Relational database engine (SQL Server 2022).
*   [Redis](https://redis.io/) - In-memory data store for caching, sliding refresh tokens, and request idempotency.
*   [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) - Real-time two-way WebSocket hubs.
*   [Stripe](https://stripe.com/) - Payment processing and webhook reconciliation.
*   [Cloudinary](https://cloudinary.com/) - Cloud media storage and asset optimization.
*   [Swagger / OpenAPI](https://swagger.io/) - API documentation and interactive explorer.
*   [ErrorOr](https://github.com/amantinband/error-or) - Fluent discriminated unions for domain and application error handling.
*   [xUnit](https://xunit.net/) - Unit and integration testing framework.
*   [Shouldly](https://shouldly.io/) - Fluent assertion library with descriptive messages.
*   [Moq](https://github.com/devlooped/moq) - Mocking library for unit testing.
*   [Testcontainers](https://testcontainers.com/) - Ephemeral Microsoft SQL Server 2022 container instances in Docker for integration tests.

## 🏁 Getting Started

Follow these steps to get a local copy up and running.

### Prerequisites

*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   [Docker & Docker Compose](https://www.docker.com/) (recommended for local SQL Server 2022, Redis 7, and Mailpit) or local instances of:
    *   [Microsoft SQL Server 2022](https://www.microsoft.com/sql-server)
    *   [Redis 7](https://redis.io/download)

### Installation

1.  **Clone the repository**
    ```sh
    git clone https://github.com/akazad13/shopizy.git
    ```
2.  **Navigate to the project directory**
    ```sh
    cd shopizy
    ```
3.  **Start dependencies via Docker Compose** *(Optional, recommended)*
    ```sh
    docker-compose up -d
    ```
    This spins up Microsoft SQL Server 2022, Redis 7, and Mailpit for email preview.
4.  **Restore dependencies**
    ```sh
    dotnet restore
    ```
5.  **Configure Database**
    Update the connection string in `src/Shopizy.Api/appsettings.json` (or `appsettings.Development.json`):
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost,1433;Database=ShopizyDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
    }
    ```
6.  **Run Migrations**
    The application applies migrations automatically on startup via `DbMigrationsHostedService`, or you can apply them manually:
    ```sh
    dotnet ef database update --project src/Shopizy.Infrastructure --startup-project src/Shopizy.Api
    ```
7.  **Run the API**
    ```sh
    dotnet run --project src/Shopizy.Api
    ```

## 🔌 Usage

Once the API is running, you can explore the endpoints using Swagger UI or Bruno.

*   **Swagger UI**: `https://localhost:7172/swagger` (or `http://127.0.0.1:18080/swagger`)
*   **Bruno Collection**: Manual testing collections located under `Bruno/Shopizy/`.

### Key Endpoints

*   `POST /api/v1.0/auth/register` & `POST /api/v1.0/auth/login`: Customer & Admin authentication.
*   `POST /api/v1.0/products/faceted-search`: Multi-token search with typo tolerance and dynamic facets.
*   `GET /api/v1.0/products`: Search and filter catalog products.
*   `POST /api/v1.0/users/{userId}/cart/items`: Manage user shopping cart items.
*   `POST /api/v1.0/users/{userId}/orders`: Place an order (requires `X-Idempotency-Key`).
*   `POST /api/v1.0/shipping/estimate-rates`: Live carrier rate quotes (USPS, UPS, FedEx, DHL).
*   `POST /api/v1.0/webhooks/stripe`: Stripe payment status webhook reconciliation.
*   `PATCH /api/v1.0/admin/users/{userId}/role`: Administrator role and permission assignment.
*   `/hubs/orders` & `/hubs/admin-dashboard`: Real-time WebSocket channels.

## 📚 Documentation

For detailed guides and architecture specifications, please refer to:

*   **[Product Requirements Document (PRD)](docs/ProductRequirementsDocument.md)**: Product vision, user personas, functional epics, critical business invariants, and phased engineering roadmap.
*   **[Repository Memory & Architecture Reference](docs/RepositoryMemory.md)**: High-density map of domain aggregates, messaging pipelines, security, and developer quick reference.
*   **[Feature Documentation](docs/FeatureDocumentation.md)**: End-to-end guide of all platform features, workflows, and business logic.
*   **[Technical Documentation](docs/TechnicalDocumentation.md)**: Architectural patterns, DDD design, CQRS messaging, caching, and infrastructure references.
*   **[API Documentation](docs/Api.md)**: Details on API endpoints, OpenAPI/Swagger specifications, and contract versioning.
*   **[Domain Models](docs/Domain.md)**: In-depth explanation of domain aggregates, entities, and value objects.
*   **[Eventual Consistency & Outbox](docs/EventualConsistency.md)**: In-transaction domain event dispatching and outbox worker policies.
*   **[Frontend Handoff Guide](docs/FrontendHandoffDoc.md)**: TypeScript interfaces, SignalR integration, and endpoint specifications for frontend developers.
*   **[Project Structure Reference](docs/ProjectStructure.md)**: Detailed project file structure and layer dependency rules.
*   **[Threat Model & Security Review](docs/ThreatModel.md)**: STRIDE analysis of identity, storage, and API surface.
*   **[Improvement Scope & Roadmap](docs/ImprovementScope.md)**: Full-solution architectural audit and completed roadmap.

## 🏗️ Architecture

The solution follows **Clean Architecture** and **Domain-Driven Design (DDD)** principles:

*   **Shopizy.Domain**: Contains enterprise logic and types (Aggregates, Entities, Value Objects, Enums, Domain Events). No external dependencies.
*   **Shopizy.SharedKernel**: Contains shared domain and application primitives (`Entity`, `AggregateRoot`, `ValueObject`, `IDispatcher`, `IUnitOfWork`, `ErrorOr`).
*   **Shopizy.Application**: Contains business logic and use cases. Implements a custom CQRS pattern with Commands and Queries.
    *   **Custom Dispatcher**: A custom `IDispatcher` resolves and executes handlers using dependency injection.
    *   **Decorator Pattern**: Uses Scrutor to apply cross-cutting concerns like Validation, Unit of Work, and Caching via decorators, avoiding library-heavy pipeline behaviors.
*   **Shopizy.Infrastructure**: Implements interfaces defined in Application (Data access, External services, Redis, SignalR, Outbox Processor). Depends on Application.
    *   **Database Engine**: Configured for Microsoft SQL Server 2022 using EF Core 10 migrations.
    *   **Distributed State**: Redis-backed caching (`ICacheHelper`), sliding refresh tokens (`IRefreshTokenStore`), and idempotency store (`IIdempotencyStore`).
*   **Shopizy.Api**: The entry point (Minimal APIs, Middleware, SignalR Hubs). Depends on Application and Infrastructure.
    *   **Eventual Consistency**: Uses `EventualConsistencyMiddleware` to dispatch Domain Events inside transactions.
*   **Shopizy.Contracts**: Shared Data Transfer Objects (DTOs) for HTTP request and response models.

## 🧪 Running Tests

To run the automated tests, execute the following command in the root directory:

```sh
dotnet test
```

The solution includes **1,164 automated tests**:
-   **Unit Tests**: Logic verification for Domain, Application, Contracts, and Infrastructure layers.
-   **Architecture Tests**: Enforces layer boundaries and dependency rules using NetArchTest.
-   **Integration Tests**: End-to-end API and Database verification using **Testcontainers** (Microsoft SQL Server 2022).

## 🤝 Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

To enable the repo hook in this clone, run:

```sh
git config core.hooksPath .githooks
```

1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## ✍️ Authors

*   **Md Abul Kalam** - *Initial work* - [akazad13](https://github.com/akazad13)

## 🙏 Acknowledgments

*   Clean Architecture templates and resources.
*   Open source libraries used in this project.
