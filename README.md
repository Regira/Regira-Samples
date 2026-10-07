# Regira — Sample Applications

Ten self-contained, full-stack sample applications built on the [Regira](https://regira.com) framework:
an **ASP.NET Core 10 API** on [Regira Entities](https://regira.com/entities) plus a **Vue 3 SPA** on
[`@regira/modules`](https://www.npmjs.com/package/@regira/modules) — each a complete app with a real
domain, seeded data (~500 rows of the primary entity in most) and a UI you can click through.

What makes them unusual: every sample was **generated end-to-end by an AI agent** — no manual scaffolding,
no boilerplate written by hand — driven exclusively by the **Regira MCP server**
(`https://mcp.regira.com/mcp`) as the single source of truth for package selection, setup and APIs.
Each sample carries its own `README.md` covering what it does, how to run it, and the design decisions
(and deviations) the agent made along the way, plus the `prompt.txt` it was generated from. That file
holds the domain; three standing lines went with every one — use the Regira MCP server for anything
Regira, seed the database through the entity services, and close with a README documenting the run. The samples
were not touched after their generating session completed — except [StableGenius](StableGenius/), which
was developed a little further afterwards with vibe coding.

| Site | URL |
|---|---|
| 🏢 Regira | [regira.com](https://www.regira.com) |
| 📚 Regira Entities | [Regira Entities framework](https://regira.com/entities) |
| 📦 Package sources | [Regira/Regira-Packages](https://github.com/Regira/Regira-Packages) |
| 🏭 Production sample | [Regira/Regira-PIM-Backend](https://github.com/Regira/Regira-PIM-Backend) |

> ✅ **No license key required.** Every sample stays inside the Regira Entities **free tier**
> (≤ 5 simple + 2 complex entity registrations) — AssetHub and HelpDesk use all seven slots.
> To raise the limits, add a key under `Regira:LicenseKeys` in your configuration and call `services.UseRegira(configuration)` before `UseEntities()`.

---

## The samples

| Sample | Domain | Entity budget | Primary seed | Front-end character |
|---|---|---|---|---|
| [AssetHub](AssetHub/) | Company asset management — assets, assignments, warranties, maintenance, attachments | 5 simple / 2 complex | 500 assets | Inventory table/card toggle, color-coded status chips, dashboard |
| [Blog](Blog/) | Blog publishing — posts, categories, tags | 2 simple / 1 complex | 500 posts | Editorial reader site **and** a `/manage` studio in one SPA |
| [EventPlanner](EventPlanner/) | Event management — events, sessions, speakers, locations, registrations | 3 simple / 2 complex | 500 events (~10,000 registrations) | Event banners and card grids, per-day agenda timeline, speaker cards |
| [Fleet](Fleet/) | Fleet maintenance — vehicles, interventions, intervention types, suppliers, invoices | 3 simple / 2 complex | 500 interventions | KPI dashboard with charts over an aggregate endpoint |
| [HelpDesk](HelpDesk/) | Support ticket desk — tickets, comments, attachments, teams, customers and agents | 5 simple / 2 complex | 520 tickets | Kanban board, work queues, conversation threads |
| [QCredits](QCredits/) | Employee training credits — requests, allocations, credit years, group trainings | 4 simple / 2 complex | ~550 requests | Approval workflow, balance progress bars |
| [RoomPlanner](RoomPlanner/) | Meeting-room reservations — buildings, floors, rooms, equipment, attendees | 4 simple / 2 complex | 500 reservations | Day-planner timeline, week calendar, room finder |
| [ShopMate](ShopMate/) | Shopping lists — shoppers, lists, articles, multi-parent categories | 3 simple / 1 complex | 500 articles | Mobile-first: bottom tab bar, swipe actions, drag to reorder |
| [StableGenius](StableGenius/) | Satirical quiz where every answer is right — questions, praise templates, games | 2 simple / 1 complex | 65 questions, 150 praise templates (CSV) | Gilded-palace game UI + white-and-gold `/admin`; developed further with vibe coding |
| [Webshop](Webshop/) | E-commerce — products, categories, brands, promotions, orders | 3 simple / 2 complex | 500 products | Hand-built storefront, cart & checkout + scaffolded `/admin` back office |

Four samples sign users in: **AssetHub**, **EventPlanner**, **HelpDesk** and **QCredits** use ASP.NET Identity
with Regira's JWT authentication, roles and a write-authorization filter; each README lists the seeded demo
accounts. The other six run on the anonymous back-end template and the SPA's no-auth scaffold; their READMEs
explain that choice and the upgrade path.

### What each sample shows

Pick the one closest to the pattern you need:

| Framework capability | Where to look |
|---|---|
| Owned collections & m2m joins via `e.Related()` (no registration slot) | every sample |
| Computed read-side fields via processors | [Blog](Blog/) (post counts), [EventPlanner](EventPlanner/) (seats taken, wait list), [QCredits](QCredits/) (credit balances), [HelpDesk](HelpDesk/) (comment counts, hides internal notes), [AssetHub](AssetHub/) (current asset count) |
| Write-side business rules via preppers | [RoomPlanner](RoomPlanner/) (no double booking, capacity, per-room approval), [EventPlanner](EventPlanner/) (capacity and wait list), [Fleet](Fleet/) (supplier capabilities, cost totals), [Webshop](Webshop/) (server-side pricing, price-tamper guard), [StableGenius](StableGenius/) (positivity guard: no negative words, no spoilers) |
| Server-owned fields & generated codes | [Webshop](Webshop/) (`e.ServerOwned` order codes), [HelpDesk](HelpDesk/) (`HD-000123` ticket codes), [AssetHub](AssetHub/) (`AST-00001` asset tags), [ShopMate](ShopMate/) (`[ServerOwned]` sort order) |
| Post-commit side effects via reactors | [Fleet](Fleet/) (invoice totals), [Webshop](Webshop/) (stock) |
| Workflow actions as the single writer of workflow fields | [AssetHub](AssetHub/) (assign / return), [QCredits](QCredits/) (submit / approve / reject), [RoomPlanner](RoomPlanner/) (approve / reject per room) |
| Cross-entity aggregate / dashboard endpoints (outside the entity pipeline) | [Fleet](Fleet/), [HelpDesk](HelpDesk/), [QCredits](QCredits/), [AssetHub](AssetHub/) (`/api/dashboard`) |
| File attachments (`WithAttachments` / `HasAttachments`) | [HelpDesk](HelpDesk/), [AssetHub](AssetHub/) |
| JWT sign-in, roles and row-level security (global filter query builders) | [HelpDesk](HelpDesk/) (customers see their own tickets), [QCredits](QCredits/) (employees see their own requests), [EventPlanner](EventPlanner/) (own registrations, hidden drafts), [AssetHub](AssetHub/) (write tiers only) |
| Hard delete with `Restrict` → 409 instead of soft delete (`IArchivable`) | [AssetHub](AssetHub/), [Blog](Blog/) |
| Self-referencing many-to-many hierarchy | [ShopMate](ShopMate/) (categories with multiple parents) |
| Free-tier overflow remedy (role-discriminated party) | [HelpDesk](HelpDesk/) (`Person` with a role, instead of Customer/Agent/Admin) |
| Hand-built public UI next to a scaffolded back office | [Webshop](Webshop/) (storefront + `/admin`), [Blog](Blog/) (reader site + `/manage`) |
| Calling the API origin directly with CORS instead of a dev proxy | [QCredits](QCredits/) |
| Seed data from CSV files (`Regira.Office.Csv`), reloadable at runtime | [StableGenius](StableGenius/) |
| Hosting under an IIS sub-path (SPA virtual directory + API application) | [StableGenius](StableGenius/) |

---

## Stack (shared across all samples)

| Layer | Technology |
|---|---|
| API | .NET 10 / ASP.NET Core, [`Regira.Entities.Web`](https://www.nuget.org/packages/Regira.Entities.Web) 6.4.0 |
| ORM | [Entity Framework Core](https://www.nuget.org/packages/microsoft.entityframeworkcore/) 10 (SQLite) |
| DTO mapping | [Mapster](https://www.nuget.org/packages/Mapster/) via `Regira.Entities.Mapping.Mapster` |
| API docs | OpenAPI + [Scalar](https://www.nuget.org/packages/Scalar.AspNetCore) (`/scalar`) |
| Seed data | [Bogus](https://www.nuget.org/packages/Bogus), seeded through `IEntityService<>` |
| Logging | [Serilog](https://www.nuget.org/packages/Serilog) |
| Authentication (4 samples) | ASP.NET Identity + [`Regira.Security.Authentication.Web`](https://www.nuget.org/packages/Regira.Security.Authentication.Web) (JWT) |
| SPA | Vue 3.5 + TypeScript 6 + Vite 8, Pinia 3, vue-router 5, Bootstrap 5 |
| SPA framework | [`@regira/modules`](https://www.npmjs.com/package/@regira/modules) 6.4.0 (entities client, UI kit, scaffolder) |

StableGenius differs slightly: it still uses the 6.3.3 packages, reads its seed data from CSV files
(`Regira.Office.Csv.CsvHelper`) instead of Bogus, and has an xUnit test project.

Everything installs straight from nuget.org / npmjs.com — no custom feed or private registry needed.

---

## Running a sample

Each sample is independent: start its API, then its SPA. The API creates and seeds its SQLite database on
first run (`Database.EnsureCreated()`); delete the `.db` file, or start it with `-- --ResetDatabase=true`
in Development, to force a reseed.

```bash
# 1. API — the port comes from launchSettings.json, Scalar docs open at /scalar
cd Fleet/Fleet.Api
dotnet run --launch-profile http

# 2. SPA (second terminal)
cd Fleet/Fleet.Web
npm install
npm run dev
```

Paths and ports per sample:

| Sample | API project | API port | SPA project | SPA port |
|---|---|---|---|---|
| AssetHub | `AssetHub/AssetHub.Api` | 5801 | `AssetHub/AssetHub.Web` | 5802 |
| Blog | `Blog/Blog.Api` | 5811 | `Blog/Blog.Web` | 5812 |
| EventPlanner | `EventPlanner/EventPlanner.Api` | 5821 | `EventPlanner/EventPlanner.Web` | 5822 |
| Fleet | `Fleet/Fleet.Api` | 5831 | `Fleet/Fleet.Web` | 5832 |
| HelpDesk | `HelpDesk/HelpDesk.Api` | 5841 | `HelpDesk/HelpDesk.Web` | 5842 |
| QCredits | `QCredits/QCredits.Api` | 5851 | `QCredits/qcredits-spa` | 5852 <sup>†</sup> |
| RoomPlanner | `RoomPlanner/RoomPlanner.Api` | 5861 | `RoomPlanner/RoomPlanner.Web` | 5862 |
| ShopMate | `ShopMate/backend/ShopMate.Api` | 5871 | `ShopMate/frontend` | 5872 |
| StableGenius | `StableGenius/GeniusTest.Api` | 5711 | `StableGenius/GeniusTest.Web` | 5712 |
| Webshop | `Webshop/Webshop.Api` | 5881 | `Webshop/webshop-spa` | 5882 |

Every SPA proxies `/api` to its API through Vite, so the browser talks to a single origin.
<sup>†</sup> QCredits is the exception: its SPA calls the API origin directly, with CORS enabled server-side.

### Solution

`Regira-Samples.slnx` opens all ten APIs plus the StableGenius test project at once
(`dotnet build Regira-Samples.slnx` builds them all). To work on a single sample, run `dotnet build` /
`dotnet run` from its API folder.

---

## Repository layout

```
Regira-Samples/
├── AssetHub/            README.md + prompt.txt + AssetHub.Api/ + AssetHub.Web/
├── Blog/                …
├── EventPlanner/        …
├── Fleet/               …
├── HelpDesk/            …
├── QCredits/            README.md + prompt.txt + QCredits.Api/ + qcredits-spa/
├── RoomPlanner/         …
├── ShopMate/            README.md + prompt.txt + backend/ShopMate.Api/ + frontend/
├── StableGenius/        README.md + GeniusTest.Api/ + GeniusTest.Web/ + GeniusTest.Tests/
├── Webshop/             README.md + prompt.txt + Webshop.Api/ + webshop-spa/
├── .mcp.json            Regira MCP server registration (for agents working in this repo)└── Regira-Samples.slnx  all ten APIs in one solution
```

A back end follows the framework's own layout — one folder per entity holding the model, its DTOs, search
object and service configuration, plus thin controllers and a Bogus-driven seeder:

```
Fleet/Fleet.Api/
├── Entities/Interventions/    Intervention.cs, InterventionDtos.cs, InterventionSearch.cs,
│                              InterventionServices.cs, InterventionServiceConfiguration.cs
├── Controllers/               EntityControllerBase<> subclasses + a DashboardController
├── Data/                      DbContext + Seeding/
├── Extensions/                AddEntityServices() — the DI wiring
└── Infrastructure/            HostingExtensions — service registration and the request pipeline
```

A front end is a set of generated-then-customized entity *slices*:

```
Fleet/Fleet.Web/src/entities/interventions/
├── config/              slice config (title, routes, navigation)
├── data/                Entity.ts, EntityService.ts, store.ts
├── overview/            Overview.vue, List.vue, ListItem.vue
├── details/             Details.vue, Form.vue, FormModalButton.vue
├── filter/              SearchObject.ts, Filter*.vue
├── selecting/           Autocomplete.vue, InputSelector.vue, Selector*.vue
└── intervention-lines/  owned sub-slice for the work-order lines
```

---

## How Regira Entities is used

A single `UseEntities()` call sets up global defaults and the Mapster mapping layer; each entity then
registers itself through its own extension method:

```csharp
// Fleet/Fleet.Api/Extensions/ServiceCollectionExtensions.cs
services
    .UseEntities<FleetDbContext>(options =>
    {
        options.UseDefaults();
        options.UseMapsterMapping();
    })
    .AddInterventionTypes()
    .AddSuppliers()
    .AddInvoices()
    .AddVehicles()
    .AddInterventions();
```

Inside each domain folder, `.For<>()` wires the filter, sorting, includes, owned collections and write
hooks in one place:

```csharp
// Fleet — InterventionServiceConfiguration.cs (abridged)
services.For<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes>(e =>
{
    e.Filter((query, so) =>
    {
        if (so == null) return query;
        if (so.VehicleId?.Any() == true) query = query.Where(x => so.VehicleId.Contains(x.VehicleId));
        if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
        if (so.IsInvoiced.HasValue) query = query.Where(x => (x.InvoiceId != null) == so.IsInvoiced);
        return query;
    });
    e.SortBy((query, sortBy) => sortBy switch
    {
        // SQLite cannot ORDER BY decimal -> sort on a REAL projection
        InterventionSortBy.TotalCostDesc => query.OrderOrThenByDescending(x => (double)x.TotalCost),
        _ => query.OrderOrThenByDescending(x => x.ScheduledDate).ThenByDescending(x => x.Id)
    });
    e.Includes((query, includes) =>
    {
        query = query.Include(x => x.Vehicle).Include(x => x.Supplier).Include(x => x.Invoice);
        if (includes?.HasFlag(InterventionIncludes.Lines) == true)
            query = query.Include(x => x.Lines!).ThenInclude(l => l.InterventionType);
        return query;
    });
    e.Related<InterventionLine>(x => x.Lines);   // owned child rows — no registration slot, no controller
    e.AddPrepper<InterventionPrepper>();         // supplier capability check + TotalCost from the lines
    e.AddNormalizer<InterventionNormalizer>();   // folds plate, supplier and type titles into ?q=
    e.AddReactor<InterventionInvoiceReactor>();  // re-totals the affected invoice after the commit
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
so the full write pipeline (primers, preppers, normalization, reactors) runs exactly as it does for API
requests. [Bogus](https://www.nuget.org/packages/Bogus) generates the data with a fixed seed for
reproducibility, and seeding only runs on an empty database.

---

## API endpoints

All registered entities expose the standard Regira CRUD + search surface. The nine generated samples put
every controller under a central `api` route prefix (`UseCentralRoutePrefix`); StableGenius maps its routes
at the root and lets its Vite proxy (or the IIS application path) supply the `/api` part.

| Method | Route | Action |
|---|---|---|
| `GET` | `/api/{entity}` | List (query-string filters) |
| `GET` | `/api/{entity}/{id}` | Details |
| `GET` / `POST` | `/api/{entity}/search` | Search **+ count** |
| `POST` | `/api/{entity}` | Create |
| `POST` | `/api/{entity}/save` | Upsert |
| `PUT` | `/api/{entity}/{id}` | Full update |
| `PATCH` | `/api/{entity}/{id}` | Partial update (JSON Merge Patch) |
| `DELETE` | `/api/{entity}/{id}` | Delete |

Complex registrations also accept a batch `POST /api/{entity}/list`.

---

## Related packages

| Package | Role |
|---|---|
| [`Regira.Entities`](https://www.nuget.org/packages/Regira.Entities) | Entity abstractions, service interfaces |
| [`Regira.Entities.EFcore`](https://www.nuget.org/packages/Regira.Entities.EFcore) | `EntityRepository`, EF interceptors |
| [`Regira.Entities.DependencyInjection`](https://www.nuget.org/packages/Regira.Entities.DependencyInjection) | `UseEntities()` / `.For<>()` builder |
| [`Regira.Entities.Web`](https://www.nuget.org/packages/Regira.Entities.Web) | Web meta-package (controllers + DI + EF Core) — what the samples reference |
| [`Regira.Entities.Mapping.Mapster`](https://www.nuget.org/packages/Regira.Entities.Mapping.Mapster) | Mapster DTO pipeline integration |
| [`Regira.IO.Storage`](https://www.nuget.org/packages/Regira.IO.Storage) | File storage behind `WithAttachments` (AssetHub, HelpDesk) |
| [`Regira.Security.Authentication.Web`](https://www.nuget.org/packages/Regira.Security.Authentication.Web) | JWT authentication and account endpoints (AssetHub, EventPlanner, HelpDesk, QCredits) |
| [`@regira/modules`](https://www.npmjs.com/package/@regira/modules) | Vue 3 entities client, UI kit and `scaffold.mjs` generator |

---

## Credits

All ten applications were generated by **Claude (Anthropic)** agents running in **Claude Code**, driven
entirely by the **Regira MCP server** (`https://mcp.regira.com/mcp`) — package selection, setup, entity
classification, scaffolding and conventions all came from the MCP docs rather than prior model knowledge.

The nine generated samples were each built by their own Claude Code sub-agent (general-purpose agent type),
running in parallel, all on **Claude Opus 5.5** with **low reasoning effort**. Each was told to set aside
its memory and the other projects on disk, so no agent used either as a reference.

| Sample | Regira MCP calls | Wall-clock | Tokens (approx.) |
|---|---|---|---|
| AssetHub | 36 | ~35 min | ~465k |
| Blog | 28 | ~25 min | ~375k |
| EventPlanner | 41 | ~32 min | ~475k |
| Fleet | 39 | ~33 min | ~485k |
| HelpDesk | 35 | ~38 min | ~550k |
| QCredits | 30 | ~31 min | ~443k |
| RoomPlanner | 25 | ~34 min | ~480k |
| ShopMate | 29 | ~28 min | ~440k |
| Webshop | 30 | ~33 min | ~442k |

StableGenius is not in the table: its generating session's tallies were not recorded, and it was developed
a little further afterwards with vibe coding.

These are the agents' own self-reported tallies — approximations, not instrumented measurements; the token
figures come from each session's budget counter and are mostly cached context. Per-sample detail (back-end /
front-end split, runtime verification, deviations from the generated scaffold) lives in each sample's
`README.md`.

## License

MIT — see [LICENSE](LICENSE). The referenced Regira packages keep their own licenses (most Apache-2.0; the
Entities registration packages are commercial with a free tier).
