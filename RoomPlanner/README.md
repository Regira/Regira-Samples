# RoomPlanner

Meeting-room reservations for a multi-building office: buildings contain floors, floors contain meeting rooms,
rooms offer equipment and a capacity, and employees book one or more rooms for a meeting with invited attendees.
Some rooms are booked instantly, others need a facility manager's approval.

| Part | Folder | Stack | URL |
|------|--------|-------|-----|
| Back-end API | `RoomPlanner.Api/` | .NET 10, ASP.NET Core, EF Core 10 + SQLite, Regira Entities 6.4 (Mapster), Serilog, OpenAPI + Scalar | http://localhost:5861 (Scalar: `/scalar`, OpenAPI: `/openapi/v1.json`) |
| Front-end SPA | `RoomPlanner.Web/` | Vue 3.5, TypeScript 6, Vite 8, Pinia, vue-router 5, Bootstrap 5, `@regira/modules` 6.4 (full reference scaffold, no-auth) | http://localhost:5862 |

## Running

```bash
# API - creates + seeds roomplanner.db on first start (EnsureCreated, no migrations)
cd RoomPlanner.Api
dotnet run --launch-profile http          # http://localhost:5861
dotnet run --launch-profile http -- --ResetDatabase=true   # drop, recreate and reseed

# SPA - proxies /api to the API
cd RoomPlanner.Web
npm install
npm run dev                                # http://localhost:5862 (strictPort)
npm run build                              # vue-tsc -b + vite build
```

URL contract: `public/config.json → api` = `/api` (axios base) + each slice's relative `IConfig.api` (`/rooms`) →
Vite proxy `/api` → `http://localhost:5861` → the API's central `api` route prefix (`UseCentralRoutePrefix`).
HTTPS redirection is skipped in Development so the proxy can talk plain HTTP.

## Domain

| Entity | Registration | Notes |
|---|---|---|
| `Building` | simple `For<Building>()` | code, address, city, opening hours (`TimeOnly`) used by the planner's time axis |
| `Floor` | simple `For<Floor, int, FloorSearchObject>()` | belongs to a building (unique level per building); building eager-loaded on every row |
| `Equipment` | simple `For<Equipment>()` | lookup: projector, whiteboard, video conferencing, ... with a bootstrap-icons name |
| `Employee` | simple `For<Employee, int, EmployeeSearchObject>()` | display name (`Title`) derived by a prepper; department / active filters |
| `Room` | complex `For<Room, RoomSearchObject, RoomSortBy, RoomIncludes>()` | capacity, `RequiresApproval`, `IsActive`, colour; owned `RoomEquipment` join rows (equipment + quantity) |
| `Reservation` (primary, ~500 seeded) | complex `For<Reservation, ReservationSearchObject, ReservationSortBy, ReservationIncludes>()` | organizer, UTC start/end, owned `ReservationRoom` rows (per-room approval) and owned `ReservationAttendee` rows (response, optional) |

Budget: **4 simple / 2 complex → free tier** (confirmed by the startup log line). The three join/child tables are
owned collections synced by `e.Related(...)` - no registration, no controller.

### Filtering rooms

`GET /api/rooms/search` accepts `buildingId`, `floorId`, `minCapacity`, `equipmentId` (repeatable, **AND**: the room
must offer every listed equipment), `requiresApproval`, `isActive`, and `availableFrom` + `availableTo` (only rooms
without a live - non-cancelled, non-rejected - booking overlapping that period), plus `q` keywords.

### Reservation rules (server side, `ReservationPrepper`)

* End after start, at most 24 hours; organizer and attendees must exist; at least one room, no duplicates.
* **Approval per room**: a newly booked room is `Approved` automatically, or `Pending` when the room requires approval.
  Rescheduling puts approval-required rooms back to `Pending`.
* **Derived status**: `Pending` (any room pending) / `Approved` (all approved) / `PartiallyApproved` / `Rejected` / `Cancelled`.
* **No double booking**: a live booking of the same room overlapping the slot answers 400 (checked against the
  database *and* the rows queued in the same `SaveChanges`, so bulk writes are covered too).
* **Capacity**: organizer + attendees must fit the combined capacity of the requested rooms.
* `Status`, `AttendeeCount`, the cancel fields and the per-room approval fields are **not** on the input DTOs; the
  prepper restores them from the stored row, so ordinary PUT/PATCH cannot forge an approval. Only the workflow
  actions (a scoped `WorkflowContext.IsTrustedWriter` flag) and the seeder may set them.

Workflow endpoints (`ReservationWorkflowController`, same `reservations` resource, answer `{ item }` like `GET /{id}`):

```
POST /api/reservations/{id}/rooms/{roomId}/approve   { "note": "..." }
POST /api/reservations/{id}/rooms/{roomId}/reject    { "note": "..." }
POST /api/reservations/{id}/cancel                   { "note": "reason" }
```

Deletes are protected with `Restrict` foreign keys (SQLite `Foreign Keys=True`): deleting a room, floor, building,
equipment type or employee that is still referenced answers **409**.

## Front-end

Full `@regira/modules` scaffold (`scaffold.mjs --shell --no-auth` + one slice per entity) with config-driven
dashboard/navbar, counted server-side paging, filters, pooled relation labels, `InputSelector` relation pickers,
confirmed deletes and `Feedback` everywhere. On top of the slices, four calendar-oriented views:

* **Planner** (`/planner`) - day timeline per building: rooms × hours (building opening hours), bookings as bars coloured
  by status (striped = awaiting approval), a live "now" line, and a per-room **availability indicator** (free/busy now
  for today, % booked for other days). Filter by capacity and equipment; click free space to book that room/slot,
  click a bar to open the reservation.
* **Find a room** (`/find-a-room`) - pick a start, duration, head count, building and equipment → **room cards** with
  an Available/Booked badge (API availability filter), capacity, equipment chips, approval badge and a mini day
  timeline; "Book"/"Request" opens a pre-filled reservation.
* **Calendar** (`/calendar`) - week grid (Mon-Fri, weekend optional) for a room, a participant (organizer or attendee)
  or a building; overlapping meetings are laid out in lanes; click an empty slot to book.
* **Approvals** (`/approvals`) - queue of reservations with rooms awaiting approval, approve/reject per room with a note.

Entity pages: the reservation form (tabs *Reservation / Attendees / Approval*) shows the rooms as chips tinted by
approval state, a seats indicator, and a live availability timeline of the chosen rooms on the chosen day (click to
move the meeting); cancel is a confirmed action with a reason. The room page has tabs *Room / Equipment (editable
table with quantities) / Schedule (7-day strip + upcoming bookings)*.

Deliberate deviations from the scaffold defaults:

* The navbar gets four extra links for the planning views (they are not entity slices) in front of the config-driven
  `NavBar`.
* Equipment filters use toggle chips (`components/planning/EquipmentToggles.vue`) over the 9-row lookup instead of an
  `InputSelector`: the filter is multi-value with AND semantics, which a single-id selector cannot express.
* `Equipment` is edited in a modal (`isComplex: false`, a flat lookup); every other entity uses a Details page.

## Seed data

Seeded through the `IEntityService` implementations (so preppers/primers/normalizers run) with Bogus (`nl_BE` names),
deterministic (fixed random seed, dates relative to "today"): 9 equipment types, 4 buildings (Brussels, Antwerp, Ghent,
Leuven), 16 floors, 59 rooms (sizes from focus booths to 110-seat auditoriums, equipment correlated with room size,
boardrooms/auditoriums require approval, 2 rooms out of service), 150 employees and **500 reservations** spread over
working days from ~2 weeks back to ~2 weeks ahead. Meeting types are correlated tuples (title, head count, duration),
rooms are the smallest free fit (overflow events book two large rooms), attendees mostly come from the organizer's
department, past approval-required bookings are approved/rejected, future ones partly pending, ~7 % cancelled.
The seeder keeps an in-memory occupancy map so no seeded bookings overlap (the prepper would reject them anyway).

## Project layout

```
RoomPlanner.Api/
  Controllers/            EntityControllers.cs (6 entity controllers), ReservationWorkflowController.cs
  Data/                   AppDbContext.cs, Seeding/DataSeeder.cs
  Entities/<Plural>/      entity, DTOs, search object / sort / includes, query builder, prepper, service configuration
  Extensions/             AddEntityServices (UseEntities + budget tally)
  Infrastructure/         HostingExtensions (Serilog, JSON, CORS, OpenAPI/Scalar, DB init)
  Services/               WorkflowContext (trusted-writer flag)
RoomPlanner.Web/
  public/                 config.json (api, navigation), data/translations.json
  src/entities/<plural>/  scaffolded slices (+ owned sub-slices room-equipments, reservation-rooms, reservation-attendees)
  src/components/planning DayTimeline, RoomDayAvailability, RoomAgenda, EquipmentToggles, StatusBadge
  src/views/              Home, Planner, RoomFinder, Calendar, Approvals (+ error views)
  src/utilities/          planning.ts (status constants, date helpers)
```

## Build status

* `dotnet build` - succeeded, 0 warnings, 0 errors; `dotnet list package --vulnerable --include-transitive` - none.
* `npm run build` (`vue-tsc -b && vite build`) - succeeded.
* Runtime-verified: startup (no DI/validation warnings left, free tier), CRUD + save-twice for rooms (owned equipment)
  and reservations (owned rooms + attendees), PATCH keeps server-owned fields, conflict/capacity 400s, approve/reject/
  cancel, 409 on deleting a referenced room, all views in the browser incl. a 375 px viewport.

## Credits

Built by **Claude** (Anthropic, model Claude Opus 5.5) running as a **Claude Code** sub-agent (general-purpose agent
type) with **low reasoning effort**, following the Regira MCP documentation (bootstrap guide golden path, package cards,
heading-scoped guides) - no memory and no other projects were used as reference.

| Metric | Back-end | Front-end | Total |
|---|---|---|---|
| Regira MCP calls | 15 | 10 | **25** |
| Wall-clock time | ~12 min (12:19-12:31) | ~22 min (12:31-12:53) | **~34 min** |
| Tokens (approx., context budget consumed) | ~230 k | ~250 k | **~480 k** |

Cost: not measurable from inside the agent (no usage/billing API in this session); see the Claude Code `/cost`
output of the parent session for the exact figure.
