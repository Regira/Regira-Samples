# ShopMate

A mobile-first shopping list app built on the **Regira** framework and its Regira MCP server. Shoppers
manage multiple shopping lists; each list holds sortable articles that can be marked active ("need to
buy") or inactive ("bought"); articles carry one or more categories, and categories form a many-to-many
hierarchy (a category can have several parents *and* several children — e.g. "Organic" sits under both
"Produce" and "Dairy & Eggs").

## Stack

| Layer | Tech |
|---|---|
| Backend API | ASP.NET Core (.NET 10), `Regira.Entities.Web` + EF Core (SQLite), Mapster mapping, OpenAPI + Scalar |
| Frontend SPA | Vue 3 + TypeScript + Vite, `@regira/modules` (entities client, UI kit), Bootstrap 5 |
| Seeding | [Bogus](https://github.com/bchavez/Bogus) driving the registered `IEntityService` implementations |

## Ports

- Backend API: `http://localhost:6170` (Scalar docs at `/scalar`, OpenAPI at `/openapi/v1.json`)
- Frontend SPA: `http://localhost:6171`

## Running it

```bash
# Terminal 1 - API (seeds the SQLite DB on first run)
cd backend/ShopMate.Api
dotnet run

# Terminal 2 - SPA
cd frontend
npm install
npm run dev
```

Open `http://localhost:6171`. No authentication is configured (the spec did not call for user accounts),
so the app is usable immediately.

## Domain model

- **ShoppingList** — `Title`, `OwnerName` (the shopper), `Description`, `Icon`/`ColorHex`, `IsArchived`
  (soft-delete — an archived list and its articles just drop out of the normal views). Simple entity.
- **Category** — `Title`, `Icon`/`ColorHex`, and a **self-referencing many-to-many hierarchy**
  (`RelatedCategory` join rows carry `ParentId`/`ChildId`) — a category can have several parents and
  several children at once, not just a single-parent tree. Simple entity.
- **Article** — `Title`, `Notes`, `Quantity`/`Unit`, `IsActive` (need to buy vs. bought), `SortOrder`
  (per-list drag-order), a required `ShoppingListId` FK, and a many-to-many `Categories` join
  (`ArticleCategory`). Complex entity (typed sort/includes, full-text `Q` search over
  title+notes via `NormalizedContent`).

Free-tier budget: 2 simple + 1 complex registration (`ShoppingList`, `Category` simple; `Article`
complex) — the two join entities (`ArticleCategory`, `RelatedCategory`) are owned via `e.Related()` and
cost no registration slot. Confirmed at every startup via the framework's own log line:
`Regira.Entities: 2 simple / 1 complex registered -> tier = free`.

## Seeded data

Seeded through `IEntityService<T>` (never straight against the `DbSet`), using Bogus for names, owners,
dates and free text:

- **~40 categories**, 12 root groups (Produce, Dairy & Eggs, Bakery, Meat & Seafood, Pantry, Frozen,
  Beverages, Household, Personal Care, Snacks, Baby, Pet Supplies) each with 2-4 children, plus two
  deliberately **multi-parent** categories (`Organic` under Produce *and* Dairy & Eggs; `Bulk Buy` under
  Pantry *and* Household) to exercise the hierarchy end to end.
- **25 shopping lists** (Weekly Groceries, Weekend BBQ, Camping Trip, Baby Essentials, …), 3 of them
  seeded already archived to demonstrate the soft-delete / restore flow.
- **500 articles** (the primary entity), distributed across the lists, each drawn from a curated
  per-category grocery item pool (Apples, Whole Milk, Chicken Breast, Toothpaste, Dog Food, …), with
  randomized quantity/unit, ~65%/35% active/bought split, 1-2 categories each, and backdated
  `Created`/`LastModified` timestamps.

## Mobile-first UI

- Bottom tab bar (Lists / Items / Categories) replaces the framework's default dashboard+navbar shell —
  the shell components are documented as *default implementations, not requirements*, and this app
  deliberately replaces them while keeping the underlying functionality (config-driven entity slices,
  paging, filtering, feedback).
- Cards instead of table rows everywhere; large (≥44px) touch targets on every control.
- A small app-owned `SwipeActions` component (`src/components/ui/SwipeActions.vue`, pointer-events based,
  no external library) powers swipe-to-reveal actions: swipe a shopping-list or category card left to
  archive/delete (with a confirm step via the kit's `ConfirmButton`), swipe an article row right to
  reorder, left to delete; a big tap-target circle toggles "bought" without opening anything.
- Shopping-list detail page embeds a hand-rolled `ArticleManager` (built on `useSearchView`, per the
  framework's own guidance that a hand-written view is the right tool outside the scaffolded Overview)
  with an inline "add item" bar, category chip filters, a to-buy/bought toggle, and free-text search.

## Known framework interaction (documented, not hidden)

Ordering the **unfiltered, cross-list** `Article` search by `SortOrder` returns one row fewer than the
requested `pageSize` on every page (the count is correct, only `items` comes up short) — reproducible with
a plain `.OrderBy(x => x.SortOrder)`, with or without an `Id` tiebreaker, and *not* reproduced ordering by
another equally-duplicated column (`ShoppingListId`) instead. `SortOrder` is scoped per shopping list (many
rows legitimately share the same value across different lists), so this only surfaces cross-list; scoped to
a single `shoppingListId` — its intended use, the per-list reorder view where values are unique — it is
unaffected. Worked around in `ArticleServiceConfiguration.cs` by defaulting the unscoped browse to a
`Title` sort; full repro notes are in the code comment there. This is the one non-cosmetic surprise found
during an otherwise clean build.

## Project structure

```
ShopMate/
  backend/ShopMate.Api/         ASP.NET Core API (33 .cs files)
    Entities/ShoppingLists/     entity, DTOs, search object, processor, service config
    Entities/Categories/        entity, RelatedCategory join, DTOs, processor, service config
    Entities/Articles/          entity, ArticleCategory join, DTOs, sort/includes enums, query builder
    Data/                       DbContext + Bogus-driven DataSeeder
    Controllers/                three thin EntityControllerBase<> controllers
  frontend/                     Vue 3 SPA (88 .vue/.ts files under src/)
    src/entities/{shopping-lists,categories,articles}/   generated-then-customized entity slices
    src/entities/articles/article-categories/            owned Article<->Category join sub-slice
    src/components/ui/SwipeActions.vue                   app-owned swipe gesture wrapper
    src/components/layout/                               mobile shell (TheHeader, BottomNav, Main)
```

## Verification performed

- `dotnet build` — 0 warnings, 0 errors.
- `npm run build` (`vue-tsc -b && vite build`) — 0 type errors.
- Runtime: startup log confirms the 2 simple / 1 complex tier; create -> update -> update-again -> re-read
  round-trips on `ShoppingList` and `Article` (including the many-to-many `Categories` join surviving an
  update that omits the collection); archived-list filtering and restore; category multi-parent hierarchy
  create/read verified via the API; the SPA end-to-end: browse lists, open a list, add an article, mark it
  bought (verified it re-sorts to the bottom), delete it, filter articles by category and free text, create
  a category with a parent link — all confirmed against the live API (network tab + response payloads), not
  just a green build.

## Effort tracking

Tracked by the agent during the build (approximate — reconstructed from the session transcript, not an
instrumented counter).

| | Backend | Frontend | Total |
|---|---|---|---|
| Regira MCP calls | ~27 | ~20 | **~47** |
| Wall-clock time | ~45 min | ~26 min | **~71 min** |

- Total session wall-clock time: ~71 minutes (single continuous session, one mid-session resume with no
  time lost).
- Agent context consumed: ~500K tokens over the session (from the harness's own remaining-budget counter;
  not a direct proxy for API input+output token billing).
- Total cost: not independently metered by the harness in this environment; given the token volume above
  and current Claude Sonnet pricing, a rough order-of-magnitude estimate is in the low single-digit USD
  range for this build. Treat this figure as indicative only, not a billed total.

## Credits

Built by **Claude Sonnet 5** (`claude-sonnet-5`), running as **Claude Code** (Anthropic's CLI agent), at
**medium reasoning effort**, using the Regira MCP server for framework guidance and the Regira package
documentation throughout.
