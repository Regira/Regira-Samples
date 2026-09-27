# The Margin - a Regira blog

A full-stack blog: a .NET 10 API (Regira Entities) that manages and publishes blog posts, and a Vue 3 SPA
(`@regira/modules`) with an editorial reader site plus a management "Studio".

| Part | Folder | URL |
|---|---|---|
| Back-end API | `Blog.Api/` | http://localhost:5811 (OpenAPI `/openapi/v1.json`, Scalar UI `/scalar`) |
| Front-end SPA | `Blog.Web/` | http://localhost:5812 (strict port; `/api` is proxied to the API) |

## Features

- **Blog posts** with title, slug, summary, Markdown content, cover image, author, category, tags,
  publish switch + publication date (a future date schedules the post), featured flag and a server-computed
  reading time.
- **Categories** (colour, description, sort order) and **tags** - each fully manageable (create, edit, delete);
  post/published counts per category and tag.
- **Public pages show published posts only** - "published" means `IsPublished` **and** `PublishedAt <= now`,
  so drafts and scheduled posts never reach readers:
  - `/` - front page: featured lead stories, category pills, keyword search, tag / year / sort filters,
    active-filter chips, paged card grid (all state lives in the URL, so every view is deep-linkable).
  - `/read/:slug` - article page: category, headline, dek, byline, cover, rendered article, tags,
    "More from {category}".
- **Studio** (`/manage`): status counters (published / scheduled / draft / featured) and the generated
  management slices for posts (page form with tabs: *Article* and *Content* with a live Markdown preview),
  categories and tags (modal forms). The post overview filters on keywords, category, tag, status, year,
  author, featured and sort order.

## Back-end (`Blog.Api`)

- Template: `BasicApi` (no authentication was requested), Serilog, OpenAPI + Scalar, SQLite +
  `EnsureCreated()`, Mapster, every controller under the central `api` route prefix.
- Entity budget (free tier = 5 simple + 2 complex): `Category` simple, `Tag` simple, `BlogPost` complex
  (typed `BlogPostSortBy` + `[Flags] BlogPostIncludes`), `BlogPostTag` is an owned many-to-many join synced by
  `e.Related(x => x.Tags)` (no slot). Startup logs `2 simple / 1 complex registered -> tier = free`.
- Pipeline pieces:
  - `SlugPrepper<T>` - generates a unique slug from the title when empty (also counts rows pending in the change tracker).
  - `BlogPostPrepper` - derives `ReadingTimeMinutes`; stamps `PublishedAt` on first publication and keeps the stored date otherwise.
  - `BlogPostQueryBuilder` - filters: `categoryId`, `categorySlug`, `tagId`, `tagSlug`, `slug`, `author`, `isPublished`,
    `status` (Draft/Scheduled/Published), `isFeatured`, `minPublishedAt`/`maxPublishedAt`, `year`; `?q=` is the global
    normalized-content search over title, summary and author.
  - `CategoryProcessor` / `TagProcessor` - post counts; `hasPublishedPosts` filters on both.
- Integrity: deleting a category that still has posts answers **409** (`Restrict`); deleting a tag removes it from its
  posts (deliberate `Cascade` - tags are disposable labels). SQLite foreign keys are enabled in the connection string.
- **Seeding** (`Data/Seeding/BlogSeeder.cs`) runs through the `IEntityService` implementations (preppers, normalizers
  and primers run as for API writes) with Bogus (fixed seed): 10 categories, 54 tags, **500 posts** (~82% live,
  ~6% scheduled, ~12% drafts, ~7% of live posts featured), back-dated `Created`/`PublishedAt`, topic-consistent tags,
  generated Markdown bodies and picsum.photos cover images (loaded from the internet).

Run:

```bash
cd Blog.Api
dotnet run --launch-profile http                       # http://localhost:5811/scalar
dotnet run --launch-profile http -- --ResetDatabase=true  # drop, recreate and reseed blog.db
```

## Front-end (`Blog.Web`)

- Scaffolded with `scaffold.mjs --shell --no-auth` and one slice per entity
  (`BlogPost --api posts --rel Category --owns BlogPostTag --as tags --picker Tag`, `Category`, `Tag`),
  then customised: models, filters, lists, forms (tabs, `InputSelectorInline` tag chips, `DateInput`), studio dashboard.
- Public reader pages are custom views (`src/views/blog/`) on the pooled entity stores; Markdown is rendered by a
  small, HTML-escaping renderer (`src/utilities/markdown.ts`).
- Design: warm paper background, Fraunces display headlines, Source Serif body text, Inter UI text; responsive
  down to phone width (the masthead section strip scrolls horizontally).

Run:

```bash
cd Blog.Web
npm install
npm run dev      # http://localhost:5812 - start the API first
npm run build    # vue-tsc -b + vite build
```

## Build status

- `dotnet build` - succeeded, 0 warnings, 0 errors.
- `npm run build` - succeeded (vue-tsc type-check + Vite production bundle).
- Verified at runtime: seeded counts and invariants through `/search`, create -> update -> update again -> PATCH round-trip,
  slug de-duplication, 400 on a missing title, 409 on deleting a used category; in the browser: front-page filters,
  paging, article page, hidden drafts/scheduled posts (404 view), studio counters, post form double save with tag
  chip removal and addition, new post creation, mobile viewport without horizontal scroll.

## Project metrics

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls | 18 | 10 | **28** |
| Wall-clock time | ~9 min (12:19 - 12:28) | ~16 min (12:28 - 12:44) | **~25 min** (+ wrap-up) |
| Tokens (session budget counter) | ~201k | ~174k | **~375k** |

Token figures are read from the agent's session token-budget counter (context tokens processed), not from a billing
report; an exact dollar cost is not observable from inside the agent session - check the Claude usage dashboard for it.

## Credits

Designed and built by **Claude** (Anthropic), model **Claude Opus 5.5**, running as a Claude Code sub-agent
(general-purpose agent type) with **low reasoning effort**, following the Regira MCP golden path
(`get_bootstrap_guide` -> package cards -> instructions -> scaffolds). No other projects on disk and no memory were
used as a reference.
