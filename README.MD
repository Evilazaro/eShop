[![eShop Pull Request Validation](https://github.com/dotnet/eShop/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/dotnet/eShop/actions/workflows/pr-validation.yml)
[![eShop Pull Request Validation - .NET MAUI](https://github.com/dotnet/eShop/actions/workflows/pr-validation-maui.yml/badge.svg)](https://github.com/dotnet/eShop/actions/workflows/pr-validation-maui.yml)
[![Playwright Tests for eShop](https://github.com/dotnet/eShop/actions/workflows/playwright.yml/badge.svg)](https://github.com/dotnet/eShop/actions/workflows/playwright.yml)

# eShop

A **reference .NET application** implementing an **eCommerce** web site using a **services-based architecture**.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [High-level Architecture](#high-level-architecture)
- [Repository Structure](#repository-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Examples](#examples)
- [Support / Getting Help](#support--getting-help)
- [Contributing](#contributing)
- [License](#license)
- [Author / Maintainer](#author--maintainer)

## Overview

**eShop** exists to show, in working code, how a complete online store is built on modern .NET. It implements catalog browsing, a shopping basket, order placement, payment processing and outbound webhooks as separate services that are developed independently yet run together as one distributed application.

Standing up a cloud-native system normally means solving service discovery, configuration, resilient HTTP, asynchronous messaging, database migrations, sign-in and telemetry before a single line of business logic is written. eShop removes that blank-page problem: a **.NET Aspire** app host provisions **PostgreSQL**, **Redis** and **RabbitMQ**, wires every service to them, waits for dependencies to become healthy, and starts the whole system from one command.

The intended audience is .NET developers, solution architects and open-source contributors who want a realistic reference rather than a toy demo — people evaluating how to structure services, how to introduce messaging, or how to add AI to an existing storefront. Newcomers are explicitly welcomed: issues tagged `help wanted` and `good first issue` are curated as an on-ramp.

The repository ships seven backend services, a **Blazor** storefront, a webhook subscriber client, a **.NET MAUI** mobile app and a Blazor Hybrid app, together with **semantic search** across the product catalog and an **AI shopping assistant**. Five .NET test projects and a Playwright end-to-end suite ship alongside the source, so a change can be written, run and verified locally before it becomes a pull request.

The stack is **.NET 10** with **.NET Aspire** for orchestration, **ASP.NET Core** minimal APIs and Blazor for the web tier, **Entity Framework Core** with Npgsql and **pgvector** plus Dapper for data access, MediatR and FluentValidation in the ordering domain, **Duende IdentityServer** for identity, **gRPC** for the basket contract, **YARP** for the mobile back end for front end, **RabbitMQ** for integration events, **Redis** for caching, and **OpenTelemetry** for tracing and metrics.

## Features

Everything listed below is implemented in this repository and runs on a developer machine — no cloud subscription is needed. The optional AI integrations are switched off by default, so the store works end to end without any model provider.

The table groups the capabilities that matter most when you first explore the code, with the file or project that implements each one reachable from the [Repository Structure](#repository-structure) section.

|                           | Feature                       | Description                                                                                                                          | Benefits                                                          |
| :-----------------------: | ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------- |
|      :shopping_cart:      | Complete purchase flow        | Catalog, Basket, Ordering, Payment and Webhooks services cover browse, add to basket, checkout and notify.                           | Study one realistic transaction path from end to end.             |
|           :mag:           | **Semantic** catalog search   | `GET /items/withsemanticrelevance` ranks products by embedding similarity, stored as vectors in **PostgreSQL** through **pgvector**. | AI-ranked product search without adding a separate search engine. |
|     :speech_balloon:      | **AI shopping assistant**     | A Blazor chatbot component in the storefront answers shopper questions in context.                                                   | Conversational shopping built directly into the web app.          |
|      :electric_plug:      | Pluggable AI providers        | OpenAI, Azure OpenAI or a local Ollama model, selected in the app host and **disabled by default**.                                  | Run fully offline, or bring your own model and key.               |
|        :satellite:        | Event-driven messaging        | A **RabbitMQ** event bus with a transactional outbox keeps services consistent without direct coupling.                              | Reliable cross-service integration you can trace locally.         |
|          :lock:           | Centralised identity          | **Duende IdentityServer** issues tokens for every client, with demo users seeded on first run.                                       | OpenID Connect sign-in works immediately after `run`.             |
|           :zap:           | gRPC basket contract          | The basket service exposes a typed `basket.proto` contract alongside HTTP.                                                           | Low-latency, strongly typed calls between services.               |
| :arrows_counterclockwise: | Mobile back end for front end | A **YARP** reverse proxy fronts catalog, ordering and identity for mobile clients.                                                   | One stable endpoint for mobile apps to call.                      |
|        :bar_chart:        | Built-in observability        | **OpenTelemetry** tracing and metrics plus HTTP health checks are configured through shared service defaults.                        | Diagnose behaviour from the Aspire dashboard while it runs.       |
|         :iphone:          | Mobile and hybrid clients     | A **.NET MAUI** app and a Blazor Hybrid app share the same back end as the web storefront.                                           | See one API surface serve web, desktop and mobile.                |
|        :test_tube:        | Layered test suite            | Unit and functional tests for basket, catalog and ordering, plus Playwright browser tests.                                           | Verify a change locally before opening a pull request.            |

## High-level Architecture

[PLACEHOLDER IT WILL BE UPDATED MANUALLY BY THE USER LATER]

## Repository Structure

| Path                       | Contains                                                                            |
| -------------------------- | ----------------------------------------------------------------------------------- |
| `src/`                     | All application, service and shared library projects.                               |
| `tests/`                   | Five unit and functional test projects.                                             |
| `e2e/`                     | Playwright end-to-end specifications and the sign-in setup script.                  |
| `.github/workflows/`       | Continuous integration: build and test, .NET MAUI build, Playwright, Markdown lint. |
| `.config/`                 | WinGet DSC configurations that install the developer prerequisites.                 |
| `.aspire/`                 | Pointer to the app host project used by Aspire tooling.                             |
| `img/`                     | Architecture and home page screenshots.                                             |
| `eShop.slnx`               | Full solution, including the .NET MAUI client.                                      |
| `eShop.Web.slnf`           | Web-only solution filter — the one continuous integration builds.                   |
| `Directory.Packages.props` | Central NuGet package version management for every project.                         |
| `global.json`              | Pinned .NET SDK version and the test runner selection.                              |

The `src/` folder is organised as follows.

```text
src/
├── eShop.AppHost/            # .NET Aspire orchestrator — the entry point
├── eShop.ServiceDefaults/    # Shared telemetry, health, auth and HTTP defaults
├── Catalog.API/              # Products, images, semantic search, embeddings
├── Basket.API/               # Redis-backed basket, exposed over gRPC
├── Identity.API/             # Duende IdentityServer, demo user seeding
├── Ordering.API/             # Order endpoints and application layer
├── Ordering.Domain/          # Aggregates, domain events, seed work
├── Ordering.Infrastructure/  # EF Core persistence for ordering
├── OrderProcessor/           # Background order state processing
├── PaymentProcessor/         # Payment integration events
├── Webhooks.API/             # Webhook subscriptions and delivery
├── WebhookClient/            # Sample app that subscribes to webhooks
├── EventBus/                 # Event bus abstractions
├── EventBusRabbitMQ/         # RabbitMQ event bus implementation
├── IntegrationEventLogEF/    # Transactional outbox for integration events
├── WebApp/                   # Blazor storefront and AI chatbot
├── WebAppComponents/         # Razor components shared by web and hybrid apps
├── HybridApp/                # Blazor Hybrid client
├── ClientApp/                # .NET MAUI mobile client
└── Shared/                   # Cross-cutting helpers and database migration support
```

## Prerequisites

| Requirement                       | Version                                                  | Why it is needed                                                              | Required for |
| --------------------------------- | -------------------------------------------------------- | ----------------------------------------------------------------------------- | :----------: |
| .NET SDK                          | `10.0.100` or later feature band, prerelease allowed     | Pinned in `global.json`; every project targets `net10.0`.                     |  Everything  |
| Container runtime                 | Any Docker-compatible engine, for example Docker Desktop | The app host starts **PostgreSQL**, **Redis** and **RabbitMQ** as containers. |   Running    |
| **HTTPS development certificate** | Trusted locally                                          | Services default to HTTPS launch profiles.                                    |   Running    |
| Node.js                           | Latest LTS                                               | Installs and drives the Playwright browser tests.                             |  E2E tests   |
| **.NET MAUI workloads**           | `android`, `ios`, `maccatalyst`, `maui`                  | Required only to build the mobile client.                                     | `ClientApp`  |

> [!IMPORTANT]
> A running **container runtime** is required before the first launch. The app host provisions PostgreSQL, Redis and RabbitMQ as containers, so start Docker Desktop — or an equivalent engine — before running eShop.

## Getting Started

1. Clone the repository and change into it.

   ```bash
   git clone https://github.com/dotnet/eShop.git
   cd eShop
   ```

2. Trust the local HTTPS development certificate.

   ```bash
   dotnet dev-certs https --trust
   ```

3. Build the web solution to restore packages and verify your toolchain.

   ```bash
   dotnet build eShop.Web.slnf
   ```

4. Start the whole application through the .NET Aspire app host.

   ```bash
   dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj
   ```

5. Open the Aspire dashboard printed in the console, then follow the **Online Store** link to reach the storefront at `https://localhost:7298` or `http://localhost:5045`.

6. Sign in with a seeded demo account — user `alice` or `bob`, password `Pass123$` — to place an order.

> [!NOTE]
> The AI features are **disabled by default**: the `useOpenAI` and `useOllama` switches in the app host are set to `false`, so no model provider or API key is needed to run eShop. Enable one of them to activate semantic catalog search and the shopping assistant.

## Examples

| Command                                                                                                      | What it does                                                                             |
| ------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------- |
| `dotnet build eShop.Web.slnf`                                                                                | Builds every web, service and test project — the same build continuous integration runs. |
| `dotnet test --solution eShop.Web.slnf --no-build --no-progress --output detailed`                           | Runs the unit and functional test suites.                                                |
| `dotnet build src/ClientApp/ClientApp.csproj`                                                                | Builds the .NET MAUI client after the workloads are installed.                           |
| `dotnet test --project tests/ClientApp.UnitTests/ClientApp.UnitTests.csproj --no-progress --output detailed` | Runs the .NET MAUI client unit tests.                                                    |
| `npm ci`                                                                                                     | Installs the Playwright toolchain and its dependencies.                                  |
| `npx playwright install chromium`                                                                            | Downloads the browser used by the end-to-end suite.                                      |
| `npx playwright test`                                                                                        | Runs the browse, add-item and remove-item end-to-end scenarios.                          |

Set `ESHOP_USE_HTTP_ENDPOINTS=1` before starting the app host to force plain HTTP endpoints; the Playwright workflow uses this so browser tests do not depend on a trusted certificate.

## Support / Getting Help

- **Questions, bugs and feature ideas** — open an issue in the [issue tracker](https://github.com/dotnet/eShop/issues) with a clear title, a description, and any examples or mockups.
- **New contributors** — start with issues labelled `help wanted` or `good first issue`.
- **Contribution process** — see [CONTRIBUTING.md](CONTRIBUTING.md).
- **Community expectations** — see the [Code of Conduct](CODE-OF-CONDUCT.md).

**TODO (reviewer):** add a link to the project wiki or extended documentation site here; no wiki or `docs/` location is recorded anywhere in this repository.

## Contributing

Contributions are welcome, and no contribution is too small — typos and other small fixes can go straight to a pull request without opening an issue first. Larger proposals should start as an issue so the direction can be discussed before code is written.

Five principles guide what gets accepted:

- **Best practices** — the sample should stay canonical and reflect current industry and .NET practice.
- **Selectivity in tools and libraries** — the repository models a realistic technology set, not a showcase of every available library.
- **Architectural integrity** — refactoring is welcome when it comes with a clear rationale.
- **Reliability and scalability** — improvements to error handling, redundancy and data access are valued, ideally with test scenarios or metrics.
- **Performance** — speed-ups are welcome with benchmark comparisons; changes that reduce readability receive more scrutiny.

All participation is governed by the [**Code of Conduct**](CODE-OF-CONDUCT.md), adopted from the Contributor Covenant.

## License

Released under the **MIT License**. See [LICENSE](LICENSE) for the full text.

## Author / Maintainer

Copyright © **.NET Foundation and Contributors**.

eShop is maintained by the .NET team — repository metadata routes work to the DevDiv `ASP.NET Core` area — together with the open-source community contributing at [github.com/dotnet/eShop](https://github.com/dotnet/eShop). Dependency updates are automated weekly through Dependabot.
