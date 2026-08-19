# Webshop

A standalone e-commerce demo application built on the **Regira** framework (.NET 10 back-end +
Vue 3 front-end), created end-to-end through the **Regira MCP server**.

Customers can browse a catalog of ~500 products across 14 categories, filter and sort the catalog,
view product detail pages, build a shopping cart, and complete a guest checkout that creates a real
order against the back-end API.

---

## Ports

| App | URL |
|---|---|
| Back-end API (Scalar docs at `/scalar`) | http://localhost:6180 |
| Front-end SPA | http://localhost:6181 |

## Running it

**Back-end**

```bash
cd backend/Webshop.Api
dotnet run --urls http://localhost:6180
```

On first run it creates `webshop.db` (SQLite) and seeds the catalog automatically (see *Seeding*
below). Delete `webshop.db` to force a full reseed (schema changes are not migrated automatically —
this is the standard `Database.EnsureCreated()` starter setup, not EF migrations).

**Front-end**

```bash
cd frontend
npm install
npm run dev
```

The Vite dev server proxies `/api/*` to `http://localhost:6180`, so start the back-end first.

`npm run build` runs `vue-tsc -b` (strict type-check) followed by the production Vite build; both
complete cleanly with zero errors.

---

## Architecture

### Back-end &mdash; `backend/Webshop.Api`

ASP.NET Core 10 Web API on the Regira Entities stack (`Regira.Entities.Web` +
`Regira.Entities.Mapping.Mapster`, EF Core + SQLite, Serilog, OpenAPI/Scalar). No authentication
(`BasicApi` template) &mdash; this is a public storefront + guest checkout, not an authenticated back
office.

**Entities** (free tier: 5 simple + 2 complex registrations &mdash; this app uses 1 simple + 2 complex):

| Entity | Classification | Notes |
|---|---|---|
| `Category` | simple | Reference data. Not `IArchivable` on purpose &mdash; a required FK from `Product` behind soft-delete would silently drop rows from list `items` while `/search` kept counting them (see the framework's soft-delete guidance for reference data). Products are hard-deleted on `OnDelete(Restrict)` instead. |
| `Product` | complex (`TSortBy` + `TIncludes`) | Complex so the shop page can offer server-side sorting (price, rating, newest, title) via `?sortBy=`. `Category` is eager-loaded unconditionally (a to-one shown on every product card). |
| `Order` | complex | Owns `OrderLines` via `e.Related()` &mdash; no separate registration, no controller, no budget slot. A `Prepare` hook resolves `UnitPrice`/`SubTotal` from the current `Product.Price` server-side (price-tampering guard) and recomputes `Total`. `OrderManager` (an `EntityWrappingServiceBase`) rejects a save with zero order lines and mints the `ORD-XXXXXXXX` code on create. |
| `OrderLine` | owned child of `Order` | No own `.For<>()`/controller &mdash; edited only through the parent. |

**No separate `Customer` entity.** This is a guest-checkout storefront (no login), so
name/e-mail/phone/shipping address live directly on `Order`. That keeps the entity budget small and
matches how most demo webshops actually check out.

### Front-end &mdash; `frontend/`

Vue 3 + TypeScript + Vite + Pinia + vue-router. Built on the Regira Vue module family's **headless
tier** (`regira_modules.vue.entities` &rsaquo; *Headless quick-start*): the shared `initAxios` HTTP
client from `@regira/modules/vue/http` talks to the API, but the storefront UI (product cards, promo
banners, cart, checkout) is fully hand-built rather than generated from the framework's admin-CRUD
scaffold (`scaffold.mjs --shell` + per-entity List/Details/Form slices). That scaffold is optimised
for back-office data management; a customer-facing shopping experience with cart/checkout doesn't fit
that shape, and the guide explicitly sanctions a bespoke-UI/headless build for exactly this case
(*"Hand-rolling one of these on a lean or headless build is a deviation to declare, not a shortcut"* &mdash;
declared here). The `@regira/modules/vue/ui` component kit (Feedback, Paging, LoadingContainer, ...)
was likewise skipped in favour of hand-built, storefront-styled equivalents (toasts, pagination,
skeleton loaders) for visual cohesion with the custom design system.

```
src/
  api/            typed axios calls (categories, products, orders) against the Regira Entities wire
                  contract ({ item } / { items, count } envelopes, field-error responses)
  types/models.ts hand-written DTO types mirroring the back-end DTOs
  stores/         Pinia: cart (persisted to localStorage), toast notifications
  components/
    layout/       header (search, cart badge, mobile nav), footer
    home/         hero, category grid, promo banners, USP strip, product rails
    product/      ProductCard, RatingStars, PriceTag
    shop/         FilterSidebar (category/brand/price/availability)
    common/       Pagination, ToastContainer
  views/          Home, Shop, ProductDetail, Cart, Checkout, OrderConfirmation, NotFound
```

Pages: **Home** (hero banner, category grid, two promo banners, featured/on-sale product rails) &middot;
**Shop** (full catalog with category/brand/price/availability filters, sort, pagination) &middot;
**Product detail** (gallery, rating, price, stock, related products) &middot; **Cart** &middot;
**Checkout** (shipping form, order summary, submits to the API) &middot; **Order confirmation**.

---

## Seeding

Seeded once on first run via `IEntityService<T,TKey>` (never raw `DbContext.Add`), using
[Bogus](https://github.com/bchavez/Bogus) for realistic names/addresses/commerce text, with a fixed
seed (`20260819`) for reproducibility:

- **14 categories** (Electronics, Fashion, Home & Kitchen, ...)
- **500 products** &mdash; the primary entity &mdash; spread evenly across categories, with brand,
  price, an optional sale price (`CompareAtPrice`, ~22% of products), stock (~8% intentionally at 0
  for an out-of-stock state), rating, review count and a `Created` date spread across the last ~300
  days (so "Newest" sorting and any recency badge aren't a 0%/100% bucket)
- **150 orders**, 1&ndash;5 lines each, statuses weighted across all five `OrderStatus` values and
  skewed so newer orders lean Pending/Processing and older ones lean Delivered/Cancelled (verified
  distribution: Pending 16, Processing 15, Shipped 22, Delivered 81, Cancelled 16 &mdash; no bucket
  sits at 0% or 100%)

---

## What was verified

- `dotnet build` &mdash; 0 warnings, 0 errors. `npm run build` (`vue-tsc -b` + Vite) &mdash; 0 type
  errors.
- Startup log confirms the entity budget: `Regira.Entities: 1 simple / 2 complex registered → tier =
  free`.
- Golden-path round trip via the live API: create an order &rarr; `PATCH` status only &rarr; re-read
  &mdash; `Code` and `Total` (server-owned fields excluded from `OrderInputDto`) survive the partial
  update unchanged, confirming the `Prepare` hook's restore logic.
- Full browser walkthrough: home &rarr; shop (filter/sort/paginate 500 products across 42 pages)
  &rarr; product detail (slug-based routing, related products) &rarr; add to cart &rarr; cart review
  &rarr; checkout form &rarr; order created against the API &rarr; confirmation page with the real
  order code and line items.
- This walkthrough caught and fixed one real bug: the checkout page redirected back to `/cart`
  instead of the confirmation page, because a live `watch` on "cart is empty" fired the instant a
  successful order cleared the cart, racing the post-order navigation. Fixed by replacing the live
  watch with a one-time `onMounted` guard (only redirect away from checkout when it's *entered* with
  an empty cart) and awaiting the confirmation navigation before releasing the submit button.

---

## MCP calls, time and cost

Tracked for this build session (Regira MCP server calls, `https://mcp.regira.com/mcp`):

| | Regira MCP calls |
|---|---|
| Back-end (bootstrap guide, package docs, `how_to` recipes, `get_type` signature checks) | 21 |
| Front-end (bootstrap guide, package docs, `get_type`) | 6 |
| **Total** | **27** |

Wall-clock time for the full build (project scaffolding through verified, seeded, working app):
**~35 minutes**.

Token/cost accounting isn't exposed to the agent inside this session, so an exact dollar cost can't
be reported from here; the Regira MCP call count above is the closest available proxy for how much
external documentation was pulled in versus generated from local reasoning.

---

## Credits

Built by **Claude** (model **Claude Sonnet 5**), running as the **Claude Code** CLI agent, in one
continuous session with no explicit extended-thinking/reasoning-effort override configured (default
effort). Scaffolding, package/API decisions and UI design followed the Regira MCP server's bootstrap
guides and package documentation throughout.
