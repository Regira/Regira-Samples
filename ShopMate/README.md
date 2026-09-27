# ShopMate

A mobile-first shopping-list app: a .NET 10 **Regira Entities** API plus a Vue 3 **@regira/modules** SPA.

- A **shopper** manages several **shopping lists**.
- A list holds **articles** that are **sortable** (drag the grip) and can be **activated / deactivated**
  (active = still to buy, inactive = in the cart).
- An article has **multiple categories**; shoppers filter by category chips (a pick includes all its
  sub-categories) and by free text.
- **Categories form a multi-parent hierarchy** (e.g. *Cheese* sits under both *Dairy & Eggs* and *Deli*).

| Part | Folder | URL |
|---|---|---|
| API (ASP.NET Core, SQLite) | `backend/ShopMate.Api` | http://localhost:5871 (Scalar UI: `/scalar`, OpenAPI: `/openapi/v1.json`) |
| SPA (Vue 3 + Vite) | `frontend` | http://localhost:5872 (proxies `/api` to the API) |

## Running it

```bash
# API - creates + seeds shopmate.db on first start
cd backend/ShopMate.Api
dotnet run --launch-profile http
# start over with a fresh, re-seeded database:
dotnet run --launch-profile http -- --ResetDatabase=true

# SPA
cd frontend
npm install
npm run dev        # http://localhost:5872 (strictPort)
npm run build      # vue-tsc -b + vite build
```

Requirements: .NET SDK 10, Node 24 / npm 11.

## Back-end

Template `BasicApi` (no authentication) from the Regira bootstrap guide: Serilog, OpenAPI + Scalar,
`ConfigureDefaultJsonOptions()`, a central `api` route prefix, SQLite with `Foreign Keys=True`,
`EnsureCreated()` and seeding between `Build()` and `Run()`.

### Entities (free tier: 3 simple / 1 complex)

| Entity | Registration | Notes |
|---|---|---|
| `Shopper` | simple | name, e-mail, avatar colour |
| `ShoppingList` | simple + `ShoppingListSearchObject` | FK `ShopperId`; shopper eager-loaded on every row; `ShoppingListProcessor` fills `articleCount` / `activeCount` |
| `Category` | simple + `CategorySearchObject` | multi-parent DAG via the owned self-join `RelatedCategory` (`ParentEntities` / `ChildEntities`, both `Related()`); `CategoryProcessor` fills `articleCount` |
| `Article` | **complex** (`ArticleSortBy`, `[Flags] ArticleIncludes`) | FK `ShoppingListId`; owned m2m join `ArticleCategory` via `Related()`; normalized full-text (`q`) over title + note |

Design decisions

- **Articles are a top-level entity** (not an owned child of the list) so they can be searched, filtered
  and paged server-side (`/articles/search?q=&categoryId=&isActive=&shoppingListId=&shopperId=`).
- **`SortOrder` is `[ServerOwned]`**: a PUT/PATCH of one article can never shuffle its siblings. It is minted
  on create (appended to the end of the list, in a prepper) and rewritten only by the collection-level domain
  action `POST /api/articles/reorder { shoppingListId, ids[] }`.
- **Activate / deactivate** is a single-field `PATCH /api/articles/{id} { "isActive": false }`.
- Category filter: the SPA expands a picked category to its descendants (the DAG is small reference data
  and already loaded client-side) and sends the id set as `categoryId=..&categoryId=..`.
- Delete behaviour: shopper -> lists -> articles cascade; a category still used by articles answers **409**
  (`Restrict`); removing a category removes only its hierarchy links.
- An unknown `ShoppingListId` on an article is a field-level **400** (prepper), not a 409.

### Seed data (through `IEntityService`, Bogus)

`Data/Seeding/DataSeeder.cs` seeds, in waves and only when the database is empty:
55 categories (14 roots, topological waves so every parent exists before its children),
8 shoppers, 31 lists (every shopper has a pinned "Weekly groceries" + 2-4 themed lists) and
**500 articles** drawn from a correlated product catalog (`SeedCatalog.cs`: product + unit + quantity range +
categories travel together; notes are picked per category branch). About 60 % of the articles are still to buy.

## Front-end

Full reference scaffold of `@regira/modules` (`scaffold.mjs --shell --no-auth` + one slice per entity,
`Article --owns ArticleCategory --as categories --picker Category`), then restyled mobile-first:

- **App bar** (brand, global article search, shopper switcher) and a **bottom tab bar** built from
  `config.json -> navigation` via `useNavigation()`.
- **Home**: the current shopper's lists as large cards with progress, "New list", and the config-driven dashboard.
- **List board** (`/lists/:id`), the in-store screen:
  - big check circles to tick articles off; **swipe right** = bought / put back, **swipe left** = delete (confirmed);
  - **drag the grip** to reorder (persisted via `/articles/reorder`; disabled while a filter narrows the list;
    ArrowUp / ArrowDown on a focused grip);
  - sticky **search + category chips** (root chips, then a second row of sub-categories);
  - "in the cart" section with *Uncheck all* / *Clear the cart*;
  - **quick add** bar that understands `2 kg apples`, `6 eggs` and copies the categories of an earlier
    article with the same name;
  - tap an article to edit it in a modal (quantity stepper, unit, note, list, category chips).
- **Articles** overview: server-side paging + count, status segments (All / To buy / Bought), category chips,
  advanced filter (list, shopper, status); rows swipe like on the board.
- **Categories**: list with parents and article counts; the form has a *Hierarchy* tab with an owned-m2m chip
  editor for the parent categories (`InputSelectorInline`, `_deleted` marking) and the read-only sub-categories.
- **Shoppers**: simple entity edited in a modal; the "who is shopping" choice is kept per device (localStorage).

App-specific classes use the `sm-` prefix; the theme lives in `src/assets/theme.scss`.

## Verification

- `dotnet build` - 0 warnings, 0 errors; startup logs `3 simple / 1 complex registered -> tier = free` and no warnings.
- API round-trips: create -> update -> update again -> re-read (categories survive, `SortOrder` unchanged),
  PATCH `isActive`, reorder, category filter, 400 on an unknown list, 409 on deleting a used category,
  category parent links saved twice without duplicates.
- `npm run build` (vue-tsc -b + vite build) green; driven in the browser at 375 px and 1280 px: toggle, swipe
  both ways, drag-reorder, quick add, filters restored from the URL after reload, category hierarchy edits,
  new list defaults to the current shopper, no horizontal overflow.

## Build statistics

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls | 15 | 14 | **29** |
| Wall-clock time | ~8 min (12:19 - 12:27) | ~20 min (12:27 - 12:47) | **~28 min** |
| Tokens (cumulative context processed, harness counter) | ~0.20 M | ~0.24 M | **~0.44 M** |

Cost: most of those tokens are cached context re-reads; at typical Opus-class rates with prompt caching this
lands roughly in the **USD 2 - 5** range (an estimate - the exact billed amount is not visible from inside the session).

## Credits

Built by **Claude** (Anthropic), model *Claude Opus 5.5*, running as a **Claude Code sub-agent**
(general-purpose agent type) with **low reasoning effort**, following the Regira MCP golden path
(bootstrap guide -> package cards -> scoped guides -> generators). Framework: [Regira](https://regira.com).
