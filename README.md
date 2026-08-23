# Shopizy - Online Store Management API

[![.NET](https://github.com/akazad13/shopizy/actions/workflows/dotnet.yml/badge.svg)](https://github.com/akazad13/shopizy/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/akazad13/shopizy-api/branch/main/graph/badge.svg)](https://codecov.io/gh/akazad13/shopizy-api)

Shopizy is a robust and scalable Online Store Management API built with .NET 10, following the principles of Clean Architecture. It provides a comprehensive set of endpoints for managing products, categories, carts, orders, and users, designed to support modern e-commerce applications.

## 🚀 About The Project

Shopizy aims to provide a solid foundation for e-commerce platforms. It separates concerns into distinct layers (Domain, Application, Infrastructure, API) to ensure maintainability, testability, and scalability.

Key features include:
-   **Product Management**: Create, update, delete, and search products.
-   **Category Management**: Organize products into hierarchical categories.
-   **Shopping Cart**: Manage user carts and items.
-   **Order Processing**: Place and track orders.
-   **User Management**: User authentication and profile management.
-   **Clean Architecture**: Decoupled layers for better code organization.

## 🛠️ Built With

*   [ASP.NET Core 10](https://dotnet.microsoft.com/en-us/apps/aspnet) - The web framework used.
*   **Custom Messaging System** - Handrolled implementation for Commands, Queries, and Domain Events.
*   [Scrutor](https://github.com/khellang/Scrutor) - Assembly scanning and decorator support for Microsoft.Extensions.DependencyInjection.
*   [Mapster](https://github.com/MapsterMapper/Mapster) - A fast, fun and stimulating object to object mapper.
*   [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - Object-Relational Mapper (ORM).
*   [Redis](https://redis.io/) - In-memory data structure store, used for caching.
*   [Swagger](https://swagger.io/) - API Documentation.
*   [ErrorOr](https://github.com/amantinband/error-or) - A simple, fluent discrimination union for error handling.
*   [Npgsql](https://www.npgsql.org/) - .NET data provider for PostgreSQL.
*   [xUnit](https://xunit.net/) - Testing framework.
*   [Shouldly](https://shouldly.io/) - Assertion library with a focus on readability.
*   [Moq](https://github.com/devlooped/moq) - Mocking library for .NET.
*   [Testcontainers](https://testcontainers.com/) - Spin up real databases in Docker for integration testing.

## 🏁 Getting Started

Follow these steps to get a local copy up and running.

### Prerequisites

*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   [Redis](https://redis.io/download) (Running locally or via Docker)
*   SQL Server (or configured database provider)

### Installation

1.  **Clone the repository**
    ```sh
    git clone https://github.com/akazad13/shopizy.git
    ```
2.  **Navigate to the project directory**
    ```sh
    cd shopizy
    ```
3.  **Restore dependencies**
    ```sh
    dotnet restore
    ```
4.  **Configure Database**
    Update the connection string in `src/Shopizy.Api/appsettings.json` (or `appsettings.Development.json`).
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER;Database=ShopizyDb;Trusted_Connection=True;TrustServerCertificate=True;"
    }
    ```
5.  **Run Migrations**
    The application is configured to apply migrations on startup (`DbMigrationsHelper`), but you can also run them manually:
    ```sh
    cd src/Shopizy.Api
    dotnet ef database update
    ```
6.  **Run the API**
    ```sh
    dotnet run
    ```

## 🔌 Usage

Once the API is running, you can explore the endpoints using Swagger UI.

*   **Swagger UI**: `https://localhost:7066/swagger/index.html` (Port may vary)

### Key Endpoints

*   `GET /api/v1.0/products`: Search and filter products.
*   `POST /api/v1.0/users/{userId}/cart`: Add items to cart.
*   `POST /api/v1.0/users/{userId}/orders`: Place an order.

## 📚 Documentation

For detailed guides and architecture specifications, please refer to:

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

*   **Shopizy.Domain**: Contains enterprise logic and types (Aggregates, Entities, Value Objects, Enums, Domain Events). No dependencies.
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

The solution includes:
-   **Unit Tests**: Logic verification for Domain, Application, and Infrastructure layers.
-   **Integration Tests**: End-to-end API and Database verification using **Testcontainers** (PostgreSQL).

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
