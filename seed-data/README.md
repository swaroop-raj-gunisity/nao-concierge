# NAO Concierge — Seed Data

Realistic seed data for the Account Opening Concierge prototype. Two client cases exercising all 4 prototype states across the full 32-attribute catalog.

## Files

```
seed-data/
├── attribute-catalog.json              # 32 attributes, 12 groups, 5 registrations, enums, PII flags
├── cases/
│   ├── case-001-margaret-chen.json     # Individual, fully onboarded (completed)
│   └── case-002-rajiv-mehta.json       # Joint WROS, in progress (intake)
├── crm/
│   ├── crm-leads.json                  # 2 CRM lead records
│   └── crm-notes.json                  # 2 meeting notes (source data for prefilled/extracted values)
└── README.md
```

## Clients

| Case | Client | Registration | Status | Attributes Filled | States Used |
|------|--------|-------------|--------|-------------------|-------------|
| CASE-0001 | Margaret A. Chen | INDIVIDUAL | completed | 27/27 applicable | All 4 |
| CASE-0002 | Rajiv K. Mehta + Ananya R. Mehta | JOINT_WROS | in_progress | 26/28 primary + 2/7 joint owner | All 4 |

## Prototype States

| State | Meaning |
|-------|---------|
| `captured_direct` | Typed/selected by the advisor |
| `extracted_confirmed` | Agent extracted from conversation, proposed, advisor confirmed |
| `prefilled_accepted` | From CRM, non-material, auto-accepted |
| `prefilled_confirmed` | From CRM, material, advisor confirmed |

## State Distribution

### Case 1 — Margaret Chen (Individual, Completed)
- `captured_direct`: 13 (account type, program, PII fields, financial bands, regulatory, disclosures, trusted contact, funding amount)
- `extracted_confirmed`: 9 (purpose, citizenship, employment, household, investment profile, funding method, delivering institution)
- `prefilled_accepted`: 2 (email, phone)
- `prefilled_confirmed`: 2 (legal name, address)

### Case 2 — Rajiv Mehta (Joint WROS, In Progress)
- `captured_direct`: 13 (program, purpose, PII, financial, regulatory, funding, joint owner name/email)
- `extracted_confirmed`: 7 (account type, employment, household, investment profile)
- `prefilled_accepted`: 3 (citizenship, email, phone)
- `prefilled_confirmed`: 2 (legal name, address)
- Not yet collected: 8 (joint owner fields, trusted contact, disclosures)

## CRM Data Lineage

Every `prefilled_*` and `extracted_confirmed` value traces back to a specific CRM record:
- **CRM Leads** (`crm-leads.json`) — Contact info sources `prefilled_accepted` (email, phone) and `prefilled_confirmed` (name, address)
- **Meeting Notes** (`crm-notes.json`) — Free-text notes source `extracted_confirmed` values. The `heard` field on each attribute references specific phrases from these notes.

## Advisors

| ID | Name | Clients |
|----|------|---------|
| ADV-KP-042 | Kevin Park | Margaret Chen |
| ADV-SK-017 | Sarah Kim | Rajiv Mehta |

## Loading

These JSON files are the data contract for Cosmos DB (primary) or SQLite (fallback). To load:

1. Parse `attribute-catalog.json` first — it defines the schema
2. Load `crm/crm-leads.json` and `crm/crm-notes.json` into CRM tables
3. Load `cases/*.json` into the cases collection

Each case document is self-contained and can be loaded independently.
