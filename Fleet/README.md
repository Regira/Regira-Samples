# FleetOps - Fleet intervention management

A full-stack app for fleet managers: vehicles (cars, vans, trucks, buses, motorcycles, trailers), their
maintenance/repair **interventions**, the **suppliers** that perform them, the editable catalogue of
**intervention types** (and which supplier may perform which), and supplier **invoices** that bundle
interventions. A dashboard layout with KPI tiles, charts, status badges and server-paged data tables.

| Part | Tech | URL |
|---|---|---|
| Back-end `Fleet.Api` | .NET 10, ASP.NET Core controllers, Regira Entities 6.4 (EF Core 10 + SQLite, Mapster), Serilog, OpenAPI + Scalar, Bogus | http://localhost:5831 (Scalar UI: `/scalar`, OpenAPI: `/openapi/v1.json`) |
| Front-end `Fleet.Web` | Vue 3 + TypeScript + Vite 8, Pinia, vue-router 5, Bootstrap 5, `@regira/modules` 6.4 (full reference scaffold, no auth) | http://localhost:5832 (strictPort, proxies `/api` to 5831) |

## Run it

```bash
# API (creates + seeds fleet.db on first start; add -- --ResetDatabase=true to drop & reseed)
cd Fleet.Api
dotnet run --launch-profile http

# SPA (second terminal)
cd Fleet.Web
npm install
npm run dev          # http://localhost:5832
npm run build        # vue-tsc -b + vite build
```

## Domain model

| Entity | Regira registration | Notes |
|---|---|---|
| `InterventionType` | simple `For<T, int, SearchObject>` | code, title, category, km/month interval, standard cost, active flag |
| `Supplier` | simple `For<T, int, SearchObject>` | contact data, rating, active; owns `SupplierInterventionType` join rows (capabilities) via `Related()` |
| `Invoice` | simple `For<T, int, SearchObject>` | supplier's invoice number (unique per supplier), dates, status, VAT; amounts **derived** from its interventions |
| `Vehicle` | complex (typed `VehicleSortBy`) | plate, VIN, make/model/year, type, fuel, status, mileage, department/driver, next service date |
| `Intervention` | complex (typed `InterventionSortBy` + `[Flags] InterventionIncludes`) | vehicle, supplier, optional invoice, status, priority, dates, mileage; owns `InterventionLine` rows (type + cost) via `Related()` |

Free-tier budget: **3 simple / 2 complex** (logged at startup: `Regira.Entities: 3 simple / 2 complex registered -> tier = free`).
The two join/child collections are owned (`Related()`), so they cost no slot and have no controller.

Business rules (server-side, `InterventionPrepper` / invoice prepper):

- an intervention needs at least one intervention type, each type once; `TotalCost` = sum of line costs (server-owned, re-derived on every save, also on a status-only PATCH);
- **a supplier may only perform the intervention types it is assigned** (400 with a field message otherwise);
- only *completed* interventions can be invoiced, and only on an invoice of the same supplier;
- invoice `SubTotal / VatAmount / TotalAmount` are derived from the interventions pointing at it. The intervention owns the
  `InvoiceId` write (one writer); an `EntityReactor` (`InterventionInvoiceReactor`) re-saves the affected invoice(s) after a
  committed change of an intervention's invoice or cost, so totals never go stale;
- deleting a vehicle/supplier/type that is in use answers 409 (`Restrict`, SQLite `Foreign Keys=True`); deleting an invoice
  un-bills its interventions (`SetNull`).

`GET /api/dashboard` is a read-only aggregate controller (KPIs, spend per month/category, top suppliers, status
distributions, upcoming services). Every entity also gets the standard Regira endpoints (`/search` with count,
`GET/POST/PUT/PATCH/DELETE`, `POST /save`, and batch `POST /list|/search` for the complex ones). Keyword search `?q=`
uses normalized content (interventions fold in plate, make/model, supplier, type titles and description).

## Front-end

- Shell: dark sidebar built from `config.json -> navigation` via `useNavigation()` (off-canvas drawer below `lg`),
  top bar with global vehicle search, dashboard home.
- **Dashboard**: 6 KPI tiles (availability, in maintenance, open/urgent interventions, service due, spend YTD,
  unpaid/overdue invoices - each links to the pre-filtered list), monthly spend column chart with hover tooltips and a
  data-table fallback, spend by category, "needs attention" table, intervention/invoice status distributions,
  upcoming services, top suppliers, fleet composition.
- One scaffolded slice per entity (`scaffold.mjs --no-auth`, `--rel`, `--owns`, `--picker`): server-paged overviews with
  count, inline + advanced filters (incl. sort order), status badges (icon + label + tint), due-date badges.
- Detail pages: vehicle (tabs: details / maintenance history), supplier (capability chips with `_deleted` undo),
  intervention (work order + editable line table with type picker, cost pre-filled from the type's standard cost,
  capability warning), invoice (amount tiles; tab to add/remove the supplier's billable completed interventions),
  intervention type.
- "Plan intervention" from a vehicle pre-selects that vehicle (`?vehicleId=`).
- Declared deviation: the supplier list shows capability *codes* as compact chips (the full chips with quick-edit
  buttons are on the supplier form).

## Seed data

Seeded through the `IEntityService` implementations (preppers, normalizers and the invoice reactor all run), with
Bogus (`nl_BE`, fixed seed): 25 intervention types, 31 suppliers (dealers, independents, tire/body/glass/truck/EV
specialists, inspection stations, cleaning) with realistic capability sets, 160 vehicles (correlated make/model/type/fuel
tuples, mileage by age and usage), **500 interventions** (the primary entity; statuses follow their dates, suppliers are
always capable, brand dealers service their own make) and ~300 invoices (monthly batches per supplier, payment
status by age).

## Build status

- `dotnet build`: succeeded, 0 warnings, 0 errors.
- `npm run build` (vue-tsc -b + vite build): succeeded.
- Verified at runtime: create -> update -> update again -> re-read (owned lines keep their ids), status-only PATCH keeps
  `TotalCost`, 400 on capability/validation errors, 409 on deleting a referenced vehicle, invoice reactor on link/unlink,
  UI saves twice (intervention lines, supplier capability chips), filters restored from the URL, 375 px viewport without
  horizontal scroll.

## Effort (tracked by the building agent)

| Phase | Regira MCP calls | Wall-clock | Tokens (approx., harness token counter) |
|---|---|---|---|
| Back-end | 23 | ~14 min (12:19 - 12:33) | ~254k |
| Front-end | 16 | ~19 min (12:33 - 12:52) | ~231k |
| **Total** | **39** | **~33 min** | **~485k** |

Cost: not measurable from inside the session; the token figure above is the harness counter (mostly cached
input/context), so any dollar figure would be a guess - roughly a few US dollars at Opus-class API list prices.

## Credits

Built by **Claude** (Anthropic), model **Claude Opus 5.5**, running as a **Claude Code** sub-agent (general-purpose agent type)
with **low reasoning effort**, following the Regira MCP documentation server's golden path
(`get_bootstrap_guide` -> Regira.Setup project.setup -> Regira.Entities guides -> front-end guide + scaffolder).
