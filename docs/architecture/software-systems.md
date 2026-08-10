# eShop — Software Systems Analysis

> Grounded in `src/eShop.AppHost/Program.cs` (Aspire orchestration) and the solution/project structure of the workspace.

---

## Software System

**Name:** eShop (Reference .NET Aspire e-commerce platform)

**Purpose:** Provide an online store that lets customers browse a product catalog, manage a shopping basket, place and track orders, process payments, and receive webhook notifications.

**Description:** A cloud-native, microservices-based reference application built on .NET Aspire. It is composed of independently deployable service APIs, background processors, customer-facing apps (web and mobile), a mobile Backend-for-Frontend (BFF), and an identity provider. Services communicate synchronously over HTTP/gRPC and asynchronously through a RabbitMQ event bus using integration events and the transactional outbox pattern. Persistence uses PostgreSQL (with pgvector for catalog AI embeddings) and Redis (basket cache).

**Type:** Software System (distributed / microservices)

**Architecture style:** Microservices; Event-driven (publish/subscribe); Domain-Driven Design + CQRS (Ordering); Backend-for-Frontend (mobile-bff); Cloud-native (Aspire-orchestrated containers); Transactional Outbox.

**Layers:**

| Layer                           | Description                                                       | Type            |
| ------------------------------- | ----------------------------------------------------------------- | --------------- |
| Presentation / Apps             | Blazor web store, MAUI/Hybrid mobile client, webhook demo client  | UI              |
| API / Edge                      | Service APIs and mobile-bff (YARP) reverse proxy                  | Service/Gateway |
| Application                     | Use-case orchestration (CQRS commands/queries, MediatR behaviors) | Application     |
| Domain                          | Rich domain model, aggregates, domain events (Ordering)           | Domain          |
| Infrastructure                  | EF Core persistence, event bus, integration-event log/outbox      | Infrastructure  |
| Cross-cutting (ServiceDefaults) | Observability, auth, health, resilience, service discovery        | Shared          |

**Interactions:**

| Interaction                           | Description                          | Type         |
| ------------------------------------- | ------------------------------------ | ------------ |
| Browser → WebApp                      | Customer uses Blazor store           | HTTP         |
| Mobile app → mobile-bff               | Mobile client routed to backend APIs | HTTP         |
| WebApp → Basket/Catalog/Ordering APIs | Synchronous service calls            | HTTP/gRPC    |
| Apps/Services → Identity              | OIDC authentication / token issuance | HTTPS/OIDC   |
| Services ↔ RabbitMQ                   | Publish/subscribe integration events | AMQP (async) |
| Services → PostgreSQL                 | Persistence                          | TCP/SQL      |
| Basket → Redis                        | Cache basket state                   | TCP          |
| Catalog → OpenAI/Ollama               | Generate embeddings (optional)       | HTTP         |

**Relationships:**

| Relationship                          | Description                   | Type             |
| ------------------------------------- | ----------------------------- | ---------------- |
| WebApp → Basket/Catalog/Ordering      | Uses                          | Dependency       |
| Ordering → EventBus → Catalog/Payment | Choreography via events       | Async dependency |
| PaymentProcessor → Ordering           | Payment result events         | Async dependency |
| OrderProcessor → Ordering DB          | Grace-period order processing | Data dependency  |
| All apps/services → Identity          | Authentication                | Trust            |
| All services → ServiceDefaults        | Shared cross-cutting          | Composition      |

**Bounded Contexts:**

| Bounded Context | Purpose                  | Description                              | Type       | Interactions        | Relationships             |
| --------------- | ------------------------ | ---------------------------------------- | ---------- | ------------------- | ------------------------- |
| Catalog         | Product catalog & search | Items, brands, types, AI semantic search | Core       | Events in/out, HTTP | Catalog.API               |
| Basket          | Shopping basket          | Redis-backed transient basket            | Supporting | gRPC, events        | Basket.API                |
| Ordering        | Order lifecycle          | DDD aggregates, CQRS, order workflow     | Core       | HTTP, events        | Ordering.API/Domain/Infra |
| Identity        | AuthN/AuthZ              | OIDC/OAuth2 provider (Duende)            | Generic    | OIDC                | Identity.API              |
| Webhooks        | Subscription callbacks   | Registers & dispatches webhooks          | Supporting | HTTP, events        | Webhooks.API              |
| Payment         | Payment processing       | Simulated payment outcome                | Supporting | events              | PaymentProcessor          |

**Domains:**

| Domain              | Purpose              | Description                 | Type | Interactions | Relationships |
| ------------------- | -------------------- | --------------------------- | ---- | ------------ | ------------- |
| E-commerce / Retail | Sell products online | Overarching business domain | Core | All          | Whole system  |

**Sub-Domains:**

| Sub-Domain | Purpose                | Description                 | Type       | Interactions     | Relationships    |
| ---------- | ---------------------- | --------------------------- | ---------- | ---------------- | ---------------- |
| Catalog    | Browse/search products | Includes AI-assisted search | Core       | Catalog context  | Catalog.API      |
| Ordering   | Purchase & fulfilment  | Order aggregate + workflow  | Core       | Ordering context | Ordering.\*      |
| Basket     | Pre-purchase cart      | Transient cart state        | Supporting | Basket context   | Basket.API       |
| Payment    | Settle payment         | Simulated processor         | Supporting | Payment context  | PaymentProcessor |
| Identity   | Access control         | Users, tokens, consent      | Generic    | Cross-cutting    | Identity.API     |
| Webhooks   | Event notifications    | External integrations       | Supporting | Webhooks context | Webhooks.API     |

**Actors:**

| Actor                       | Purpose               | Description                             | Type                  | Interactions         | Relationships |
| --------------------------- | --------------------- | --------------------------------------- | --------------------- | -------------------- | ------------- |
| Customer / Shopper          | Browse and buy        | End user via web or mobile              | Human (primary)       | WebApp, mobile-bff   | Uses store    |
| Administrator               | Manage catalog        | Maintains items/brands/types            | Human                 | Catalog.API / WebApp | Manages       |
| External Webhook Subscriber | Receive notifications | Third-party endpoint consuming webhooks | External system/human | Webhooks.API         | Subscribes    |

**Applications:**

| Application           | Purpose                 | Description                      | Type               | Interactions                       | Relationships        |
| --------------------- | ----------------------- | -------------------------------- | ------------------ | ---------------------------------- | -------------------- |
| WebApp                | Online store UI         | Blazor Server storefront         | Web app            | Basket/Catalog/Ordering, Identity  | Uses APIs            |
| HybridApp / ClientApp | Mobile store UI         | .NET MAUI (Blazor Hybrid) client | Mobile app         | mobile-bff, Identity               | Uses BFF             |
| WebhookClient         | Webhook demo            | Sample subscriber UI             | Web app            | Webhooks.API, Identity             | Uses API             |
| mobile-bff            | Aggregation gateway     | YARP reverse proxy for mobile    | BFF/Gateway        | Catalog/Ordering/Identity          | Routes               |
| Identity.API          | Identity provider       | Duende IdentityServer + UI       | Service API        | All apps/services                  | Authenticates        |
| Catalog.API           | Catalog service         | Products + AI embeddings         | Service API        | RabbitMQ, catalogdb, OpenAI/Ollama | Publishes/subscribes |
| Basket.API            | Basket service          | gRPC + Redis cache               | Service API        | Redis, RabbitMQ                    | Publishes events     |
| Ordering.API          | Ordering service        | CQRS/DDD order API               | Service API        | RabbitMQ, orderingdb               | Publishes events     |
| OrderProcessor        | Order background worker | Grace-period processing          | Background service | RabbitMQ, orderingdb               | Consumes/processes   |
| PaymentProcessor      | Payment worker          | Simulates payment result         | Background service | RabbitMQ                           | Publishes events     |
| Webhooks.API          | Webhooks service        | Manage & dispatch webhooks       | Service API        | RabbitMQ, webhooksdb               | Publishes/dispatch   |

**External Systems:**

| External System            | Purpose              | Description                                       | Type          | Interactions        | Relationships   |
| -------------------------- | -------------------- | ------------------------------------------------- | ------------- | ------------------- | --------------- |
| OpenAI / Azure OpenAI      | Text embeddings      | Optional embedding generation for semantic search | SaaS/AI       | Catalog.API → HTTP  | Optional        |
| Ollama                     | Local LLM/embeddings | Optional local embedding model                    | AI runtime    | Catalog.API → HTTP  | Optional        |
| External webhook endpoints | Receive callbacks    | Third-party subscriber URLs                       | External HTTP | Webhooks.API → HTTP | Dispatch target |

**Cross-Cutting Concerns:**

| Cross-Cutting Concern          | Purpose            | Description                                  | Type        | Interactions                 | Relationships             |
| ------------------------------ | ------------------ | -------------------------------------------- | ----------- | ---------------------------- | ------------------------- |
| Authentication/Authorization   | Secure access      | OIDC/JWT via Identity + ServiceDefaults      | Security    | All services                 | Composition               |
| Observability                  | Telemetry          | OpenTelemetry traces/metrics/logs            | Ops         | All services                 | ServiceDefaults           |
| Health checks                  | Liveness/readiness | `/health` probes                             | Ops         | All services                 | ServiceDefaults           |
| Service discovery + resilience | Robust calls       | Aspire discovery, HttpClient resilience      | Infra       | All services                 | ServiceDefaults           |
| Messaging / Event bus          | Async integration  | RabbitMQ pub/sub integration events          | Integration | Producing/consuming services | EventBus/EventBusRabbitMQ |
| Transactional outbox           | Reliable eventing  | IntegrationEventLogEF ensures atomic publish | Integration | Catalog/Ordering             | IntegrationEventLogEF     |
| API versioning + OpenAPI       | Contracts          | Versioned APIs and OpenAPI docs              | API         | Service APIs                 | ServiceDefaults           |

---

## Applications (detailed)

### Application — Catalog.API

**Name:** Catalog.API

**Purpose:** Serve product catalog data and AI-assisted semantic search.

**Description:** Minimal-API service exposing catalog items, brands and types; generates vector embeddings (pgvector) via OpenAI/Ollama for semantic search; publishes/consumes integration events for price and order status changes.

**Type:** Service API (microservice)

**Architecture style:** Layered microservice; Event-driven; optional AI integration.

**Layers:**

| Layer             | Description                       | Type           |
| ----------------- | --------------------------------- | -------------- |
| Apis              | Minimal API endpoints (v1/v2)     | Presentation   |
| Services          | CatalogAI embedding service       | Application    |
| Model             | Catalog entities                  | Domain/Model   |
| Infrastructure    | EF Core `CatalogContext`, seeding | Infrastructure |
| IntegrationEvents | Event publishing/handlers         | Integration    |

**Interactions:**

| Interaction                 | Description                   | Type |
| --------------------------- | ----------------------------- | ---- |
| Client → Catalog.API        | Query items                   | HTTP |
| Catalog.API → catalogdb     | Persist/query + vector search | SQL  |
| Catalog.API → OpenAI/Ollama | Embeddings                    | HTTP |
| Catalog.API ↔ RabbitMQ      | Integration events            | AMQP |

**Relationships:**

| Relationship                                      | Description      | Type  |
| ------------------------------------------------- | ---------------- | ----- |
| Subscribes OrderStatusChangedToAwaitingValidation | Stock validation | Async |
| Subscribes OrderStatusChangedToPaid               | Confirm stock    | Async |
| Publishes price changed                           | Notify basket    | Async |

**Bounded Contexts:** Catalog. **Domains:** Catalog. **Sub-Domains:** Catalog search.

**Actors:**

| Actor         | Purpose      | Description       | Type   | Interactions | Relationships |
| ------------- | ------------ | ----------------- | ------ | ------------ | ------------- |
| Administrator | Manage items | CRUD catalog      | Human  | HTTP         | Manages       |
| WebApp        | Read catalog | Storefront browse | System | HTTP         | Uses          |

**External Systems:**

| External System | Purpose    | Description      | Type | Interactions | Relationships |
| --------------- | ---------- | ---------------- | ---- | ------------ | ------------- |
| OpenAI/Ollama   | Embeddings | Semantic vectors | AI   | HTTP         | Optional      |

**Cross-Cutting Concerns:** Auth, observability, health, outbox, event bus (as system-level table).

**Components:**

| Component                      | Purpose         | Description                 | Type                 | Interactions | Relationships      |
| ------------------------------ | --------------- | --------------------------- | -------------------- | ------------ | ------------------ |
| Catalog endpoints              | Expose API      | Minimal APIs                | Controller/Endpoint  | HTTP         | Calls services     |
| CatalogAI                      | Embeddings      | IEmbeddingGenerator wrapper | Service              | HTTP         | Uses OpenAI/Ollama |
| CatalogContext                 | Persistence     | EF Core + pgvector          | Repository/DbContext | SQL          | catalogdb          |
| CatalogIntegrationEventService | Reliable events | Outbox publish              | Service              | AMQP         | EventBus           |

### Application — Ordering.API (+ Ordering.Domain / Ordering.Infrastructure)

**Name:** Ordering (Ordering.API, Ordering.Domain, Ordering.Infrastructure)

**Purpose:** Manage the full order lifecycle from creation to fulfilment.

**Description:** Clean Architecture service with a rich DDD domain model (Order and Buyer aggregates, domain events), CQRS application layer using MediatR (commands, queries, validation and logging behaviors), and EF Core infrastructure. Publishes integration events driving downstream Catalog/Payment choreography.

**Type:** Service API (microservice) with domain + application + infrastructure layers.

**Architecture style:** Clean Architecture; DDD; CQRS; Event-driven.

**Layers:**

| Layer          | Description                                                                       | Type           |
| -------------- | --------------------------------------------------------------------------------- | -------------- |
| Apis           | Versioned Orders API v1                                                           | Presentation   |
| Application    | Commands, Queries, Behaviors, Validations, DomainEventHandlers, IntegrationEvents | Application    |
| Domain         | OrderAggregate, BuyerAggregate, Events, SeedWork                                  | Domain         |
| Infrastructure | EF Core persistence, repositories                                                 | Infrastructure |

**Interactions:**

| Interaction               | Description                     | Type |
| ------------------------- | ------------------------------- | ---- |
| Client → Ordering.API     | Place/track orders (authorized) | HTTP |
| Ordering.API → orderingdb | Persist orders                  | SQL  |
| Ordering.API ↔ RabbitMQ   | Integration events              | AMQP |

**Relationships:**

| Relationship                   | Description                 | Type  |
| ------------------------------ | --------------------------- | ----- |
| Publishes OrderStatusChanged\* | Drives choreography         | Async |
| Consumes payment/stock events  | Advances order state        | Async |
| OrderProcessor → orderingdb    | Background grace processing | Data  |

**Bounded Contexts:** Ordering. **Domains:** Ordering. **Sub-Domains:** Ordering, Payment (via events).

**Actors:**

| Actor    | Purpose     | Description | Type  | Interactions | Relationships |
| -------- | ----------- | ----------- | ----- | ------------ | ------------- |
| Customer | Place order | Checkout    | Human | HTTP         | Uses          |

**External Systems:** none direct (payment simulated internally via PaymentProcessor).

**Cross-Cutting Concerns:** Auth (RequireAuthorization), observability, health, outbox, MediatR behaviors (logging/validation/transaction).

**Components:**

| Component              | Purpose     | Description                    | Type                 | Interactions  | Relationships     |
| ---------------------- | ----------- | ------------------------------ | -------------------- | ------------- | ----------------- |
| Orders API v1          | Endpoints   | Versioned minimal API          | Endpoint             | HTTP          | Calls Application |
| Command/Query handlers | Use cases   | MediatR handlers               | Handler              | in-process    | Uses Domain       |
| Behaviors              | Pipeline    | Logging/Validation/Transaction | Cross-cutting        | in-process    | Wraps handlers    |
| OrderAggregate         | Domain      | Order root + invariants        | Aggregate            | domain events | Domain            |
| BuyerAggregate         | Domain      | Buyer + payment methods        | Aggregate            | domain events | Domain            |
| OrderingContext        | Persistence | EF Core                        | Repository/DbContext | SQL           | orderingdb        |

### Application — Basket.API

**Name:** Basket.API

**Purpose:** Maintain the customer's shopping basket.

**Description:** gRPC service storing transient basket state in Redis; reacts to catalog price changes and order events via RabbitMQ.

**Type:** Service API (microservice, gRPC).

**Architecture style:** Layered microservice; Event-driven; gRPC.

**Layers:**

| Layer             | Description              | Type           |
| ----------------- | ------------------------ | -------------- |
| Grpc / Proto      | gRPC service + contracts | Presentation   |
| Model             | Basket models            | Domain/Model   |
| Repositories      | Redis basket repository  | Infrastructure |
| IntegrationEvents | Event handlers           | Integration    |

**Interactions:**

| Interaction           | Description   | Type |
| --------------------- | ------------- | ---- |
| WebApp → Basket.API   | Manage basket | gRPC |
| Basket.API → Redis    | Store basket  | TCP  |
| Basket.API ↔ RabbitMQ | Events        | AMQP |

**Relationships:**

| Relationship              | Description          | Type  |
| ------------------------- | -------------------- | ----- |
| Consumes price changed    | Update basket prices | Async |
| Publishes basket checkout | Start ordering       | Async |

**Bounded Contexts:** Basket. **Domains:** Basket. **Sub-Domains:** Basket.

**Actors:**

| Actor    | Purpose          | Description | Type  | Interactions      | Relationships |
| -------- | ---------------- | ----------- | ----- | ----------------- | ------------- |
| Customer | Add/remove items | Via WebApp  | Human | gRPC (via WebApp) | Uses          |

**External Systems:** none.

**Cross-Cutting Concerns:** Auth, observability, health, event bus.

**Components:**

| Component             | Purpose           | Description        | Type       | Interactions | Relationships |
| --------------------- | ----------------- | ------------------ | ---------- | ------------ | ------------- |
| BasketService (gRPC)  | Expose basket ops | gRPC endpoints     | Service    | gRPC         | Uses repo     |
| RedisBasketRepository | Persistence       | Redis-backed       | Repository | TCP          | Redis         |
| Event handlers        | React to events   | Price/order events | Handler    | AMQP         | EventBus      |

### Application — Identity.API

**Name:** Identity.API

**Purpose:** Provide authentication and authorization for all apps/services.

**Description:** Duende IdentityServer-based OIDC/OAuth2 provider with login UI (Quickstart), user store, and client/callback configuration for every eShop app.

**Type:** Service API + UI (identity provider).

**Architecture style:** Identity provider; MVC + IdentityServer.

**Layers:**

| Layer              | Description                 | Type           |
| ------------------ | --------------------------- | -------------- |
| Quickstart / Views | Login/consent UI            | Presentation   |
| Services           | Identity services           | Application    |
| Data / Models      | User store (EF, identitydb) | Infrastructure |
| Configuration      | Clients, resources, scopes  | Configuration  |

**Interactions:**

| Interaction              | Description                  | Type       |
| ------------------------ | ---------------------------- | ---------- |
| Apps/services → Identity | OIDC flows, token validation | HTTPS/OIDC |
| Identity → identitydb    | User persistence             | SQL        |

**Relationships:**

| Relationship                      | Description                           | Type  |
| --------------------------------- | ------------------------------------- | ----- |
| Configures callbacks for all apps | Client registration (cyclic env refs) | Trust |

**Bounded Contexts:** Identity. **Domains:** Identity. **Sub-Domains:** Identity/Access.

**Actors:**

| Actor    | Purpose | Description  | Type  | Interactions | Relationships |
| -------- | ------- | ------------ | ----- | ------------ | ------------- |
| Customer | Sign in | Authenticate | Human | HTTPS        | Uses          |

**External Systems:** none.

**Cross-Cutting Concerns:** Security (core), observability, health.

**Components:**

| Component          | Purpose       | Description           | Type       | Interactions | Relationships |
| ------------------ | ------------- | --------------------- | ---------- | ------------ | ------------- |
| IdentityServer     | Issue tokens  | OIDC engine           | Service    | HTTPS        | Clients       |
| Account/Consent UI | Login         | MVC controllers/views | Controller | HTTP         | Users         |
| User store         | Persist users | EF Core               | Repository | SQL          | identitydb    |

### Application — Webhooks.API + WebhookClient

**Name:** Webhooks

**Purpose:** Let external subscribers register for and receive event notifications.

**Description:** Webhooks.API stores subscriptions (webhooksdb) and dispatches HTTP callbacks on domain events; WebhookClient is a sample subscriber application.

**Type:** Service API + demo client.

**Architecture style:** Event-driven microservice; webhook dispatch.

**Layers:**

| Layer             | Description               | Type           |
| ----------------- | ------------------------- | -------------- |
| API               | Subscription endpoints    | Presentation   |
| Infrastructure    | webhooksdb persistence    | Infrastructure |
| IntegrationEvents | Consume events → dispatch | Integration    |

**Interactions:**

| Interaction                   | Description           | Type |
| ----------------------------- | --------------------- | ---- |
| Subscriber → Webhooks.API     | Register subscription | HTTP |
| Webhooks.API → subscriber URL | Dispatch callback     | HTTP |
| Webhooks.API ↔ RabbitMQ       | Consume events        | AMQP |

**Relationships:**

| Relationship                     | Description        | Type  |
| -------------------------------- | ------------------ | ----- |
| Consumes order events            | Trigger webhooks   | Async |
| Dispatches to external endpoints | Notify subscribers | HTTP  |

**Bounded Contexts:** Webhooks. **Domains:** Webhooks. **Sub-Domains:** Notifications.

**Actors:**

| Actor                       | Purpose        | Description | Type     | Interactions | Relationships |
| --------------------------- | -------------- | ----------- | -------- | ------------ | ------------- |
| External Webhook Subscriber | Receive events | Third-party | External | HTTP         | Subscribes    |

**External Systems:**

| External System      | Purpose           | Description      | Type          | Interactions | Relationships |
| -------------------- | ----------------- | ---------------- | ------------- | ------------ | ------------- |
| Subscriber endpoints | Receive callbacks | Third-party URLs | External HTTP | HTTP         | Dispatch      |

**Cross-Cutting Concerns:** Auth, observability, health, event bus.

**Components:**

| Component          | Purpose              | Description           | Type     | Interactions | Relationships |
| ------------------ | -------------------- | --------------------- | -------- | ------------ | ------------- |
| Subscription API   | Manage subscriptions | Endpoints             | Endpoint | HTTP         | DB            |
| Webhook dispatcher | Send callbacks       | HTTP sender           | Service  | HTTP         | Subscribers   |
| Event handlers     | Consume events       | Map events → webhooks | Handler  | AMQP         | EventBus      |

### Application — WebApp (+ WebAppComponents), HybridApp/ClientApp, mobile-bff

**Name:** Client Applications & BFF

**Purpose:** Deliver the storefront experience to web and mobile customers.

**Description:** WebApp is a Blazor Server storefront consuming Basket/Catalog/Ordering; WebAppComponents holds shared Razor components; HybridApp/ClientApp are .NET MAUI (Blazor Hybrid) mobile clients routed through the mobile-bff (YARP) reverse proxy.

**Type:** Web app + mobile apps + BFF gateway.

**Architecture style:** Component-based UI (Blazor/MAUI); Backend-for-Frontend (YARP).

**Layers:**

| Layer      | Description             | Type         |
| ---------- | ----------------------- | ------------ |
| Components | Razor UI components     | Presentation |
| Services   | Typed HTTP/gRPC clients | Application  |
| Gateway    | mobile-bff routing      | Gateway      |

**Interactions:**

| Interaction                                     | Description       | Type      |
| ----------------------------------------------- | ----------------- | --------- |
| Browser → WebApp                                | Storefront        | HTTP      |
| Mobile → mobile-bff → APIs                      | Aggregated access | HTTP      |
| WebApp → Basket (gRPC), Catalog/Ordering (HTTP) | Data access       | gRPC/HTTP |

**Relationships:**

| Relationship                           | Description  | Type       |
| -------------------------------------- | ------------ | ---------- |
| WebApp → APIs                          | Uses         | Dependency |
| mobile-bff → Catalog/Ordering/Identity | Routes       | Dependency |
| Apps → Identity                        | Authenticate | Trust      |

**Bounded Contexts:** cross-context UI aggregation. **Domains:** E-commerce presentation.

**Actors:**

| Actor    | Purpose | Description     | Type  | Interactions | Relationships |
| -------- | ------- | --------------- | ----- | ------------ | ------------- |
| Customer | Shop    | Web/mobile user | Human | HTTP         | Uses          |

**External Systems:** none direct.

**Cross-Cutting Concerns:** Auth, observability, resilience, service discovery.

**Components:**

| Component               | Purpose     | Description                     | Type      | Interactions | Relationships |
| ----------------------- | ----------- | ------------------------------- | --------- | ------------ | ------------- |
| Blazor pages/components | UI          | Storefront pages                | Component | HTTP         | Services      |
| Typed API clients       | Data access | Basket/Catalog/Ordering clients | Client    | gRPC/HTTP    | APIs          |
| mobile-bff (YARP)       | Routing     | Reverse proxy                   | Gateway   | HTTP         | APIs          |

### Background Processors — OrderProcessor & PaymentProcessor

**Name:** OrderProcessor, PaymentProcessor

**Purpose:** Asynchronously advance order and payment workflows.

**Description:** OrderProcessor performs grace-period order processing against orderingdb; PaymentProcessor simulates payment outcomes and publishes result events. Both are event-bus connected background services.

**Type:** Background workers (hosted services).

**Architecture style:** Event-driven worker.

**Layers:**

| Layer          | Description                | Type           |
| -------------- | -------------------------- | -------------- |
| Worker         | Background processing loop | Application    |
| Infrastructure | DB / event bus access      | Infrastructure |

**Interactions:**

| Interaction                 | Description                    | Type |
| --------------------------- | ------------------------------ | ---- |
| OrderProcessor → orderingdb | Process pending orders         | SQL  |
| PaymentProcessor ↔ RabbitMQ | Consume/publish payment events | AMQP |

**Relationships:**

| Relationship                                     | Description           | Type       |
| ------------------------------------------------ | --------------------- | ---------- |
| PaymentProcessor → Ordering                      | Payment result events | Async      |
| OrderProcessor depends on Ordering EF migrations | Startup ordering      | Dependency |

**Bounded Contexts:** Ordering, Payment. **Domains:** Ordering/Payment.

**Actors:** none (system-triggered).

**External Systems:** none.

**Cross-Cutting Concerns:** Observability, event bus, resilience.

**Components:**

| Component           | Purpose          | Description      | Type    | Interactions | Relationships       |
| ------------------- | ---------------- | ---------------- | ------- | ------------ | ------------------- |
| Grace-period worker | Process orders   | Hosted service   | Worker  | SQL/AMQP     | orderingdb/EventBus |
| Payment handler     | Simulate payment | Publishes result | Handler | AMQP         | EventBus            |

---

## Deployment (grounded in AppHost + build/)

| Deployment Node/Unit                      | Description                                                                                                  | Type              |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------------------ | ----------------- |
| Aspire AppHost                            | Orchestrates all resources locally/dev                                                                       | Orchestrator      |
| redis container                           | Basket cache                                                                                                 | Container (infra) |
| rabbitmq container (persistent)           | Event bus                                                                                                    | Container (infra) |
| postgres container (pgvector, persistent) | catalogdb/identitydb/orderingdb/webhooksdb                                                                   | Container (infra) |
| Service containers                        | identity/basket/catalog/ordering/webhooks APIs, order/payment processors, mobile-bff, webapp, webhooksclient | Container (app)   |
| ACR build + multiarch manifests           | `build/acr-build`, `build/multiarch-manifests`                                                               | CI/registry       |
| CI pipeline                               | `ci.yml`                                                                                                     | Pipeline          |
