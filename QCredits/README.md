# QCredits

A standalone demo application for managing employee **training credits**, built on the Regira framework
(.NET 10 API + Vue 3 SPA) using the Regira MCP server.

> 1 QCredit = half a working day = EUR 250.

## What it does

- Employees receive an annual budget of **20 QCredits** per year: 5 reserved for mandatory company
  training days, 15 freely available for courses, books, online subscriptions or self-study.
- Employees submit **QCredit requests** bundling one or more purchases/activities; requests need
  administrator approval before credits are deducted.
- Administrators manage the yearly **credit policy** (annual/reserved credits, max carry-over, minimum
  allowed balance), and per-employee **carry-over** of unused credits into the next year (capped at 10).
  Approved balances may go as low as **-10** credits.
- **Group trainings** are funded separately and never touch a personal QCredit balance.
- A **Balances** dashboard shows every employee's remaining/pending credits with progress bars, plus a
  drill-down for a single employee.

## Stack & ports

| App | Tech | Port |
|---|---|---|
| Back-end API | ASP.NET Core 10, `Regira.Entities`, EF Core + SQLite, Serilog, Scalar/OpenAPI | `http://localhost:6150` |
| Front-end SPA | Vue 3, `@regira/modules`, Vite, Pinia, Bootstrap 5 | `http://localhost:6151` |

No authentication: this is a small internal demo. Admin vs. employee actions are separated by workflow
(the approve/reject panel only appears on pending requests, and only lists `Employee.role == Admin` as a
valid approver), not by a login wall — see *Design notes* below.

## Running it

```bash
# back-end (from backend/QCredits.Api)
dotnet run --urls http://localhost:6150
# Scalar UI at http://localhost:6150/scalar — the DB (qcredits.db) is created and seeded on first run

# front-end (from frontend, in a second terminal)
npm install
npm run dev -- --port 6151
# http://localhost:6151
```

## Domain model

Five entities, all under the free tier (5 simple + 2 complex registrations):

| Entity | Classification | Notes |
|---|---|---|
| `Employee` | simple | Person + `Role` (`Employee`/`Admin`) — role gates who can appear as an approver |
| `CreditPolicy` | simple | One row per year: `AnnualCredits`, `ReservedCredits`, `MaxCarryOver`, `MinBalance` |
| `EmployeeCarryOver` | simple | Admin-set carry-over per employee/year |
| `GroupTraining` | simple | Independent — no FK to `Employee`, funded separately |
| `QCreditRequest` | **complex** | Owns `QCreditRequestItem` via `e.Related()`; `Status`/decision fields kept off the input DTO |
| `QCreditRequestItem` | owned child | Course/Book/Subscription/SelfStudy line items, no own registration |

**Budget tally:** 4 simple / 1 complex → fits the free tier (confirmed at startup: `Regira.Entities: 4
simple / 1 complex registered → tier = free`).

### Approval workflow

`Status`, `DecisionDate`, `ApproverId` and `DecisionNotes` are intentionally absent from
`QCreditRequestInputDto` — an ordinary `PUT`/`PATCH` can never change them. The only writer is
`QCreditRequestWorkflowController` (`POST /qcredit-requests/{id}/approve|reject`), which flips a scoped
`RequestWorkflowContext.IsTrustedWriter` flag before saving; `QCreditRequestStatusPrimer` restores the
stored values from `EntityEntry.OriginalValues` on every other write. Approving also re-validates the
employee's live balance against `CreditPolicy.MinBalance` and rejects with a 400 if it would go too low.
The seeder is a trusted writer too, so historical Approved/Rejected rows can be stamped directly.

### Balances

`GET /balances` and `GET /balances/{employeeId}` are a read-only cross-entity aggregate endpoint
(`BalancesController` + `BalanceCalculator`) — they bypass the entity pipeline entirely and compute
`Remaining = (Annual - Reserved) + CarriedOver - Sum(Approved.TotalCredits)` directly against the
`DbContext`, per the *Cross-entity aggregates & report endpoints* pattern.

## Seed data

Seeded once (skipped if `Employees` already has rows) through the registered `IEntityService`
implementations with [Bogus](https://github.com/bchavez/Bogus), so the same primers/preppers the API
uses at runtime also produce the seed:

- **160 employees** (14 administrators), 10 departments
- **3 years** of `CreditPolicy` (current year and the two before it)
- **~130 `EmployeeCarryOver` rows** (~40% of employees, into each non-first seeded year)
- **28 `GroupTraining` sessions**
- **500 `QCreditRequest`s** (the primary entity) with 1-3 owned line items each, weighted towards the two
  most recent years, status mix ≈ 55% Approved / 25% Pending / 20% Rejected

## Design notes / deviations

- **No authentication.** The bootstrap guide treats auth as optional and explicitly warns against
  assuming it's required; a login/JWT/roles round-trip would have doubled the build for a feature the
  brief didn't ask for. Admin/employee separation is expressed through the workflow endpoints and the
  `Employee.Role` field instead.
- **`CreditPolicy` is a modal-edit (simple) front-end slice** (`isComplex: false`) — four numeric fields,
  no relations. Every other entity is a full Details-page slice.
- **`GroupTraining` has no FK to `Employee`** — the spec asks that group trainings never affect a personal
  balance, so it was kept fully independent rather than linked-but-ignored.

## Verification performed

- `dotnet build` — 0 warnings / 0 errors. `npm run build` (`vue-tsc -b` + `vite build`) — 0 errors.
- Runtime: startup budget log, `create → update → update again → re-read` idempotency, PATCH/PUT round-trip
  proving `Status`/`TotalCredits` survive a partial update, 409 on a constrained `Employee` delete,
  approve/reject happy path + guard rails (re-approve a decided request, non-admin approver, balance floor),
  and the SPA driven live in a browser (dashboard, 500-row paged overview, request details with tabs, the
  approval panel, the Balances dashboard for all 160 employees, a mobile viewport) — all with a clean
  console.

## Credits

Built by **Claude** (model `claude-sonnet-5`), running as the **Claude Code** CLI agent, reasoning effort
**medium-low (40)**, via the Regira MCP server (`https://mcp.regira.com/mcp`).

### Effort tracking

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls | ~20 | ~12 | **32** |
| Wall-clock time | ~25 min | ~25 min | **~50 min** (shared discovery/reading time up front) |

Token usage and USD cost are not observable from inside the agent session (no tool surfaces them), so
they are omitted rather than estimated. The MCP call count above is a manual tally of every
`get_bootstrap_guide` / `get_package` / `get_package_card` / `get_type` / `how_to` call made during the
build (documentation lookups only — it excludes ordinary file/bash/browser tool calls).
