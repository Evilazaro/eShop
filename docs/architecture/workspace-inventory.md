# eShop — Workspace Architecture Inventory

> Grounded in workspace source: `src/eShop.AppHost/Program.cs` (resource graph), `global.json` (.NET 10 SDK 10.0.100), each project's `*.csproj` / `Program.cs`, and exact `AddSubscription<>` event-handler wiring. Deployment target selected by user: **Azure Container Apps**.

---

## 1. Software System

**Name:** eShop

**Purpose:** Reference e‑commerce application demonstrating a cloud‑native, event‑driven .NET microservices architecture orchestrated with .NET Aspire.

**Description:** A composite online‑store system composed of independently deployable back‑end services (Identity, Catalog, Basket, Ordering, Payment, Webhooks) fronted by web/mobile experiences (Blazor WebApp, MAUI ClientApp, Hybrid app, Webhook client) and a YARP mobile BFF. Services communicate synchronously over HTTP/gRPC and asynchronously via a RabbitMQ event bus, persisting state to PostgreSQL (pgvector) and Redis. Cross‑cutting concerns (telemetry, resilience, service discovery, auth) are centralized in `eShop.ServiceDefaults`.

**Type:** Software System (cloud‑native, multi‑service)

**Architecture style:** Event‑driven microservices · Domain‑Driven Design + Clean Architecture (Ordering) · CQRS (MediatR) · Transactional Outbox · Backend‑for‑Frontend / API Gateway (YARP) · Publish/Subscribe · Cloud‑native (.NET Aspire)

**Layers:**

| Layer                 | Description                                                                      | Type           |
| --------------------- | -------------------------------------------------------------------------------- | -------------- |
| Presentation / Client | Web, mobile, hybrid, and webhook UIs consumed by end users                       | UI             |
| Edge / Gateway        | YARP mobile BFF routing/aggregation for mobile clients                           | Gateway/BFF    |
| Application (API)     | Service HTTP/gRPC endpoints; MediatR command/query handlers & pipeline behaviors | Application    |
| Domain                | DDD aggregates, domain events, value objects (Ordering)                          | Domain         |
| Infrastructure        | EF Core, repositories, event bus, outbox, external integrations                  | Infrastructure |
| Messaging             | RabbitMQ event bus for async integration events                                  | Messaging      |
| Data                  | PostgreSQL (pgvector) databases, Redis cache                                     | Persistence    |
| Cross‑cutting         | Observability, resilience, service discovery, authentication (ServiceDefaults)   | Cross‑cutting  |

**Interactions:**

| Interaction                         | Description                                         | Type                    |
| ----------------------------------- | --------------------------------------------------- | ----------------------- |
| Browser ⇄ WebApp                    | Interactive server‑side Blazor rendering            | HTTPS                   |
| WebApp ⇄ Basket.API                 | Basket read/write                                   | gRPC                    |
| WebApp ⇄ Catalog.API / Ordering.API | Product browse & order placement                    | HTTPS (REST, versioned) |
| Mobile ⇄ mobile‑bff ⇄ services      | Mobile requests routed to Catalog/Ordering/Identity | HTTPS (YARP proxy)      |
| Apps ⇄ Identity.API                 | OIDC/OAuth2 authentication & token issuance         | OIDC/OAuth2             |
| Services ⇄ RabbitMQ                 | Publish/subscribe integration events                | AMQP (async)            |
| Services ⇄ PostgreSQL               | Relational persistence + vector search              | TCP (Npgsql/EF Core)    |
| Basket.API ⇄ Redis                  | Cart storage                                        | TCP (RESP)              |
| Webhooks.API → Subscriber           | Event notification delivery                         | HTTPS POST              |

**Relationships:**

| Relationship                           | Description                                 | Type                 |
| -------------------------------------- | ------------------------------------------- | -------------------- |
| WebApp → Basket/Catalog/Ordering       | Frontend depends on back‑end APIs           | Uses                 |
| mobile‑bff → Catalog/Ordering/Identity | Gateway routes to services                  | Routes to            |
| All secured services → Identity.API    | JWT/OIDC trust                              | Authenticates with   |
| All eventing services → RabbitMQ       | Async decoupling                            | Publishes/Subscribes |
| OrderProcessor → Ordering.API          | Waits for migrations, monitors grace period | Depends on           |
| Ordering/Catalog/Webhooks → PostgreSQL | Owns a private database                     | Persists to          |
| Basket.API → Redis                     | Owns cart cache                             | Persists to          |

**Bounded Contexts:**

| Bounded Context | Purpose                | Description                                                        | Type       | Interactions                                                          | Relationships                                                 |
| --------------- | ---------------------- | ------------------------------------------------------------------ | ---------- | --------------------------------------------------------------------- | ------------------------------------------------------------- |
| Ordering        | Manage order lifecycle | Order aggregate, state machine, grace period, payment coordination | Core       | Consumes stock/payment/grace events; publishes order‑status events    | Integrates via events with Catalog, Basket, Payment, Webhooks |
| Catalog         | Product & inventory    | Products, brands, types, stock, AI vector search                   | Supporting | Consumes order‑validation/paid events; publishes stock & price events | Feeds Ordering stock validation                               |
| Basket          | Shopping cart          | Redis‑backed cart per user                                         | Supporting | Consumes OrderStarted (clear cart)                                    | Read by WebApp/Mobile via gRPC                                |
| Identity        | AuthN/AuthZ            | Duende IdentityServer + ASP.NET Identity                           | Generic    | Issues tokens to all clients                                          | Trust anchor for all services                                 |
| Payment         | Payment processing     | Simulated payment authorization                                    | Generic    | Consumes stock‑confirmed; publishes payment succeeded/failed          | Coordinates with Ordering                                     |
| Webhooks        | Outbound notifications | Subscription mgmt + event delivery                                 | Generic    | Consumes price/order events; POSTs to subscribers                     | Notifies external subscribers                                 |

**Domains:**

| Domain                      | Purpose                        | Description                        | Type | Interactions                                     | Relationships              |
| --------------------------- | ------------------------------ | ---------------------------------- | ---- | ------------------------------------------------ | -------------------------- |
| E‑commerce Order Management | Sell products & fulfill orders | The competitive core of the system | Core | Orchestrates catalog, basket, payment via events | Central to all sub‑domains |

**Sub-Domains:**

| Sub-Domain            | Purpose                 | Description                       | Type       | Interactions                    | Relationships               |
| --------------------- | ----------------------- | --------------------------------- | ---------- | ------------------------------- | --------------------------- |
| Ordering              | Order lifecycle & rules | Aggregate + saga‑style event flow | Core       | Stock/payment/grace events      | Depends on Catalog, Payment |
| Catalog               | Product discovery       | Catalog + semantic/vector search  | Supporting | Stock validation, price changes | Supports Ordering           |
| Basket                | Cart management         | Ephemeral cart in Redis           | Supporting | Cleared on order start          | Feeds Ordering              |
| Identity & Access     | Authentication          | OIDC/OAuth2 provider              | Generic    | Token issuance                  | Used by all                 |
| Payment               | Payment authorization   | Simulated processor               | Generic    | Payment events                  | Supports Ordering           |
| Webhook Notifications | External integration    | Subscription & delivery           | Generic    | Delivers events                 | Notifies subscribers        |

**Actors:**

| Actor                    | Purpose             | Description                                    | Type          | Interactions                               | Relationships              |
| ------------------------ | ------------------- | ---------------------------------------------- | ------------- | ------------------------------------------ | -------------------------- |
| Customer / Shopper       | Browse, cart, order | End user via web or mobile                     | Person        | Uses WebApp, ClientApp, HybridApp          | Authenticated via Identity |
| Store Administrator      | Manage catalog      | Updates products/prices/stock                  | Person        | Uses Catalog admin endpoints               | Authenticated via Identity |
| Webhook Subscriber (Dev) | Receive events      | External developer/system registering webhooks | Person/System | Registers via Webhooks.API; receives POSTs | Uses WebhookClient sample  |

**Applications:**

| Application      | Purpose                 | Description                          | Type          | Interactions                               | Relationships                 |
| ---------------- | ----------------------- | ------------------------------------ | ------------- | ------------------------------------------ | ----------------------------- |
| WebApp           | Storefront              | Blazor interactive server storefront | Web app       | gRPC→Basket, HTTP→Catalog/Ordering, events | OIDC→Identity                 |
| mobile‑bff       | Mobile gateway          | YARP reverse proxy/BFF               | Gateway       | Routes→Catalog/Ordering/Identity           | Fronts mobile clients         |
| ClientApp        | Mobile app              | .NET MAUI native app                 | Mobile app    | gRPC→Basket, HTTP→Catalog/Ordering         | Via mobile‑bff; OIDC→Identity |
| HybridApp        | Hybrid app              | MAUI + Blazor WebView                | Mobile/Hybrid | Reuses WebAppComponents                    | OIDC→Identity                 |
| WebhookClient    | Webhook receiver sample | Blazor Server receiver               | Web app       | Receives POST from Webhooks.API            | OIDC→Identity                 |
| Identity.API     | Identity provider       | Duende IdentityServer                | Service       | Token endpoints                            | Trusted by all                |
| Catalog.API      | Catalog service         | REST + pgvector + AI                 | Service       | REST, events                               | catalogdb, RabbitMQ           |
| Basket.API       | Basket service          | gRPC service                         | Service       | gRPC, events                               | Redis, RabbitMQ               |
| Ordering.API     | Ordering service        | Clean Arch/DDD/CQRS                  | Service       | REST, events                               | orderingdb, RabbitMQ          |
| OrderProcessor   | Grace‑period worker     | Background hosted service            | Worker        | Publishes GracePeriodConfirmed             | orderingdb, RabbitMQ          |
| PaymentProcessor | Payment worker          | Simulated payment                    | Worker        | Payment events                             | RabbitMQ                      |
| Webhooks.API     | Webhook service         | Subscription + delivery              | Service       | REST, events, outbound POST                | webhooksdb, RabbitMQ          |

**External Systems:**

| External System       | Purpose           | Description                             | Type        | Interactions                    | Relationships               |
| --------------------- | ----------------- | --------------------------------------- | ----------- | ------------------------------- | --------------------------- |
| OpenAI / Azure OpenAI | Embeddings & chat | Optional (`useOpenAI=false` by default) | External AI | Catalog embeddings, WebApp chat | Used by Catalog.API, WebApp |
| Ollama                | Local LLM         | Optional (`useOllama=false` by default) | External AI | Local models                    | Used by Catalog.API, WebApp |
| Subscriber endpoints  | Receive webhooks  | External HTTPS endpoints                | External    | HTTPS POST from Webhooks.API    | Registered subscribers      |

**Cross-Cutting Concerns:**

| Cross-Cutting Concern    | Purpose             | Description                              | Type           | Interactions               | Relationships               |
| ------------------------ | ------------------- | ---------------------------------------- | -------------- | -------------------------- | --------------------------- |
| Observability            | Traces/metrics/logs | OpenTelemetry + OTLP export              | Telemetry      | Instruments all services   | ServiceDefaults             |
| Resilience               | Fault tolerance     | Polly retry/timeout/circuit breaker      | Reliability    | All HTTP clients           | ServiceDefaults             |
| Service Discovery        | Locate services     | DNS‑based discovery via Aspire           | Infrastructure | HTTP client resolution     | ServiceDefaults             |
| Authentication           | Secure APIs         | JWT bearer validation                    | Security       | All secured services       | Identity.API                |
| Event Bus                | Async messaging     | RabbitMQ publish/subscribe               | Messaging      | All eventing services      | EventBus / EventBusRabbitMQ |
| Outbox                   | Reliable publishing | IntegrationEventLog persisted with state | Reliability    | Ordering (+ event logging) | IntegrationEventLogEF       |
| Health Checks            | Liveness/readiness  | `/health`, `/alive`                      | Ops            | All services               | ServiceDefaults             |
| API Versioning + OpenAPI | Contract mgmt       | Asp.Versioning + Scalar                  | API            | Catalog/Ordering/Webhooks  | ServiceDefaults             |

---

## 2. Applications (Component detail)

### 2.1 WebApp

**Name:** WebApp · **Purpose:** Customer‑facing storefront · **Type:** Blazor (Razor Components, interactive server) · **Architecture style:** Component‑based UI + BFF client

**Layers:** Presentation (Razor components) · Application (service clients) · Cross‑cutting (ServiceDefaults)

**Components:**

| Component                      | Purpose               | Description                        | Type     | Interactions                         | Relationships                           |
| ------------------------------ | --------------------- | ---------------------------------- | -------- | ------------------------------------ | --------------------------------------- |
| Catalog service client         | Browse products       | HTTP client (API v2)               | Client   | HTTP→Catalog.API                     | Service discovery                       |
| Basket service client          | Manage cart           | gRPC client                        | Client   | gRPC→Basket.API                      | —                                       |
| Ordering service client        | Place/track orders    | HTTP client (API v1)               | Client   | HTTP→Ordering.API                    | —                                       |
| OrderStatusNotificationService | Live order status     | In‑memory subscription updating UI | Handler  | Consumes 6 OrderStatusChanged events | RabbitMQ                                |
| OIDC auth                      | Sign‑in               | OpenID Connect client (`webapp`)   | Security | →Identity.API                        | Scopes: openid, profile, orders, basket |
| Product image proxy            | Serve images          | `/product-images/{id}` → Catalog   | Proxy    | HTTP→Catalog.API                     | —                                       |
| AI chat (optional)             | Conversational search | OpenAI/Ollama assistant            | AI       | →OpenAI/Ollama                       | Disabled by default                     |

**Consumes events:** OrderStatusChangedTo{AwaitingValidation, Paid, StockConfirmed, Shipped, Cancelled, Submitted}

### 2.2 mobile-bff (YARP)

**Name:** mobile‑bff · **Purpose:** Mobile backend‑for‑frontend · **Type:** YARP reverse proxy · **Architecture style:** API Gateway / BFF

**Components:**

| Component            | Purpose         | Description | Type  | Interactions  | Relationships            |
| -------------------- | --------------- | ----------- | ----- | ------------- | ------------------------ |
| Route: /catalog-api  | Catalog access  | Proxy route | Route | →Catalog.API  | ConfigureMobileBffRoutes |
| Route: /ordering-api | Ordering access | Proxy route | Route | →Ordering.API | —                        |
| Route: /identity     | Auth access     | Proxy route | Route | →Identity.API | —                        |

### 2.3 Identity.API

**Name:** Identity.API · **Purpose:** OIDC/OAuth2 provider · **Type:** ASP.NET Core + Duende IdentityServer + ASP.NET Identity · **Architecture style:** Identity Provider

**Components:**

| Component                | Purpose                  | Description                                                    | Type     | Interactions          | Relationships   |
| ------------------------ | ------------------------ | -------------------------------------------------------------- | -------- | --------------------- | --------------- |
| IdentityServer endpoints | Token/authorize/userinfo | OIDC discovery, `/connect/*`                                   | Endpoint | ←all clients          | —               |
| ASP.NET Identity store   | Users/roles/claims       | EF Core on identitydb                                          | Store    | identitydb            | —               |
| Client/scope config      | Trust config             | `webapp`, `webhooksclient`, `mobile-bff`; scopes orders/basket | Config   | Callback URLs to apps | Cyclic env refs |

### 2.4 Catalog.API

**Name:** Catalog.API · **Purpose:** Product catalog & inventory · **Type:** ASP.NET Core Web API (REST, versioned) · **Architecture style:** Layered API + Vector search

**Components:**

| Component                    | Purpose             | Description                                       | Type       | Interactions   | Relationships |
| ---------------------------- | ------------------- | ------------------------------------------------- | ---------- | -------------- | ------------- |
| Catalog endpoints (v1/v2)    | Products CRUD/query | Minimal API + Asp.Versioning                      | Endpoint   | HTTP           | —             |
| CatalogContext               | Persistence         | EF Core + pgvector                                | Repository | catalogdb      | —             |
| ICatalogAI                   | Embeddings          | Generate product embeddings                       | Service    | →OpenAI/Ollama | Optional      |
| Integration event handlers   | React to orders     | Awaiting‑validation, Paid                         | Handler    | RabbitMQ       | —             |
| Integration event publishers | Emit changes        | ProductPriceChanged, OrderStockConfirmed/Rejected | Publisher  | RabbitMQ       | Outbox‑logged |

**Consumes:** OrderStatusChangedToAwaitingValidation, OrderStatusChangedToPaid · **Publishes:** ProductPriceChanged, OrderStockConfirmed, OrderStockRejected

### 2.5 Basket.API

**Name:** Basket.API · **Purpose:** Shopping cart · **Type:** ASP.NET Core gRPC service · **Architecture style:** gRPC service + cache‑aside

**Components:**

| Component                           | Purpose      | Description                         | Type       | Interactions | Relationships |
| ----------------------------------- | ------------ | ----------------------------------- | ---------- | ------------ | ------------- |
| BasketService (gRPC)                | Cart ops     | GetBasket/UpdateBasket/DeleteBasket | Endpoint   | gRPC         | —             |
| RedisBasketRepository               | Cart storage | Redis hash per user                 | Repository | Redis        | —             |
| OrderStartedIntegrationEventHandler | Clear cart   | On order start                      | Handler    | RabbitMQ     | —             |

**Consumes:** OrderStartedIntegrationEvent

### 2.6 Ordering.API (+ Domain + Infrastructure)

**Name:** Ordering · **Purpose:** Order lifecycle management · **Type:** ASP.NET Core Web API · **Architecture style:** Clean Architecture + DDD + CQRS + Outbox

**Layers:**

| Layer                   | Description                                                                       | Type           |
| ----------------------- | --------------------------------------------------------------------------------- | -------------- |
| Ordering.API            | Application: MediatR commands/queries, pipeline behaviors, REST endpoints         | Application    |
| Ordering.Domain         | Aggregates (Order root, OrderItem, Buyer), Address VO, domain events, OrderStatus | Domain         |
| Ordering.Infrastructure | OrderingContext, EF Core repositories, migrations                                 | Infrastructure |

**Components:**

| Component                  | Purpose            | Description                                        | Type      | Interactions         | Relationships         |
| -------------------------- | ------------------ | -------------------------------------------------- | --------- | -------------------- | --------------------- |
| Order aggregate            | Enforce invariants | State machine Submitted→…→Shipped/Cancelled        | Aggregate | Raises domain events | —                     |
| CQRS handlers              | Commands/queries   | CreateOrder, CancelOrder, ShipOrder, queries       | Handler   | MediatR              | —                     |
| Pipeline behaviors         | Cross‑cutting      | Logging, Validator (FluentValidation), Transaction | Behavior  | Wraps handlers       | —                     |
| Domain→Integration mappers | Publish externally | Convert domain events to integration events        | Handler   | RabbitMQ             | Outbox                |
| IntegrationEventLogService | Outbox             | Atomic event persistence                           | Service   | orderingdb           | IntegrationEventLogEF |
| Integration event handlers | React              | Grace/stock/payment events                         | Handler   | RabbitMQ             | —                     |

**Consumes:** GracePeriodConfirmed, OrderStockConfirmed, OrderStockRejected, OrderPaymentFailed, OrderPaymentSucceeded · **Publishes:** OrderStarted + OrderStatusChangedTo\* integration events

### 2.7 OrderProcessor

**Name:** OrderProcessor · **Purpose:** Grace‑period orchestration · **Type:** Worker / BackgroundService · **Architecture style:** Background processor

**Components:**

| Component                 | Purpose             | Description                   | Type           | Interactions      | Relationships          |
| ------------------------- | ------------------- | ----------------------------- | -------------- | ----------------- | ---------------------- |
| GracePeriodManagerService | Confirm after grace | Polls orderingdb; emits event | Hosted service | Npgsql→orderingdb | Waits for Ordering.API |

**Publishes:** GracePeriodConfirmedIntegrationEvent

### 2.8 PaymentProcessor

**Name:** PaymentProcessor · **Purpose:** Simulated payments · **Type:** Worker · **Architecture style:** Event‑driven processor (stateless)

**Components:**

| Component                                                 | Purpose           | Description              | Type    | Interactions | Relationships |
| --------------------------------------------------------- | ----------------- | ------------------------ | ------- | ------------ | ------------- |
| OrderStatusChangedToStockConfirmedIntegrationEventHandler | Authorize payment | Simulate success/failure | Handler | RabbitMQ     | —             |

**Consumes:** OrderStatusChangedToStockConfirmed · **Publishes:** OrderPaymentSucceeded, OrderPaymentFailed

### 2.9 Webhooks.API

**Name:** Webhooks.API · **Purpose:** Webhook subscriptions & delivery · **Type:** ASP.NET Core Web API · **Architecture style:** Layered API + outbound integration

**Components:**

| Component                  | Purpose              | Description              | Type     | Interactions | Relationships |
| -------------------------- | -------------------- | ------------------------ | -------- | ------------ | ------------- |
| Webhooks endpoints         | Manage subscriptions | REST CRUD                | Endpoint | HTTP         | —             |
| WebhooksRetriever          | Read subscriptions   | From webhooksdb          | Service  | webhooksdb   | —             |
| WebhooksSender             | Deliver events       | HTTP POST to subscribers | Service  | →Subscriber  | —             |
| GrantUrlTesterService      | Validate URLs        | Pre‑registration check   | Service  | →Subscriber  | —             |
| Integration event handlers | React                | Price/order events       | Handler  | RabbitMQ     | —             |

**Consumes:** ProductPriceChanged, OrderStatusChangedToShipped, OrderStatusChangedToPaid

### 2.10 WebhookClient

**Name:** WebhookClient · **Purpose:** Sample webhook receiver · **Type:** Blazor Server · **Architecture style:** Component‑based UI + inbound webhook endpoint

**Components:**

| Component                 | Purpose        | Description           | Type     | Interactions  | Relationships |
| ------------------------- | -------------- | --------------------- | -------- | ------------- | ------------- |
| /webhooks/events endpoint | Receive events | HTTP POST receiver    | Endpoint | ←Webhooks.API | —             |
| OIDC auth                 | Sign‑in        | OpenID Connect client | Security | →Identity.API | —             |

### 2.11 ClientApp

**Name:** ClientApp · **Purpose:** Native mobile shopping · **Type:** .NET MAUI · **Architecture style:** MVVM cross‑platform

**Components:**

| Component                        | Purpose        | Description              | Type     | Interactions           | Relationships |
| -------------------------------- | -------------- | ------------------------ | -------- | ---------------------- | ------------- |
| Catalog/Basket/Ordering services | Backend access | HTTP + gRPC clients      | Client   | →mobile‑bff / services | —             |
| OidcClient                       | Auth           | IdentityModel.OidcClient | Security | →Identity.API          | —             |
| MVVM (CommunityToolkit)          | UI logic       | ViewModels + views       | UI       | —                      | —             |

### 2.12 HybridApp

**Name:** HybridApp · **Purpose:** Hybrid web/native · **Type:** MAUI + Blazor WebView · **Architecture style:** Hybrid component reuse

**Components:**

| Component     | Purpose     | Description             | Type     | Interactions            | Relationships |
| ------------- | ----------- | ----------------------- | -------- | ----------------------- | ------------- |
| BlazorWebView | Host web UI | Embeds Razor components | Host     | Reuses WebAppComponents | —             |
| OIDC auth     | Auth        | OpenID Connect          | Security | →Identity.API           | —             |

---

## 3. Shared Libraries / Components

| Library               | Purpose                                                  | Type           | Used by                    |
| --------------------- | -------------------------------------------------------- | -------------- | -------------------------- |
| eShop.ServiceDefaults | OTel, resilience, discovery, health, JWT, versioning     | Cross‑cutting  | All services/apps          |
| EventBus              | Event abstractions (IEventBus, IIntegrationEventHandler) | Contract       | All eventing services      |
| EventBusRabbitMQ      | RabbitMQ event bus implementation                        | Infrastructure | All eventing services      |
| IntegrationEventLogEF | Transactional outbox persistence                         | Infrastructure | Ordering (+ event logging) |
| WebAppComponents      | Shared Razor UI components                               | UI library     | WebApp, HybridApp          |
| Shared                | DB migration + activity helpers                          | Utility        | Services with EF Core      |

---

## 4. Infrastructure & External Systems

| Resource                         | Type                   | Owned/Used by                                 | Notes                      |
| -------------------------------- | ---------------------- | --------------------------------------------- | -------------------------- |
| PostgreSQL (ankane/pgvector)     | Relational + vector DB | catalogdb, identitydb, orderingdb, webhooksdb | Persistent container (dev) |
| Redis                            | Cache                  | Basket.API                                    | Persistent container (dev) |
| RabbitMQ (`eventbus`)            | Message broker         | All eventing services                         | Durable queues             |
| OpenAI / Azure OpenAI            | External AI            | Catalog.API, WebApp                           | Optional (default off)     |
| Ollama                           | Local LLM              | Catalog.API, WebApp                           | Optional (default off)     |
| OTLP endpoint / Aspire dashboard | Telemetry sink         | All services                                  | Via ServiceDefaults        |

---

## 5. Architecture Styles Summary

- **Event‑driven microservices** — RabbitMQ pub/sub across bounded contexts.
- **Domain‑Driven Design + Clean Architecture** — Ordering (Domain → Infrastructure → API).
- **CQRS** — MediatR commands/queries with pipeline behaviors in Ordering.
- **Transactional Outbox** — IntegrationEventLogEF for reliable publishing.
- **Backend‑for‑Frontend / API Gateway** — YARP `mobile-bff`.
- **Cloud‑native orchestration** — .NET Aspire AppHost; deployment target Azure Container Apps.
- **Publish/Subscribe + Cache‑aside + Saga‑style choreography** — order fulfillment flow.
