# EventPlanner

A vibrant event-management demo app built on the **Regira** framework (`Regira.Entities` on the backend,
`@regira/modules` / Vue 3 on the frontend). Organizations create multi-day events at venues, build an
agenda of sessions with speakers and capacity limits, and employees register for events and optionally
pick individual sessions.

- **Back-end API:** http://localhost:6120 (Scalar UI at `/scalar`, OpenAPI at `/openapi/v1.json`)
- **Front-end SPA:** http://localhost:6121
- No authentication — this is an open internal-tool style demo (`--no-auth` scaffold), matching the spec's
  silence on login/credentials.

## Domain model

| Entity | Kind | Notes |
|---|---|---|
| **Location** (SPA: `Venue`) | simple | Venue: address, city, country, capacity, image |
| **Speaker** | simple | bio, job title, company, photo |
| **EventCategory** | simple | color + icon, edited as a modal (flat lookup) |
| **Employee** | simple | the people who register for events |
| **Session** | simple | belongs to an `Event` (FK), many-to-many with `Speaker` |
| **Event** (SPA: `EventItem`) | complex | venue + category (to-one), banner, multi-day start/end, `Sessions` back-ref |
| **Registration** | complex | employee + event, status (Pending/Confirmed/Cancelled/Attended), optional many-to-many with `Session` |

Two owned join entities ride on their parents via `e.Related()` and cost no registration slot:
`SessionSpeaker` (Session ↔ Speaker) and `RegistrationSession` (Registration ↔ Session, "which sessions
did this employee pick").

**Free-tier budget:** 5 simple + 2 complex = **7 registrations, exactly the hard ceiling** — the app logs
`Regira.Entities: 5 simple / 2 complex registered → tier = free` at startup with zero warnings.

`Event`/`Registration`/`Session` renamed `EventItem`/`Registration`/`Session` were kept as-is except `Event`
→ `EventItem` and `Location` → `Venue` on the **frontend only** (both collide with DOM globals — `window.Event`,
`window.Location` — flagged by the scaffolder; the backend class names and API routes are unaffected).

## Running it

**Backend** (SQLite, auto-created + seeded on first run):
```bash
cd backend
dotnet run --urls http://localhost:6120
```
Seeding is idempotent — it only runs when the `Registrations` table is empty. Delete `eventplanner.db*` to
reseed from scratch.

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```
`public/config.json` points the SPA straight at `http://localhost:6120/api` with CORS enabled on the API
(the "simplest dev setup" — no Vite proxy needed).

## Seed data

Generated with [Bogus](https://github.com/bchavez/Bogus), through `IEntityService` (never the raw
`DbContext`), in FK-ordered waves with backdated `Created` timestamps so date-based UI (upcoming/past,
registration trends) shows a real spread instead of everything "created just now":

| Entity | Rows |
|---|---|
| EventCategory | 8 |
| Location (Venue) | 18 |
| Speaker | 65 |
| Employee | 320 |
| Event | 70 |
| Session | 247 |
| **Registration** (primary entity) | **520** |

Registration status is weighted (Confirmed 50% / Pending 20% / Attended 20% / Cancelled 10%) so every
status bucket — and every session's seat-fill bar — has a realistic, non-degenerate distribution.

## Notable implementation details

- **Cross-entity aggregates via processors.** `Session.SeatsTaken` and `Event.SessionCount` /
  `RegistrationCount` are `[NotMapped]` fields filled by `SessionProcessor` / `EventProcessor`, which query
  the store directly — because `RegistrationSession` is owned by `Registration`, not by `Session`, and
  `Session`/`Registration` are independent entities with a back-ref to `Event`, not owned via `Related()`.
- **Nested-collection date hydration.** `Event.Sessions` arrives on the Details page as a nested
  `SessionCoreDto[]` (Details always eager-loads every registered include) — plain JSON with string dates,
  not the pooled `Session` model. `EventItem`'s `EntityService.toEntity` lifts `startTime`/`endTime` to real
  `Date` instances; the same nested-DTO trap is why `Session`'s own `EntityService` hydrates `startTime`/
  `endTime` too (only `created`/`lastModified` are auto-converted by the framework).
  `[NotMapped] SeatsTaken` is rendered as "—" (not 0) on that nested view, since the processor that fills it
  never runs on a nested projection.
  `Session.SessionSpeakers` and `Registration.SelectedSessions` are the generated `InputSelectorInline`
  owned-collection editors — chips with `_deleted` marking, wired to their sibling slice's pool.
- **Free-text search.** `EventCategory` initially had no `IHasNormalizedContent`, so `?q=` silently matched
  nothing — the app logged the warning at startup (`?q= text search is silently ignored for: EventCategory`)
  and it was fixed by adding the interface + a `[Normalized]` field before shipping.
- **Vibrant UI, built on the standard scaffold.** Overview lists are restyled per entity rather than left as
  generic tables: `Event` is a banner-card grid (category badge, date chip, banner image, session/attendee
  counts), `Speaker` is a photo-card grid, `Session` is agenda rows (time, room, speaker names, a seat-fill
  progress bar), `EventCategory` is colored chips. All still use the shipped `useSearchView` / `useForm` /
  `InputSelector` / `InputSelectorInline` composables and components — only the markup changed.
- **Sessions tab on the Event page.** A `TabContainer` splits the Event form into `#form` and `#sessions`
  (disabled until the event is saved) — the latter is a read-only agenda of `item.sessions` plus links to
  manage sessions for that event or add a new one.

## Verification performed

- Backend: `dotnet build` clean (0 warnings/errors); full create → PATCH (partial) → PUT → re-read round
  trip on `Location`; a `Registration` created with `selectedSessions`, PATCHed on `status` only, and
  re-read to confirm the owned collection survived untouched (the documented `null` = "not sent" contract).
- Frontend: `npm run build` (`vue-tsc -b && vite build`) clean, 0 type errors, both times it was run.
  Live-browser pass against the running API: all seven overviews (banner cards, speaker cards, agenda,
  category chips, plain lists), an Event's tabbed Details page including the Sessions tab, and a full
  create round-trip driven through the actual rendered form (fill → Save → `POST /api/event-categories` →
  200 → list refreshes with the new row) — that test row was deleted afterwards to keep the shipped seed
  data exactly as listed above.

## Project metrics

Tracked for this build (both apps built in one continuous session; approximate — no built-in call/token
counter exists, so this is a manual tally against the tool-call log):

| | Backend | Frontend | Total |
|---|---|---|---|
| Regira MCP calls | ~21 | ~27 | **~48** |
| Wall-clock time | ~12 min | ~55 min | **~67 min** |
| Tokens (session total, not separable per side) | | | **~500K** |
| Estimated cost (Claude Sonnet 5, uncached list rate) | | | **~$3–5** |

The frontend took longer mainly because of the scaffold-then-hand-customize workflow across 7 full entity
slices (8 files each) plus 2 owned sub-slices, and a live-browser debugging pass that caught two real
runtime bugs (see below) that a type-check alone could not have found. The cost figure is a rough
upper bound at Anthropic's published per-token list rate — agentic sessions like this one typically qualify
for prompt-caching discounts that would bring the real figure down; there is no exact per-session cost API
available to this agent.

### Bugs found and fixed during live verification (not caught by `dotnet build` / `vue-tsc`)

1. **`formatDateTime` mask misuse.** Used `"MMM"` expecting a textual month name; the library's mask
   vocabulary is numeric-only (`d/dd`, `M/MM`, `yy/yyyy`, `h/H/m`), so `"MMM"` silently rendered `"044"`
   (`MM` → `"04"` + leftover `M` → `"4"`) instead of throwing. Fixed by composing the month abbreviation via
   `Intl.DateTimeFormat` instead, guarded against `undefined`/invalid dates.
2. **Backend process instability under the harness's background-task runner** — the API process exited
   cleanly (code 0, then later code 1) twice with no application-level error logged, unrelated to any code
   change; restarting it was the fix each time. Left as an environment note, not a code defect.

## Credits

Built by **Claude Sonnet 5** (`claude-sonnet-5`), running as **Claude Code** in agentic/auto mode with the
Regira MCP server (`https://mcp.regira.com/mcp`) for all framework guidance — no prior memory of the Regira
framework was used, per the task's instructions; every convention above was looked up fresh from the MCP
docs for this build. Reasoning effort: default (not explicitly configured for this session).
