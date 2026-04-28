# eShop

A reference .NET microservices e-commerce application built with .NET 10, .NET Aspire, and deployed to Azure Container Apps.

## Description

eShop is a production-ready reference application demonstrating best practices for building cloud-native microservices with .NET. It features a full e-commerce workflow — browsing a product catalog, managing a shopping basket, placing orders, processing payments, and managing webhooks — with identity and security baked in throughout.

The solution includes:

- A **Blazor Server** web storefront
- A **.NET MAUI** cross-platform mobile and desktop client
- A **.NET MAUI Hybrid** app (Blazor + MAUI)
- Multiple backend microservices communicating over HTTP and gRPC
- An event-driven architecture using RabbitMQ
- Orchestrated locally with **.NET Aspire** and deployable to **Azure Container Apps** via `azd`

## Features

- Browse and search the product catalog (with optional AI-powered semantic search via OpenAI or Ollama)
- Shopping basket backed by Redis
- Order placement, processing, and status tracking
- Asynchronous payment processing via event bus
- Webhook registration and delivery
- OpenID Connect / OAuth 2.0 identity via Duende IdentityServer
- Cross-platform mobile client (Android, iOS, macOS, Windows) built with .NET MAUI
- Centralized observability with OpenTelemetry (traces, metrics, logs)
- API versioning and OpenAPI documentation
- Resilient HTTP clients with Microsoft.Extensions.Http.Resilience

## Architecture

The solution is composed of the following services and projects:

| Project                 | Description                                                                              |
| ----------------------- | ---------------------------------------------------------------------------------------- |
| `eShop.AppHost`         | .NET Aspire orchestration host — wires up all services, infrastructure, and dependencies |
| `WebApp`                | Blazor Server storefront                                                                 |
| `ClientApp`             | .NET MAUI cross-platform client (Android, iOS, macOS, Windows)                           |
| `HybridApp`             | .NET MAUI Hybrid app hosting Blazor components                                           |
| `Catalog.API`           | Product catalog service with pgvector-powered AI search                                  |
| `Basket.API`            | Shopping basket service backed by Redis, communicates via gRPC                           |
| `Ordering.API`          | Order management service                                                                 |
| `OrderProcessor`        | Background service that processes submitted orders                                       |
| `PaymentProcessor`      | Background service that processes payments                                               |
| `Identity.API`          | OAuth 2.0 / OpenID Connect identity provider (Duende IdentityServer)                     |
| `Webhooks.API`          | Webhook subscription and dispatch service                                                |
| `WebhookClient`         | Sample webhook receiver client                                                           |
| `mobile-bff`            | YARP-based reverse proxy / Backend-for-Frontend for mobile clients                       |
| `eShop.ServiceDefaults` | Shared service defaults (telemetry, health checks, service discovery)                    |
| `EventBus`              | Abstraction layer for integration events                                                 |
| `EventBusRabbitMQ`      | RabbitMQ implementation of the event bus                                                 |

**Infrastructure dependencies:**

| Dependency                 | Purpose                                                          |
| -------------------------- | ---------------------------------------------------------------- |
| PostgreSQL (with pgvector) | Persistent storage for Catalog, Identity, Ordering, and Webhooks |
| Redis                      | Basket cache                                                     |
| RabbitMQ                   | Asynchronous integration event bus                               |

```
┌─────────────┐   ┌──────────────┐   ┌──────────────┐
│   WebApp    │   │  ClientApp   │   │  HybridApp   │
│  (Blazor)   │   │  (.NET MAUI) │   │(MAUI+Blazor) │
└──────┬──────┘   └──────┬───────┘   └──────┬───────┘
       │                  │  mobile-bff        │
       └──────────────────┼────────────────────┘
                          │
          ┌───────────────┼───────────────┐
          ▼               ▼               ▼
    ┌───────────┐  ┌────────────┐  ┌────────────┐
    │Catalog.API│  │ Basket.API │  │Ordering.API│
    └─────┬─────┘  └─────┬──────┘  └─────┬──────┘
          │              │                │
     PostgreSQL         Redis         PostgreSQL
     (pgvector)                    + RabbitMQ events
                                         │
                              ┌──────────┴──────────┐
                              ▼                     ▼
                       OrderProcessor        PaymentProcessor
```

## Technologies Used

- [.NET 10](https://dotnet.microsoft.com/)
- [ASP.NET Core 10](https://learn.microsoft.com/aspnet/core)
- [.NET Aspire 13](https://learn.microsoft.com/dotnet/aspire/)
- [Blazor Server](https://learn.microsoft.com/aspnet/core/blazor/)
- [.NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- [Entity Framework Core 10](https://learn.microsoft.com/ef/core/) with [Npgsql](https://www.npgsql.org/)
- [PostgreSQL](https://www.postgresql.org/) + [pgvector](https://github.com/pgvector/pgvector)
- [Redis](https://redis.io/)
- [RabbitMQ](https://www.rabbitmq.com/)
- [gRPC](https://grpc.io/) via `Grpc.Net.Client`
- [YARP](https://microsoft.github.io/reverse-proxy/) (reverse proxy / BFF)
- [Duende IdentityServer 7](https://duendesoftware.com/products/identityserver)
- [OpenTelemetry](https://opentelemetry.io/)
- [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/) (deployment target)
- Optional AI: [Azure OpenAI](https://azure.microsoft.com/products/ai-services/openai-service) / [Ollama](https://ollama.com/)

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`10.0.100` or later)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for container dependencies)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

Install the Aspire workload if not already present:

```bash
dotnet workload install aspire
```

### Run locally

Clone the repository and start the Aspire host:

```bash
git clone https://github.com/Evilazaro/eShop.git
cd eShop
dotnet run --project src/eShop.AppHost
```

The Aspire dashboard will open automatically. All services, databases, and message brokers are started as containers.

> **Note:** The first run pulls container images for PostgreSQL, Redis, and RabbitMQ, which may take a few minutes.

### Run the web storefront

Once all services are healthy, open the `webapp` URL shown in the Aspire dashboard (typically `https://localhost:<port>`).

Default login credentials (seeded by Identity.API):

| Username            | Password   |
| ------------------- | ---------- |
| `alice@example.com` | `Pass123$` |
| `bob@example.com`   | `Pass123$` |

### Run the mobile client

Open `src/ClientApp/ClientApp.sln` in Visual Studio 2022 or later and run the project for your desired target platform (Android, iOS, macOS, or Windows).

## Configuration

Configuration is managed through `appsettings.json` and `appsettings.Development.json` files in each service project, as well as environment variables set by the Aspire host at runtime.

### AI / Semantic Search (optional)

To enable AI-powered catalog search, edit `src/eShop.AppHost/Program.cs` and set one of the following flags:

```csharp
// Use Azure OpenAI
bool useOpenAI = true;
// builder.AddOpenAI(catalogApi, webApp, OpenAITarget.AzureOpenAI);

// Or use a local Ollama instance
bool useOllama = true;
```

### HTTPS vs HTTP endpoints

The app defaults to HTTPS. To use HTTP (e.g., in environments without certificates), set the environment variable:

```bash
ESHOP_USE_HTTP_ENDPOINTS=true
```

### Environment variables

Each service exposes the following standard environment variables (set automatically by Aspire):

| Variable                      | Description                       |
| ----------------------------- | --------------------------------- |
| `Identity__Url`               | URL of the Identity.API service   |
| `ConnectionStrings__*`        | Database/cache connection strings |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry collector endpoint  |

## Deployment

The application is configured for deployment to **Azure Container Apps** using the [Azure Developer CLI (`azd`)](https://learn.microsoft.com/azure/developer/azure-developer-cli/).

### Prerequisites

- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- An Azure subscription

### Deploy

```bash
# Authenticate
azd auth login

# Provision infrastructure and deploy all services
azd up
```

`azd up` will:

1. Provision a resource group, Azure Container Apps environment, PostgreSQL, Redis, RabbitMQ, and supporting resources defined in `infra/`.
2. Build and push container images to Azure Container Registry.
3. Deploy all microservices as Container Apps.

To deploy code changes after the initial provisioning:

```bash
azd deploy
```

To tear down all resources:

```bash
azd down
```

### Infrastructure

Infrastructure is defined as Bicep templates in the `infra/` directory:

| File                    | Description                                                   |
| ----------------------- | ------------------------------------------------------------- |
| `infra/main.bicep`      | Subscription-level entry point, creates the resource group    |
| `infra/resources.bicep` | All Azure resources (Container Apps, databases, caches, etc.) |

## Usage

### Web Storefront

1. Navigate to the webapp URL.
2. Browse the product catalog and use the search bar to find items.
3. Add items to your basket.
4. Proceed to checkout and place an order.
5. View order history from the account menu.

### Webhooks

Register a webhook endpoint via the WebhookClient app or directly against the `Webhooks.API` to receive real-time notifications when orders are shipped.

### API Documentation

Each API exposes an OpenAPI (Swagger) document at `/swagger` when running in development mode:

| Service      | OpenAPI URL              |
| ------------ | ------------------------ |
| Catalog.API  | `https://<host>/swagger` |
| Basket.API   | `https://<host>/swagger` |
| Ordering.API | `https://<host>/swagger` |
| Webhooks.API | `https://<host>/swagger` |

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting a pull request.

Key guidelines:

- Follow existing code style and architecture patterns.
- Keep contributions focused — large-scale changes should include a clear rationale.
- Include tests for new features or bug fixes where applicable.
- For small fixes (typos, docs), open a PR directly. For feature suggestions, open an issue first.

## License

This project is licensed under the [MIT License](LICENSE).
