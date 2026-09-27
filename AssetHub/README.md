# AssetHub

Company asset management: laptops, monitors, phones, tools and more, organised by category and a
color-coded status. Assets are assigned to employees with a complete hand-over history, and carry
warranties, maintenance records and file attachments. Administrators manage the reference data
(categories, statuses, locations, suppliers).

| Part | Tech | URL |
|---|---|---|
| `AssetHub.Api` | .NET 10 Web API, Regira Entities 6.4, EF Core 10 + SQLite, ASP.NET Identity + JWT, Serilog, OpenAPI + Scalar | http://localhost:5801 (`/scalar`, `/openapi/v1.json`) |
| `AssetHub.Web` | Vue 3 + Vite 8 + TypeScript 6, `@regira/modules` 6.4 (full scaffold), Bootstrap 5 | http://localhost:5802 (strict port, proxies `/api` to 5801) |

## Running it

```bash
# API: creates + seeds assethub.db on first start (delete the file, or pass --ResetDatabase=true, to reseed)
cd AssetHub.Api
dotnet run --launch-profile http

# SPA
cd AssetHub.Web
npm install
npm run dev        # http://localhost:5802
npm run build      # vue-tsc -b + vite build
```

### Seeded accounts (development only)

| User | Password | Role | Can |
|---|---|---|---|
| `admin@assethub.local` | `AssetHub-Admin1` | Admin | everything, incl. categories / statuses / locations / suppliers and creating user accounts |
| `manager@assethub.local` | `AssetHub-Manager1` | Manager | assets, assignments, attachments, employees (reference data read-only) |
| `viewer@assethub.local` | `AssetHub-Viewer1` | (none) | read-only |

They are configured under `Seeding:Users` in `AssetHub.Api/appsettings.json`. Replace the JWT secret and these
accounts before deploying anywhere.

## Features

- **Inventory overview**: a compact, sortable, filterable table or a card grid (toggle, remembered per browser),
  with color-coded status chips for one-click filtering and server-side paging (`/search` + count).
- **Color-coded statuses**: administrators define statuses with a color and a *kind* (Available, Assigned,
  Maintenance, Inactive). The kind drives the workflow; the color drives every badge, chip, card border and the
  dashboard's status bar.
- **Assignments**: assign / reassign / return from the asset's *Assignment* tab. Every hand-over is a history
  row (assigned on, returned on, notes). Assigning sets the first "Assigned" status, returning the first
  "Available" one; assets in repair or inactive cannot be assigned. The employee page lists current assets and the
  full history.
- **Warranties and maintenance**: editable tables inside the asset form (add, edit, mark for deletion, undo),
  saved together with the asset. Warranty coverage shows as Active / Expiring / Expired; overdue maintenance is
  highlighted.
- **Attachments**: drag & drop upload, rename, reorder, download and remove files per asset.
- **Dashboard**: totals and value, utilisation, expiring warranties (60 days), maintenance due (30 days),
  assets by status / category / location and recent assignments; every tile links into a filtered overview.
- **Advanced asset filters**: keywords (tag, name, serial, manufacturer, model), category, status, status kind,
  location, supplier, holder, assigned yes/no, under warranty, has files, warranty expiring before, maintenance due
  before, and sort order.
- **Roles**: the API enforces write tiers with a global write-authorization filter; the SPA mirrors them
  (`src/infrastructure/access.ts`) and renders read-only views where the user may not write.
- Full account surface (sign in, forgot / reset / change password, sign out), responsive down to phone width.

## Back-end design

### Entities and the free-tier budget (5 simple + 2 complex)

| Entity | Registration | Notes |
|---|---|---|
| `Category` | simple | icon hint + lifespan; reference data (not archivable, `Restrict` -> 409 while in use) |
| `AssetStatus` | simple | title, color, kind, sort order |
| `Location` | simple | |
| `Supplier` | simple | |
| `AssetAttachment` | simple | per-owner attachment join (`HasAttachments`) |
| `Employee` | complex | typed sort + includes (`Assignments`), `CurrentAssetCount` via a processor |
| `Asset` | complex | typed sort + includes (`Assignments`, `Warranties`, `MaintenanceRecords`, `Attachments`) |
| `AssetAssignment`, `Warranty`, `MaintenanceRecord` | owned via `e.Related()` | no slot, no controller |

The API logs `Regira.Entities: 5 simple / 2 complex registered -> tier = free` at startup.

### Notable decisions

- **One writer per save path.** `AssetInputDto` does not carry `Assignments`, `CurrentEmployeeId` or `AssignedOn`.
  They are written only by `AssetWorkflowService` (`POST api/assets/{id}/assign` and `/return`), which writes
  through `IEntityService` with a scoped `WorkflowContext.IsTrustedWriter` flag. `AssetPrepper` restores those
  fields on every other write, mints the asset tag (`AST-00001`) when left empty and validates references
  (400 instead of an FK 409/500).
- Warranties and maintenance records travel on the asset input DTO (nullable, uninitialised: `null` = untouched,
  `[]` = delete all) and are synced by `Related()`.
- Reference data is not `IArchivable`; deleting a category / status / location / supplier / employee that is in
  use answers 409. Employees are deactivated (`IsActive`) instead.
- The dashboard (`GET api/dashboard`) is a read-only aggregate endpoint outside the entity pipeline.
- `DateOnly` for calendar dates (purchase, warranty and maintenance dates, hire date), UTC `DateTime` for
  hand-over timestamps.
- Seeding (`Data/Seeding/DataSeeder.cs`) goes through the `IEntityService` implementations with Bogus (fixed seed):
  10 categories, 6 statuses, 8 locations, 24 suppliers, 150 employees, **500 assets** with warranties, maintenance
  records and a sequential assignment history, and ~106 attachment files.

### Project layout

```
AssetHub.Api/
  Controllers/        entity controllers, attachment controller, workflow + dashboard, auth controllers
  Data/               AppDbContext (IdentityDbContext), Seeding/DataSeeder.cs
  Entities/<Name>/    model, DTOs, search object, sort/includes enums, preppers, query builders, processors
  Extensions/         AddEntityServices() with the budget tally
  Infrastructure/     HostingExtensions (services + pipeline), Security (roles, write filter, dev mailer)
  Services/           AssetWorkflowService, WorkflowContext
AssetHub.Web/
  src/entities/<slice>/   one scaffolded slice per entity (assets includes warranties/ and maintenance-records/)
  src/entities/entity-attachments/  shared file slice
  src/components/     layout, navigation, StatusBadge
  src/views/          dashboard (HomeView), account and error views
  src/infrastructure/ roles, access mirror, user plugin
```

## Build status

- `dotnet build`: 0 warnings, 0 errors. No vulnerable packages (`dotnet list package --vulnerable --include-transitive`).
- `npm run build` (`vue-tsc -b` + `vite build`): green.
- Verified at runtime: create, update twice, owned rows surviving repeated saves, status-only PATCH, assign /
  reassign / return, role gates (viewer 403, manager 403 on reference data, admin 409 on a category in use),
  attachment upload and download through the dev proxy, and every main view at a 375px viewport without
  horizontal scroll.

## Build statistics

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls | 23 | 13 | **36** |
| Wall-clock time | ~14 min (12:19-12:30, plus ~4 min of fixes later) | ~20 min (12:30-12:50) | **~35 min** |
| Tokens (session budget consumed) | ~235k | ~230k | **~465k** |

Tokens are the session's consumed token budget (input incl. cached context, plus output), measured from the
budget counter at the phase boundaries. A monetary cost cannot be reported: the session ran on a subscription
plan without per-request metering.

## Credits

Built by **Claude** (Anthropic), model **Claude Opus 5.5**, running as a **Claude Code sub-agent**
(general-purpose agent type) with **low reasoning effort**, following the Regira MCP golden path
(`get_bootstrap_guide` -> package cards -> heading-scoped guides -> generators).
