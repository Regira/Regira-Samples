# Fleet Manager

A fleet maintenance management demo built on the Regira framework: vehicles, suppliers, intervention
types, interventions and invoices, with a professional KPI dashboard for fleet managers.

- **Back-end API**: http://localhost:6130 (ASP.NET Core / .NET 10, `Regira.Entities`, SQLite, Scalar UI at `/scalar`)
- **Front-end SPA**: http://localhost:6131 (Vue 3 / TypeScript / Vite, `@regira/modules`)

## Domain model

| Entity | Classification | Notes |
|---|---|---|
| `Vehicle` | simple | plate, brand/model, type, status, mileage, year, VIN |
| `Supplier` | simple | workshops that perform interventions; owns `SupportedInterventionTypes` (m2m) |
| `InterventionType` | simple | catalog of maintenance/repair operations, editable |
| `Intervention` | complex | links a `Vehicle` + `Supplier`, owns `InterventionTypes` (m2m), optional `InvoiceId` |
| `Invoice` | complex | belongs to a `Supplier`; `TotalAmount` is aggregated live from its interventions |

Entity budget (free tier = 5 simple + 2 complex): **3 simple / 2 complex registered** — logged at startup
(`Regira.Entities: 3 simple / 2 complex registered -> tier = free`). The two m2m join tables
(`SupplierInterventionType`, `InterventionInterventionType`) are owned via `e.Related()` and cost no
registration slot.

Two relationship patterns worth calling out, both drawn straight from the Regira Entities decision table:

- **Invoice <-> Intervention** is an *optional parent FK* (`Intervention.InvoiceId?`), not an owned
  collection — an intervention is billed later, so the invoice cannot own the write. `Invoice.TotalAmount`
  is a server-owned aggregate recomputed by `InterventionInvoiceTotalPrepper` whenever an intervention is
  added/edited (its own invoice **and** its previous invoice, if it moved). Verified live in the browser:
  editing an intervention's cost updates the parent invoice's total in the same save, and reloading the
  invoice shows the correct sum.
- **Supplier <-> InterventionType** ("which repairs can this supplier perform") is a pure m2m join, edited
  as chips directly on the supplier's form.

## Running it

**Back-end**
```
cd backend/Fleet.Api
dotnet run --urls http://localhost:6130
```
First run creates `fleet.db` (SQLite, `EnsureCreated`) and seeds it automatically (idempotent — skipped on
later runs once data exists). Scalar API docs open automatically at `/scalar`.

**Front-end**
```
cd frontend
npm install
npm run dev -- --port 6131
```
Points at `http://localhost:6130/api` in `public/config.json` (direct-origin + CORS, no dev proxy).

## Seed data

Seeded through `IEntityService` in dependency-ordered waves (`Data/FleetSeeder.cs`), using Bogus for
realistic names/addresses/VINs:

- 15 intervention types (oil change, brake service, annual inspection, ...)
- 150 vehicles (cars/vans/trucks/trailers/motorcycles, weighted status distribution)
- 25 suppliers, each supporting 2-6 intervention types
- 180 invoices across suppliers, weighted status distribution
- **500 interventions** (primary entity) — vehicle + supplier picked with supplier/type affinity, 1-3
  intervention types each, ~65% of completed interventions linked to an invoice from the same supplier

A final pass recomputes every invoice's `TotalAmount` directly against the persisted intervention rows
(the documented "seeding needs a second pass" consequence of an aggregate over a non-owned child collection).

## Dashboard

The home page (`src/components/dashboard/FleetDashboard.vue`) calls four read-only aggregate endpoints on
a plain `DashboardController` (`Regira.Entities` pattern: *Cross-entity aggregates & report endpoints* —
outside the entity pipeline, `AsNoTracking()`, no write path):

- `GET /api/dashboard/summary` — fleet/intervention/supplier/invoice KPI counters
- `GET /api/dashboard/spend-by-month?year=` — monthly spend bar chart
- `GET /api/dashboard/interventions-by-status` — status breakdown with progress bars
- `GET /api/dashboard/top-suppliers?take=` — top suppliers by spend, data table

Status badges (vehicle/intervention/invoice) use a single shared `StatusBadge.vue` + `variants.ts` so the
color mapping stays consistent across the dashboard, overview lists and detail forms.

## Verification performed

- `dotnet build` — 0 warnings, 0 errors.
- `npm run build` (`vue-tsc -b && vite build`) — 0 type errors.
- Runtime, against the live API in a browser: vehicle/intervention overview paging, filtering, and status
  badges; an intervention's Details form (vehicle/supplier/invoice pickers, owned `InterventionTypes` m2m
  chip editor); saved the same intervention **twice** in a row (idempotency + m2m re-sync check — chip
  count stayed at 1, not duplicated); confirmed the parent invoice's total recomputed correctly after the
  edit; confirmed `DELETE` on a vehicle with interventions on file returns `409 Conflict` (SQLite
  `Foreign Keys=True` + `OnDelete(Restrict)`) instead of silently orphaning rows.

## Effort tracking

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls (docs/types lookups) | ~23 | ~22 | ~45 |
| Wall-clock time | ~13 min | ~23 min | ~36 min |
| Files written | ~30 | ~65 | ~95 |

- Session token spend (harness context budget): approx. 425,000 tokens consumed of the session's budget
  (measured from the harness's remaining-token counter, start to finish of this build).
- Approximate cost: on the order of a few dollars for the whole build (most tokens were documentation
  reads, not generation) — an exact input/output/cache split isn't exposed to the agent, so this is a
  rough order-of-magnitude estimate, not a billed figure.

Total MCP tool calls include every `get_bootstrap_guide` / `get_package` / `get_package_card` /
`get_section_toc` / `get_type` / `how_to` / `search_docs` call made while building this app — the backend
count is dominated by the `Regira.Entities` instructions/examples/patterns/setup sections (classification,
relationship decision table, seeding, primers/preppers); the front-end count is dominated by
`regira_modules.vue.entities` (slice anatomy, setup, patterns) plus a handful of `get_type` calls to verify
exact component prop contracts (`DateInput`, `NullableCheckBox`, `useAxios`) before using them.

## Credits

Built by **Claude** (model: **Claude Sonnet 5**), running as **Claude Code** (agentic CLI, top-level
session — no sub-agent delegation; the reasoning-effort level for the top-level session isn't a value
exposed to the agent itself, so it isn't claimed here), using the Regira MCP server for all framework
guidance — no prior memory of this or any other Regira project was used, per the task's isolation
requirement.
