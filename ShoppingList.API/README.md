# ShoppingList.API

A REST API for running shopping lists, built on the **[Regira](https://regira.com) Entities**
framework. Shoppers keep multiple lists, put articles on them, and activate/deactivate those
articles as they shop. Articles are organised in a multi-parent/multi-child category graph and are
fully searchable.

Built with **.NET 10**, **EF Core 10 (SQLite)**, **Mapster** mapping, **OpenAPI + Scalar** docs and
**Serilog** logging. Sample data (~500 articles) is generated with **Bogus**.

---

## What it does

| Requirement | How it is implemented |
|---|---|
| A shopper can activate/deactivate articles on a list | `ShoppingListItem.IsActive` toggled via dedicated `POST /shoppinglists/{id}/items/{itemId}/activate` & `/deactivate` endpoints |
| An article can have multiple categories, filterable + text search | `Article` ⇄ `Category` many-to-many (`ArticleCategory`); filter by `categoryId[]`, `brand`, and normalized full-text `q` |
| A category can have multiple parent/child categories | Self-referencing `RelatedCategory` join; eager-load with `?includes=Parents,Children,All` |
| Each shopper can have multiple lists | `Shopper` 1‑to‑many `ShoppingList`; filter lists by `shopperId[]` |

---

## Domain model

```
Shopper ──< ShoppingList ──< ShoppingListItem >── Article >──< (ArticleCategory) >──< Category
                                                                                        │
                                                                       RelatedCategory (parent/child)
```

| Entity | Key | Registration | Notes |
|---|---|---|---|
| `Category` | int | **complex** | Hierarchical (parent/child via `RelatedCategory`); derived `ArticleCount` |
| `Article` | int | **complex** | Many-to-many categories; sortable; full-text searchable |
| `Shopper` | int | simple | Owns lists; searchable by name/email |
| `ShoppingList` | int | simple | Owns `ShoppingListItem` children; always loads items + article + shopper |
| `RelatedCategory`, `ArticleCategory`, `ShoppingListItem` | int | *(owned)* | Join/child entities — managed through their parent via `Related()`, no own registration |

> **Licensing / tier budget.** Regira's free tier allows **5 simple + 2 complex** entity
> registrations. This project uses exactly **2 complex** (`Category`, `Article`) and **2 simple**
> (`Shopper`, `ShoppingList`), so it runs with no license key. Set `Regira:LicenseKey` in
> `appsettings.json` to lift the limits.

---

## Running

```bash
cd ShoppingList.API
dotnet run
```

On first start the app creates a local SQLite database (`shoppinglist.db`, recreated on demand —
no migrations) and seeds sample data. Then browse:

- **Scalar API UI** → <https://localhost:7299/scalar>
- **OpenAPI document** → <https://localhost:7299/openapi/v1.json>

(HTTP is also exposed on `http://localhost:5299`.)

---

## API surface

Every entity gets a full CRUD controller from `EntityControllerBase`. **Complex** controllers
(`articles`, `categories`) additionally expose the search/list-with-count endpoints.

| Method | Route | Description |
|---|---|---|
| `GET` | `/{entity}/{id}` | Get one (supports `?includes=`) |
| `GET` | `/{entity}` | List (filter via query string, paged) |
| `GET` / `POST` | `/{entity}/search` | Search **with total count** *(complex only)* |
| `POST` | `/{entity}/list` | List with a body filter *(complex only)* |
| `POST` | `/{entity}` | Create |
| `POST` | `/{entity}/save` | Upsert (create or update by `id`) |
| `PUT` | `/{entity}/{id}` | Full update |
| `PATCH` | `/{entity}/{id}` | Partial update (JSON Merge Patch) |
| `DELETE` | `/{entity}/{id}` | Delete |

Entities: `articles`, `categories`, `shoppers`, `shoppinglists`.

### Shopping-list item endpoints (activate / deactivate)

| Method | Route | Description |
|---|---|---|
| `POST` | `/shoppinglists/{listId}/items` | Add an article (`{ "articleId": 5, "quantity": 2, "note": "…" }`); re-activates if already present |
| `POST` | `/shoppinglists/{listId}/items/{itemId}/activate` | Activate the article on the list |
| `POST` | `/shoppinglists/{listId}/items/{itemId}/deactivate` | Deactivate (keeps it on the list) |
| `DELETE` | `/shoppinglists/{listId}/items/{itemId}` | Remove the article from the list |

### Filtering, search, sorting, paging

```http
# Full-text search articles (matches title + description + brand), with total count
GET /articles/search?q=tasty&pageSize=20

# Filter articles by one or more categories + brand, sorted by title
GET /articles/search?categoryId=2&categoryId=5&brand=Acme&sortBy=Title

# Load an article including its categories
GET /articles/428?includes=All

# Category hierarchy with parents + children and per-category article counts
GET /categories?includes=All&q=dairy

# Only root categories
GET /categories?isRoot=true

# All lists for a shopper
GET /shoppinglists?shopperId=3
```

- **Paging**: `?page=1&pageSize=50` (default page size 50, max 200).
- **Sorting** (articles): `Title`, `TitleDesc`, `Brand`, `BrandDesc`, `Newest`.
- **Includes**: articles → `All`; categories → `Parents`, `Children`, `All`.
- **`q`** performs a normalized (case/accent-insensitive) search powered by the framework's global
  normalized-content filter.

---

## How it is wired up

- **`Program.cs`** — thin host: JSON (ignore cycles + nulls), OpenAPI/Scalar, `DbContext` with
  Regira primer/normalizer/auto-truncate interceptors, `AddEntityServices(...)`, then
  `EnsureCreated()` + seed. DI is validated on build (`ValidateOnBuild`/`ValidateScopes`).
- **`Extensions/ServiceCollectionExtensions.cs`** — `UseRegira()` → `UseEntities<ShoppingListDbContext>()`
  (`UseDefaults()` + Mapster + paging) → one `.AddXxx()` per entity.
- **`Entities/<Feature>/`** — per-entity folder: entity, search object, includes/sort enums, DTOs,
  query builder/processor, and the `For<>()` service configuration.
- **`Seeding/DataSeeder.cs`** — generates the sample data through the
  `IEntityService<TEntity, int>` implementations (so primers, normalizers and `Related()` collection
  sync all run), not raw EF inserts.

### Project structure

```
ShoppingList.API/
├── Program.cs
├── Data/ShoppingListDbContext.cs
├── Extensions/ServiceCollectionExtensions.cs
├── Entities/
│   ├── Categories/   (Category, RelatedCategory, search, includes, DTOs, processor, config)
│   ├── Articles/     (Article, ArticleCategory, search, sort, DTOs, query builder, config)
│   ├── Shoppers/     (Shopper, search, DTOs, config)
│   └── Lists/        (ShoppingList, ShoppingListItem, search, DTOs, item service, config)
├── Controllers/      (one per entity + ShoppingListItemsController)
└── Seeding/DataSeeder.cs
```

---

## Seed data

Generated on first run (deterministic seed): **59 categories** (12 roots × children), **500 articles**
(1–3 categories each), **15 shoppers**, **30 shopping lists** (5–20 items each, ~70 % active). The
database is treated as disposable — delete `shoppinglist.db` to reseed.

---

## Credits

Designed and implemented by **Claude** (Anthropic **Opus 4.8**) running as **Claude Code** — the
default general-purpose coding agent. All Regira API knowledge (package selection, setup, entity
patterns, namespaces and signatures) was sourced live from the **Regira MCP server**
(`https://mcp.regira.com/mcp`); no local reference projects were consulted. The workspace also ships
a specialized **`entities-agent`** dedicated to Regira Entities work.

Built on the Regira framework — © Regira.
