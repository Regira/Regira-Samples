# Fleet.API

A REST API for managing **fleet maintenance interventions**, built on the
[Regira](https://regira.com) entity framework packages.

Vehicles (cars, vans, trucks, …) have recurring maintenance needs. Those needs are
fulfilled by **interventions** — concrete maintenance operations of a given
**intervention type**, performed by a **supplier**, and (once completed) billed
through an **invoice**.

- **Intervention types** are an editable catalog (oil change, brake inspection, …).
- **Suppliers** are assigned the intervention types they are *able to perform*.
- **Vehicles** are assigned the intervention types they are *allowed to undergo*.

---

## Domain model

```
InterventionType ──< SupplierInterventionType >── Supplier        (capabilities, M:N)
InterventionType ──< VehicleInterventionType  >── Vehicle         (allowed types, M:N)

Vehicle ─────────────< Intervention >───────────── Supplier       (who performs it)
InterventionType ────< Intervention                               (what is performed)
Invoice ─────────────< Intervention (optional InvoiceId)          (how it is billed)

Supplier ────────────< Invoice                                    (who sends it)
```

| Entity | Key fields | Relationships |
|--------|-----------|---------------|
| **InterventionType** | `Code`, `Title`, `EstimatedDurationMinutes` | M:N Vehicle, M:N Supplier |
| **Vehicle** | `LicensePlate`, `Brand`, `Model`, `VehicleType`, `Mileage` | M:N allowed `InterventionType`s |
| **Supplier** | `Title`, `Email`, `VatNumber`, `City` | M:N `InterventionType` capabilities; 1:N Invoices |
| **Intervention** | `Status`, `ScheduledDate`, `Cost`, `MileageAtService` | FK Vehicle, Supplier, InterventionType; optional FK Invoice |
| **Invoice** | `InvoiceNumber`, `IssueDate`, `DueDate`, `Status`, `TotalAmount` | FK Supplier; 1:N Interventions |

The two many-to-many relations use explicit join entities (`VehicleInterventionType`,
`SupplierInterventionType`) that are *owned* by their parent and synchronized through
the Regira `Related()` mechanism.

---

## Tech stack

| Concern | Choice |
|---------|--------|
| Framework | .NET 10 / ASP.NET Core (controllers) |
| CRUD framework | `Regira.Entities` v6 (`Regira.Entities.Web`) |
| ORM | EF Core 10 + **SQLite** (`fleet.db`, recreated on run) |
| DTO mapping | Mapster (`Regira.Entities.Mapping.Mapster`) |
| API docs | OpenAPI + **Scalar** UI (`/scalar`) |
| Logging | Serilog (console + rolling file) |
| Sample data | [Bogus](https://github.com/bchavez/Bogus) |

---

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Access to the Regira NuGet feed. It is already declared in [`NuGet.Config`](../NuGet.Config)
  (`https://packages.regira.com/v3/index.json`) alongside nuget.org.

### Run

```bash
cd Fleet.API
dotnet run
```

On startup the app:
1. creates the SQLite database (`Database.EnsureCreated()`),
2. seeds a coherent sample data set (see below),
3. serves the API and opens the **Scalar** UI.

| Surface | URL |
|---------|-----|
| Scalar UI | `https://localhost:7048/scalar` |
| OpenAPI JSON | `https://localhost:7048/openapi/v1.json` |

> No license key is required: the Regira free tier (5 simple / 2 complex entity
> registrations) covers this project exactly (3 simple + 2 complex). To raise the
> limits, set `Regira:LicenseKey` in `appsettings.json`.

---

## Seeding

[`Data/FleetDbSeeder.cs`](Data/FleetDbSeeder.cs) generates sample data **through the
`IEntityService<TEntity,TKey>` implementations** (not raw `DbContext` inserts), so the
full Regira write pipeline — preppers, normalizers, primers, `Related()` sync — runs
exactly as it would for real API calls. Bogus produces the fake values, with a fixed
seed for reproducibility. Seeding is **idempotent** (skipped when data already exists).

Generated volume:

| Entity | Count |
|--------|------:|
| Intervention types | 12 |
| Suppliers (with capabilities) | 15 |
| Vehicles (with allowed types) | 40 |
| **Interventions** | **500** |
| Invoices (bundling completed work) | ~60 |

Coherence rules applied while seeding: an intervention's type is drawn from the
vehicle's *allowed* types, its supplier from those *capable* of that type, and each
invoice bundles a supplier's completed interventions with `TotalAmount` equal to the
sum of the bundled costs.

---

## API endpoints

Every entity is exposed through a controller deriving from Regira's
`EntityControllerBase`. Base routes:

| Resource | Route | Registration |
|----------|-------|--------------|
| Intervention types | `/api/intervention-types` | simple |
| Vehicles | `/api/vehicles` | simple |
| Suppliers | `/api/suppliers` | simple |
| Interventions | `/api/interventions` | **complex** |
| Invoices | `/api/invoices` | **complex** |

Endpoints per resource:

| Method | Route | Action | Availability |
|--------|-------|--------|--------------|
| `GET` | `/{id}` | Details | all |
| `GET` | `/` | List (`?q=` keyword search) | all |
| `POST` | `/list` | List (search object in body) | complex only |
| `GET` / `POST` | `/search` | Search **with total count** | complex only |
| `POST` | `/` | Create | all |
| `POST` | `/save` | Upsert | all |
| `PUT` | `/{id}` | Replace (full update) | all |
| `PATCH` | `/{id}` | Partial update (JSON Merge Patch) | all |
| `DELETE` | `/{id}` | Delete | all |

### Filtering, sorting & includes

Complex resources accept rich query parameters via their search object, `sortBy`, and
`includes` flags. Examples:

```http
# Completed interventions, most expensive first, with the billing invoice loaded
GET /api/interventions/search?Status=Completed&SortBy=CostDesc&includes=Invoice&pageSize=20

# Unbilled interventions for vehicles 3 and 7
GET /api/interventions/search?VehicleId=3&VehicleId=7&HasInvoice=false

# Overdue invoices for a supplier, newest issue date first
GET /api/invoices/search?SupplierId=13&Status=Overdue&SortBy=IssueDateDesc&includes=Interventions

# Vehicles allowed to undergo an engine repair (type 8)
GET /api/vehicles?InterventionTypeId=8
```

`Vehicle` and `Supplier` always eager-load their assigned intervention types.
`Intervention` always loads its vehicle, supplier and type; the invoice is loaded on
demand via `includes=Invoice`.

Paging defaults: 25 items per page, max 200 (override with `page` / `pageSize`).

---

## Project structure

```
Fleet.API/
├── Controllers/                 # EntityControllerBase-derived controllers (1 per resource)
├── Data/
│   ├── FleetDbContext.cs        # DbSets + relationship configuration
│   └── FleetDbSeeder.cs         # Bogus seeding via IEntityService
├── Entities/                    # Per-entity folders
│   ├── InterventionTypes/       # entity, search object, DTOs, service config
│   ├── Vehicles/                # + VehicleInterventionType join, VehicleType enum
│   ├── Suppliers/               # + SupplierInterventionType join
│   ├── Interventions/           # + Status/SortBy/Includes enums (complex)
│   └── Invoices/                # + Status/SortBy/Includes enums (complex)
├── Extensions/
│   └── ServiceCollectionExtensions.cs   # UseEntities() + per-entity .For<>() wiring
├── Program.cs                   # thin host: DI, interceptors, EnsureCreated, seed
└── appsettings.json
```

Each entity is registered with `.For<>()` in its own `*ServiceConfiguration.cs`
extension method, configuring filtering, sorting, includes, DTO mapping and (for the
M:N relations) `Related()` collection synchronization.

---

## Credits

Designed and implemented by **Claude** (Anthropic), running as **Claude Code** — the
primary interactive coding agent (model **Claude Opus 4.8**). The build used the
**Regira MCP server** (`https://mcp.regira.com/mcp`) as the single source of truth for
package selection, setup conventions, API signatures and namespaces; no other projects
were referenced.

> Agent type: the general-purpose **Claude Code** agent (not a specialized sub-agent).
> A dedicated `entities-agent` was available for Regira entity work but was not used —
> the implementation was done inline by the main agent.

Built with the [Regira](https://regira.com) packages · `Regira.Entities` v6.
