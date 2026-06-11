# Fleet.API

A REST API for managing **fleet maintenance interventions**, built on the [Regira](https://regira.com) packages
(the `Regira.Entities` framework on top of EF Core).

Fleet vehicles (cars, vans, trucks, …) have various maintenance needs. Those needs are carried out as
**interventions** of a given **intervention type**, performed by **suppliers** who send **invoices** for the work.
Intervention types are editable; suppliers can be assigned the types they are *able to perform*, and vehicles can be
assigned the types they are *allowed to undergo*.

---

## Domain model

| Entity | Description | Key relationships |
|--------|-------------|-------------------|
| **Vehicle** | A fleet vehicle (car/van/truck/…) with a license plate, brand, model, type, year and mileage. | M:N `AllowedInterventionTypes`; 1:N `Interventions` |
| **InterventionType** | Editable catalogue of maintenance operations (oil change, brake service, …) with a code, default km interval and estimated duration. | referenced by suppliers, vehicles & interventions |
| **Supplier** | A garage / service provider that performs interventions and issues invoices. | M:N `Capabilities`; 1:N `Invoices`, `Interventions` |
| **Intervention** | A maintenance operation on one vehicle, of one type, by one supplier, optionally billed on an invoice. | FKs to Vehicle, InterventionType, Supplier, Invoice |
| **Invoice** | A supplier invoice covering one or more interventions; its amount is the sum of the billed interventions. | FK to Supplier; 1:N `Interventions` |

The two many-to-many relationships use explicit join entities managed through the parent via `e.Related(...)`:

- `VehicleInterventionType` — the types a vehicle may undergo.
- `SupplierInterventionType` — the types a supplier can perform.

```
Vehicle ──< VehicleInterventionType >── InterventionType ──< SupplierInterventionType >── Supplier
   │                                          │                                            │
   └───────────< Intervention >───────────────┘                                            │
                      │ │                                                                   │
                      │ └──────────────── Supplier ────────────────────────────────────────┘
                      └──────────────── Invoice >──── Supplier
```

---

## Tech stack

- **.NET 10** / **C# 14**, ASP.NET Core Web API (controllers).
- **Regira.Entities** framework — generic CRUD services, query builders, DTO mapping, generated endpoints:
  - `Regira.Entities.DependencyInjection` — `UseEntities()` / `.For<>()` DI builder
  - `Regira.Entities.EFcore` — EF Core repository, primers, normalizers, interceptors
  - `Regira.Entities.Web` — `EntityControllerBase` HTTP endpoints
  - `Regira.Entities.Mapping.Mapster` — entity ⇄ DTO mapping (default)
- **EF Core 10 + SQLite** — disposable local database (`fleet.db`), created with `EnsureCreated()`.
- **Bogus** — reproducible fake data for seeding.
- **Serilog** — console + rolling file logging.
- **OpenAPI + Scalar** — API documentation UI.

---

## Running

```bash
cd Fleet.API
dotnet run
```

On first start the app creates `fleet.db` and seeds the sample dataset, then serves:

- **Scalar API reference UI:** `/scalar`
- **OpenAPI document:** `/openapi/v1.json`

The `NuGet.Config` in this folder adds the Regira package feed
(`https://packages.regira.com/v3/index.json`) next to nuget.org.

> No license key is required: the project runs on the Regira Entities **free tier**
> (5 simple + 2 complex entity registrations). To raise the limits, set `Regira:LicenseKey`
> in `appsettings.json` and call `services.UseRegira(configuration["Regira:LicenseKey"])` before `UseEntities()`.

---

## API endpoints

Each entity is exposed by an `EntityControllerBase` controller. Common routes (provided out of the box):

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/{entity}` | List (paged) |
| `GET` | `/{entity}/{id}` | Details |
| `GET` / `POST` | `/{entity}/search` | Search **+ count** *(complex entities only — see below)* |
| `POST` | `/{entity}` | Create |
| `POST` | `/{entity}/save` | Upsert |
| `PUT` | `/{entity}/{id}` | Full update |
| `PATCH` | `/{entity}/{id}` | Partial update (JSON Merge Patch) |
| `DELETE` | `/{entity}/{id}` | Delete |

Routes: `/vehicles`, `/intervention-types`, `/suppliers`, `/invoices`, `/interventions`.

### Filtering, sorting & includes

To stay within the free tier, the two entities with the richest query needs are registered as
**complex** (custom `SearchObject` + `SortBy` + `Includes`); the rest are **simple** (default
`SearchObject`, so they still support `id`/`ids` and `q` full-text search plus default includes).

**Vehicles** (`/vehicles/search`)
- `vehicleType`, `brand`, `interventionTypeId`, `minYear`, `maxYear`, `q`
- `includes`: `AllowedInterventionTypes` (1), `Interventions` (2), `All` (3)

**Interventions** (`/interventions/search`)
- `vehicleId`, `supplierId`, `interventionTypeId`, `invoiceId`, `status`, `isInvoiced`,
  `minScheduledDate`, `maxScheduledDate`, `minCost`, `maxCost`, `q`
- `sortBy`: `ScheduledDate`, `ScheduledDateDesc`, `Cost`, `CostDesc`, `Status`
- `includes`: `Vehicle` (1), `InterventionType` (2), `Supplier` (4), `Invoice` (8), `All` (15)

Example:

```
GET /interventions/search?status=Completed&sortBy=CostDesc&includes=15&pageSize=10
GET /vehicles/search?vehicleType=Truck&includes=1
```

> Simple entities (`/suppliers`, `/invoices`, `/intervention-types`) expose `GET /{entity}` (list) and
> `GET /{entity}/{id}`; they do not expose the `/search` route, but support `?q=` full-text search on the list.

---

## Sample data

Seeded once into an empty database by [`Infrastructure/DataSeeder.cs`](Infrastructure/DataSeeder.cs),
**through the `IEntityService<>` implementations** (so preppers, normalizers and primers run exactly as via the API):

| Entity | Count |
|--------|------:|
| Intervention types | 12 |
| Suppliers | 15 (each can perform a random 3–8 types) |
| Vehicles | 45 (each may undergo a random 4–9 types) |
| Invoices | 90 (4–7 per supplier; amount = sum of billed interventions) |
| **Interventions** | **520** |

Each intervention is generated consistently: a random vehicle, a type that is **both** allowed for that vehicle
**and** performable by a supplier, a capable supplier, a weighted status (≈70 % completed), a realistic cost
per type (with a heavier multiplier for trucks/buses), and — for most completed work — a link to one of the
supplier's invoices. The Bogus randomizer uses a fixed seed for reproducible data.

---

## Project layout

```
Fleet.API/
├── Program.cs                       # thin host: JSON, DbContext + interceptors, entity services, seed, OpenAPI/Scalar
├── NuGet.Config                     # nuget.org + Regira feed
├── appsettings.json                 # connection string + Serilog
├── Data/
│   └── FleetDbContext.cs            # DbSets, relationships, decimal precision
├── Infrastructure/
│   ├── ServiceCollectionExtensions.cs   # UseEntities() + per-entity registrations
│   └── DataSeeder.cs                # IEntityService-based seeding (Bogus)
├── Entities/
│   ├── Common/Enums.cs              # VehicleType, InterventionStatus, InvoiceStatus
│   ├── Vehicles/                    # entity, join, search/includes, DTOs, query builder, config (complex)
│   ├── Interventions/               # entity, search/sortby/includes, DTOs, query builder, config (complex)
│   ├── InterventionTypes/           # entity, DTOs, config (simple)
│   ├── Suppliers/                   # entity, join, DTOs, config (simple)
│   └── Invoices/                    # entity, DTOs, config (simple)
└── Controllers/                     # one EntityControllerBase per entity
```

Per-entity folders follow the Regira convention: entity + `SearchObject`/`SortBy`/`Includes` +
DTOs + query builder + a `.For<>()` configuration extension method.

---

## Credits

- **Authored by:** Claude (Anthropic) — model **Claude Opus 4.8** — running in **Claude Code**.
- **Agent type:** the built-in general-purpose `claude` agent (no sub-agents were spawned).
- **Knowledge source:** built exclusively from the **Regira MCP server** (`https://mcp.regira.com/mcp`)
  bootstrap guide, package documentation, examples and signatures — no external references.
- **Frameworks:** [Regira](https://regira.com) packages, EF Core, ASP.NET Core, Bogus, Serilog, Scalar.
