# eShop — Cloud-Native .NET Microservices Reference Application

[![Build Status](https://github.com/Evilazaro/eShop/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/Evilazaro/eShop/actions/workflows/pr-validation.yml)
[![Playwright Tests](https://github.com/Evilazaro/eShop/actions/workflows/playwright.yml/badge.svg)](https://github.com/Evilazaro/eShop/actions/workflows/playwright.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![.NET Aspire](https://img.shields.io/badge/.NET%20Aspire-13.x-blueviolet.svg)](https://learn.microsoft.com/dotnet/aspire)

**eShop** is a canonical reference implementation of a cloud-native .NET application built on a **microservices architecture**. It demonstrates production-ready patterns and best practices using .NET 10, .NET Aspire for local orchestration and service discovery, and a carefully chosen set of Microsoft and open-source technologies. The application models a complete online retail store with independently deployable backend services, an API gateway, and multiple client applications targeting web and mobile platforms.

The architecture showcases event-driven communication via **RabbitMQ**, identity and access management with **Duende IdentityServer**, and data persistence with **PostgreSQL** (extended with pgvector for AI search) and **Redis**. Services communicate through REST, gRPC, and asynchronous integration events. **Optional AI capabilities** — powered by OpenAI, Azure OpenAI, or a self-hosted Ollama instance — can be enabled to unlock semantic product search and AI-assisted chat.

Designed as a learning resource and starting-point template for real-world cloud-native applications, eShop covers the complete development lifecycle: from local development with .NET Aspire's developer dashboard to automated CI/CD via GitHub Actions and one-command Azure deployment using the **Azure Developer CLI** (`azd`).

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Technologies Used](#technologies-used)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Usage](#usage)
- [Known Limitations](#known-limitations)
- [Contributing](#contributing)
- [License](#license)

---

## Features

- 🛍️ **Product Catalog** — Browse and search products with pagination, brand/type filtering, and optional AI-powered semantic search using pgvector
- 🛒 **Shopping Basket** — Persistent, per-user basket backed by Redis via a high-performance gRPC service
- 📦 **Order Management** — Full order lifecycle from placement through payment confirmation, driven by event-driven processing
- 🔐 **Identity & Access** — OAuth 2.0 / OpenID Connect authentication and authorization via Duende IdentityServer 7
- 🔔 **Webhooks** — Event-notification system for external consumers to subscribe to order and catalog integration events
- 🌐 **Blazor Web App** — Interactive server-side Blazor frontend with reusable component library (`WebAppComponents`)
- 📱 **Cross-Platform Mobile App** — .NET MAUI client for iOS, Android, macOS, and Windows
- 🧩 **MAUI Hybrid App** — .NET MAUI Hybrid app that embeds Blazor components for shared native/web UI
- 📡 **API Gateway (Mobile BFF)** — YARP-based reverse proxy serving as the Backend-for-Frontend for mobile clients
- 🤖 **Optional AI Integration** — Plug-in support for OpenAI, Azure OpenAI, or Ollama (text embeddings + GPT-4.1-mini chat)
- ☁️ **One-Command Azure Deployment** — Full Azure Container Apps deployment via `azd up` with Bicep infrastructure-as-code
- 🧪 **Comprehensive Testing** — Unit tests, functional/integration tests, and end-to-end Playwright browser tests

---

## Architecture

The diagram below shows the high-level components of eShop, their responsibilities, and how they interact.

> [!NOTE]
> **Solid arrows** (`→`) represent synchronous HTTP/gRPC calls. **Dashed arrows** (`⤳`) represent asynchronous event-driven communication over the RabbitMQ event bus.

```mermaid
flowchart TB
    %% Client Applications
    subgraph Clients["Client Applications"]
        WebApp["WebApp<br/>(Blazor Server)"]
        ClientApp["ClientApp<br/>(.NET MAUI)"]
        HybridApp["HybridApp<br/>(.NET MAUI Hybrid)"]
    end

    %% API Gateway
    subgraph Gateway["API Gateway"]
        MobileBFF["Mobile BFF<br/>(YARP Proxy)"]
    end

    %% Backend Services
    subgraph Services["Backend Services"]
        IdentityAPI["Identity API<br/>(Duende IdentityServer)"]
        CatalogAPI["Catalog API"]
        BasketAPI["Basket API<br/>(gRPC)"]
        OrderingAPI["Ordering API"]
        WebhooksAPI["Webhooks API"]
        OrderProcessor["Order Processor"]
        PaymentProcessor["Payment Processor"]
    end

    %% Infrastructure
    subgraph Infrastructure["Infrastructure"]
        RabbitMQ[("RabbitMQ<br/>(Event Bus)")]
        PostgreSQL[("PostgreSQL<br/>+ pgvector")]
        Redis[("Redis Cache")]
    end

    %% Optional AI
    OpenAI(["OpenAI /<br/>Azure OpenAI"])

    %% Client to gateway and services
    ClientApp -->|"HTTP/REST"| MobileBFF
    HybridApp -->|"HTTP/REST"| MobileBFF
    MobileBFF -->|"HTTP"| CatalogAPI
    MobileBFF -->|"HTTP"| OrderingAPI

    WebApp -->|"HTTP/REST"| CatalogAPI
    WebApp -->|"gRPC"| BasketAPI
    WebApp -->|"HTTP/REST"| OrderingAPI
    WebApp -->|"OIDC/OAuth2"| IdentityAPI

    %% Services to datastores
    CatalogAPI --> PostgreSQL
    OrderingAPI --> PostgreSQL
    OrderProcessor --> PostgreSQL
    WebhooksAPI --> PostgreSQL
    IdentityAPI --> PostgreSQL
    BasketAPI --> Redis

    %% Async event-driven communication
    CatalogAPI -.->|"publish events"| RabbitMQ
    BasketAPI -.->|"publish events"| RabbitMQ
    OrderingAPI -.->|"pub / sub"| RabbitMQ
    OrderProcessor -.->|"subscribe"| RabbitMQ
    PaymentProcessor -.->|"pub / sub"| RabbitMQ
    WebhooksAPI -.->|"subscribe"| RabbitMQ

    %% Optional AI integration
    CatalogAPI -.->|"AI search (optional)"| OpenAI
    WebApp -.->|"AI chat (optional)"| OpenAI

    %% Visual styles
    classDef client fill:#2980b9,stroke:#1a5276,color:#fff
    classDef service fill:#27ae60,stroke:#1a7a40,color:#fff
    classDef datastore fill:#8e44ad,stroke:#6c3483,color:#fff
    classDef messaging fill:#c0392b,stroke:#922b21,color:#fff
    classDef gateway fill:#e67e22,stroke:#ca6f1e,color:#fff
    classDef external fill:#7f8c8d,stroke:#5d6d7e,color:#fff

    class WebApp,ClientApp,HybridApp client
    class CatalogAPI,BasketAPI,OrderingAPI,IdentityAPI,WebhooksAPI,OrderProcessor,PaymentProcessor service
    class PostgreSQL,Redis datastore
    class RabbitMQ messaging
    class MobileBFF gateway
    class OpenAI external
```

### Component Summary

| Component                 | Technology              | Responsibility                                              |
| ------------------------- | ----------------------- | ----------------------------------------------------------- |
| **WebApp**                | Blazor Server           | Interactive web storefront                                  |
| **ClientApp**             | .NET MAUI               | Cross-platform mobile client (iOS, Android, macOS, Windows) |
| **HybridApp**             | .NET MAUI Hybrid        | Native + Blazor hybrid client                               |
| **Mobile BFF**            | YARP                    | Reverse proxy / API gateway for mobile clients              |
| **Identity API**          | Duende IdentityServer 7 | OAuth 2.0 / OIDC token authority                            |
| **Catalog API**           | ASP.NET Core REST       | Product catalog with versioned API and AI-powered search    |
| **Basket API**            | ASP.NET Core gRPC       | Per-user shopping basket                                    |
| **Ordering API**          | ASP.NET Core REST       | Order placement and order history                           |
| **Order Processor**       | Background Worker       | Processes confirmed orders after payment                    |
| **Payment Processor**     | Background Worker       | Handles payment event flows                                 |
| **Webhooks API**          | ASP.NET Core REST       | Webhook subscription and event dispatch                     |
| **PostgreSQL + pgvector** | PostgreSQL              | Relational storage and vector embeddings for AI search      |
| **Redis**                 | Redis                   | Distributed session/basket cache                            |
| **RabbitMQ**              | RabbitMQ                | Asynchronous integration event bus                          |

---

## Technologies Used

### Platform & Runtime

| Technology      | Version  | Purpose                                                   |
| --------------- | -------- | --------------------------------------------------------- |
| **.NET SDK**    | 10.0.100 | Application runtime and build toolchain                   |
| **.NET Aspire** | 13.x     | Local orchestration, service discovery, and observability |
| **C#**          | 13       | Primary programming language                              |

### Backend

| Technology                | Version | Purpose                                        |
| ------------------------- | ------- | ---------------------------------------------- |
| **ASP.NET Core**          | 10.0    | Web API and application framework              |
| **Entity Framework Core** | 10.x    | ORM for PostgreSQL data access                 |
| **gRPC / Protobuf**       | 2.76    | High-performance Basket API transport          |
| **Duende IdentityServer** | 7.3     | OAuth 2.0 / OpenID Connect provider            |
| **YARP (Reverse Proxy)**  | 13.x    | Mobile BFF API gateway                         |
| **Npgsql + pgvector**     | 10.x    | PostgreSQL driver with vector-search extension |
| **Asp.Versioning**        | 8.1     | API versioning for Catalog and Ordering APIs   |

### Frontend & Mobile

| Technology           | Version | Purpose                                 |
| -------------------- | ------- | --------------------------------------- |
| **Blazor Server**    | 10.0    | Interactive server-rendered web UI      |
| **.NET MAUI**        | 10.0    | Cross-platform mobile application       |
| **.NET MAUI Hybrid** | 10.0    | Hybrid native/Blazor mobile application |

### Infrastructure & Messaging

| Technology     | Purpose                                                     |
| -------------- | ----------------------------------------------------------- |
| **PostgreSQL** | Primary relational database with vector search (`pgvector`) |
| **Redis**      | Distributed cache for shopping basket                       |
| **RabbitMQ**   | Asynchronous integration events (event bus)                 |

### AI & ML (Optional)

| Technology                | Purpose                                                              |
| ------------------------- | -------------------------------------------------------------------- |
| **Azure OpenAI / OpenAI** | Text embeddings (`text-embedding-3-small`) and chat (`gpt-4.1-mini`) |
| **Ollama**                | Self-hosted local LLM inference                                      |

### Testing

| Technology                           | Purpose                            |
| ------------------------------------ | ---------------------------------- |
| **MSTest 4 / MSTest.Sdk**            | Unit and functional test framework |
| **NSubstitute**                      | Mocking library                    |
| **Playwright**                       | End-to-end browser automation      |
| **Microsoft.AspNetCore.Mvc.Testing** | In-process integration test host   |

### DevOps & Deployment

| Technology                      | Purpose                                        |
| ------------------------------- | ---------------------------------------------- |
| **GitHub Actions**              | CI/CD pipeline (build, test, Playwright)       |
| **Azure Developer CLI (`azd`)** | One-command Azure provisioning and deployment  |
| **Azure Container Apps**        | Production hosting platform                    |
| **Bicep**                       | Infrastructure as Code for all Azure resources |

---

## Quick Start

### Prerequisites

Ensure the following tools are installed before running eShop locally:

| Requirement        | Minimum Version | Download                                                                  |
| ------------------ | --------------- | ------------------------------------------------------------------------- |
| **.NET SDK**       | 10.0.100        | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **Docker Desktop** | Latest stable   | [docker.com](https://www.docker.com/products/docker-desktop)              |
| **Git**            | Any recent      | [git-scm.com](https://git-scm.com)                                        |

> [!IMPORTANT]
> Docker (or a compatible OCI container runtime) is **required**. .NET Aspire automatically launches PostgreSQL, Redis, and RabbitMQ as containers on first run.

> [!NOTE]
> The `global.json` file at the repository root pins the SDK to version `10.0.100` with `latestFeature` roll-forward. Install this version or any later feature-band release.

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/Evilazaro/eShop.git
   cd eShop
   ```

2. Restore NuGet packages:

   ```bash
   dotnet restore eShop.Web.slnf
   ```

3. Trust the ASP.NET Core development HTTPS certificate (run once):

   ```bash
   dotnet dev-certs https --trust
   ```

### Run Locally

Start the complete application stack with a single command:

```bash
dotnet run --project src/eShop.AppHost
```

.NET Aspire opens the **Developer Dashboard** automatically at `https://localhost:15888`. It displays all running services, health status, structured logs, and distributed traces.

> [!TIP]
> The first run pulls container images for PostgreSQL, Redis, and RabbitMQ. These containers use `ContainerLifetime.Persistent`, so subsequent runs start faster because the containers are reused.

After startup, find the service URLs in the Aspire Dashboard under **Resources**:

| Application          | Description                                         |
| -------------------- | --------------------------------------------------- |
| **WebApp**           | Online store Blazor frontend                        |
| **Aspire Dashboard** | Service orchestration and observability             |
| **Identity API**     | IdentityServer discovery document and management    |
| **Catalog API**      | OpenAPI / Swagger UI for product catalog endpoints  |
| **Ordering API**     | OpenAPI / Swagger UI for order management endpoints |

### Build Only

Verify the solution compiles without running it:

```bash
dotnet build eShop.Web.slnf
```

### Run Tests

```bash
dotnet test --solution eShop.Web.slnf --no-build --output detailed
```

---

## Configuration

eShop uses standard **ASP.NET Core configuration** with `appsettings.json` as the baseline, overridden by environment variables. In local development, .NET Aspire injects all connection strings and service URLs automatically at startup.

> [!NOTE]
> Manual configuration is required only for **optional features** (AI integration) or when deploying outside the .NET Aspire orchestration environment.

### Core Configuration Options

| Setting                               | Location                        | Default            | Description                                |
| ------------------------------------- | ------------------------------- | ------------------ | ------------------------------------------ |
| `ConnectionStrings:EventBus`          | Each service `appsettings.json` | `amqp://localhost` | RabbitMQ connection string                 |
| `EventBus:SubscriptionClientName`     | Each service `appsettings.json` | Per-service name   | RabbitMQ consumer group name               |
| `Identity:Url`                        | Backend services                | Injected by Aspire | Base URL of the Identity API               |
| `Identity:Audience`                   | Backend services                | Injected by Aspire | JWT audience for token validation          |
| `SessionCookieLifetimeMinutes`        | `WebApp/appsettings.json`       | `60`               | Session cookie lifetime in minutes         |
| `CatalogOptions:UseCustomizationData` | `Catalog.API/appsettings.json`  | `false`            | Load custom seed data from disk on startup |

### Environment Variables

Override any `appsettings.json` value using environment variables. Use double-underscores (`__`) as the section separator:

```bash
# Override the Identity API base URL
Identity__Url=https://identity.example.com

# Override the RabbitMQ connection string
ConnectionStrings__EventBus=amqp://user:password@rabbitmq-host:5672

# Override the session cookie lifetime
SessionCookieLifetimeMinutes=120
```

### Enabling AI Integration

AI features are **disabled by default**. Enable them by editing `src/eShop.AppHost/Program.cs`:

```csharp
// Option 1: OpenAI (requires API key)
bool useOpenAI = true;
builder.AddOpenAI(catalogApi, webApp, OpenAITarget.OpenAI);

// Option 2: Azure OpenAI (uses managed identity by default)
builder.AddOpenAI(catalogApi, webApp, OpenAITarget.AzureOpenAI);

// Option 3: Ollama (self-hosted, no API key required)
bool useOllama = true;
builder.AddOllama(catalogApi, webApp);
```

> [!CAUTION]
> Never commit API keys or secrets to source control. Store sensitive values in .NET user secrets or environment variables only.

Set the OpenAI API key using .NET user secrets scoped to the AppHost project:

```bash
dotnet user-secrets set "OpenAIKeyParameter" "<your-openai-api-key>" --project src/eShop.AppHost
```

### Configuration for Different Environments

| Environment                | Strategy                                                                                 |
| -------------------------- | ---------------------------------------------------------------------------------------- |
| **Local Development**      | .NET Aspire injects connection strings; use `appsettings.Development.json` for overrides |
| **CI / GitHub Actions**    | Environment variables set in the workflow YAML                                           |
| **Azure (Container Apps)** | `azd` provisions secrets and injects them as Container Apps environment variables        |

---

## Deployment

eShop deploys to **Azure Container Apps** using the Azure Developer CLI (`azd`). All Azure resources are defined as Bicep templates in the `infra/` directory.

### Prerequisites for Deployment

| Requirement             | Notes                                                                                        |
| ----------------------- | -------------------------------------------------------------------------------------------- |
| **Azure Subscription**  | Active subscription with Container Apps and PostgreSQL quota                                 |
| **Azure Developer CLI** | [Install `azd`](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) |
| **Azure CLI**           | [Install `az`](https://learn.microsoft.com/cli/azure/install-azure-cli)                      |
| **Docker Desktop**      | Required to build and push container images                                                  |

> [!WARNING]
> `azd up` provisions Azure resources and **incurs costs**. Review pricing for Azure Container Apps, Azure Database for PostgreSQL Flexible Server, Azure Cache for Redis, and Azure Container Registry before proceeding.

### Deploy to Azure

1. Authenticate with Azure:

   ```bash
   azd auth login
   ```

2. Create a new environment (first-time setup):

   ```bash
   azd env new <your-environment-name>
   ```

3. Provision infrastructure and deploy all services:

   ```bash
   azd up
   ```

   `azd up` performs two steps in sequence:

   | Step      | Command         | What it does                                                   |
   | --------- | --------------- | -------------------------------------------------------------- |
   | Provision | `azd provision` | Creates Azure resources from `infra/main.bicep`                |
   | Deploy    | `azd deploy`    | Builds container images, pushes to ACR, updates Container Apps |

4. To redeploy application code only (without re-provisioning):

   ```bash
   azd deploy
   ```

5. To destroy all Azure resources when finished:

   ```bash
   azd down
   ```

### Infrastructure Overview

| File                         | Purpose                                                                        |
| ---------------------------- | ------------------------------------------------------------------------------ |
| `infra/main.bicep`           | Subscription-scope deployment; creates the resource group                      |
| `infra/resources.bicep`      | Container Apps environment, PostgreSQL, Redis, RabbitMQ, ACR, Managed Identity |
| `infra/main.parameters.json` | Default parameter values for the deployment                                    |
| `azure.yaml`                 | `azd` service definitions mapping projects to Container Apps                   |

The deployment provisions the following Azure resources:

- **Azure Container Apps Environment** — runtime host for all microservices
- **Azure Container Registry (ACR)** — private registry for container images
- **Azure Database for PostgreSQL Flexible Server** — persistent relational storage
- **Azure Cache for Redis** — basket session cache
- **Managed Identity** — service-to-service authentication without credentials
- **Log Analytics Workspace** — centralized logs and metrics

---

## Usage

### Browsing the Store

Navigate to the **WebApp** URL displayed in the Aspire Dashboard. The homepage lists all catalog products. Use the search bar or the brand and product-type filters to narrow results.

### Placing an Order

1. Find a product in the catalog and click **Add to Cart**.
2. Open the **Shopping Basket** to review selected items and quantities.
3. Click **Checkout**. You are redirected to the Identity API login page.
4. Sign in with an existing account or register a new one.
5. Confirm the order. A confirmation page displays the order number on success.

### Exploring the APIs

Each backend API exposes an **OpenAPI (Swagger UI)** endpoint in development mode:

| Service          | Path                         |
| ---------------- | ---------------------------- |
| **Catalog API**  | `<catalog-api-url>/swagger`  |
| **Ordering API** | `<ordering-api-url>/swagger` |

> [!TIP]
> Service ports are dynamically assigned by .NET Aspire. Open the **Aspire Dashboard → Resources** tab to find the exact URL for each service.

### Running End-to-End Tests

Install Node.js dependencies and Playwright browser binaries:

```bash
npm install
npx playwright install --with-deps
```

Start the application with `dotnet run --project src/eShop.AppHost`, then run the Playwright test suite:

```bash
npx playwright test
```

E2E test scenarios:

| Test File                    | Scenario                              |
| ---------------------------- | ------------------------------------- |
| `e2e/BrowseItemTest.spec.ts` | Browse and filter the product catalog |
| `e2e/AddItemTest.spec.ts`    | Add a product to the shopping basket  |
| `e2e/RemoveItemTest.spec.ts` | Remove a product from the basket      |

### Running the Mobile App

1. Open `src/ClientApp/ClientApp.sln` in **Visual Studio** (Windows/macOS).
2. Select a target platform: **Android**, **iOS**, **macOS**, or **Windows**.
3. Run the project. The app connects to the locally running Aspire-hosted services.

> [!NOTE]
> Android Emulator requires special loopback configuration. See the `AuthenticationExtensions.cs` comments for details on the `ValidIssuers` setup for Android.

---

## Known Limitations

- **No CHANGELOG.md** — version history is tracked via [GitHub Releases](https://github.com/Evilazaro/eShop/releases) and commit history.
- **No production key management** — `KeyManagement.Enabled = false` in Identity API is intentional for the development sample; enable key management for real production deployments.
- **AI features are opt-in** — `useOpenAI` and `useOllama` flags in `Program.cs` must be set manually; there is no environment-variable toggle today.
- **Webhook Client is a demo consumer** — `WebhookClient` is a sample application only and is not hardened for production use.

---

## Contributing

Contributions are welcome! Read [CONTRIBUTING.md](CONTRIBUTING.md) for the full contribution guidelines.

**Quick guide to contribute:**

1. Fork the repository and create a feature or fix branch.
2. Follow the **best practices** and **architectural integrity** principles described in [CONTRIBUTING.md](CONTRIBUTING.md).
3. Add or update tests to cover your changes.
4. Open a Pull Request against `main`. The CI pipeline automatically validates the build, runs unit and functional tests, and runs the Playwright e2e suite.

> [!NOTE]
> Browse issues labelled **`help wanted`** or **`good first issue`** on GitHub to find a good entry point for new contributors.

> [!TIP]
> For small typo fixes or documentation improvements, open a Pull Request directly — no need to file an issue first.

This project follows the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct). See [CODE-OF-CONDUCT.md](CODE-OF-CONDUCT.md) for details.

---

## License

eShop is licensed under the **MIT License**.

See [LICENSE](LICENSE) for the full license text.

Copyright © .NET Foundation and Contributors.
