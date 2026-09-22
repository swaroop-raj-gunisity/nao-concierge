# NAO Concierge — Conversational Intake: 2-Week Detailed Implementation Plan

## 1. Executive Summary & Architecture Blueprint

This plan operationalizes the **Conversational Intake** feature of the **New Account Onboarding (NAO) Concierge**, aligning with:
- **Technical Stack**: **Plan 1 (.NET 8 + React)** from [`NAO-Concierge-POC-Plan 1.html`](file:///c:/Users/swaroop.raj/Documents/workspace/nao-concierge/NAO-Concierge-POC-Plan%201.html).
- **Lifecycle Phases**: **P1 to P7** and the FDE task breakdown from [`Implementation Lifecycle.pdf`](file:///c:/Users/swaroop.raj/Documents/workspace/nao-concierge/Implementation%20Lifecycle.pdf).
- **Team Allocation**: Capability mapping from [`team-skills-matrix.html`](file:///c:/Users/swaroop.raj/Documents/workspace/nao-concierge/team-skills-matrix.html) for Saravanan (SA), Vinisha (VI), Jegan (JE), and Smita (SM).
- **Delivery Model**: **Claude Code-based Forward Deployment Engineer (FDE)** model with $\ge 50\%$ Claude agent involvement for core engineering tasks.
- **Sprint Window**: **2 Weeks (10 Business Days)** to deliver a functional, cited agent prototype with complete backend specifications.

```
+-----------------------------------------------------------------------------------------------+
|                                    REACT 18 + VITE FRONTEND                                   |
|   Split-Screen Layout: Left = Live Form Ledger (32 Attr) | Right = Streaming Chat + Ledger    |
+-----------------------------------------------+-----------------------------------------------+
                                                | SSE Stream / REST APIs
                                                v
+-----------------------------------------------------------------------------------------------+
|                                 ASP.NET CORE 8 WEB API BACKEND                                |
|                                                                                               |
|  +---------------------------+    +--------------------------+    +------------------------+  |
|  |    CASE & SESSION STORE   |    |    REDACTION GATE (C#)   |    |   AUDIT TRAIL API      |  |
|  |  Cosmos DB / SQLite Fallback | |  Deterministic 4-Tier Map   | |  Schema Validation     |  |
|  +---------------------------+    +-------------+------------+    |  Event Ingestion Logger|  |
|                                                 | (Safe Context)  +------------------------+  |
|                                                 v                                             |
|  +-----------------------------------------------------------------------------------------+  |
|  |                        AGENT ORCHESTRATION LAYER (Semantic Kernel)                       |  |
|  |   - System Prompt Assembly (Conversation History + Redacted Session State + RAG Chunks) |  |
|  |   - Confidence Floor Evaluator (Threshold >= 0.70 Propose vs. < 0.70 Clarify)          |  |
|  |   - MCP Middleware Tools (ProposeField, ConfirmField, CorrectField, CheckConflict)      |  |
|  +----------------------------------------------+------------------------------------------+  |
|                                                 | Portkey Gateway (Employer-Hosted)           |
|                                                 v                                             |
|                                   +----------------------------+                              |
|                                   |  Claude 3.5 Sonnet Engine  |                              |
|                                   +----------------------------+                              |
|                                                                                               |
|  +-----------------------------------------------------------------------------------------+  |
|  |                                  CRM RAG RETRIEVAL PIPELINE                             |  |
|  |   Azure AI Search (Hybrid) / Qdrant Fallback  <--  text-embedding-3-small (Embeddings)   |  |
|  |   Indexed Data: crm-leads.json + crm-notes.json (Agnostic Chunking + Primary Key Citations)|
|  +-----------------------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------------------+
```

---

## 2. Strategic Technical Decisions & Best Practices

### 2.1 Target Environment Strategy: Cloud-First vs. Local-First
*User Guidance Addressed: Starting cloud-first with personal trial Azure services vs. local-first before employer sandbox.*

#### Recommendation: Hybrid "Local-First Development, Cloud-First Verification"
Attempting to develop purely on personal Azure trial accounts introduces severe friction:
1. **Azure Free/Trial Quota Bottlenecks**: Azure AI Search free tier allows only 1 service with a 50MB storage ceiling; Cosmos DB free tier has strict 1000 RU/s throughput limits and can throttle concurrent load tests; personal subscriptions can incur surprise charges or quota lockouts.
2. **Network Latency & Development Velocity**: Iterating through Claude Code on local machines with a local SQLite database and in-memory/Qdrant vector search is **5x to 10x faster** than round-tripping to cloud endpoints on every automated test run.
3. **Seamless Employer Sandbox Transition**: By adopting the **Repository / Adapter Pattern** in .NET, the application connects to local services during development and seamlessly toggles to Azure services via environment variables.

| Layer | Local Development Default (Days 1–7) | Azure Trial / Employer Sandbox Target (Days 8–10) | Switch Mechanism |
|---|---|---|---|
| **App DB** | SQLite via `Microsoft.Data.Sqlite` (`nao_local.db`) | Azure Cosmos DB Serverless | `IDatabaseProvider` injection via `appsettings.json` |
| **Vector DB** | Qdrant local container / In-Memory Vector Store | Azure AI Search (vector + semantic ranker) | `IVectorSearchService` adapter |
| **Embeddings** | Local ONNX / Azure OpenAI embedding key | Azure OpenAI `text-embedding-3-small` | Configuration toggle in Portkey |
| **LLM Gateway** | Employer-Hosted Portkey (Local Env Config) | Employer-Hosted Portkey (Staging Key Vault) | Portkey Virtual Key / Base URL config |
| **Secrets** | `dotnet user-secrets` + `.env.local` | Azure Key Vault + Managed Identity | ASP.NET Core `AddAzureKeyVault` |
| **Hosting** | Local Kestrel (`http://localhost:5000`) + Vite (`:5173`) | Azure Container Apps + Azure Static Web Apps | Dockerfile & GitHub Actions workflow |

---

### 2.2 Agent Orchestration: Semantic Kernel vs. LangGraph
*User Guidance Addressed: Semantic Kernel (.NET) vs. LangGraph with custom Portkey.*

#### Comparative Evaluation

| Factor | Option A: Microsoft Semantic Kernel (.NET 8) | Option B: LangGraph (Python FastAPI Sidecar) |
|---|---|---|
| **Team Skill Match** | **100% Fit**: Saravanan, Jegan, and Smita are 10–14 yr .NET veterans. No skill gap. | **Poor Fit**: Team skills matrix explicitly notes zero Python ML depth across team. |
| **Architecture** | Single-process ASP.NET Core 8 service. Native C# async pipeline. Direct memory passing. | Two-service architecture (.NET Web API + Python FastAPI). Inter-process HTTP/gRPC overhead. |
| **State Management** | Directly binds to C# Case State domain models and Cosmos DB SDK. | State must be serialized/deserialized across process boundary on every turn. |
| **Portkey Compatibility** | Uses Portkey's OpenAI-compatible or Anthropic endpoint via custom `HttpClient` / `IChatCompletionService`. | Uses Portkey Python SDK or LangChain ChatOpenAI/ChatAnthropic wrapper. |
| **Deterministic Redaction** | Redaction gate runs in C# memory before payload reaches Semantic Kernel. Zero leak risk. | Must run redaction in C# before HTTP call to Python, or replicate logic in Python. |

**Final Decision**: **Option A (Semantic Kernel in .NET 8)** is the primary architectural choice for Plan 1. It maximizes team velocity, leverages existing .NET Core expertise, eliminates multi-runtime deployment headaches, and provides strict type safety for the 32-attribute state machine.

---

### 2.3 Attribute Scope for Extraction (Conversational Intake)
*User Guidance Addressed: Confirmed 14 inferable attributes and 2 prototype registrations.*

The Conversational Intake prototype exercises **14 inferable attributes** across **3 primary functional groups** plus **Funding**, validated against two account titling registrations:
1. **INDIVIDUAL** (Baseline path — Margaret Chen case)
2. **JOINT_WROS** (Joint with Right of Survivorship — Rajiv Mehta case)

#### In-Scope Attribute Catalog

| Attribute Key | Group | Type | Materiality | PII Tier | Target Enum / Format |
|---|---|---|---|---|---|
| `account.type` | Account Setup | enum | Material | `none` | `INDIVIDUAL`, `JOINT_WROS` |
| `account.advisory_program` | Account Setup | enum | Material | `none` | `ADVISOR_DRIVEN`, `UMA`, `THIRD_PARTY_MF`, `DIGITAL_ADVICE` |
| `account.purpose` | Account Setup | enum | Non-material | `none` | `RETIREMENT`, `EDUCATION`, `WEALTH_ACCUMULATION`, `INCOME`, `ESTATE_PLANNING` |
| `applicant.citizenship_status` | Applicant Identity | enum | Material | `indirect` | `US_CITIZEN`, `US_RESIDENT_ALIEN`, `NON_RESIDENT_ALIEN` |
| `applicant.legal_name` | Applicant Identity | composite | Material | `direct` | Role Token: `OWNER_1` (First, Last) |
| `applicant.email` | Applicant Identity | email | Non-material | `direct` | Role Token: `EMAIL_1` |
| `applicant.mobile_phone` | Applicant Identity | phone | Non-material | `direct` | Role Token: `PHONE_1` |
| `applicant.tax_id` | Applicant Identity | masked | Material | `sensitive` | Vaulted Flag: `{"state":"vaulted","filled":true}` |
| `employment.status` | Employment & Finances | enum | Material | `indirect` | `EMPLOYED`, `SELF_EMPLOYED`, `RETIRED`, `STUDENT`, `NOT_EMPLOYED` |
| `employment.employer_name` | Employment & Finances | text | Material | `indirect` | Text string (only if employed/self-employed) |
| `financial.annual_income` | Employment & Finances | enum | Material | `indirect` | `UNDER_50K`, `50K_100K`, `100K_250K`, `250K_500K`, `OVER_500K` |
| `financial.liquid_net_worth` | Employment & Finances | enum | Material | `indirect` | `UNDER_50K`, `50K_250K`, `250K_1M`, `1M_5M`, `OVER_5M` |
| `funding.method` | Funding | enum | Material | `none` | `ACAT_TRANSFER`, `ACH`, `CHECK`, `WIRE`, `IRA_ROLLOVER` |
| `funding.initial_amount` | Funding | currency | Material | `indirect` | Numeric currency value (e.g., `$400,000`) |

---

## 3. Detailed Component Architecture

### 3.1 Deterministic Redaction Gate (Section 5 Spec)
The Redaction Gate is a deterministic, high-performance C# middleware filter that executes **before** prompt assembly. It strictly enforces that **zero PII values ever reach the LLM**:
- **Tier 1 (`none`)**: Full value passed into prompt context (`account.type`, `account.purpose`, `funding.method`).
- **Tier 2 (`indirect`)**: Categorical/banded value passed (`annual_income = "100K_250K"`, `employment.status = "EMPLOYED"`).
- **Tier 3 (`direct`)**: Replaced mechanically with session role tokens (`applicant.legal_name` $\rightarrow$ `"OWNER_1"`, `email` $\rightarrow$ `"EMAIL_1"`).
- **Tier 4 (`sensitive`)**: Replaced with vaulted presence flag only (`applicant.tax_id` $\rightarrow$ `{"state":"vaulted", "filled": true}`). Never values, never tokens.

### 3.2 Confidence Floor Evaluator (Section 1.3 Spec)
Every LLM extraction turn generates an internal confidence score ($0.00$ to $1.00$):
- **Confidence $\ge 0.70$ (Explicit / Clear)**: Agent formats a field proposal into the Proposal Approval card. State = `extracted_unconfirmed`.
- **Confidence $< 0.70$ (Suggestive / Inconclusive)**: Agent holds the hint in session context (`held for context`), leaves the form field unset, and generates a polite, one-line clarifying question (e.g., *"Noted. Quick one so I set this up right — is this account just for you, or joint with your spouse?"*).

### 3.3 Confirmation Ledger & State Transition Engine
Values on the case record advance through strict, audited states:
1. `prefilled_accepted`: Auto-accepted from CRM, non-material (e.g., email, mobile). Rendered with purple CRM badge.
2. `prefilled_confirmed`: Material CRM data (e.g., legal name, address) confirmed by advisor.
3. `extracted_unconfirmed`: Proposed by Claude agent from advisor notes. Amber badge. **Blocks submission**.
4. `extracted_confirmed`: Advisor clicked `[Confirm]` or `[Confirm all]`. Green badge.
5. `extracted_corrected`: Advisor clicked `[Edit]` and amended the value. Retains original `heard` snippet, original value, corrected value, and advisor ID.
6. `vaulted`: Secure field typed directly by advisor into form; agent only receives presence flag.

### 3.4 Downstream Conflict Cross-Checks (Section 1.4 Spec)
Real-time rule engine evaluates compatibility upon every value change:
- **Liquidity Check**: `funding.initial_amount` vs. `financial.liquid_net_worth` band. If funding exceeds declared net worth, surface instant advisory alert.
- **Suitability Check**: `profile.investment_objective` vs. `profile.risk_tolerance` vs. `profile.time_horizon`.

---

## 4. Resource Allocation & Skills Mapping

| Resource | Primary Responsibilities | Skills Leveraged | Claude Code / FDE Ratio |
|---|---|---|---|
| **Saravanan (SA)**<br/>*Lead Technical Consultant* | **FDE & AI Lead**: ASP.NET Core 8 Web API architecture, Semantic Kernel orchestration with Portkey gateway, prompt engineering for extraction, Deterministic Redaction Gate, Confidence Floor logic, MCP tool calling endpoints. | .NET Core, Web API, Azure, Claude Code, Cursor, SpecKit, AI QA | **40% Human / 60% Claude** |
| **Vinisha Ravishankar (VI)**<br/>*Lead Technical Consultant* | **Frontend Track Lead**: React 18 + Vite + TypeScript split-screen shell, SSE streaming client, Confirmation Ledger (`Confirm/Edit/Reject`), citation rendering, real-time form ledger synchronization, visual status badges. | React, TypeScript, Redux, Design Systems, Modern UI/UX | **40% Human / 60% Claude** |
| **Jegan Rajasekar (JE)**<br/>*Sr. Technical Consultant* | **Persistence & State Lead (Paired with SA)**: Case & Session State domain models, Cosmos DB container setup & queries, SQLite local fallback provider, Case CRUD APIs, state transition validations, submission blocking gates. | .NET Core, Cosmos DB, CQRS, Minimal APIs, SQL Server | **50% Human / 50% Claude**<br/>*(Onboarding via SA)* |
| **Smita Dutt (SM)**<br/>*Technical Lead* | **Data & RAG Pipeline Lead**: CRM seed data ingestion (`crm-leads.json`, `crm-notes.json`), agnostic chunking & metadata tagging, Azure AI Search / Qdrant vector index, Audit Trail logging APIs & schema, Golden Dataset evaluation. | .NET, ETL / Data Warehousing, Microservices, Cursor / Copilot | **40% Human / 60% Claude** |

---

## 5. Day-by-Day 2-Week Sprint Execution Plan

```
Sprint Timeline:
[Day 1-2: Setup & Scaffolding] -> [Day 3-5: Data Prep, Redaction & Core APIs] -> [Day 6-8: Agent Extraction, Citations & UI] -> [Day 9-10: E2E Testing & Demo]
```

### Week 1: Environment Setup, Data Ingestion, Redaction Gate & Core APIs

#### Day 1: Project Scaffolding & Repository Setup (Lifecycle P2)
- **SA**: Scaffold ASP.NET Core 8 Web API project (`NaoConcierge.Api`); configure Portkey SDK/HttpClient; setup `dotnet user-secrets` for local development.
- **VI**: Scaffold React 18 + Vite + TypeScript project (`nao-concierge-web`) with Vanilla CSS / Design Tokens matching POC Plan 1 styling.
- **JE**: Initialize `NaoConcierge.Infrastructure` with EF Core / SQLite provider and Cosmos DB SDK; create initial `CaseDocument` entity.
- **SM**: Ingest and validate [`attribute-catalog.json`](file:///c:/Users/swaroop.raj/Documents/workspace/nao-concierge/seed-data/attribute-catalog.json), [`crm-leads.json`](file:///c:/Users/swaroop.raj/Documents/workspace/nao-concierge/seed-data/crm/crm-leads.json), and [`crm-notes.json`](file:///c:/Users/swaroop.raj/Documents/workspace/nao-concierge/seed-data/crm/crm-notes.json).

#### Day 2: Architecture Decision Records (ADRs) & Data Contracts (Lifecycle P4)
- **SA**: Author ADR-001 (Semantic Kernel & Portkey LLM Routing) and ADR-002 (Deterministic Redaction Gate Specification); publish API contract map.
- **VI**: Implement base UI layout with responsive CSS grid: Left panel (Form Ledger), Right panel (Chat & Actions).
- **JE**: Implement C# domain entities for the 32 catalog attributes, supporting 7 state flags and citation metadata.
- **SM**: Design Audit Trail JSON schema (`AuditEvent`: event_id, case_id, timestamp, actor, event_type, confidence_score, citations, payload).

#### Day 3: CRM Ingestion & RAG Retrieval Pipeline (Lifecycle P5)
- **SM**: Build CRM Seed Data Ingestion service; implement agnostic chunking for meeting notes (chunk ID, note ID, lead ID, timestamp, excerpt).
- **SM & SA**: Connect Azure OpenAI `text-embedding-3-small` (or local fallback) and index chunks into Azure AI Search / Qdrant local vector store.
- **JE**: Implement Case CRUD endpoints: `POST /api/cases` (create case), `GET /api/cases/{id}` (fetch full case ledger with attribute states).
- **VI**: Implement Form Ledger accordion groups in React: Account Setup, Applicant Identity, Employment & Finances, Funding.

#### Day 4: Deterministic Redaction Gate & Local DB Integration (Lifecycle P4 / P6)
- **SA**: Implement `RedactionGateService` in .NET:
  - Unit tests verifying 100% PII stripping across all 4 tiers before LLM invocation.
  - Zero raw PII leaked into prompt context.
- **JE**: Build SQLite local database migration and seed data loader for `case-001-margaret-chen.json` and `case-002-rajiv-mehta.json`.
- **VI**: Implement field visual state indicators: purple dot (`prefilled_accepted`), amber dot (`extracted_unconfirmed`), green dot (`extracted_confirmed`), padlock (`vaulted`).

#### Day 5: CRM-Assisted Start Flow & MCP Endpoints (Lifecycle P6)
- **SM & SA**: Implement CRM-Assisted Start backend endpoint (`POST /api/cases/{id}/initiate-from-crm`):
  - Automatically loads contact info and pre-fills `prefilled_accepted` (email, phone) and `prefilled_confirmed` (name, address).
  - Prepares Concierge opening greeting citing CRM contact and meeting note.
- **SA**: Implement ASP.NET Core MCP tool-use endpoints: `GetCrmContext`, `ProposeFields`, `ConfirmField`, `ClarifyIntent`.
- **VI**: Implement SSE chat message renderer in React (`useChatStream`), supporting markdown rendering and bot/advisor avatar bubbles.
- **JE**: Complete xUnit automated test suite for case state transitions.

---

### Week 2: Agent Extraction, Citations, UI Ledger & End-to-End Validation

#### Day 6: Note Extraction Agent & Confidence Floor Reasoning (Lifecycle P6)
- **SA**: Author Claude 3.5 Sonnet extraction prompt template in Semantic Kernel:
  - Extracts 14 inferable attributes into closed enum sets.
  - Generates `heard` exact substring snippet and confidence score.
  - Branching: If confidence $\ge 0.70$, output structured JSON field proposals; if $< 0.70$, output clarifying question.
- **VI**: Build the **Proposal Approval Card** (Confirmation Ledger component) inside the React chat stream:
  - Renders proposed fields with `[Confirm all]`, `[Edit]`, `[Reject]` controls.
- **JE**: Implement `POST /api/cases/{id}/actions/confirm` and `POST /api/cases/{id}/actions/reject` endpoints.
- **SM**: Build Audit Trail event logger API (`POST /api/audit/events`) capturing agent decisions and confidence metrics.

#### Day 7: Value Correction & Downstream Conflict Cross-Checks (Lifecycle P6)
- **SA**: Implement business rule validation service in .NET:
  - Cross-check `funding.initial_amount` against `financial.liquid_net_worth` band.
  - Emit conflict warnings in agent reply when detected.
- **JE**: Implement `POST /api/cases/{id}/actions/correct` endpoint:
  - Saves corrected value, preserves original `heard` snippet, records advisor ID and timestamp. Sets state to `extracted_corrected`.
- **VI**: Wire up inline `[Edit]` input field on Proposal Cards and render advisory conflict alert banners.
- **SM**: Wire citation persistence: ensure every confirmed field record stores its source lineage and timestamp.

#### Day 8: Citations Formatting & Dual Retrieval Integration (Lifecycle P6)
- **SA & SM**: Implement Dual Retrieval prompt assembly:
  - Block 1: `conversation_history` (last 6 turns).
  - Block 2: `session_state` (redacted case JSON).
  - Block 3: `crm_context` (top-2 RAG chunks from CRM notes).
- **SA**: Implement structured citation formatters:
  - `crm_retrieval`: chunk_id, record_type, record_id, record_timestamp, excerpt.
  - `session_recall`: field, state, turn_index, confirmed_at, value_disclosed.
- **VI**: Build Citation Line component: renders grey italic citation text with icon (📎 `CRM note from 17 Apr 2026`) under agent replies.
- **JE**: Implement Case Validation endpoint (`GET /api/cases/{id}/validate`) verifying `can_submit = false` while unconfirmed fields remain.

#### Day 9: Golden Dataset Evaluation & Adversarial Testing (Lifecycle P7)
- **SM & SA**: Execute automated evaluation runner against 20 curated advisor test notes:
  - Measure field extraction precision, recall, and enum adherence.
  - Test edge cases: ambiguous spousal remarks, multi-sentence paragraphs, complex currency notations.
- **SA**: Conduct adversarial prompt injection tests (attempting to force titling writes without confirmation).
- **VI**: Polish UI micro-animations, loading indicators, keyboard shortcuts (Enter to send, Tab to confirm), and responsive styling.
- **JE**: Performance benchmarking: optimize SQLite/Cosmos DB queries to guarantee API response times $< 2.0$ seconds.

#### Day 10: Full End-to-End Walkthrough & Handover Demo (Lifecycle P7)
- **All Team Members**:
  - Run full end-to-end rehearsal using Margaret Chen (Individual) and Rajiv Mehta (Joint WROS) seed cases.
  - Validate complete audit trail: from CRM intake $\rightarrow$ pasted note extraction $\rightarrow$ clarifying question $\rightarrow$ inline correction $\rightarrow$ confirmation.
  - Export OpenAPI / Swagger specifications and Postman test collection.
  - Package final prototype deliverable and evaluation report for executive presentation.

---

## 6. Deliverables & Quality Gates

```
+---------------------------------------------------------------------------------------------------+
|                                    PROTOTYPE QUALITY GATES                                        |
+------------------------------------+--------------------------------------------------------------+
| 1. Deterministic Redaction Gate    | 100% pass on automated tests. Zero PII transmitted to LLM.   |
| 2. Advisor Confirmation Rule       | ZERO values written to persistent state without confirmation |
| 3. Confidence Floor (< 0.70)       | Ambiguous inputs trigger clarifying questions, not guesses.  |
| 4. Citation Grounding              | 100% of CRM-derived fields carry chunk ID and timestamp.    |
| 5. Dual Persistence Support        | App runs seamlessly on local SQLite and Azure Cosmos DB.     |
+------------------------------------+--------------------------------------------------------------+
```

---

## 7. Next Actions

1. Review and open [`NAO-Concierge-Conversational-Intake-Plan.html`](file:///c:/Users/swaroop.raj/Documents/workspace/nao-concierge/NAO-Concierge-Conversational-Intake-Plan.html) for an interactive visual representation of this plan.
2. Initialize Day 1 repository scaffolding using Claude Code across the 4 allocated tracks.
