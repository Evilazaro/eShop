# Workload Inventory — eShop

> Grounded in the workspace source: [eShop.slnx](../eShop.slnx), [src/eShop.AppHost/Program.cs](../src/eShop.AppHost/Program.cs), and the referenced project files. This inventory captures the architectural model (C4-aligned) used to generate the architecture diagrams.

---

## Software System

**Name:** eShop

**Purpose:** Provide a reference e-commerce online store enabling customers to browse a product catalog, manage a shopping basket, place and track orders, and process payments.

**Description:** A cloud-native, .NET Aspire–orchestrated distributed application composed of independently deployable microservices, background workers, and web/mobile clients. Services communicate synchronously over HTTP/gRPC and asynchronously via a RabbitMQ event bus, each owning its own PostgreSQL database or Redis store.

**Type:** Distributed application / Microservices software system

**Architecture style:** Microservices + Event-driven architecture (asynchronous integration events), with Domain-Driven Design, Clean Architecture, and CQRS in the Ordering context; orchestrated via .NET Aspire.

**Layers:**

| Layer                           | Description                                                                                   | Type           |
| ------------------------------- | --------------------------------------------------------------------------------------------- | -------------- |
| Client / Presentation           | Blazor web store, webhook client, MAUI mobile app, MAUI Blazor Hybrid app                     | UI             |
| API / Gateway                   | Mobile BFF (YARP reverse proxy) and per-service HTTP/gRPC APIs                                | Edge / API     |
| Application Services            | Microservices implementing business use cases (Catalog, Basket, Ordering, Identity, Webhooks) | Service        |
| Background Processing           | Worker services reacting to integration events (Order, Payment)                               | Worker         |
| Domain                          | Ordering aggregates, domain events, business rules                                            | Domain         |
| Data / Persistence              | PostgreSQL (pgvector), Redis, EF Core, transactional outbox                                   | Data           |
| Messaging                       | RabbitMQ event bus + integration event log (outbox)                                           | Infrastructure |
| Cross-Cutting (ServiceDefaults) | Telemetry, health, service discovery, resilient HTTP, auth, OpenAPI                           | Infrastructure |

**Interactions:**

| Interaction                                      | Description                                   | Type                    |
| ------------------------------------------------ | --------------------------------------------- | ----------------------- |
| Customer → WebApp                                | Browse, basket, checkout via Blazor Server UI | Synchronous (HTTPS)     |
| WebApp → Catalog/Basket/Ordering APIs            | Typed HTTP/gRPC service clients               | Synchronous (HTTP/gRPC) |
| Mobile client → Mobile BFF (YARP) → APIs         | Aggregated routing for mobile                 | Synchronous (HTTP)      |
| Clients ↔ Identity.API                           | OpenID Connect / OAuth2 authentication        | Synchronous (OIDC)      |
| Services → RabbitMQ event bus                    | Publish/subscribe integration events          | Asynchronous (AMQP)     |
| Ordering/Catalog/Webhooks → transactional outbox | Reliable event publishing (outbox)            | Asynchronous            |
| order-processor / payment-processor ← event bus  | Event-driven background processing            | Asynchronous            |

**Relationships:**

| Relationship                     | Description                                                | Type                |
| -------------------------------- | ---------------------------------------------------------- | ------------------- |
| eShop → PostgreSQL               | Per-service databases (catalog/identity/ordering/webhooks) | Uses (data)         |
| eShop → Redis                    | Basket state store                                         | Uses (data)         |
| eShop → RabbitMQ                 | Event bus backbone                                         | Uses (messaging)    |
| eShop → Identity Provider        | Duende IdentityServer for AuthN/AuthZ                      | Uses (security)     |
| eShop → OpenAI/Ollama (optional) | Text embeddings & chat for catalog (disabled by default)   | Uses (AI, optional) |

**Bounded Contexts:**

| Bounded Context | Purpose                        | Description                                    | Type       | Interactions                                  | Relationships    |
| --------------- | ------------------------------ | ---------------------------------------------- | ---------- | --------------------------------------------- | ---------------- |
| Catalog         | Product catalog management     | Items, brands, types, AI semantic search       | Core       | Publishes price/stock events                  | Owns catalogdb   |
| Basket          | Shopping basket                | Redis-backed basket via gRPC                   | Supporting | Consumes catalog data; publishes checkout     | Uses redis       |
| Ordering        | Order lifecycle                | DDD/CQRS order aggregate, buyer aggregate      | Core       | Reacts to checkout; emits order status events | Owns orderingdb  |
| Identity        | Authentication & authorization | OIDC/OAuth2 token issuance                     | Generic    | Authenticates all clients & services          | Owns identitydb  |
| Payment         | Payment processing             | Confirms/rejects payment on stock confirmation | Supporting | Subscribes stock-confirmed event              | Stateless worker |
| Webhooks        | Outbound webhook subscriptions | Notifies external subscribers                  | Supporting | Subscribes domain integration events          | Owns webhooksdb  |

**Domains:**

| Domain     | Purpose              | Description                 | Type | Interactions             | Relationships |
| ---------- | -------------------- | --------------------------- | ---- | ------------------------ | ------------- |
| E-Commerce | Sell products online | Overarching business domain | Core | Encompasses all contexts | Root domain   |

**Sub-Domains:**

| Sub-Domain         | Purpose                  | Description                  | Type       | Interactions              | Relationships      |
| ------------------ | ------------------------ | ---------------------------- | ---------- | ------------------------- | ------------------ |
| Catalog management | Manage sellable products | Browsing, search, pricing    | Core       | Feeds basket & ordering   | → Catalog context  |
| Order management   | Capture & fulfill orders | Order/buyer aggregates, saga | Core       | Coordinates payment/stock | → Ordering context |
| Basket management  | Hold pre-checkout items  | Session basket               | Supporting | Converts to order         | → Basket context   |
| Payment            | Authorize payments       | Payment result events        | Supporting | Reacts to ordering        | → Payment context  |
| Identity & access  | Authenticate users       | Users, clients, tokens       | Generic    | Cross-cutting security    | → Identity context |
| Notifications      | External event delivery  | Webhook subscriptions        | Supporting | Reacts to events          | → Webhooks context |

**Actors:**

| Actor              | Purpose                     | Description                 | Type              | Interactions                   | Relationships              |
| ------------------ | --------------------------- | --------------------------- | ----------------- | ------------------------------ | -------------------------- |
| Customer / Shopper | Buy products                | End user of the store       | Human (primary)   | Uses WebApp / mobile clients   | Authenticated via Identity |
| Administrator      | Manage catalog/orders       | Privileged operator         | Human             | Uses WebApp with elevated role | Authenticated via Identity |
| Webhook Subscriber | Receive event notifications | External third-party system | External (system) | Receives HTTP callbacks        | Registered in Webhooks.API |

**Applications:**

| Application      | Purpose               | Description                              | Type       | Interactions                        | Relationships                         |
| ---------------- | --------------------- | ---------------------------------------- | ---------- | ----------------------------------- | ------------------------------------- |
| WebApp           | Online store UI       | Blazor Server storefront                 | Web app    | Calls catalog/basket/ordering; OIDC | → APIs, Identity, event bus           |
| WebhookClient    | Webhook management UI | Blazor client for subscriptions          | Web app    | Calls webhooks-api                  | → Webhooks.API, Identity              |
| ClientApp        | Mobile store          | .NET MAUI MVVM app                       | Mobile app | Calls Mobile BFF                    | → mobile-bff, Identity                |
| HybridApp        | Hybrid mobile store   | .NET MAUI Blazor Hybrid                  | Mobile app | Calls Mobile BFF / APIs             | → mobile-bff, Identity                |
| Identity.API     | AuthN/AuthZ           | Duende IdentityServer                    | Service    | Issues tokens                       | → identitydb                          |
| Catalog.API      | Catalog service       | Versioned REST + AI search               | Service    | Publishes events                    | → catalogdb, event bus                |
| Basket.API       | Basket service        | gRPC over Redis                          | Service    | Publishes checkout                  | → redis, event bus                    |
| Ordering.API     | Ordering service      | DDD/CQRS REST API                        | Service    | Emits order events                  | → orderingdb, domain/infra, event bus |
| OrderProcessor   | Order worker          | Background grace-period/status processor | Worker     | Consumes/produces events            | → orderingdb, event bus               |
| PaymentProcessor | Payment worker        | Payment result publisher                 | Worker     | Subscribes stock-confirmed          | → event bus                           |
| Webhooks.API     | Webhooks service      | Manages & dispatches webhooks            | Service    | Subscribes events                   | → webhooksdb, event bus               |
| Mobile BFF       | Backend-for-frontend  | YARP reverse proxy for mobile            | Gateway    | Routes to catalog/ordering/identity | → APIs                                |

**External Systems:**

| External System              | Purpose              | Description                                        | Type               | Interactions                         | Relationships          |
| ---------------------------- | -------------------- | -------------------------------------------------- | ------------------ | ------------------------------------ | ---------------------- |
| OpenAI / Azure OpenAI        | AI embeddings & chat | Semantic catalog search (optional, off by default) | External SaaS      | Catalog & WebApp call for embeddings | Optional               |
| Ollama                       | Local LLM runtime    | Alternative to OpenAI (optional, off by default)   | External container | Catalog & WebApp                     | Optional               |
| Webhook Subscriber endpoints | Receive callbacks    | Third-party HTTP receivers                         | External system    | Receive webhook POSTs                | Registered dynamically |

**Cross-Cutting Concerns:**

| Cross-Cutting Concern          | Purpose             | Description                                     | Type           | Interactions             | Relationships        |
| ------------------------------ | ------------------- | ----------------------------------------------- | -------------- | ------------------------ | -------------------- |
| Observability                  | Traces/metrics/logs | OpenTelemetry + health checks (ServiceDefaults) | Infrastructure | All services             | MapDefaultEndpoints  |
| Security                       | AuthN/AuthZ         | OIDC/JWT bearer, claims                         | Infrastructure | All clients/services     | Identity.API         |
| Messaging & Outbox             | Reliable eventing   | RabbitMQ bus + IntegrationEventLogEF            | Infrastructure | Event-producing services | Transactional outbox |
| Resilience & Service Discovery | Robust calls        | Resilient HttpClient, Aspire discovery, YARP    | Infrastructure | Inter-service calls      | ServiceDefaults      |
| API Versioning & OpenAPI       | API governance      | Asp.Versioning, OpenAPI docs                    | Infrastructure | HTTP APIs                | Catalog/Ordering     |
| Validation                     | Input integrity     | FluentValidation + MediatR behaviors            | Application    | Ordering commands        | Application layer    |

---

## Application — Ordering.API (representative DDD / Clean Architecture service)

**Name:** Ordering.API

**Purpose:** Manage the full order lifecycle (creation, buyer/payment info, stock confirmation, shipment, cancellation) as the system's core transactional context.

**Description:** A REST API implementing Domain-Driven Design with Clean Architecture and CQRS. Commands/queries flow through a MediatR pipeline with FluentValidation behaviors; the Domain project holds aggregates and domain events; the Infrastructure project provides EF Core persistence, repositories, idempotency, and a transactional outbox for integration events.

**Type:** Microservice (REST API)

**Architecture style:** Clean Architecture + Domain-Driven Design + CQRS (event-driven integration).

**Layers:**

| Layer                                    | Description                                                                       | Type           |
| ---------------------------------------- | --------------------------------------------------------------------------------- | -------------- |
| API (Apis/)                              | Minimal API endpoints, versioning, auth                                           | Presentation   |
| Application (Application/)               | Commands, Queries, Behaviors, Validations, DomainEventHandlers, IntegrationEvents | Application    |
| Domain (Ordering.Domain)                 | OrderAggregate, BuyerAggregate, domain events, SeedWork                           | Domain         |
| Infrastructure (Ordering.Infrastructure) | OrderingContext (EF Core), repositories, migrations, idempotency                  | Infrastructure |

**Interactions:**

| Interaction                            | Description                             | Type                |
| -------------------------------------- | --------------------------------------- | ------------------- |
| Client → API endpoints                 | Create/query orders (authorized)        | Synchronous (HTTP)  |
| API → Application (MediatR)            | Dispatch commands/queries               | In-process          |
| Application → Domain                   | Invoke aggregate behavior               | In-process          |
| Application/Infrastructure → event bus | Publish order status integration events | Asynchronous (AMQP) |
| Infrastructure → orderingdb            | Persist orders/buyers via EF Core       | Synchronous (DB)    |

**Relationships:**

| Relationship                           | Description                             | Type         |
| -------------------------------------- | --------------------------------------- | ------------ |
| Ordering.API → Ordering.Domain         | Uses aggregates & domain events         | Depends on   |
| Ordering.API → Ordering.Infrastructure | Uses EF context & repositories          | Depends on   |
| Ordering.API → EventBusRabbitMQ        | Publishes/subscribes integration events | Depends on   |
| Ordering.API → IntegrationEventLogEF   | Transactional outbox                    | Depends on   |
| OrderProcessor → Ordering              | Reacts to order events, updates status  | Collaborates |

**Bounded Contexts:**

| Bounded Context | Purpose         | Description                           | Type | Interactions                | Relationships   |
| --------------- | --------------- | ------------------------------------- | ---- | --------------------------- | --------------- |
| Ordering        | Order lifecycle | The context this application realizes | Core | Emits/consumes order events | Owns orderingdb |

**Domains:**

| Domain           | Purpose                  | Description        | Type | Interactions                | Relationships      |
| ---------------- | ------------------------ | ------------------ | ---- | --------------------------- | ------------------ |
| Order management | Capture & fulfill orders | Core business flow | Core | Coordinates payment & stock | → Payment, Catalog |

**Sub-Domains:**

| Sub-Domain      | Purpose                           | Description                 | Type       | Interactions         | Relationships     |
| --------------- | --------------------------------- | --------------------------- | ---------- | -------------------- | ----------------- |
| Order aggregate | Consistency boundary for an order | Order + order items         | Core       | State transitions    | → Buyer aggregate |
| Buyer aggregate | Buyer & payment methods           | Payment method verification | Supporting | Referenced by orders | → Order aggregate |

**Actors:**

| Actor          | Purpose              | Description           | Type   | Interactions                        | Relationships |
| -------------- | -------------------- | --------------------- | ------ | ----------------------------------- | ------------- |
| Customer       | Place/track orders   | Authenticated shopper | Human  | Calls ordering endpoints via WebApp | Via Identity  |
| OrderProcessor | Advance order status | Background worker     | System | Consumes/produces order events      | Via event bus |

**External Systems:**

| External System  | Purpose        | Description                  | Type                                  | Interactions  | Relationships |
| ---------------- | -------------- | ---------------------------- | ------------------------------------- | ------------- | ------------- |
| PaymentProcessor | Payment result | Reacts to stock confirmation | Internal worker (external to context) | Via event bus | Collaborates  |

**Cross-Cutting Concerns:**

| Cross-Cutting Concern | Purpose               | Description                         | Type           | Interactions     | Relationships              |
| --------------------- | --------------------- | ----------------------------------- | -------------- | ---------------- | -------------------------- |
| Validation            | Command integrity     | FluentValidation + MediatR behavior | Application    | All commands     | Behaviors/                 |
| Idempotency           | Exactly-once handling | Request idempotency store           | Infrastructure | Command handlers | Infrastructure/Idempotency |
| Observability         | Tracing               | OpenTelemetry via ServiceDefaults   | Infrastructure | All requests     | ServiceDefaults            |
| Security              | AuthZ                 | Bearer token auth                   | Infrastructure | API endpoints    | Identity.API               |

**Components:**

| Component               | Purpose                | Description                               | Type                | Interactions            | Relationships    |
| ----------------------- | ---------------------- | ----------------------------------------- | ------------------- | ----------------------- | ---------------- |
| Order Endpoints (Apis/) | HTTP surface           | Create/cancel/ship/query orders           | Controller/endpoint | Calls MediatR           | → Application    |
| Commands & Handlers     | Write use cases        | CreateOrder, ShipOrder, CancelOrder, etc. | Application service | Invoke domain           | → Domain         |
| Queries                 | Read models            | Order lists & details                     | Application service | Read via EF             | → Infrastructure |
| Pipeline Behaviors      | Cross-cutting pipeline | Validation, logging, transaction          | Behavior            | Wrap handlers           | → Validations    |
| Domain Event Handlers   | React to domain events | Buyer/payment/stock reactions             | Handler             | Emit integration events | → Event bus      |
| OrderAggregate          | Order consistency      | Root entity + order items                 | Aggregate root      | Raises domain events    | → BuyerAggregate |
| BuyerAggregate          | Buyer/payment methods  | Payment method verification               | Aggregate root      | Referenced by orders    | → OrderAggregate |
| OrderingContext         | Persistence            | EF Core DbContext + configs               | Repository/context  | CRUD orderingdb         | → orderingdb     |
| Integration Event Log   | Outbox                 | Reliable event publishing                 | Infrastructure      | Writes with order tx    | → Event bus      |
