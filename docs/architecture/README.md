# eShop Architecture Documentation

This folder contains the TOGAF 10-aligned architecture documentation for the **eShop** reference application — a cloud-native, microservices-based digital commerce platform built on .NET 10 and .NET Aspire 13.x, deployed to Azure Container Apps.

## Contents

| Document                                                | Description                                                                                                                                                    |
| ------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Business Architecture](business-architecture.md)       | Six commerce capabilities, value streams, business processes, and domain events. **67 components** · Maturity Level 2.                                         |
| [Application Architecture](application-architecture.md) | Microservice inventory, service contracts (gRPC/OpenAPI), integration patterns, and AI semantic search. **48 components** · Maturity Level 4 — Measured.       |
| [Data Architecture](data-architecture.md)               | Polyglot persistence (PostgreSQL, Redis, RabbitMQ), DDD aggregate models, outbox pattern, and data governance. **74 components** · Maturity Level 3 — Defined. |
| [Technology Architecture](technology-architecture.md)   | Azure Container Apps deployment, .NET Aspire orchestration, Bicep IaC, security posture, and observability. **46 components** · Average confidence 0.97.       |

## Architecture at a Glance

```mermaid
---
title: eShop TOGAF Architecture Layers
config:
  theme: base
  look: classic
  layout: dagre
  themeVariables:
    fontSize: '16px'
  flowchart:
    htmlLabels: true
---
flowchart TB
    accTitle: eShop TOGAF Architecture Layers
    accDescr: WCAG AA compliant layered architecture diagram showing the four TOGAF layers of the eShop platform stacked top-to-bottom — Business, Application, Data, and Technology — with directional arrows indicating dependency flow.

    %% ═══════════════════════════════════════════════════════════════════════════
    %% AZURE / FLUENT ARCHITECTURE PATTERN v2.0
    %% (Semantic + Structural + Font + Accessibility Governance)
    %% ═══════════════════════════════════════════════════════════════════════════
    %% PHASE 1 - FLUENT UI: All styling uses approved Fluent UI palette only
    %% PHASE 2 - GROUPS: Every subgraph has semantic color via style directive
    %% PHASE 3 - COMPONENTS: Every node has semantic classDef + icon prefix
    %% PHASE 4 - ACCESSIBILITY: accTitle/accDescr present, WCAG AA contrast
    %% PHASE 5 - STANDARD: Governance block present, classDefs centralized
    %% ═══════════════════════════════════════════════════════════════════════════

    subgraph BIZ["🏢 Business Architecture"]
        direction TB
        B1("🎯 Capabilities"):::warning
        B2("🔄 Value Streams"):::warning
        B3("⚙️ Business Processes"):::warning
    end

    subgraph APP["🖥️ Application Architecture"]
        direction TB
        A1("🌐 Microservices"):::core
        A2("📡 APIs & Contracts"):::core
        A3("📣 Integration Events"):::core
    end

    subgraph DAT["🗄️ Data Architecture"]
        direction TB
        D1("🐘 PostgreSQL"):::neutral
        D2("⚡ Redis"):::success
        D3("📨 RabbitMQ · Outbox"):::warning
    end

    subgraph TECH["☁️ Technology Architecture"]
        direction TB
        T1("🚀 Azure Container Apps"):::neutral
        T2("🔧 .NET Aspire · Bicep"):::neutral
        T3("🔒 Security · Observability"):::neutral
    end

    BIZ --> APP
    APP --> DAT
    DAT --> TECH

    %% Centralized classDefs (AZURE/FLUENT v1.1)
    classDef neutral  fill:#FAFAFA,stroke:#8A8886,stroke-width:2px,color:#323130
    classDef core     fill:#EFF6FC,stroke:#0078D4,stroke-width:2px,color:#323130
    classDef success  fill:#DFF6DD,stroke:#107C10,stroke-width:2px,color:#323130
    classDef warning  fill:#FFF4CE,stroke:#FFB900,stroke-width:2px,color:#323130
    classDef danger   fill:#FDE7E9,stroke:#D13438,stroke-width:2px,color:#323130

    %% Subgraph style directives (style only — never class on subgraph)
    style BIZ  fill:#F3F2F1,stroke:#8A8886,stroke-width:2px,color:#323130
    style APP  fill:#F3F2F1,stroke:#8A8886,stroke-width:2px,color:#323130
    style DAT  fill:#F3F2F1,stroke:#8A8886,stroke-width:2px,color:#323130
    style TECH fill:#F3F2F1,stroke:#8A8886,stroke-width:2px,color:#323130
```

## Key Design Decisions

- **Database-per-Service** — `catalogdb`, `identitydb`, `orderingdb`, and `webhooksdb` are provisioned as separate PostgreSQL instances; cross-domain access is performed exclusively through integration events or synchronous API calls.
- **Event-Driven Integration** — All cross-service communication uses a RabbitMQ-backed event bus with a transactional outbox (`IntegrationEventLogEF`) for **at-least-once delivery guarantees**.
- **AI-Powered Search** — The Catalog service stores 384-dimensional `pgvector` embeddings on `CatalogItem`, enabling semantic product search via Azure OpenAI or Ollama.
- **Infrastructure as Code** — Two-layer IaC: Bicep (`infra/`) for Azure PaaS scaffolding and ACA YAML templates (`src/eShop.AppHost/infra/`) per container, deployed with `azd`.
- **Zero-Trust Security** — External HTTPS ingress enforces `allowInsecure: false`; a **User-Assigned Managed Identity** handles ACR image pull; secrets are stored in the ACA secret store.

## Framework & Standards

| Concern                | Standard / Tool                            |
| ---------------------- | ------------------------------------------ |
| Architecture framework | TOGAF 10                                   |
| Orchestration          | .NET Aspire 13.1.0                         |
| Deployment target      | Azure Container Apps (Consumption profile) |
| Observability          | OpenTelemetry → OTLP → Aspire Dashboard    |
| IaC language           | Bicep + `azd`                              |
| Service contracts      | gRPC (protobuf) · OpenAPI/Scalar (REST)    |

## Related Documentation

- [Business Architecture](../../README.md) — Repository root with setup and quickstart instructions.
- [`infra/`](../../infra/) — Bicep templates for Azure infrastructure provisioning.
- [`src/eShop.AppHost/`](../../src/eShop.AppHost/) — .NET Aspire host that wires all services together for local development and ACA deployment.
