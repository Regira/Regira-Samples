# Regira Blog

A standalone demo application built on the Regira framework: a blog publishing platform with full
content management (posts, categories, tags) and a public, editorial-style reading experience.

Built with the **Regira MCP server** as the source of truth for every framework decision (package
choice, entity classification, scaffolding commands, conventions) — no prior framework knowledge was
assumed; everything below follows directly from the MCP guides and generators.

## Stack & ports

| App | Tech | Port |
|---|---|---|
| Back-end API | ASP.NET Core 10 (`Regira.Entities`), SQLite, OpenAPI + Scalar | http://localhost:6110 |
| Front-end SPA | Vue 3 + TypeScript + Vite (`@regira/modules`) | http://localhost:6111 |

Port 6112 (reserved for an optional separate public site) was **not used** — the public blog and the
admin/management UI are both served from the one SPA (see *Design decisions* below), keeping the app
simpler without losing any required functionality.

## Running it

**Back-end**

```bash
cd backend/Blog.Api
dotnet run
```

Serves the API at `http://localhost:6110`, opens Scalar (`/scalar`) for interactive docs, creates
`blog.db` (SQLite) on first run and seeds it automatically. Delete `blog.db` to force a full reseed
(the model has changed since a run's last seed, or you just want fresh random data).

**Front-end**

```bash
cd frontend/blog-spa
npm install
npm run dev
```

Serves the SPA at `http://localhost:6111` (fixed port, `vite.config.ts`). The API origin is configured
directly in `public/config.json` (`http://localhost:6110`) with CORS enabled on the API — no dev proxy
needed.

Both `npm run build` (`vue-tsc -b && vite build`) and `dotnet build` complete cleanly with **zero
warnings and zero errors**.

## Data model

- **Category** — `Title`, `Slug`, `Description`, computed `PostCount` (via an `IEntityProcessor` that
  counts posts per category on read). Simple registration.
- **Tag** — `Title`, `Slug`. Simple registration, edited as a modal (a textbook flat lookup table).
- **BlogPost** — `Title`, `Slug`, `Summary`, `Content`, `CoverImageUrl`, `IsPublished`, `PublishedAt`,
  a required `Category` (to-one, eager-loaded unconditionally), and a `Tags` many-to-many collection.
  Complex registration (typed `SortBy` + `Includes`) for filtering by category/tag/published-state and
  sorting by title/publish date.
- **BlogPostTag** — the many-to-many join entity between `BlogPost` and `Tag`, owned by `BlogPost` via
  `e.Related(x => x.Tags)`. No own registration, no controller, no budget slot — edited inline as chips
  on the post form.

**Entity budget** (free tier = 5 simple + 2 complex): Category + Tag = 2 simple, BlogPost = 1 complex.
Comfortably within budget; logged at startup as
`Regira.Entities: 2 simple / 1 complex registered → tier = free`.

## Design decisions

- **One SPA, two audiences.** `/` is the public, editorial blog (only published posts, filterable by
  category and free-text search, card-based overview + a slug-addressed detail page,
  `/posts/:slug`). `/admin` is the management dashboard (full CRUD over posts/categories/tags,
  paging, filtering, soft validation) built from the standard Regira entity scaffold. Both share the
  same API and the same pooled entity stores — publishing a post from the admin form is reflected
  immediately if the public overview is revisited.
- **No authentication.** The spec didn't call for user accounts, so the app uses the `BasicApi`
  back-end template and the SPA's no-auth variant (`scaffold.mjs --shell --no-auth`) throughout —
  every route is anonymous.
- **Editorial front-end.** Serif display type (Playfair Display) for headings, a warm paper background,
  category chip filters, and card hover elevation — layered on top of the standard Regira UI kit via
  `src/assets/theme.scss` (theme tokens + a handful of page-specific classes), not a fork of the library.
- **Slug-based public routing.** The back-end's `BlogPostSearchObject` gained a `Slug` equality filter
  (beyond the scaffolded category/tag/published-state/date filters) purely to resolve `/posts/:slug` on
  the public site through the same `/blog-posts/search` endpoint the admin overview already uses.

## Seeding

Seeded once, automatically, on first run (`Database.EnsureCreated()` + `IEntityService` seeding between
`builder.Build()` and `app.Run()`, per the framework's seeding recipe) using **Bogus** for realistic
content:

- 10 categories (Technology, Travel, Food & Cooking, Health & Wellness, Business, Lifestyle, Science,
  Culture & Arts, Sports, Personal Finance)
- 24 tags (tutorial, guide, how-to, trends, startup, remote-work, artificial-intelligence, …)
- **520 blog posts** — the primary entity — each with a random category, 1-4 random tags, a
  Lorem-ipsum title/summary/multi-paragraph body, a deterministic `picsum.photos` cover image, and a
  historical `Created`/`PublishedAt` timestamp spread over the last two years (~82% published, the rest
  drafts or a few scheduled-in-the-future posts) so the admin list and the public overview both look like
  a real, mature blog rather than a same-instant batch insert.

## Credits

Built by **Claude Sonnet 5**, running as **Claude Code** (Anthropic's CLI agent), at the default/auto
reasoning effort for this session — following the Regira MCP server's bootstrap guides, package docs and
code generators throughout, with no prior-session memory or sibling-project reference used.

## Session metrics (approximate)

- **Regira MCP calls:** ~34 (bootstrap guides, package cards, section/table-of-contents lookups,
  targeted heading reads, `get_type`/`get_example` signature checks) — roughly 23 for the back-end
  research pass and 11 for the front-end pass, before any code was written.
- **Wall-clock time:** documentation research ran first (untimed precisely, but the bulk of the MCP call
  volume above); from the first timestamp taken (directory setup) to the final verified build was
  **~31 minutes** — backend scaffold/build/seed/verify in ~4.5 minutes, frontend scaffold/customize/build
  in ~22 minutes, browser verification (and troubleshooting an unstable background-process environment
  for the live-server checks) in the remainder.
- **Tokens:** this session's context window started at a 15,000,000-token budget; by the end of the
  build roughly **360,000 tokens** of that budget had been consumed. A separate dollar-cost figure isn't
  available to the agent from within the session.
