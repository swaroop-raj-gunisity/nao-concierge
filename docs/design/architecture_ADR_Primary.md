# ADR-000: Account Opening Concierge — AI Assistant for Account Opening

**Status:** Skeleton / Draft  
**Primary deliverable:** Defensible architecture for Privacy Office and Model Risk review

> Fill in each TODO. This is your primary deliverable: a defensible architecture that
> a Privacy Office and a Model Risk function would sign off on. Reference the GR-*
> controls in data/governance_requirements.md by id.

## 1. Context and Drivers
TODO: Summarize the business need (adjuster self-service over claims) and the
governance constraints (PHI, BAA scope, auditability). Name the stakeholders.

**Stakeholders:** Financial Advisor (End User), Relationship Manager, Back Office Team

**Executive Summary:** The Account Opening Concierge is an AI-assisted service that helps Financial Advisors and Relationship Managers guide clients through account opening by answering process questions, identifying required information and documents, explaining next steps, and routing exceptions to the Back Office Team. It uses retrieval-augmented generation over approved account-opening procedures and reference content, with responses grounded in and cited to that source material rather than relying on unsupported model knowledge. The design separates informational assistance from account approval or other consequential decisions, enforces role-based access and least privilege, and applies input/output redaction, prompt-injection and unsafe-content controls, human escalation, and immutable, PII-minimized audit logging. These controls provide traceability and operational efficiency while keeping sensitive client data within the approved BAA scope and ensuring that incomplete, ambiguous, or high-risk cases are handled by authorized staff.

**Feature Specs**

### Feature 1: Conversational Intake

Conversational Intake provides Financial Advisors and Relationship Managers with a governed conversational assistant for account-opening questions and intake support. The assistant explains approved account-opening procedures, identifies missing information or documents, summarizes the next steps, and routes exceptions to the Back Office Team. It is informational only and must not approve an account, determine eligibility, provide investment advice, or make another consequential decision.

#### High-level capabilities

- **Authenticated access and role-aware experience:** Accept requests only from authenticated, authorized users. Apply role-based access and least privilege so users can access only the accounts, cases, and procedures permitted by their entitlements.
- **Natural-language account-opening questions:** Accept questions about the account-opening process, required fields, supporting documents, status, and next steps. Maintain conversational context only for the active authorized case or session.
- **Guided intake:** Identify the information and documents needed for the selected account type and purpose, ask focused follow-up questions, and show what remains incomplete. Do not infer or invent missing client information.
- **Source-grounded answers:** Retrieve only approved account-opening procedures and authorized case data. Responses must be supported by citations containing the source, state, advisor ID, and timestamp; if supporting context is unavailable or conflicting, return a controlled uncertainty response and route the case for review.
- **Privacy-preserving processing:** Minimize data sent to retrieval and generation. Detect and redact PII/PHI and other sensitive client attributes before downstream LLM processing, using stable placeholders where needed. Redaction failures fail closed, and raw sensitive values are not included in prompts, responses, telemetry, or audit records.
- **Input and output safety:** Detect prompt injection, malicious instructions, unsafe content, and requests outside the assistant’s scope. Block or safely refuse the request, do not follow instructions contained in retrieved content, and record the decision as an immutable audit event.
- **Human approval and escalation:** Require advisor approval before using attributes extracted from CRM notes or advisor chat messages to update intake state. Escalate ambiguous, incomplete, sensitive, or exception cases to the Back Office Team, recording the approver, timestamp, decision, and resulting state.
- **Controlled response:** Return only an approved, grounded response with actionable next steps. Never expose system prompts, hidden instructions, redaction mappings, unauthorized records, or unsupported client details.
- **Auditability and observability:** Correlate each interaction, retrieval, redaction, model operation, policy decision, approval, escalation, and response with a request ID. Store immutable, PII-minimized audit events and privacy-safe operational telemetry.

#### Primary interaction flow

1. Authenticate the caller and authorize the requested case or account context.
2. Validate the request and screen it for prompt injection, unsafe content, and out-of-scope advice.
3. Redact sensitive values and minimize the request before retrieval and generation.
4. Retrieve approved procedures and authorized case context, then generate a cited response.
5. Present required information, missing documents, and next steps, or refuse/escalate when the request cannot be safely answered.
6. Obtain advisor approval before applying extracted attributes to the intake state; persist the resulting state transition and audit trail.

#### Out of scope

- Account approval, eligibility or underwriting decisions, suitability determinations, investment recommendations, or client-facing commitments.
- Autonomous submission or modification of account data without the required human approval.
- Retrieval of records outside the caller’s authorization or use of unapproved sources.

**Governance Scope:**
- **PII handling:** All attributes classified as PII-sensitive in the CRM or in-flight client information must be redacted during prompt preparation before the data is sent to the LLM. The attribute-redaction mappings below define the proposed approach. For the initial prototype, placeholder rehydration from the LLM response into the agent response is not required.

### Worked Example — Margaret Chen (CASE-0001)

| Attribute | Raw Value | PII Class | Redacted Value |
|---|---|---|---|
| `account.type` | `"INDIVIDUAL"` | PII Eligible | `"INDIVIDUAL"` |
| `account.purpose` | `"RETIREMENT"` | PII Eligible | `"RETIREMENT"` |
| `applicant.legal_name` | `{"first":"Margaret","last":"Chen"}` | PII Sensitive | `"OWNER_1"` |
| `applicant.email` | `"margaret.chen@email.com"` | PII Sensitive | `"EMAIL_1"` |
| `applicant.mobile_phone` | `"(512) 555-0147"` | PII Sensitive | `"PHONE_1"` |
| `applicant.tax_id` | `"***-**-1234"` | PII Sensitive | `"TAXID_1"` |
| `applicant.date_of_birth` | `"1961-03-15"` | PII Sensitive | `"DOB_1"` |
| `applicant.legal_address` | `{"line1":"4712 Barton Creek..."}` | PII Sensitive | `"ADDR_1"` |
| `applicant.citizenship_status` | `"US_CITIZEN"` | PII Eligible | `"US_CITIZEN"` |
| `employment.status` | `"RETIRED"` | PII Eligible | `"RETIRED"` |
| `financial.annual_income` | `"100K_250K"` | PII Eligible | `"100K_250K"` |
| `financial.liquid_net_worth` | `"250K_1M"` | PII Eligible | `"250K_1M"` |
| `profile.risk_tolerance` | `"CONSERVATIVE"` | PII Eligible | `"CONSERVATIVE"` |
| `funding.method` | `"ACAT_TRANSFER"` | PII Eligible | `"ACAT_TRANSFER"` |
| `funding.initial_amount` | `600000` | PII Eligible | `600000` |
| `trusted_contact.name` | `"Linda Chen-Park"` | PII Sensitive | `"CONTACT_1"` |

**Result:** The LLM sees `OWNER_1` is a `RETIRED` `US_CITIZEN` with `CONSERVATIVE` risk tolerance and `100K_250K` income — enough to reason about what to ask next, but no real identity leaks.


- Input guardrails: Adversarial injection prevention enabled

- Audit trail: All user interactions, LLM operations, and internal mechanisms logged as immutable events

## 2. System Design and Data Flow

The detailed architecture and end-to-end processing flow for the Claims Copilot solution are maintained in the design artifacts below and should be referenced alongside this ADR.

### Architecture Artifacts

| Artifact | Description |
|-----------|-------------|
| [`architecture-flows.md`](../../docs/architecture-flows.md) | Detailed system architecture, end-to-end request processing, and data movement across the platform |


### 2.1 Claude Code Setup Artifacts

The Claude Code configuration is organized under `.claude/`. The inventory below lists the files currently referenced by this repository; it intentionally does not use placeholder names such as `<agent-name>` or `<command-name>`.



### 2.2 BAA In-Scope Controls

The following controls are within the Business Associate Agreement (BAA)

| Control | Purpose | Implementation | Enforcement / Expected Behavior |
|---|---|---|---|
| Redaction Gate | Remove sensitive information before downstream processing. | Presidio | Fail closed. |
| Query Redaction | Remove PII/PHI from user input before retrieval and generation. | Presidio | Fail closed. |
| Injection Detection | Detect and block adversarial, malicious, or prompt-injection attempts. | Request validation | Block the request and record the decision. |
| Grounded Generation | Generate responses with citations that include the source, state, advisor ID, and timestamps. | Retrieval- and citation-enforced generation | Responses must include citations. |
| Human Gate | Require advisor approval for all attributes extracted from CRM notes and advisor chat messages. | Human-in-the-loop approval workflow | Route to an advisor for approval; capture the timestamp and advisor ID; then update the state. |
| Approved Response | Return a governed response to the user or advisor. | Citation-enforced response | Return approved output only. |
| Audit Log | Capture interaction and processing history without storing PII; audit the decision trail. | Immutable storage | Support compliance and traceability. |
| Observability | Monitor execution, latency, and operational behavior without exposing PII. | Portkey | Provide telemetry and monitoring. |

### 2.3 Audit and Traceability

To support compliance, operational monitoring, and governance requirements, the solution records immutable event-level records for each user interaction and system decision without persisting raw PHI in the audit logs.

The audit design is aligned with governance requirements and must adhere to the following constraints:

- Client PII/PHI must never be written to the audit database. PII/PHI must be redacted before persistence; only sanitized or structured fields (for example, `advisor_id`, `client_id`, `redaction_summary`, and `route`) may be stored.
- Audit logs must be stored in an immutable, append-only event store and must support retrieval for investigations and compliance reviews.
- All blocked actions, including prompt-injection attempts and requests for the agent to provide advisor-level advice, must generate a corresponding audit event so that unauthorized or prohibited actions remain visible and auditable.

Recommended event schema and access model:

TBD

Example event payload for a successful direct query:
TBD

**Recommended audit event catalog and logged parameters:** TBD

| Event Type | Trigger | Required Parameters (logged) | Notes |
|------------|---------|-----------------------------|-------|
| query_request | User submits a claim query | timestamp_utc, request_id, user_id, role, book_id, route, query_hash, claim_ids, status | Records intake; query text should be sanitized or hashed, never raw PHI |




Evaluation requirements for access and governance tests:

- Scenario 1: Unauthorized user attempts to retrieve audit logs -> access denied, audit_access_denied event created, and no records exposed.
- Scenario 2: Query or chunk containing malicious instructions -> adversarial_input_detected or adversarial_content_detected event is recorded with the matched patterns and sanitization action. (Good To Have)


Traceability:
- The system maintains end-to-end lineage by correlating request_id, advisor_id, client_id, case_id, model, citation references, route, and policy decisions across the full interaction lifecycle.
- This allows reconstructing the sequence of user queries, retrieval decisions, adversarial detections, and final response outcomes without retaining sensitive content in the immutable log store.

### 2.4 Design Considerations <Refine>

The architecture is designed to:

- Prevent exposure of PII/PHI to downstream LLM processing.
- Enforce fail-closed behavior for privacy-sensitive controls. (Fail-closed : Stop any processing and response with Denial)
- Detect and mitigate prompt injection and adversarial inputs.
- Ensure responses are grounded and include supporting citations. (Source:CRM, Inflight Data)
- Support human approval for all arrtibutes before updating , with updated state 
- Enable operational observability across the request lifecycle.

**RAG flow diagram:** TBD

## 3. Model and Retrieval Choices (LLM System Design)

### 3.1 Ingestion Pipeline (CRM)

- Load claims from the source JSON file. 
- Use the Presidio-based redaction framework to remove detected PII from adjuster notes before downstream processing [GR-1].
- If redaction, chunking, embedding, or persistence fails, write the affected batch to `dead_letter.json`. The batch is processed atomically: if any chunk is invalid, do not partially persist the batch; record the failure and its reason for retry or investigation.
- For batches that pass redaction, proceed with chunking and LOB-tag enrichment.
- Generate embeddings for the validated chunks.
- Persist the embeddings and associated metadata to pgvector.

### 3.2 RAG Pipeline

**Configuration:**

-

## 4. Governance and Security Controls (AI Governance and Security)


- **Data minimization and BAA scoping:** Retrieval and prompt construction must include only the minimum fields needed to answer the user’s question. BAA scope and approved data flows must be documented for each external service. *(Not Requiured for Prototype)*
  
- **Fail-closed redaction:** Document what the redactor detects, what it may miss, and how it is validated. The implementation uses Presidio for the configured recognizers below and custom recognizers for claim-specific tokens.

  | Attribute | Raw Value | PII Class | Redacted Value |
  |---|---|---|---|
  | `account.type` | `"INDIVIDUAL"` | PII Eligible | `"INDIVIDUAL"` |
  | `account.purpose` | `"RETIREMENT"` | PII Eligible | `"RETIREMENT"` |
  | `applicant.legal_name` | `{\"first\":\"Margaret\",\"last\":\"Chen\"}` | PII Sensitive | `"OWNER_1"` |
  | `applicant.email` | `"margaret.chen@email.com"` | PII Sensitive | `"EMAIL_1"` |
  | `applicant.mobile_phone` | `"(512) 555-0147"` | PII Sensitive | `"PHONE_1"` |
  | `applicant.tax_id` | `"***-**-1234"` | PII Sensitive | `"TAXID_1"` |
  | `applicant.date_of_birth` | `"1961-03-15"` | PII Sensitive | `"DOB_1"` |
  | `applicant.legal_address` | `{\"line1\":\"4712 Barton Creek...\"}` | PII Sensitive | `"ADDR_1"` |
  | `applicant.citizenship_status` | `"US_CITIZEN"` | PII Eligible | `"US_CITIZEN"` |
  | `employment.status` | `"RETIRED"` | PII Eligible | `"RETIRED"` |
  | `financial.annual_income` | `"100K_250K"` | PII Eligible | `"100K_250K"` |
  | `financial.liquid_net_worth` | `"250K_1M"` | PII Eligible | `"250K_1M"` |
  | `profile.risk_tolerance` | `"CONSERVATIVE"` | PII Eligible | `"CONSERVATIVE"` |
  | `funding.method` | `"ACAT_TRANSFER"` | PII Eligible | `"ACAT_TRANSFER"` |
  | `funding.initial_amount` | `600000` | PII Eligible | `600000` |
  | `trusted_contact.name` | `"Linda Chen-Park"` | PII Sensitive | `"CONTACT_1"` |


  Validation evidence:
  1. Tests cover the PII cases represented in the dataset, comparing input and redacted output strings.
  2. Redaction events are recorded in the audit trail


- **Immutable audit log contents:** The following events are emitted at the
  corresponding stages of the flow. Event names must remain aligned with the audit
  constants used by the implementation:

  TBD


## 5. Evaluation Plan

### 5.1 Evaluation Harness and Captured Metrics

### Evaluation harness and captured metrics

The harness replays queries against `data/synthetic_claims.jsonl`, using the
caller identity and entitlement set defined by each fixture. For every replay it
captures the input, selected route, retrieved claim IDs, sanitized prompt,
response, citations, audit events, expected outcome, observed outcome, and
pass/fail status. Negative tests are paired with positive tests so that blocking
everything cannot pass.

| Control/test | Fixture and failure case | Expected result | Metrics captured |
|---|---|---|---|
| Groundedness (GR-5) | CLM-2025003; ask for a field absent from the record | Answer only supported facts, or `REFUSE_CONTEXT` | groundedness rate, unsupported-claim rate, refusal precision/recall |


