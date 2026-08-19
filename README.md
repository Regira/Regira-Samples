# Regira — Sample Applications

Nine self-contained, full-stack sample applications built on the [Regira](https://regira.com) framework:
an **ASP.NET Core 10 API** on [Regira Entities](https://regira.com/entities) plus a **Vue 3 SPA** on
[`@regira/modules`](https://www.npmjs.com/package/@regira/modules) — each a complete app with a real
domain, ~500 seeded rows and a UI you can click through.

What makes them unusual: every sample was **generated end-to-end by an AI agent** — no manual scaffolding,
no boilerplate written by hand — driven exclusively by the **Regira MCP server**
(`https://mcp.regira.com/mcp`) as the single source of truth for package selection, setup and APIs.
Each sample carries its own `README.md` covering what it does, how to run it, and the design decisions
(and deviations) the agent made along the way.

| Site | URL |
|---|---|
| 🏢 Regira | [regira.com](https://www.regira.com) |
| 📚 Regira Entities | [Regira Entities framework](https://regira.com/entities) |
| 📦 Package sources | [Regira/Regira-Packages](https://github.com/Regira/Regira-Packages) |
| 🏭 Production sample | [Regira/Regira-PIM-Backend](https://github.com/Regira/Regira-PIM-Backend) |

> ✅ **No license key required.** Every sample stays inside the Regira Entities **free tier**
> (≤ 5 simple + 2 complex entity registrations) — several sit at exactly 7/7.
> To raise the limits, set `Regira:LicenseKey` in your configuration.

---

## The samples

| Sample | Domain | Entity budget | Primary seed | Front-end character |
|---|---|---|---|---|
| [AssetHub](AssetHub/) | Company asset/inventory management — assets, assignments, warranties, maintenance | 5 simple / 2 complex | ~500 assets | Full admin scaffold, tabbed asset form |
| [Blog](Blog/) | Blog publishing — posts, categories, tags | 2 simple / 1 complex | 520 posts | Public editorial site **and** `/admin` CRUD in one SPA |
| [EventPlanner](EventPlanner/) | Event management — events, venues, sessions, speakers, registrations | 5 simple / 2 complex | 520 registrations | Banner-card grids, agenda rows, seat-fill bars |
| [Fleet](Fleet/) | Fleet maintenance — vehicles, suppliers, interventions, invoices | 3 simple / 2 complex | 500 interventions | KPI dashboard over aggregate endpoints |
| [HelpDesk](HelpDesk/) | Support ticket desk — tickets, comments, attachments, teams | 5 simple / 2 complex | 500 tickets | Kanban board, conversation threads, file uploads |
| [QCredits](QCredits/) | Employee training credits — requests, approvals, balances | 4 simple / 1 complex | 500 requests | Approval workflow + balances dashboard |
| [RoomPlanner](RoomPlanner/) | Meeting-room reservations — buildings, floors, rooms, attendees | 3 simple / 2 complex | 500 reservations | Custom day-timeline calendar |
| [ShopMate](ShopMate/) | Shopping lists — lists, articles, hierarchical categories | 2 simple / 1 complex | 500 articles | Mobile-first: bottom tab bar, swipe actions |
| [Webshop](Webshop/) | E-commerce storefront — catalog, cart, guest checkout | 1 simple / 2 complex | 500 products | Headless tier: hand-built storefront, cart & checkout |

None of the samples use authentication — they are demo/internal-tool scoped, built on the anonymous
back-end template and the SPA's no-auth scaffold. Each sample's README explains that choice and the
documented upgrade path.

### What each sample shows

Pick the one closest to the pattern you need:

| Framework capability | Where to look |
|---|---|
| Owned collections & m2m joins via `e.Related()` (no registration slot) | every sample |
| Computed read-side fields via `IEntityProcessor` | [Blog](Blog/) (post counts), [EventPlanner](EventPlanner/) (seats taken), [AssetHub](AssetHub/) (current holder) |
| Write-side hooks via preppers | [Fleet](Fleet/) (invoice totals), [AssetHub](AssetHub/) (one active assignment per asset) |
| Server-owned fields & generated codes via primers | [QCredits](QCredits/) (approval status restore), [Fleet](Fleet/) (invoice codes), [AssetHub](AssetHub/) (asset codes) |
| Business rules via `EntityWrappingServiceBase` | [RoomPlanner](RoomPlanner/) (auto-approve reservations), [Webshop](Webshop/) (order validation, price-tamper guard) |
| Cross-entity aggregate / report endpoints (outside the entity pipeline) | [Fleet](Fleet/) (`/dashboard/*`), [QCredits](QCredits/) (`/balances`) |
| File attachments (`WithAttachments` / `HasAttachments`) | [HelpDesk](HelpDesk/) |
| Soft delete (`IArchivable`) — and when *not* to use it | [AssetHub](AssetHub/), [ShopMate](ShopMate/) |
| Self-referencing many-to-many hierarchy | [ShopMate](ShopMate/) (categories with multiple parents) |
| Free-tier overflow remedy (role-discriminated party) | [HelpDesk](HelpDesk/) (`Person` with a `Role`, instead of Customer/Agent/Admin) |
| Headless front-end (framework HTTP client, bespoke UI) | [Webshop](Webshop/) |

---

## Stack (shared across all samples)

| Layer | Technology |
|---|---|
| API | .NET 10 / ASP.NET Core, [`Regira.Entities.Web`](https://www.nuget.org/packages/Regira.Entities.Web) 6.1.2 |
| ORM | [Entity Framework Core](https://www.nuget.org/packages/microsoft.entityframeworkcore/) 10 (SQLite) |
| DTO mapping | [Mapster](https://www.nuget.org/packages/Mapster/) via `Regira.Entities.Mapping.Mapster` |
| API docs | OpenAPI + [Scalar](https://www.nuget.org/packages/Scalar.AspNetCore) (`/scalar`) |
| Seed data | [Bogus](https://www.nuget.org/packages/Bogus), seeded through `IEntityService<>` |
| Logging | [Serilog](https://www.nuget.org/packages/Serilog) |
| SPA | Vue 3.5 + TypeScript + Vite, Pinia, vue-router, Bootstrap 5 |
| SPA framework | [`@regira/modules`](https://www.npmjs.com/package/@regira/modules) 6.1.2 (entities client, UI kit, scaffolder) |

Everything installs straight from nuget.org / npmjs.com — no custom feed or private registry needed.

---

## Running a sample

Each sample is independent: start its API, then its SPA. The API creates and seeds its SQLite database on
first run (`Database.EnsureCreated()`); delete the `.db` file to force a reseed.

```bash
# 1. API — the port comes from launchSettings.json, Scalar docs open at /scalar
cd Fleet/backend/Fleet.Api
dotnet run

# 2. SPA (second terminal)
cd Fleet/frontend
npm install
npm run dev
```

Paths and ports per sample:

| Sample | API project | API port | SPA project | SPA port |
|---|---|---|---|---|
| AssetHub | `AssetHub/backend` | 6100 | `AssetHub/frontend` | 6101 <sup>†</sup> |
| Blog | `Blog/backend/Blog.Api` | 6110 | `Blog/frontend/blog-spa` | 6111 |
| EventPlanner | `EventPlanner/backend` | 6120 | `EventPlanner/frontend` | 6121 |
| Fleet | `Fleet/backend/Fleet.Api` | 6130 | `Fleet/frontend` | 6131 |
| HelpDesk | `HelpDesk/HelpDesk.API` | 6140 | `HelpDesk/HelpDesk.SPA` | 6141 |
| QCredits | `QCredits/backend/QCredits.Api` | 6150 | `QCredits/frontend` | 6151 |
| RoomPlanner | `RoomPlanner/backend/RoomPlanner.Api` | 6160 | `RoomPlanner/frontend` | 6161 |
| ShopMate | `ShopMate/backend/ShopMate.Api` | 6170 | `ShopMate/frontend` | 6171 |
| Webshop | `Webshop/backend/Webshop.Api` | 6180 | `Webshop/frontend` | 6181 <sup>‡</sup> |

<sup>†</sup> AssetHub's Vite config has no fixed port — start it with `npm run dev -- --port 6101`.
<sup>‡</sup> Webshop's SPA proxies `/api` through Vite; the others call the API origin directly, with CORS
enabled server-side.

### Solutions

`Regira-Samples.slnx` opens all nine APIs at once (`dotnet build Regira-Samples.slnx` builds them all).
Each sample also has its own solution next to its API — `AssetHub/AssetHub.slnx`, `Fleet/backend/Fleet.slnx`,
`HelpDesk/HelpDesk.slnx`, … — if you want to work on just one.

---

## Repository layout

```
Regira-Samples/
├── AssetHub/            README.md + backend/ + frontend/
├── Blog/                …
├── EventPlanner/        …
├── Fleet/               …
├── HelpDesk/            …
├── QCredits/            …
├── RoomPlanner/         …
├── ShopMate/            …
├── Webshop/             …
├── .mcp.json            Regira MCP server registration (for agents working in this repo)
└── Regira-Samples.slnx  all nine APIs in one solution
```

A back end follows the framework's own layout — one folder per entity holding the model, its DTOs, search
object and service configuration, plus thin controllers and a Bogus-driven seeder:

```
Fleet/backend/Fleet.Api/
├── Entities/Interventions/    Intervention.cs, …Dto.cs, …InputDto.cs, …SearchObject.cs,
│                              …SortBy.cs, …Includes.cs, …ServiceConfiguration.cs
├── Controllers/               EntityControllerBase<> subclasses + a DashboardController
├── Data/                      DbContext + seeder
└── Extensions/                AddEntityServices() — the DI wiring
```

A front end is a set of generated-then-customized entity *slices*:

```
Fleet/frontend/src/entities/interventions/
├── config/        slice config (title, routes, navigation)
├── data/          Entity.ts, EntityService.ts, store.ts
├── overview/      Overview.vue, List.vue, ListItem.vue
├── details/       Details.vue, Form.vue, FormModalButton.vue
├── filter/        SearchObject.ts, Filter*.vue
└── selecting/     Autocomplete.vue, InputSelector.vue
```

---

## How Regira Entities is used

A single `UseEntities()` call sets up global defaults and the Mapster mapping layer; each entity then
registers itself through its own extension method:

```csharp
// Fleet/backend/Fleet.Api/Extensions/ServiceCollectionExtensions.cs
services
    .UseEntities<FleetDbContext>(options =>
    {
        options.UseDefaults();
        options.UseMapsterMapping();
    })
    .AddVehicles()
    .AddSuppliers()
    .AddInterventionTypes()
    .AddInterventions()
    .AddInvoices();
```

Inside each domain folder, `.For<>()` wires the filter, sorting, includes, owned collections and write
hooks in one place:

```csharp
// Fleet — InterventionServiceConfiguration.cs
services.For<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes>(e =>
{
    e.Filter((query, so) =>
    {
        if (so?.VehicleId?.Any() == true) query = query.Where(x => so.VehicleId.Contains(x.VehicleId));
        if (so?.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
        if (so?.HasInvoice != null) query = so.HasInvoice.Value
            ? query.Where(x => x.InvoiceId != null)
            : query.Where(x => x.InvoiceId == null);
        return query;
    });
    e.SortBy((query, sortBy) => sortBy switch
    {
        InterventionSortBy.CostDesc => query.OrderOrThenByDescending(x => x.Cost),
        _ => query.OrderOrThenByDescending(x => x.ScheduledDate)
    });
    e.Includes((query, includes) =>
    {
        query = query.Include(x => x.Vehicle!).Include(x => x.Supplier!).Include(x => x.Invoice!);
        if (includes?.HasFlag(InterventionIncludes.InterventionTypes) == true)
            query = query.Include(x => x.InterventionTypes!).ThenInclude(x => x.InterventionType);
        return query;
    });
    e.Related(x => x.InterventionTypes);              // owned m2m — no registration slot, no controller
    e.AddPrepper<InterventionInvoiceTotalPrepper>();  // recompute the parent invoice's total on save
});
```

Controllers inherit `EntityControllerBase` and expose the full CRUD + search surface with no extra code:

```csharp
[ApiController, Route("interventions")]
public class InterventionController
    : EntityControllerBase<Intervention, InterventionSearchObject, InterventionSortBy,
                           InterventionIncludes, InterventionDto, InterventionInputDto>;
```

### Interceptors

Interceptors run transparently on every `SaveChanges`:

- **Primer** — stamps `Created` / `LastModified` timestamps and restores server-owned fields
- **Normalizer** — fills `NormalizedContent` fields for free-text `?q=` search (declared with `[Normalized]`)
- **AutoTruncate** — silently trims values that exceed the column's `[MaxLength]`

### Seeding

Every sample seeds through its registered `IEntityService<>` implementations — never the raw `DbContext` —
so the full write pipeline (primers, preppers, normalization) runs exactly as it does for API requests.
[Bogus](https://www.nuget.org/packages/Bogus) generates the data with a fixed seed for reproducibility, and
seeding is idempotent: it is skipped once data exists.

---

## API endpoints

All registered entities expose the standard Regira CRUD + search surface:

| Method | Route | Action |
|---|---|---|
| `GET` | `/{entity}` | List (query-string filters) |
| `GET` | `/{entity}/{id}` | Details |
| `GET` / `POST` | `/{entity}/search` | Search **+ count** |
| `POST` | `/{entity}` | Create |
| `POST` | `/{entity}/save` | Upsert |
| `PUT` | `/{entity}/{id}` | Full update |
| `PATCH` | `/{entity}/{id}` | Partial update (JSON Merge Patch) |
| `DELETE` | `/{entity}/{id}` | Delete |

---

## Related packages

| Package | Role |
|---|---|
| [`Regira.Entities`](https://www.nuget.org/packages/Regira.Entities) | Entity abstractions, service interfaces |
| [`Regira.Entities.EFcore`](https://www.nuget.org/packages/Regira.Entities.EFcore) | `EntityRepository`, EF interceptors |
| [`Regira.Entities.DependencyInjection`](https://www.nuget.org/packages/Regira.Entities.DependencyInjection) | `UseEntities()` / `.For<>()` builder |
| [`Regira.Entities.Web`](https://www.nuget.org/packages/Regira.Entities.Web) | Web meta-package (controllers + DI + EF Core) — what the samples reference |
| [`Regira.Entities.Mapping.Mapster`](https://www.nuget.org/packages/Regira.Entities.Mapping.Mapster) | Mapster DTO pipeline integration |
| [`Regira.IO.Storage`](https://www.nuget.org/packages/Regira.IO.Storage) | File storage behind `WithAttachments` (HelpDesk) |
| [`@regira/modules`](https://www.npmjs.com/package/@regira/modules) | Vue 3 entities client, UI kit and `scaffold.mjs` generator |

---

## Credits

All nine applications were generated by **Claude (Anthropic)** agents running in **Claude Code**, driven
entirely by the **Regira MCP server** (`https://mcp.regira.com/mcp`) — package selection, setup, entity
classification, scaffolding and conventions all came from the MCP docs rather than prior model knowledge.
Each sample was built in an isolated session, with no reference to its siblings.

| Sample | Model | Reasoning effort | Regira MCP calls | Wall-clock |
|---|---|---|---|---|
| AssetHub | Claude *(model not recorded in-session)* | medium (40) | ~46 | ~1.5–2 h |
| Blog | Claude Sonnet 5 | default | ~34 | ~31 min |
| EventPlanner | Claude Sonnet 5 | default | ~48 | ~67 min |
| Fleet | Claude Sonnet 5 | not exposed to the agent | ~45 | ~36 min |
| HelpDesk | Claude Sonnet 5 | medium | ~38 | ~90 min |
| QCredits | Claude Sonnet 5 | medium-low (40) | ~32 | ~50 min |
| RoomPlanner | Claude Sonnet 5 | medium (40) | ~45 | ~80–90 min |
| ShopMate | Claude Sonnet 5 | medium | ~47 | ~71 min |
| Webshop | Claude Sonnet 5 | default | 27 | ~35 min |

These are the agents' own self-reported tallies from their session transcripts — approximations, not
instrumented measurements. Per-sample detail (token budgets, bugs found during verification, deviations
from the generated scaffold) lives in each sample's `README.md`.

## License

MIT — see [LICENSE](LICENSE). The referenced Regira packages keep their own licenses (most Apache-2.0; the
Entities registration packages are commercial with a free tier).
