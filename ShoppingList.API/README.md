# ShoppingList.API

A small but complete **Shopping List** REST API built on the [Regira](https://regira.com) entity
framework. It lets shoppers keep multiple lists, put articles on those lists and activate/deactivate
them, organise articles into a multi-parent / multi-child category hierarchy, and search & filter the
catalog — full-text search included.

The project was generated **exclusively** from the Regira MCP server documentation (no external code
references) and ships with ~500 seeded articles so it is usable the moment it starts.

---

## Domain model

| Entity | Purpose | Key relationships |
|--------|---------|-------------------|
| **Shopper** | A person who owns shopping lists | has many *ShoppingList* |
| **ShoppingList** | A named list belonging to one shopper | has many *ShoppingListItem* |
| **ShoppingListItem** | An article placed on a list, with an `IsActive` flag and a quantity | references one *Article* |
| **Article** | A product that can be bought | many-to-many *Category* (via *ArticleCategory*) |
| **Category** | A grouping for articles, organised as a hierarchy | many-to-many self-reference (via *RelatedCategory*) |

Join entities (`ArticleCategory`, `RelatedCategory`) are **owned** by their parent and synchronized
automatically through Regira's `e.Related(...)` configuration — they have no endpoints of their own.

### Requirements coverage

- **A shopper can activate/deactivate articles on a list** → `ShoppingListItem.IsActive`, toggled with
  `PATCH /shopping-list-items/{id}` (e.g. `{ "isActive": false }`).
- **An article can have multiple categories, easily filtered, text search included** → `Article` is
  many-to-many with `Category`; the article endpoints filter by `categoryId`, `brand` and a normalized
  full-text `q` search over title/description/brand.
- **A category can have multiple parent/child categories** → self-referencing many-to-many via
  `RelatedCategory`; filter with `parentId`, `childId`, `isRoot` and include `Parents`/`Children`.
- **Each shopper can have multiple lists** → `Shopper` 1-to-many `ShoppingList`, filter lists by `shopperId`.

---

## Tech stack

- **.NET 10** / **C# 14**, ASP.NET Core Web API (controller-based, `BasicApi` template)
- **Regira Entities** framework
  - `Regira.Entities.DependencyInjection` — `UseEntities()` / `.For<>()` DI builder
  - `Regira.Entities.EFcore` — EF Core repository, interceptors (primers, normalizers, auto-truncate)
  - `Regira.Entities.Web` — `EntityControllerBase` HTTP endpoints
  - `Regira.Entities.Mapping.Mapster` — DTO mapping
- **EF Core 10** with **SQLite** (`shoppinglist.db`, created on first run via `EnsureCreated()`)
- **OpenAPI + Scalar** UI for documentation (no Swagger, per Regira conventions)
- **Serilog** for structured console + rolling-file logging
- **Bogus** for sample-data generation

---

## Project layout

```
ShoppingList.API/
├── Program.cs                       # thin host: controllers, DbContext + interceptors, entity services, seeding
├── appsettings.json                 # connection string, Serilog
├── Data/
│   └── ShoppingDbContext.cs         # DbSets + relationship configuration
├── Extensions/
│   └── ServiceCollectionExtensions.cs   # UseEntities() + per-entity registrations
├── Infrastructure/
│   └── SeedData.cs                  # seeds via IEntityService implementations (Bogus)
├── Controllers/                     # one EntityControllerBase-derived controller per entity
└── Entities/                        # per-entity folders: entity, search object, DTOs, query builder, DI config
    ├── Articles/
    ├── Categories/
    ├── Shoppers/
    ├── ShoppingLists/
    └── ShoppingListItems/
```

Each registered entity follows the Regira pipeline: **Entity → SearchObject → (SortBy/Includes) → DTO/InputDto →
QueryBuilder → ServiceConfiguration → Controller**.

---

## Running

> The Regira packages come from a private NuGet feed already configured in [`NuGet.Config`](NuGet.Config)
> (`https://packages.regira.com/v3/index.json`).

```bash
cd ShoppingList.API
dotnet run
```

On startup the app creates the SQLite database and seeds:
**~60 categories** (10 roots × children), **500 articles**, **12 shoppers**, their **lists** and **list items**.

Then browse the interactive **Scalar** UI:

- HTTPS: `https://localhost:7299/scalar`
- HTTP:  `http://localhost:5299/scalar`

OpenAPI document: `/openapi/v1.json`.

---

## Endpoints

Every entity exposes the standard Regira CRUD + search surface. Routes:
`/shoppers`, `/shopping-lists`, `/shopping-list-items`, `/articles`, `/categories`.

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/{entity}/{id}` | Details |
| `GET` | `/{entity}` | List (query-string filters) |
| `GET` / `POST` | `/{entity}/search` | Search with total count |
| `POST` | `/{entity}` | Create |
| `POST` | `/{entity}/save` | Upsert |
| `PUT` | `/{entity}/{id}` | Full update |
| `PATCH` | `/{entity}/{id}` | Partial update (JSON Merge Patch) |
| `DELETE` | `/{entity}/{id}` | Delete (soft-delete where `IArchivable`) |

### Useful filters & examples

```bash
# Full-text search the catalog
GET /articles?q=cheese

# Articles in one or more categories (repeat the param for multiple)
GET /articles/search?categoryId=55

# Articles by brand, sorted newest-first
GET /articles?brand=ACME&sortBy=Newest

# Root categories only, including their children
GET /categories?isRoot=true&includes=Children

# A category and its parents
GET /categories/55?includes=Parents

# All lists for a shopper (items + articles are eager-loaded)
GET /shopping-lists?shopperId=1

# Active items on a list, filtered to a category, with text search
GET /shopping-list-items?shoppingListId=1&isActive=true&categoryId=12&q=milk

# Activate / deactivate an article on a list
PATCH /shopping-list-items/1   { "isActive": false }

# Add an article to a list
POST /shopping-list-items      { "shoppingListId": 1, "articleId": 42, "quantity": 2 }
```

`q` is matched against normalized content that the framework maintains automatically (case/diacritics
insensitive) and supports `*` wildcards (e.g. `q=choc*`).

---

## Notes & design decisions

- **Search objects are `record` types** inheriting `SearchObject`, as required by the framework.
- **Timestamps, soft-delete and normalized search content** are populated by Regira primers/normalizers
  registered through `options.UseDefaults()` and the DbContext interceptors.
- **Nested DTO projection** and **`Related()` input collections** are wired with `e.AddMapping<,>()`.
- **Seeding goes through `IEntityService`** (not raw EF Core), so the full write pipeline (preppers,
  primers, normalizers) runs exactly as it does for API requests. Parent batches are saved before
  children so their auto-increment Ids are available for the foreign keys.
- **Licensing:** `Regira.Entities.DependencyInjection` runs on the automatic **free tier**
  (5 simple / 2 complex entity registrations) — no key required. This project uses exactly
  2 complex (`Article`, `Category`) and 3 simple (`Shopper`, `ShoppingList`, `ShoppingListItem`)
  registrations. For higher limits, set `Regira:LicenseKey` and call `services.UseRegira(key)` before
  `UseEntities()`.
- The SQLite database is treated as disposable; delete `shoppinglist.db` to reseed from scratch.

---

## Credits

- **Built by:** Claude (Anthropic) — model **Claude Opus 4.8**.
- **AI agent type:** Claude Code (the default `claude` agent), driving the **Regira MCP server**
  (`https://mcp.regira.com/mcp`) as the single source of truth for package selection, setup and APIs.
- **Framework:** [Regira](https://regira.com) entity packages (v6.0.0).
