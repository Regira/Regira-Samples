# HelpDesk

A standalone support-ticket desk built on the **Regira** framework (.NET 10 API + Vue 3 SPA), generated
end-to-end through the **Regira MCP server**. Customers raise tickets, agents work them through a Kanban
board or filtered queues, and every ticket carries a conversation thread and file attachments.

## What it does

- **Customers** create support tickets, tag them with one or more **categories**, and follow up in a
  conversation thread.
- Tickets carry a **priority**, a **status**, and an **assigned agent**; both are optional until triaged.
- Every ticket supports **multiple comments** (customer-visible or internal-only agent notes) and
  **multiple file attachments**.
- **Administrators** manage the reference data — categories, priorities, statuses and support teams — and
  the people directory (customers, agents, admins).
- The SPA offers three ways into the same data: a **Kanban board** (drag a card to change its status), a
  filterable **ticket queue** (list + advanced filters + paging), and **conversation-style** comment threads
  on each ticket's details page.

## Stack

| Layer | Tech |
|---|---|
| API | ASP.NET Core 10, `Regira.Entities` (EF Core + SQLite), OpenAPI + Scalar |
| SPA | Vue 3, TypeScript, Vite, Pinia, Bootstrap 5, `@regira/modules` |
| Auth | none (internal-tool / demo scope — see *Design notes*) |

## Running it

```bash
# API — http://localhost:6140  (Scalar UI opens automatically)
cd HelpDesk.API
dotnet run --urls http://localhost:6140

# SPA — http://localhost:6141
cd HelpDesk.SPA
npm install
npm run dev
```

The API seeds itself on first run (SQLite file `helpdesk.db`, created via `EnsureCreated()`). Delete the
`.db` file to force a full reseed after a model change.

## Data model

Regira's `Regira.Entities` package caps the **free tier** at 5 simple + 2 complex entity registrations (7
total, independent buckets). The domain was classified against that budget before any code was written:

| Entity | Classification | Why |
|---|---|---|
| `Category` | simple | flat lookup |
| `Priority` | simple | flat lookup (`Level` drives sort/urgency) |
| `Status` | simple | flat lookup (`SortOrder` drives the Kanban columns, `IsClosed` marks a done state) |
| `SupportTeam` | simple | flat lookup, `Members` is a back-reference (not owned) |
| `TicketAttachment` | simple | the per-owner attachment join `HasAttachments` registers |
| `Person` | **complex** | one role-discriminated actor (`Role: Customer \| Agent \| Admin`) instead of separate `Customer`/`Employee` registrations — the standard "Stakeholders" budget-fix remedy |
| `Ticket` | **complex** | the aggregate root: FK to `Priority`/`Status`/`SupportTeam`/two `Person`s, an owned `Categories` m2m join, an owned `Comments`-adjacent conversation, and attachments |

**→ 5 simple / 2 complex = 7/7**, confirmed at startup by the framework's own log line
(`Regira.Entities: 5 simple / 2 complex registered → tier = free`).

Two deliberate deviations from the "obvious" shape, both budget- and UX-driven:

- **`Comments` are not an owned `e.Related()` collection.** A conversation thread that required resending
  the *entire* history on every new message (the `Related()` contract) would be both wasteful and racy
  under concurrent posting. Comments are served through two narrow, hand-written actions on
  `TicketController` — `GET/POST /tickets/{id}/comments` — backed by the raw `AppDbContext`, costing no
  extra registration slot.
- **`ClosedAt` is fully server-owned.** A `Prepare` hook on the `Ticket` registration stamps it the moment
  a ticket lands on a status with `IsClosed = true`, clears it on reopen, and preserves the original
  close time across further edits — it is never present on `TicketInputDto`, so a client can't tamper with it.

## Seeded data

Generated with **Bogus** through `IEntityService` (see `HelpDesk.API/Data/Seeding/SeedData.cs`), seeded in
dependency order (lookups → people → tickets → comments → attachments):

| Entity | Count |
|---|---|
| Priorities | 4 |
| Statuses | 6 |
| Categories | 10 |
| Support teams | 5 |
| People | ~348 (≈320 customers, ≈24 agents, 4 admins) |
| **Tickets** | **500** (the primary entity) |
| Comments | ~1,000 (weighted customer/agent turns, back-dated relative to each ticket) |
| Attachments | ~90 (a representative subset, not every ticket) |

Distributions are deliberately weighted, not uniform (`Created` spread over the last 180 days, status/priority
via weighted-random, ~12% of `New` tickets left unassigned) — a flat distribution across every ticket would
make the Kanban board and the queue filters look meaningless.

## Project structure

```
HelpDesk/
├── HelpDesk.API/            # ASP.NET Core 10 API (port 6140)
│   ├── Entities/             # per-entity folders: model, DTOs, search object, service config
│   ├── Controllers/          # EntityControllerBase<> subclasses + TicketController's comment actions
│   ├── Data/                 # AppDbContext + Seeding/SeedData.cs
│   └── Extensions/           # DI wiring (AddEntityServices)
└── HelpDesk.SPA/             # Vue 3 SPA (port 6141)
    └── src/entities/
        ├── categories/ priorities/ statuses/ support-teams/   # lookup slices
        ├── people/                                            # Customer/Agent/Admin directory
        ├── entity-attachments/                                # shared file-upload slice
        └── tickets/
            ├── board/         # Kanban board (custom view, drag-and-drop status change)
            ├── comments/      # CommentsPanel.vue — conversation thread (custom, not a scaffolded slice)
            └── ticket-categories/  # generated m2m chip editor (Ticket ↔ Category)
```

## Design notes / trade-offs

- **No authentication.** The spec describes distinct actors (customers, agents, admins) but not a login
  flow; adding one would have doubled the scope without changing the CRUD/Kanban/conversation surface the
  brief asked for. The `Person.Role` field is what a future auth layer would gate on
  (`SelfHostingApiWithAuth` + `roles-end-to-end` is the documented upgrade path).
- **Priorities and Statuses are real admin-manageable entities**, not hard-coded enums, exactly as the
  spec's "administrators can manage categories, priorities, statuses and support teams" line asks — this is
  what makes them count against the entity budget and drove the `Person` merge above.
- **Kanban board fetches a capped page** (the server's default `MaxPageSize`, ~100 rows) rather than all
  500 tickets — a real board would add a "load more"/virtualized column; out of scope for this pass.

## Verification performed

- `dotnet build` — 0 warnings, 0 errors.
- `npm run build` (`vue-tsc -b && vite build`) — clean.
- API smoke-tested end-to-end via `curl`: create → update → update again (idempotency) → re-read, comment
  post/list, attachment upload/download, `ClosedAt` auto-stamp on status transition.
- SPA driven live against the running API: ticket overview + paging + filters, ticket details (all tabs),
  posting a comment through the actual UI (author picker → message → POST → thread refresh), the Kanban
  board, and every lookup/person overview page.

## Credits

Built by **Claude Sonnet 5** (`claude-sonnet-5`), running as **Claude Code** at reasoning effort **medium**
(default), driven entirely through the **Regira MCP server** tools (`get_bootstrap_guide`, `get_package`,
`how_to`, `get_type`, `search_docs`, and friends) plus the .NET/npm toolchain and a live browser for runtime
verification. No hand-authored framework code was written from memory — every Regira-specific pattern
(entity budget classification, attachments wiring, owned m2m collections, the front-end slice scaffold) was
sourced from the MCP docs for this session.

## Effort tracking

Best-effort reconstruction from the session transcript (not instrumented at the time):

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls (`get_bootstrap_guide`/`get_package`/`how_to`/`get_type`/`get_package_card`/`get_section_toc`) | ~23 | ~15 | ~38 |
| Other tool calls (Bash/Read/Write/Edit/Browser/etc.) | ~60 | ~90 | ~150 |
| Wall-clock time | ~35 min | ~55 min | ~90 min* |
| Tokens (session total, approximate) | — | — | ~490,000 |
| Estimated cost (Claude Sonnet 5 intro pricing, $2/$10 per MTok, ~80/20 input/output split assumed) | — | — | **~$1.60** |

\* Back-end and front-end phases overlapped with verification/debugging passes that touched both, so the
per-side split is approximate; total wall-clock is the reliable number. Token/cost figures come from the
session's own running token counter, not a dedicated billing API, so treat them as an order-of-magnitude
estimate rather than an invoice.
