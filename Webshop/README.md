# Nordlys Store - Webshop

A full-stack webshop built on the **Regira** framework: a .NET 10 Entities API and a Vue 3 SPA that contains
both a polished **storefront** (catalog, filters, product pages, cart, checkout) and a **back office**
(orders, products, categories, brands and promotional banners).

| Part | Folder | URL |
|---|---|---|
| Back-end API (ASP.NET Core, Regira Entities, SQLite) | `Webshop.Api/` | http://localhost:5881 (Scalar UI: `/scalar`, OpenAPI: `/openapi/v1.json`) |
| Front-end SPA (Vue 3, Vite, Bootstrap 5, @regira/modules) | `webshop-spa/` | http://localhost:5882 (storefront `/`, back office `/admin`) |

## Running it

```bash
# API - creates and seeds webshop.db on first start
cd Webshop.Api
dotnet run --launch-profile http                 # http://localhost:5881
dotnet run --launch-profile http -- --ResetDatabase=true   # drop + recreate + reseed (Development only)

# SPA - the Vite dev server proxies /api to the API
cd webshop-spa
npm install
npm run dev                                      # http://localhost:5882 (strictPort)
npm run build                                    # vue-tsc -b + vite build
```

## Features

### Storefront (`/`)
- **Home**: rotating hero carousel and compact banners driven by live `Promotion`s, USP strip, category tiles
  (with live product counts), product rails (staff picks, hot deals, new arrivals, top rated) and a newsletter block.
- **Catalog** (`/shop`): server-side filtering on category, brand (multi), price range, rating, on sale, in stock and
  staff picks; keyword search; six sort orders; server-side paging (24 per page). Every filter lives in the URL, so
  promotion banners deep-link into filtered views (e.g. `/shop?categoryId=1&onSale=true`). Only published products are listed.
- **Product page**: large visual, sale badge and savings, rating, stock status, quantity stepper, add-to-cart / buy-now,
  highlights, description and shipping tabs, related products.
- **Cart** (`/cart`): persisted in `localStorage`, prices/stock re-validated against the API on open,
  free-shipping progress bar, savings, quantity changes and removal.
- **Checkout** (`/checkout`): contact, address, delivery method (Standard / Express / Pickup) and payment method
  (demo - no payment data is collected), live totals, terms checkbox, optional "remember my details".
  Only product ids and quantities are sent - **the API prices every line itself**.
- **Order confirmation** (`/order/:id/confirmation`): order number, status timeline, lines, totals and address.
- Product imagery: an `imageUrl` when set, otherwise a generated visual in the category's colour and icon.

### Back office (`/admin`)
The scaffolded Regira app shell (config-driven dashboard + navbar) with one entity slice per entity:
- **Orders**: filter on code, email, status, total range; sortable; tabbed details page with customer/shipping data,
  status workflow and an editable **order lines table** (add via server-side product picker, quantity edits,
  undoable `_deleted` removal) - totals are recomputed by the server on every save.
- **Products**: filters (category, brand, price, published, on sale, in stock, featured) and sorting; tabbed form
  (details + pricing & stock) with relation pickers and a live storefront preview.
- **Categories** and **Brands**: lookup entities edited in modals.
- **Promotions**: banner editor with theme, icon, validity period and a live banner preview.

## Back-end design

**Regira Entities free-tier budget (5 simple + 2 complex)** - confirmed at startup:
`Regira.Entities: 3 simple / 2 complex registered -> tier = free`.

| Entity | Registration | Notes |
|---|---|---|
| `Category` | simple `For<Category>()` | `CategoryProcessor` fills `ProductCount`; delete of a used category returns 409 (`Restrict`) |
| `Brand` | simple `For<Brand>()` | normalized full-text `?q=` |
| `Promotion` | simple `For<Promotion, int, PromotionSearchObject>()` | `?live=true` = active and inside its validity period (`IHasStartEndDate`) |
| `Product` (primary, 500 seeded) | complex `For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>()` | `ProductQueryBuilder` (category, brand, price, rating, on sale, in stock, featured, published); typed sorting; category + brand eager-loaded for every row |
| `Order` | complex `For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>()` | owns `OrderLine` via `e.Related()` (no slot); lines gated behind `?includes=Lines` (Details loads them) |

Key behaviours:
- **Server-owned order data**: `Code` is minted on create with `e.ServerOwned(x => x.Code, ...)` and restored on update;
  `OrderPrepper` resolves unit prices and titles from the products (price-tampering guard; existing lines keep their
  ordered price), validates lines (at least one, quantity 1-99, product exists and is published, stock on new orders)
  and computes `ItemCount`, `Subtotal`, `ShippingCost` (free over EUR 50) and `Total` - including the
  "lines not sent" case of a status-only PATCH. Validation failures return a 400 field map.
- **Stock reactor**: `OrderStockReactor` runs after the commit - a placed order takes stock, a cancelled order puts it back.
- Money is stored as SQLite REAL (`decimal` -> `double` conversion) so range filters and `ORDER BY` compare numerically.
- `ConfigureDefaultJsonOptions()` (enum names, UTC dates, 400/409 mapping), central `api` route prefix,
  OpenAPI + Scalar, Serilog (console + `logs/`), `ValidateOnBuild`, SQLite with foreign keys on.

**Seeding** (`Data/Seeding/WebshopSeeder.cs`) runs on an empty database, entirely through the `IEntityService`
implementations (so preppers, normalizers, primers and reactors run exactly as for API writes), using Bogus with a fixed seed:
10 categories, 32 brands, **500 products** (brand, product type, price range and features drawn together per
category so names and descriptions are coherent; ~22% on sale, ~7% sold out, ~3% unpublished), 7 promotions
(live, expired and inactive) and 350 orders over the last 180 days whose status follows their age
(recent = pending/paid, older = shipped/delivered, ~5% cancelled).

## Project layout

```
Webshop/
  Webshop.Api/
    Program.cs, Infrastructure/HostingExtensions.cs, Extensions/ServiceCollectionExtensions.cs
    Controllers/Controllers.cs            # one EntityControllerBase per entity
    Data/WebshopDbContext.cs, Data/Seeding/{CatalogData,WebshopSeeder}.cs
    Entities/{Categories,Brands,Promotions,Products,Orders}/...
  webshop-spa/
    public/config.json, public/data/translations.json
    src/entities/<slice>/...              # scaffolded Regira slices (admin)
    src/shop/{ShopLayout.vue,cart.ts,catalog.ts,money.ts,components/,views/}   # storefront
    src/assets/{theme.scss,shop.scss}
```

## Notes and limitations
- No authentication: the back office is open (as requested scope did not include accounts). Adding the Regira auth
  plugin + `SelfHostingApiWithAuth` registrations would gate `/admin` and the write endpoints.
- Payments are simulated; no card or account data is ever requested.
- Editing lines of an existing order does not adjust stock (only placing and cancelling do).

## Build & verification status
- `dotnet build`: succeeded, 0 warnings, 0 errors; no vulnerable packages.
- `npm run build` (`vue-tsc -b` + `vite build`): succeeded.
- Verified at runtime: API create -> update -> update -> PATCH round-trips (code/totals survive), 400 validation, 409 on
  referenced delete, stock decrement/restock; in the browser: filters/sort/paging via URL, add to cart, full checkout,
  confirmation page, admin order line add/remove + double save, product and promotion double save, category modal.

## Project statistics

| Phase | Wall-clock time | Regira MCP calls | Tokens (approx., session budget consumed) |
|---|---|---|---|
| Back-end (API, seeding, API verification) | ~11 min (12:19 - 12:30) | 16 | ~221k |
| Front-end (SPA, storefront, admin slices, browser verification, README) | ~23 min (12:30 - 12:53) | 14 | ~221k |
| **Total** | **~33 min** | **30** | **~442k** |

Token figures are read from the session's remaining-token counter, so they are approximate; an exact monetary cost
is not observable from inside the session and is therefore not reported.

## Credits

Built by **Claude** (Anthropic), model **Claude Opus 5.5**, running as a **Claude Code general-purpose sub-agent**
with **low reasoning effort**, following the Regira MCP documentation server's golden path
(bootstrap guide -> package cards -> heading-scoped guides -> generators).
Framework: [Regira](https://regira.com) (`Regira.Entities.*` 6.4.0, `@regira/modules` 6.4.0).
