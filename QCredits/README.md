# QCredits

Employee training-credit management: yearly QCredit budgets, credit requests with an approval workflow,
carry-over between years and separately funded group trainings, with a clean HR-style web interface.

> **1 QCredit = half a working day or EUR 250.**

| Part | Tech | URL |
|---|---|---|
| Back-end API (`QCredits.Api`) | .NET 10, ASP.NET Core, Regira Entities 6.4, EF Core + SQLite, ASP.NET Identity + Regira JWT | http://localhost:5851 (Scalar UI: `/scalar`, OpenAPI: `/openapi/v1.json`) |
| Front-end SPA (`qcredits-spa`) | Vue 3 + TypeScript + Vite 8, `@regira/modules` 6.4 (full reference scaffold), Bootstrap 5 | http://localhost:5852 |

## Business rules

- Every employee gets an **annual budget of 20 QCredits**, of which **5 are reserved for mandatory company days**,
  leaving **15 freely available** credits (plus credits carried over from the previous year).
- Credits can be spent on courses, books, online subscriptions, self-study during working hours, conferences and
  certifications. One **request** can hold several purchases/activities (owned items).
- Requests follow a workflow: `Draft -> Submitted -> Approved | Rejected`, `Submitted -> Draft` (withdraw) and
  `Draft/Submitted -> Cancelled`. An administrator may also cancel an approved request, which gives the credits back.
  **Only approved requests deduct credits**. Only drafts can be edited.
- A balance may go down to the **minimum balance (-10 by default)**. Submitting checks
  `remaining - pending - request >= minimum`; approving checks `remaining - request >= minimum`.
- Administrators manage the yearly **credit policy** (annual, reserved, max carry-over, minimum balance), the
  per-employee **allocations** (incl. used company days and individual overdraft limits), **generate** a year's
  allocations and **close a year & roll over**: positive balances carry over up to **10 credits**, a deficit is
  carried over in full (never below the minimum balance). A roll-over is refused while requests of that year are
  still pending.
- **Group trainings** are a separate entity with their own budget (total cost in EUR) and participants; they never
  touch personal QCredit balances.

Balance formula: `free = annual - reserved + carried over`, `remaining = free - approved`.

## Roles & security

- JWT auth on ASP.NET Identity (users in the same SQLite DB), roles `Admin` and `Employee` (role claim `role`).
- Every endpoint requires a signed-in user. A global `WriteAuthorizationFilter` allows writes on departments,
  employees, credit years, allocations and group trainings to `Admin` only.
- Row-level security (global filter query builders): an employee only sees **their own** employee record,
  allocations and requests (linked by e-mail); administrators see everything. Employees always create requests for
  themselves (stamped server-side).
- The workflow fields (`Status`, `SubmittedAt`, `DecidedAt`, `DecidedBy`, `DecisionComment`) are only written by the
  workflow actions (`CreditRequestGuard` prepper + a scoped trusted-writer flag); approve/reject are `Admin`-only.
- The SPA mirrors the tiers (`src/access.ts`): admin-only screens are hidden for employees, forms become read-only.

### Demo accounts (development seed)

The seeded password is configured in `QCredits.Api/appsettings.Development.json` (`Seed:DemoPassword`).

| User | Role |
|---|---|
| `admin@qcredits.test` (Hanne Peeters, HR) | Admin + Employee |
| `elise.maes@qcredits.test` (Engineering) | Employee |
| `tom.wouters@qcredits.test` (Sales) | Employee |

## Running

```bash
# API (seeds the SQLite database on first start)
cd QCredits.Api
dotnet run --launch-profile http                    # http://localhost:5851
dotnet run --launch-profile http -- --ResetDatabase=true   # drop + re-create + re-seed

# SPA
cd qcredits-spa
npm install
npm run dev                                         # http://localhost:5852 (strictPort)
npm run build                                       # vue-tsc -b + vite build
```

The SPA calls the API origin directly (`public/config.json -> api`), the API allows the SPA origin via CORS
(`appsettings.json -> Cors:Origins`). JWT audience = `qcredits-spa` on both sides.

## API overview (`/api` prefix)

| Resource | Endpoints |
|---|---|
| `departments`, `employees`, `credit-years`, `credit-allocations` | Regira CRUD (`GET /`, `/search`, `/{id}`, `POST`, `PUT`, `PATCH`, `DELETE`) |
| `credit-requests`, `group-trainings` | Regira CRUD + batch `POST /list`, `POST /search`, typed `sortBy` / `includes` |
| `credit-requests/{id}/submit` \| `withdraw` \| `cancel` | workflow (owner or admin) |
| `credit-requests/{id}/approve` \| `reject` | workflow (admin; reject needs a comment) |
| `credit-allocations/generate`, `credit-allocations/rollover` | admin year actions (`{ "year": 2026 }`) |
| `dashboard`, `dashboard/me` | aggregates (row-scoped) and the signed-in user's balance |
| `auth`, `auth/validate`, `auth/refresh`, `auth/password/...` | Regira account controllers |

## Data model

Free-tier budget of Regira Entities: **4/5 simple + 2/2 complex** registrations.

| Entity | Kind | Notes |
|---|---|---|
| `Department` | simple | normalized `?q=` search |
| `Employee` | simple (+SearchObject) | linked to a login by e-mail, department eager-loaded |
| `CreditYear` | simple | yearly policy (20 / 5 / 10 / -10), `IsClosed` |
| `CreditAllocation` | simple (+SearchObject) | per employee per year; processor fills free/used/pending/remaining |
| `CreditRequest` | complex | **primary entity**; owned `CreditRequestItem` rows via `Related()`, server-computed totals |
| `GroupTraining` | complex | owned `GroupTrainingParticipant` rows (employee + attended) via `Related()` |

Credits are stored as `double` (half-credit steps, SQL aggregates on SQLite), money as `decimal`.

## Seed data

Seeded through the `IEntityService` implementations (preppers/normalizers run) with Bogus (`nl_BE`, fixed seed):
8 departments, 90 employees (some joining mid-period, a few inactive), credit years 2024-2026 (2024/2025 closed),
~260 allocations with carry-over computed from the previous year, **~550 credit requests** (coherent topic ->
activities tuples, realistic status distribution, approvals never below the minimum balance) and ~33 group trainings
with participants.

## Front-end highlights

- Home: personal balance card (progress bar used/pending/overdraft, company days, carry-over, requestable),
  admin approval queue, organisation KPIs, usage per department and per activity type, group-training summary.
- Requests: status badges, quick status filter, advanced filter (year, status, activity type, employee, department),
  editable items table (credits suggested from cost), approval panel with timeline, balance preview and actions.
- Allocations: per-employee progress bars, generate / close & roll-over actions for administrators.
- Group trainings: tabbed form with a participants editor (attendance toggles).

## Build status

- `dotnet build`: succeeded, 0 warnings, 0 errors.
- `npm run build` (vue-tsc -b + vite build): succeeded.
- Startup: `Regira.Entities: 4 simple / 2 complex registered -> tier = free`, no startup warnings.
- Verified end to end (API + browser): role scoping, create -> update -> update, PATCH round trip, submit, approve,
  reject validation, draft lock, participants save twice, no horizontal overflow at 375 px.

## Project metrics

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls | 21 | 9 | **30** |
| Wall-clock time | ~15 min (12:19 - 12:34) | ~16 min (12:34 - 12:50) | **~31 min** |
| Tokens (harness budget counter) | ~263k | ~180k | **~443k** |

Cost: not reported by the harness. As a rough indication only, most of those tokens are cached context re-reads;
check the billing console for the exact figure.

## Credits

Built by **Claude** (Anthropic), model **Claude Opus 5.5**, running as a **Claude Code** sub-agent
(general-purpose agent type) with **low reasoning effort**, following the Regira MCP golden path
(bootstrap guide -> package cards -> heading-scoped guides -> generators).
