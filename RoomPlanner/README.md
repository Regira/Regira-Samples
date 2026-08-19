# RoomPlanner

A meeting-room reservation demo built on the **Regira** framework (.NET 10 backend + Vue 3 SPA frontend),
scaffolded and coded through the **Regira MCP server**.

Buildings contain floors, floors contain meeting rooms, employees create reservations for one or more
rooms and invite attendees, and each reservation is approved automatically or held for manual approval
depending on the rooms booked.

## Ports

| App | URL |
|---|---|
| Back-end API (Scalar UI at `/scalar`) | http://localhost:6160 |
| Front-end SPA | http://localhost:6161 |

(Port 6162 was reserved for a second front-end/site but wasn't needed — the app is a single SPA.)

## Running it

**Back-end**
```bash
cd backend/RoomPlanner.Api
dotnet run --urls http://localhost:6160
```
On first run it creates `roomplanner.db` (SQLite) and seeds it automatically. Delete the `.db` file to
force a full reseed (the seeder is idempotent — it skips seeding if buildings already exist).

**Front-end**
```bash
cd frontend
npm install
npm run dev
```
Then open http://localhost:6161. The dev server calls the API directly at `http://localhost:6160/api`
(the API enables CORS for loopback origins in Development — no dev proxy needed).

## Domain model

| Entity | Kind | Notes |
|---|---|---|
| `Building` | simple entity | Name, address, city |
| `Floor` | simple entity | Belongs to a `Building`, has a `Level` for ordering |
| `MeetingRoom` | complex entity | Belongs to a `Floor`; `Capacity`, `Equipment` (`[Flags]`: Projector, Whiteboard, VideoConferencing, ConferencePhone, Monitor, Catering), `RequiresApproval`, `IsActive` |
| `Employee` | simple entity | Name, email, department, job title |
| `Reservation` | complex entity | Subject, description, `StartTime`/`EndTime` (UTC), `Organizer` (Employee), `Status` (Pending/Approved/Rejected/Cancelled) |
| `ReservationRoom` | owned m2m join (no own endpoint) | Reservation ↔ MeetingRoom — a reservation can book one or more rooms |
| `ReservationAttendee` | owned child (no own endpoint) | An internal `Employee` **or** an external guest (name/email), plus a response status (Invited/Accepted/Declined/Tentative) |

**Free-tier budget:** 3 simple + 2 complex registrations (Building, Floor, Employee simple; MeetingRoom,
Reservation complex) — comfortably inside the 5-simple/2-complex free tier. `ReservationRoom` and
`ReservationAttendee` are owned via `e.Related(...)` on `Reservation` and cost no registration slot.

**Approval rule:** `ReservationManager` (an `EntityWrappingServiceBase` around the default repository)
computes the initial status on **create**: if any of the selected rooms has `RequiresApproval = true`, the
reservation starts `Pending`; otherwise it's auto-`Approved`. Manual approve/reject afterwards is a normal
`PATCH`/`PUT` of `Status`. Rooms with capacity ≥ 12 are seeded with a higher chance of requiring approval.
A reservation must have at least one room and `EndTime > StartTime`, enforced with a 400 (`EntityInputException`),
not a raw 500.

## Front-end highlights

- Full Regira Vue scaffold (no-auth) — dashboard, navbar, paged/filterable overviews, pooled relation
  labels, `<Debug>` panels.
- **Meeting rooms** render as cards (capacity, equipment badges, active/requires-approval indicators)
  instead of a table — closer to how you'd actually browse rooms.
- **Reservations** is a tabbed Details page (Details / Rooms / Attendees): Rooms is an
  `InputSelectorInline` chip picker over `MeetingRoom` (many-to-many); Attendees is an editable table
  combining an `Employee` relation picker *or* free-text guest name/email, plus a response-status select.
- **Calendar** (`/calendar`, custom page, not generated) — a day timeline across every active room, with
  reservation blocks positioned by start/end time, a building filter, and a live available-now/occupied-now
  indicator per room.
- The reservations overview reads like a schedule: organizer, rooms, date/time and a status badge per row.

## Seeding

Seeded through the registered `IEntityService` implementations (Bogus-generated, seed `20260819` for
reproducibility), idempotent on restart:

| Entity | Count |
|---|---|
| Buildings | 5 |
| Floors | ~21 (3–5 per building) |
| Meeting rooms | ~89 (3–6 per floor) |
| Employees | 150 |
| **Reservations (primary entity)** | **500** |

Each reservation gets 1–2 rooms (weighted toward 1), 1–6 attendees (mostly internal employees, ~15% also
carry an external guest), a business-hours weekday start time spread across a ±30/+60-day window, and
~8% are seeded pre-cancelled. Room approval requirement is a mix of capacity-driven and random, so both
`Approved` and `Pending` statuses occur naturally through the same code path a real save uses.

## Tech stack

- **Backend:** .NET 10, ASP.NET Core, `Regira.Entities.Web` 6.1.2 (+ Mapster mapping), EF Core 10 / SQLite,
  Serilog, Scalar (OpenAPI UI), Bogus (seeding).
- **Frontend:** Vue 3.5, Vite 8, TypeScript 6, Pinia, vue-router 5, `@regira/modules` (entities/ui/auth-off
  stack), Bootstrap 5.

## Verification performed

- `dotnet build` — clean, 0 warnings, 0 errors.
- `npm run build` (`vue-tsc -b && vite build`) — clean, 0 type errors, on the first attempt.
- Backend started and seeded successfully; spot-checked every entity's `/search` endpoint.
- Full browser walkthrough against the live API: every overview (Buildings, Floors, Employees, Meeting
  rooms, Reservations, Calendar) renders real seeded data; opened a Reservation's Details/Rooms/Attendees
  tabs and confirmed populated data renders correctly.
- **End-to-end write test:** created a new reservation through the UI (subject, organizer via Autocomplete,
  start/end time, one room via the chip picker), saved it — verified server-side (`id 501`, correct
  organizer/room, `Status` auto-computed to `Approved` because the picked room doesn't require approval) —
  then saved it a **second time** to confirm the many-to-many room sync doesn't 500 on update (the classic
  owned-collection re-sync bug).

## Project effort tracking

Tracked from this session's own tool-call log and the harness's context-budget counter (no external
timer/cost API was available inside the session, so time and cost are estimates, not measurements).

| | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls (docs + `get_type`/`get_example`/`search_docs`) | ~20 | ~25 | **~45** |
| Wall-clock (rough, single continuous session) | ~30–35 min | ~45–55 min | **~80–90 min** |
| Context tokens consumed (session budget counter) | — | — | **~500K tokens** |

The backend phase was almost entirely MCP-doc-driven design (classification, budget tally, exact
Regira signatures) followed by a single clean `dotnet build`. The frontend phase spent more calls because
of the owned-collection patterns (many-to-many chips, scalar-table-with-relation-column) and several
`get_type` signature checks (`formatDate`, `dateTimeInputString`, `createStore`, pool types) to avoid
guessing — plus the custom Calendar view, which isn't generated by any scaffold.

No dollar cost is reported: this session had no access to per-token pricing/billing data, and estimating
one from the token count alone would be a fabricated precision the tracking doesn't support.

## Credits

Built by **Claude** (Sonnet 5, `claude-sonnet-5`) running as **Claude Code**, in a single main-session
agent (no sub-agent delegation), reasoning effort **medium (40)**, driven entirely through the **Regira
MCP server** (`get_bootstrap_guide`, `get_package`/`get_package_card`, `get_type`, `search_docs`, and the
`@regira/modules` `scaffold.mjs` generator) plus the in-browser preview tools for end-to-end verification.
