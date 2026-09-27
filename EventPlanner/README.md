# EventPlanner

Company event management: events at a location spanning one or more days, an agenda of sessions (each with
its own speakers and capacity), employee registrations with optional session selection, and an admin
back-office for locations, speakers, event categories and participant registrations.

| Part | Path | Stack | URL |
|---|---|---|---|
| Back-end API | `EventPlanner.Api/` | .NET 10, ASP.NET Core, EF Core 10 + SQLite, Regira Entities 6.4 (free tier), ASP.NET Identity + Regira JWT auth, Serilog, OpenAPI + Scalar | http://localhost:5821 (Scalar UI: `/scalar`) |
| Front-end SPA | `EventPlanner.Web/` | Vue 3 + TypeScript + Vite 8, Pinia, `@regira/modules` 6.4 (full scaffold), Bootstrap 5 + Bootstrap Icons | http://localhost:5822 |

## Running it

```bash
# API (creates + seeds eventplanner.db on first start; add -- --ResetDatabase=true to rebuild it)
cd EventPlanner.Api
dotnet run --launch-profile http

# SPA (proxies /api to the API, so both share one origin — no CORS)
cd EventPlanner.Web
npm install
npm run dev          # http://localhost:5822 (strictPort)
npm run build        # vue-tsc -b + vite build
```

### Demo accounts (seed data, development only)

| Role | User name | Password |
|---|---|---|
| Administrator (+ employee) | `admin@eventplanner.local` | `Admin123!` |
| Employee | `employee@eventplanner.local` | `Employee123!` |
| 150 generated employees | `firstname.lastname@eventplanner.local` | `Welcome123!` |

## Features

**Employees**
- Home: hero banner, stats, *my upcoming registrations*, featured and upcoming events.
- Event overview as a card grid (category-coloured banners, date chips, capacity bars), keyword search,
  advanced filter (category, location, speaker, dates, upcoming, featured) and server-side paging.
- Event page: banner, key facts, description, **agenda per day** (timeline with room, track, seats, speakers),
  **speaker cards**, and a sticky registration panel (pick sessions, notes, register / cancel / re-register;
  full events put new registrations on the **wait list**).
- *My registrations*: change the selected sessions, add notes, cancel.
- Speaker directory (cards) with profile page and the upcoming events they speak at.

**Administrators** (role `Admin`)
- Create/edit events with tabs: details, agenda editor (sessions as cards, speakers as chips), participants.
- Manage locations, speakers, event categories (colour + icon) and every registration (register any employee,
  set status incl. wait list).
- Read-only employee directory.

## Architecture

### Back-end (`EventPlanner.Api`)

```
Controllers/            entity controllers (EntityControllerBase), auth controllers, read-only EmployeesController
Data/                   EventPlannerDbContext (IdentityDbContext) + Seeding/ (DatabaseSeeder, SeedCatalog)
Entities/<Entity>/      model, DTOs, search object, query builders, preppers, processors, service configuration
Infrastructure/         HostingExtensions (DI + pipeline), Security/ (roles, ICurrentUser, write filter, claims factory)
```

**Regira Entities budget (free tier: 5 simple + 2 complex)** — logged at start-up as
`3 simple / 2 complex registered → tier = free`:

| Entity | Registration | Notes |
|---|---|---|
| `Location` | simple (`LocationSearchObject`) | normalized `?q=` search |
| `Speaker` | simple (`SpeakerSearchObject`) | normalized `?q=` search |
| `EventCategory` | simple | own `?q=` title filter |
| `Event` | complex (`EventSortBy`, `EventIncludes`) | aggregate root: owns `Session` → owns `SessionSpeaker` (nested `Related()`) |
| `Registration` | complex (`RegistrationSortBy`, `RegistrationIncludes`) | owns `RegistrationSession` (selected sessions) |

Employees are ASP.NET Identity users (`AppUser`), not Regira entities.

**Business rules**
- Every endpoint requires a signed-in user; writes on locations, speakers, categories and events are
  admin-only (`WriteAuthorizationFilter`, an allow-list keyed on the controller).
- Row scoping via global filter query builders: employees only see (and so only modify/delete) their own
  registrations; draft events are hidden from employees.
- `RegistrationPrepper`: employees always register themselves (user stamped from the token), event must be
  published and not ended, one registration per employee per event, confirmed/wait-list status from the
  event capacity, selected sessions must belong to the event and have seats left (counting rows pending in
  the same `SaveChanges` batch). `EventId`/`UserId` are `[ServerOwned]` (immutable after create).
- `EventPrepper`: end date ≥ start date, sessions inside the event, end after start, duplicate speakers removed.
- `EventProcessor` fills registration/wait-list counts, seats taken per session and the caller's own registration id.
- Referential integrity (SQLite `Foreign Keys=True`): deleting a location, category or speaker in use, or an
  event with registrations, answers **409**; removing a session removes it from everyone's selection.
- Times: `Session.StartTime/EndTime` are UTC instants; `Event.StartDate/EndDate` are `DateOnly`.

**Seed data** (through the `IEntityService` implementations, Bogus + correlated tuples in `SeedCatalog`):
10 categories, 20 locations, 80 speakers, 152 users, **500 events** (primary entity) with ~2,100 sessions and
~3,100 speaker links, and ~10,000 registrations with session selections. Verified invariants: no event or
session over capacity, wait list only on full events, cancelled events only have cancelled registrations,
no registrations on drafts, no registration created after its event started.

### Front-end (`EventPlanner.Web`)

Full Regira scaffold (`scaffold.mjs --shell` + one slice per entity) with custom views:

| Slice | Folder | Notes |
|---|---|---|
| `EventItem` | `src/entities/events` | card grid, `EventPage` (fiche), tabbed editor, `sessions/` owned-collection editor, participants tab |
| `Registration` | `src/entities/registrations` | `SessionPicker` checklist for the owned session selection |
| `Speaker` | `src/entities/speakers` | card grid, profile + "speaks at" |
| `LocationItem` | `src/entities/locations` | |
| `EventCategory` | `src/entities/event-categories` | modal form (colour + icon preview) |
| `Employee` | `src/entities/employees` | read-only directory (`GET /api/employees`) |

`EventItem` / `LocationItem` avoid shadowing the DOM globals `Event` / `Location`. `src/access.ts` mirrors the
API's write tiers so the UI only offers actions the caller may perform. The theme lives in `src/assets/theme.scss`
(`ep-*` classes).

**Deliberate deviations from the scaffold defaults**
- Event and speaker overviews render cards instead of table rows; the related category/location on an event
  card is shown as text (open them from the event editor's selectors).
- The session selection of a registration is a checklist of the event's sessions (bounded set that needs
  time/room/seats per option) instead of `InputSelectorInline` chips; unticking a stored row marks it `_deleted`.
- Existing events open on the event page (`EventItemFiche`); administrators switch to the editor from there.

## Verification

- `dotnet build` — 0 warnings, 0 errors; start-up logs no Regira warnings.
- `npm run build` (vue-tsc -b + vite build) — green.
- API smoke script: create → update → update → PATCH → re-read (owned sessions + nested speakers survive),
  400s from preppers, 403 for employee writes, 404 on foreign registrations, 409 on referenced deletes,
  per-identity counts (admin 500 events / employee 485, admin all registrations / employee own).
- Browser (Vite dev server): login, home, event cards + paging, event page, admin edit saved twice, add session
  + speaker chip saved twice, participants tab, employee registration with sessions, cancel + re-register,
  manage registration saved twice, role gating on every overview, no horizontal overflow at 375 px.

## Project statistics

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls | 26 | 15 | **41** |
| Wall-clock time | ~11 min | ~21 min (incl. browser verification) | **~32 min** (12:19 - 12:51) |
| Tokens (approx., from the agent's budget counter) | ~240k | ~235k | **~475k** |

Cost estimate: the token counter mixes cached and uncached input, so no exact figure can be given; at typical
Opus-class list prices with heavy prompt caching this corresponds to roughly USD 3-8.

## Credits

Built end-to-end by **Claude** (Anthropic, model Opus 5.5) running as a **Claude Code sub-agent**
(general-purpose agent type, reasoning effort: low), following the Regira MCP golden path
(`get_bootstrap_guide` → package cards → targeted guide sections → scaffold generators).
