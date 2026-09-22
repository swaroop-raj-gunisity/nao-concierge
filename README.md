# NAO Concierge

AI-powered conversational intake agent for new account onboarding — built for financial advisors.

## What It Does

NAO Concierge turns free-form advisor conversations and CRM data into structured, compliance-ready account applications. Instead of manually filling forms, advisors chat naturally while the agent extracts, proposes, and confirms field values with full audit trails.

### Core Capabilities

**Conversational Attribute Extraction**
- Extracts 14 inferable attributes from advisor notes and chat messages
- Maps values to closed enum sets (account type, risk tolerance, income bands, etc.)
- Captures exact `heard` text snippets as evidence for every extraction
- Confidence scoring (≥ 0.70 proposes, < 0.70 asks a clarifying question)

**CRM-Assisted Prefill**
- Ingests CRM contact records and meeting notes at case start
- Auto-prefills non-material fields (email, phone) from CRM
- Surfaces material fields (name, address) for advisor confirmation
- Sentence-level RAG retrieval from meeting notes for contextual extraction

**Deterministic PII Redaction**
- 4-tier redaction gate ensures zero PII reaches the LLM
- `none` → pass through | `indirect` → categorical value | `direct` → role token (OWNER_1) | `sensitive` → vaulted flag only
- Deterministic C# service — no ML, no exceptions, 100% tested

**Advisor Confirmation Loop**
- Nothing is saved without explicit advisor action (Confirm / Edit / Reject)
- 6 field states: `prefilled_accepted`, `prefilled_confirmed`, `extracted_unconfirmed`, `extracted_confirmed`, `extracted_corrected`, `vaulted`
- Inline correction preserves original heard text, original value, and corrected value

**Downstream Conflict Detection**
- Cross-checks funding amount vs. liquid net worth
- Validates investment objective vs. risk tolerance vs. time horizon
- Real-time advisory alerts on field change

**Citation & Audit Trail**
- Every extraction cites its source: CRM chunk ID, record type, timestamp, excerpt
- Full audit log: actor, event type, confidence score, model version, prompt version

### Security — Defense in Depth

| Layer | Protection |
|-------|-----------|
| Input | Heuristic injection filter + rate limiting |
| Prompt | System/user message separation, redaction gate, enum enforcement |
| Output | Mandatory advisor confirmation, refusal to fabricate, advice escalation |

## Prototype Scope

- **14 attributes** across 3 groups (Account Setup, Applicant Identity, Employment & Finances) + Funding
- **2 registrations**: Individual and Joint WROS
- **4 field states** exercised across 2 sample cases (Margaret Chen, Rajiv Mehta)
- **2 CRM records** with meeting notes for RAG retrieval

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | React 18 + Vite + TypeScript |
| **Streaming** | Server-Sent Events (SSE) |
| **Backend API** | ASP.NET Core 8 Web API |
| **AI Orchestration** | Microsoft Semantic Kernel (.NET) |
| **LLM Gateway** | Portkey (employer-hosted) |
| **LLM** | Claude (via Portkey) |
| **Embeddings** | text-embedding-3-small (1536d) |
| **Application DB** | Cosmos DB Serverless (cloud) / SQLite (local) |
| **Vector Store** | Azure AI Search (cloud) / SK In-Memory (local) |
| **Secrets** | Azure Key Vault (cloud) / dotnet user-secrets (local) |
| **Logging** | Serilog → Application Insights |

### Provider Abstraction

All infrastructure swaps via environment variables — zero code changes:

```
DATABASE_PROVIDER=sqlite|cosmosdb
VECTOR_PROVIDER=inmemory|aisearch|qdrant
LLM_GATEWAY_URL=https://portkey.employer.com
EMBEDDING_MODEL=text-embedding-3-small
```

## Project Structure

```
nao-concierge/
├── docs/                          # Architecture flows & decision records
│   └── architecture-flows.md      # 6 detailed data flow diagrams (Mermaid)
├── seed-data/                     # Prototype test data
│   ├── attribute-catalog.json     # 32 attributes, PII flags, enums
│   ├── cases/                     # 2 sample cases (Individual + Joint WROS)
│   └── crm/                       # CRM leads + meeting notes
├── claude-setup/                  # ADRs and setup docs
├── implementation_plan.md         # 2-week sprint execution plan
└── README.md
```

## Architecture

```
┌─────────────────────────────────────────────────────┐
│              React 18 + Vite Frontend               │
│   Form Ledger (left)  │  Chat + Proposals (right)   │
└────────────────────────┬────────────────────────────┘
                         │ SSE / REST
┌────────────────────────▼────────────────────────────┐
│             ASP.NET Core 8 Web API                  │
│  Input Filter → Redaction Gate → Semantic Kernel    │
│  Case APIs  │  Audit Trail  │  MCP Tool Endpoints   │
└──────┬──────────────┬───────────────┬───────────────┘
       │              │               │
  Cosmos DB /    Vector Store    Portkey Gateway
   SQLite       (SK In-Memory     → Claude LLM
                / AI Search)
```

## License

Proprietary — Perficient, Inc.
