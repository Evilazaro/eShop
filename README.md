# eShop

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![.NET Aspire](https://img.shields.io/badge/.NET%20Aspire-13.2-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/aspire)

**eShop** is a reference .NET application that demonstrates how to build a production-quality, cloud-native e-commerce platform using a microservices architecture. It serves as a canonical example of best practices for .NET 10 development, covering everything from gRPC-based inter-service communication to event-driven workflows and AI-ready product search powered by the `pgvector` extension.

The application addresses the challenge of building a scalable, maintainable online store from multiple loosely coupled services. Each service owns its own data, communicates asynchronously through an **integration event bus**, and is independently deployable — a pattern that removes the bottlenecks of a traditional monolith without sacrificing developer ergonomics.

The technology stack spans **.NET 10**, Blazor Server for the web storefront, .NET MAUI for native mobile and hybrid clients, Duende IdentityServer for OpenID Connect authentication, RabbitMQ for async messaging, PostgreSQL with `pgvector` for AI-assisted semantic search, Redis for basket caching, YARP as a mobile backend-for-frontend proxy, and .NET Aspire for developer orchestration and one-command cloud deployment to Azure Container Apps.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Technologies Used](#technologies-used)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Features

| Feature                            | Description                                                                                                                 |
| ---------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| 🛍️ **Catalog browsing**            | Browse and search a product catalog powered by PostgreSQL and pgvector for AI-assisted semantic search.                     |
| 🛒 **Shopping basket**             | Persistent shopping cart stored in Redis, accessible via a gRPC API with JWT-secured access.                                |
| 📦 **Order management**            | Full order lifecycle from checkout to payment confirmation, managed by dedicated background worker services.                |
| 🔐 **Identity and authentication** | OpenID Connect and OAuth 2.0 authentication provided by Duende IdentityServer with automatic database seeding.              |
| 📡 **Webhooks**                    | Subscribe to order status change notifications delivered to external systems via HTTP callbacks.                            |
| 🔄 **Event-driven processing**     | Async integration events over RabbitMQ decouple services for resilience and independent scalability.                        |
| 📱 **Multi-platform clients**      | Online store accessible via a Blazor Server web app, a .NET MAUI Blazor hybrid app, and a full .NET MAUI native mobile app. |
| ☁️ **Cloud-native deployment**     | One-command deployment to Azure Container Apps using the Azure Developer CLI (`azd up`).                                    |

## Architecture

The eShop architecture is divided into two sub-diagrams for clarity. The first covers user-facing clients and API services; the second covers the event-driven messaging backbone, background workers, and data stores.

### Application Services Diagram

Shows how end users interact with the client applications, the gateway layer, and the core backend services.

```mermaid
---
config:
  description: "Application services diagram showing actors, client layer, gateway, and core API services for the eShop e-commerce platform."
  theme: base
  themeVariables:
    htmlLabels: true
    fontFamily: "-apple-system, BlinkMacSystemFont, \"Segoe UI\", system-ui, \"Apple Color Emoji\", \"Segoe UI Emoji\", sans-serif"
    fontSize: 16
    primaryColor: "#cce4f6"
    primaryTextColor: "#242424"
    primaryBorderColor: "#0078d4"
    lineColor: "#605e5c"
    secondaryColor: "#f5f5f5"
---
flowchart TB

    %% ── Class Definitions ────────────────────────────────────────
    classDef actor     fill:#d0e7f8,stroke:#0078d4,color:#242424,font-weight:bold
    classDef service   fill:#f5f5f5,stroke:#616161,color:#242424,font-weight:bold
    classDef gateway   fill:#a6e9ed,stroke:#00b7c3,color:#242424,font-weight:bold
    classDef apiSvc    fill:#ddeeff,stroke:#0078d4,color:#242424,font-weight:bold
    classDef stub      fill:#f0f0f0,stroke:#d1d1d1,color:#605e5c,font-weight:bold

    %% ── Actors ──────────────────────────────────────────────────
    subgraph ACTORS["👥 Actors"]
        Shopper(["👤 Shopper"])
        ExternalSubscriber(["🔗 Webhook Subscriber"])
    end

    %% ── Client Layer ─────────────────────────────────────────────
    subgraph ClientLayer["🖥️ Client Layer"]
        WebApp("🌐 WebApp<br/>Blazor Server")
        HybridApp("📱 HybridApp<br/>.NET MAUI Blazor")
        ClientApp("📱 ClientApp<br/>.NET MAUI")
        WebhookClient("🌐 WebhookClient<br/>Blazor Server")
    end

    %% ── Gateway and Identity ─────────────────────────────────────
    subgraph GatewayLayer["🔀 Gateway and Identity"]
        MobileBFF("🔀 Mobile BFF<br/>YARP Reverse Proxy")
        IdentityAPI("🔐 Identity API<br/>Duende IdentityServer")
    end

    %% ── Core API Services ────────────────────────────────────────
    subgraph APILayer["🌐 API Services"]
        CatalogAPI("🌐 Catalog API<br/>REST and OpenAPI")
        BasketAPI("🛒 Basket API<br/>gRPC")
        OrderingAPI("🌐 Ordering API<br/>REST and OpenAPI")
        WebhooksAPI("🌐 Webhooks API<br/>REST and OpenAPI")
    end

    %% ── Infrastructure stub ──────────────────────────────────────
    InfraStub(["🚌 Event Bus and Data Stores<br/>see Infrastructure Diagram"])

    %% ── Edges ────────────────────────────────────────────────────
    Shopper -->|"HTTPS browse and shop"| WebApp
    Shopper -->|"HTTPS browse and shop"| HybridApp
    Shopper -->|"HTTPS browse and shop"| ClientApp

    HybridApp -->|"HTTP API requests"| MobileBFF
    ClientApp -->|"HTTP API requests"| MobileBFF

    WebApp -->|"OIDC auth"| IdentityAPI
    HybridApp -->|"OIDC auth"| IdentityAPI
    ClientApp -->|"OIDC auth"| IdentityAPI

    WebApp -->|"REST catalog"| CatalogAPI
    WebApp -->|"gRPC basket"| BasketAPI
    WebApp -->|"REST orders"| OrderingAPI

    MobileBFF -->|"proxy catalog"| CatalogAPI
    MobileBFF -->|"proxy orders"| OrderingAPI

    WebhookClient -->|"REST webhooks"| WebhooksAPI
    WebhooksAPI -.->|"HTTP callbacks"| ExternalSubscriber

    CatalogAPI -.->|"async events"| InfraStub
    BasketAPI -.->|"async events"| InfraStub
    OrderingAPI -.->|"async events"| InfraStub
    WebhooksAPI -.->|"async events"| InfraStub

    %% ── Class Assignments ────────────────────────────────────────
    class Shopper,ExternalSubscriber actor
    class WebApp,HybridApp,ClientApp,WebhookClient service
    class MobileBFF,IdentityAPI gateway
    class CatalogAPI,BasketAPI,OrderingAPI,WebhooksAPI apiSvc
    class InfraStub stub

    %% ── Subgraph Styles ──────────────────────────────────────────
    style ACTORS      fill:#f0f0f0,stroke:#d1d1d1,color:#424242
    style ClientLayer fill:#f0f0f0,stroke:#d1d1d1,color:#424242
    style GatewayLayer fill:#f0f0f0,stroke:#d1d1d1,color:#424242
    style APILayer    fill:#f0f0f0,stroke:#d1d1d1,color:#424242
```

### Infrastructure and Async Processing Diagram

Shows the event-driven messaging backbone, background worker services, and the data storage layer.

```mermaid
---
config:
  description: "Infrastructure diagram showing the event-driven messaging backbone, background workers, and data stores for the eShop e-commerce platform."
  theme: base
  themeVariables:
    htmlLabels: true
    fontFamily: "-apple-system, BlinkMacSystemFont, \"Segoe UI\", system-ui, \"Apple Color Emoji\", \"Segoe UI Emoji\", sans-serif"
    fontSize: 16
    primaryColor: "#cce4f6"
    primaryTextColor: "#242424"
    primaryBorderColor: "#0078d4"
    lineColor: "#605e5c"
    secondaryColor: "#f5f5f5"
---
flowchart TB

    %% ── Class Definitions ────────────────────────────────────────
    classDef stub      fill:#f0f0f0,stroke:#d1d1d1,color:#605e5c,font-weight:bold
    classDef worker    fill:#fef7b2,stroke:#9c5e00,color:#242424,font-weight:bold
    classDef bus       fill:#cce4f6,stroke:#0078d4,color:#242424,font-weight:bold
    classDef datastore fill:#f1faf1,stroke:#107c10,color:#0e700e,font-weight:bold

    %% ── Service stubs ────────────────────────────────────────────
    subgraph ServiceStubs["Application Services — see Application Services Diagram"]
        CatalogStub(["🌐 Catalog API"])
        BasketStub(["🛒 Basket API"])
        OrderingStub(["🌐 Ordering API"])
        WebhooksStub(["🌐 Webhooks API"])
    end

    %% ── Message Bus ──────────────────────────────────────────────
    RabbitMQ[("🚌 RabbitMQ<br/>Integration Event Bus")]

    %% ── Background Workers ───────────────────────────────────────
    subgraph Workers["⚙️ Background Workers"]
        OrderProcessor("⚙️ Order Processor<br/>Worker Service")
        PaymentProcessor("⚙️ Payment Processor<br/>Worker Service")
    end

    %% ── Data Stores ──────────────────────────────────────────────
    subgraph DataStores["💾 Data Stores"]
        PostgreSQL[("🐘 PostgreSQL and pgvector<br/>Catalog · Identity · Ordering · Webhooks")]
        Redis[("⚡ Redis Cache<br/>Basket sessions")]
    end

    %% ── Edges ────────────────────────────────────────────────────
    CatalogStub -.->|"publish catalog events"| RabbitMQ
    BasketStub -.->|"publish OrderStarted"| RabbitMQ
    OrderingStub -.->|"publish OrderStatus"| RabbitMQ
    WebhooksStub -.->|"subscribe OrderStatus"| RabbitMQ

    RabbitMQ -.->|"OrderConfirmed events"| OrderProcessor
    RabbitMQ -.->|"OrderStatusChanged events"| PaymentProcessor

    OrderProcessor -.->|"publish StockConfirmed"| RabbitMQ
    PaymentProcessor -.->|"publish PaymentStatus"| RabbitMQ

    CatalogStub -->|"reads and writes catalog data"| PostgreSQL
    OrderingStub -->|"reads and writes order data"| PostgreSQL
    WebhooksStub -->|"reads and writes webhook data"| PostgreSQL
    OrderProcessor -->|"updates order state"| PostgreSQL
    BasketStub -->|"caches basket sessions"| Redis

    %% ── Class Assignments ────────────────────────────────────────
    class CatalogStub,BasketStub,OrderingStub,WebhooksStub stub
    class OrderProcessor,PaymentProcessor worker
    class RabbitMQ bus
    class PostgreSQL,Redis datastore

    %% ── Subgraph Styles ──────────────────────────────────────────
    style ServiceStubs fill:#f0f0f0,stroke:#d1d1d1,color:#424242
    style Workers      fill:#f0f0f0,stroke:#d1d1d1,color:#424242
    style DataStores   fill:#f0f0f0,stroke:#d1d1d1,color:#424242
```

## Technologies Used

| Technology                  | Type                         | Purpose                                                        |
| --------------------------- | ---------------------------- | -------------------------------------------------------------- |
| .NET 10                     | Runtime and SDK              | Core platform for all services and applications                |
| ASP.NET Core 10             | Web framework                | REST APIs, Blazor Server, middleware pipeline                  |
| .NET Aspire 13.2            | Orchestration                | Local dev orchestration and cloud deployment manifest          |
| Blazor Server               | UI framework                 | Web storefront (`WebApp`) and webhook client (`WebhookClient`) |
| .NET MAUI                   | Mobile and Desktop framework | Native mobile app (`ClientApp`) and hybrid app (`HybridApp`)   |
| Entity Framework Core 10    | ORM                          | Data access for all services using PostgreSQL                  |
| gRPC                        | RPC protocol                 | High-performance Basket API inter-service communication        |
| Duende IdentityServer 7.3   | Identity provider            | OpenID Connect and OAuth 2.0 token issuance                    |
| RabbitMQ                    | Message broker               | Asynchronous integration event bus between services            |
| Redis                       | Cache                        | Distributed basket session storage                             |
| PostgreSQL + pgvector       | Database                     | Relational data with AI-assisted vector similarity search      |
| YARP                        | Reverse proxy                | Mobile backend-for-frontend (BFF) gateway                      |
| Azure Container Apps        | Cloud platform               | Production hosting target via `azd up`                         |
| Bicep                       | Infrastructure as Code       | Azure infrastructure definition in `infra/`                    |
| Azure Developer CLI (`azd`) | Deployment tool              | One-command provision and deploy to Azure                      |
| OpenTelemetry               | Observability                | Distributed tracing, metrics, and structured logging           |
| Playwright                  | E2E testing                  | End-to-end browser tests in `e2e/`                             |

## Quick Start

### Prerequisites

| Prerequisite                                                     | Version     | Notes                                         |
| ---------------------------------------------------------------- | ----------- | --------------------------------------------- |
| [.NET SDK](https://dotnet.microsoft.com/download)                | 10.0        | Required for all services                     |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | Latest      | Required for infrastructure containers        |
| [Node.js](https://nodejs.org)                                    | 20 or later | Required for Playwright end-to-end tests only |

> [!NOTE]
> .NET Aspire automatically starts PostgreSQL, Redis, and RabbitMQ as Docker containers when you run the AppHost. No manual container setup is required.

### Installation

1. **Clone the repository.**

```bash
git clone https://github.com/Evilazaro/eShop.git
cd eShop
```

2. **Restore .NET dependencies.**

```bash
dotnet restore eShop.slnx
```

3. **Start the application using the .NET Aspire AppHost.**

```bash
dotnet run --project src/eShop.AppHost
```

4. **Open the Aspire dashboard** at the URL printed in the terminal (typically `https://localhost:15888`) to view all running services and their logs.

5. **Open the web storefront** at the URL labelled `webapp` in the Aspire dashboard.

> [!TIP]
> To force all endpoints to use plain HTTP — useful in CI or firewall-restricted environments — set the environment variable `ESHOP_USE_HTTP_ENDPOINTS=1` before running the AppHost.

### Minimal working example

After the AppHost starts, navigate to the web storefront and sign in with the default seeded credentials:

```text
Email:    AliceSmith@email.com
Password: Pass123$
```

Browse the catalog, add an item to the basket, and place an order to verify the complete end-to-end flow.

## Configuration

The following table lists key configuration options used across services. Each service reads its settings from `appsettings.json`, overridable via environment variables using the `__` double-underscore separator convention.

| Option                            | Default            | Description                                                  |
| --------------------------------- | ------------------ | ------------------------------------------------------------ |
| `ConnectionStrings:EventBus`      | `amqp://localhost` | AMQP connection string for RabbitMQ                          |
| `ConnectionStrings:Redis`         | `localhost`        | Connection string for the Redis cache                        |
| `EventBus:SubscriptionClientName` | Service-specific   | Unique queue name per service on the event bus               |
| `Identity:Audience`               | Service-specific   | Expected JWT audience for bearer token validation            |
| `Identity__Url`                   | Set by Aspire      | URL of the Identity API, injected by the AppHost at runtime  |
| `SessionCookieLifetimeMinutes`    | `60`               | WebApp session cookie lifetime in minutes                    |
| `ESHOP_USE_HTTP_ENDPOINTS`        | `0`                | Set to `1` to force all endpoints to use HTTP (testing only) |

### Example: override the event bus connection in development

```json
// src/Catalog.API/appsettings.Development.json
{
  "ConnectionStrings": {
    "EventBus": "amqp://guest:guest@localhost:5672"
  }
}
```

> [!WARNING]
> Never commit credentials or connection strings with passwords to source control. Use environment variables, `dotnet user-secrets`, or Azure Key Vault for all sensitive values in production.

## Deployment

eShop deploys to **Azure Container Apps** using the **Azure Developer CLI** (`azd`). Infrastructure is defined as Bicep templates in the `infra/` folder.

1. **Install the Azure Developer CLI.**

```bash
winget install Microsoft.Azd
```

2. **Log in to Azure.**

```bash
azd auth login
```

3. **Initialize a new environment.** Choose a unique name; it is used to name the Azure resource group (`rg-<name>`).

```bash
azd env new <environment-name>
```

4. **Provision infrastructure and deploy all services in one step.**

```bash
azd up
```

`azd up` provisions the resource group, deploys PostgreSQL, Redis, and RabbitMQ as Container Apps, builds all service images, pushes them to Azure Container Registry, and deploys each service as a Container App.

5. **Retrieve the deployed web app URL.**

```bash
azd show
```

> [!IMPORTANT]
> The deployment automatically generates strong random passwords for PostgreSQL, Redis, and RabbitMQ via the parameter definitions in `infra/main.parameters.json`. Keep these values secure and do not share them outside your team.

## Usage

### Browse the catalog via the REST API

The Catalog API exposes a versioned REST endpoint. Use `curl` or any HTTP client to retrieve products:

```bash
curl https://<webapp-url>/catalog-api/api/v1/catalog/items
```

Expected response (excerpt):

```json
{
  "pageIndex": 0,
  "pageSize": 10,
  "count": 100,
  "data": [
    {
      "id": 1,
      "name": ".NET Bot Black Hoodie",
      "price": 19.5,
      "pictureUrl": "..."
    }
  ]
}
```

### Place an order programmatically

The Ordering API requires a valid bearer token issued by the Identity API:

```bash
# 1. Obtain a bearer token
TOKEN=$(curl -s -X POST https://<identity-url>/connect/token \
  -d "grant_type=password&client_id=webappClient&username=alice&password=Pass123%24&scope=orders" \
  | jq -r '.access_token')

# 2. Submit a new order
curl -X POST https://<webapp-url>/ordering-api/api/v1/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "city": "Redmond", "street": "15703 NE 61st Ct",
    "state": "WA", "country": "U.S.", "zipCode": "98052",
    "cardNumber": "4012888888881881", "cardHolderName": "Alice Smith",
    "cardExpiration": "12/24", "cardSecurityNumber": "123", "cardTypeId": 1,
    "orderItems": [
      { "productId": 1, "productName": ".NET Bot Black Hoodie",
        "unitPrice": 19.5, "units": 1, "pictureUrl": "" }
    ]
  }'
```

### Run end-to-end tests

```bash
npm install
npx playwright test
```

> [!NOTE]
> Functional tests in `tests/` use the Aspire host to spin up test containers and require Docker to be running as a prerequisite.

## Contributing

Contributions are welcome! Read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on proposing new features, submitting issues, and opening pull requests.

- **Bug reports and feature requests:** Open an issue with a clear title, description, and any relevant examples.
- **Code contributions:** Fork the repository, create a feature branch, and open a pull request targeting `main`.
- **Code of conduct:** All contributors are expected to follow the [Code of Conduct](CODE-OF-CONDUCT.md).

> [!TIP]
> A great place to start is the list of issues tagged `good first issue` or `help wanted`.

## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for full details.
