# AssetHub

A company asset/inventory management demo built on the **Regira** framework (.NET back end + Vue 3
front end), generated end-to-end through the **Regira MCP server**.

Assets (laptops, monitors, phones, tools, ...) belong to categories, carry a color-coded status, live at
a location, come from a supplier, and can be assigned to employees with a full assignment history.
Each asset can carry multiple attachments, warranties and maintenance records.

## Stack & ports

| App | Tech | URL |
|---|---|---|
| Back end API | ASP.NET Core 10 / EF Core / SQLite, Regira Entities | http://localhost:6100 (Scalar docs at `/scalar`, OpenAPI at `/openapi/v1.json`) |
| Front end SPA | Vue 3 / TypeScript / Vite, `@regira/modules` | http://localhost:6101 |

No authentication — this is an internal-tool style demo, wired with `scaffold.mjs --no-auth`.

## Running it

**Back end** (SQLite database is created and seeded automatically on first run):

```bash
cd backend
dotnet run
# -> http://localhost:6100, Scalar UI opens automatically
```

**Front end** (in a second terminal):

```bash
cd frontend
npm install
npm run dev -- --port 6101
# -> http://localhost:6101
```

To reseed from scratch, stop the API and delete `backend/assethub.db*` — `Database.EnsureCreated()`
recreates the schema and the seeder repopulates it on the next start.

## Domain model

**Free-tier budget: 5 simple + 2 complex = 7/7 registrations, fits exactly.**

| Entity | Classification | Notes |
|---|---|---|
| `Category` | simple | Asset category (laptops, monitors, ...) |
| `AssetStatus` | simple | Color-coded lifecycle status (hex color, operational flag, sort order) |
| `Location` (client class `LocationItem` — `Location` collides with the DOM global) | simple | Building/room/address |
| `Supplier` | simple | Vendor contact info |
| `Employee` | simple | Staff who assets can be assigned to |
| `Asset` | complex | The primary entity — ~500 seeded rows |
| `AssetAssignment` | complex | Assignment history: asset ↔ employee, assigned/returned dates |

Owned children of `Asset` (via `e.Related()` — no registration slot, no own controller, edited inside the
asset form): `AssetAttachment`, `AssetWarranty`, `AssetMaintenanceRecord`. These are metadata-only rows
(file name/size/content-type, not real binary storage) — the framework's dedicated attachment feature
(`WithAttachments`/`EntityAttachment`) was deliberately **not** used, since it costs an extra simple
registration slot per owner and metadata rows were enough for the brief.

`AssetAssignment` is a top-level (not owned) entity, since assignment history needs to be queryable and
sortable on its own (per-employee history, per-asset history) independent of its parent asset. A server-side
processor (`AssetProcessor`) computes each asset's *current holder* (`currentEmployeeId`/`currentEmployeeName`/
`currentAssignedDate`) from the active (unreturned) assignment, and an `AssetAssignmentPrepper` enforces
"at most one active assignment per asset" as a business rule (returns a 400, not a 500 or a silent double-booking).

`Asset` is `IArchivable` (soft delete — it's the natural aggregate parent of attachments/warranties/
maintenance records); the four simple lookups and `Employee` are plain reference data, deliberately **not**
archivable (a required-FK archivable lookup silently drops referencing rows from list pages — see
`Regira.Entities` → *Soft delete*), with `OnDelete(Restrict)` on the `Asset` → lookup foreign keys instead.

## Front-end notes

- Full scaffold tier (`scaffold.mjs --shell`, one slice per entity) — dashboard/navbar built from
  `public/config.json → navigation`, not hand-rolled.
- Every entity uses the **page** form tier (`isComplex: true`), including the four simple lookups. The
  card recommends a modal for "a title, maybe a code and a colour" lookups; pages were used uniformly here
  for consistency and lower integration risk across seven slices built in one pass — a deliberate
  simplification, not an oversight.
- `Asset`'s form is tabbed (Details / Assignment history / Attachments / Warranties / Maintenance).
  Assignment history is read-only inside the asset (with an "Assign to employee" action that opens
  `AssetAssignment`'s own form in a modal, pre-filled with the asset); full CRUD on assignments lives on
  its own `/asset-assignments` page.
- `Asset`'s form.vue and `AssetAssignment`'s views both need a **value** import from each other's slice
  (`InputSelector`/`FormModalButton`). Since a two-directional barrel import can cycle at module-eval time
  (`Cannot access 'Entity' before initialization`, dev-server only), one edge (`Asset` → `AssetAssignment`)
  uses a deep import (`@/entities/asset-assignments/details/FormModalButton.vue`) instead of the barrel —
  see the comment in `frontend/src/entities/assets/details/Form.vue`.
- Every custom `Date` field (`purchaseDate`, warranty/maintenance dates, assignment dates) is hydrated from
  the API's ISO strings in each slice's `EntityService.toEntity()` — only `created`/`lastModified` hydrate
  automatically — and edited with the kit's `DateInput` component rather than a bare
  `<input type="date">`, which does not accept a `Date` object directly.

## Verification performed

- Backend: `dotnet build` clean (0 warnings/errors); create → update → update again → re-read is
  idempotent and owned rows (attachments) survive it; the "one active assignment per asset" rule returns
  400 on a violation; soft delete/restore round-trips (`DELETE` archives, `GET` 404s, row is gone from
  lists, `?archived=included` brings it back).
- Frontend: `npm run build` (`vue-tsc -b && vite build`) clean. Driven live against the API in a browser:
  every overview page (Assets, Assignments, Categories, Asset statuses, Locations, Suppliers, Employees)
  paginates and renders its relation/status columns; the tabbed Asset form loads and displays existing
  attachments/warranties/maintenance/assignment history; a new Category was created and deleted through
  the UI end-to-end (POST/DELETE round-trip, including the CORS preflight).

## Credits

Built by **Claude** (Anthropic) — agent type `claude` (general-purpose, direct execution, no
sub-agent delegation), reasoning effort **medium (40)** — using the Claude Agent SDK / Claude Code CLI,
driven entirely through the **Regira MCP server** (`https://mcp.regira.com/mcp`) for framework guidance,
plus the browser tool for live in-app verification.

### Effort tracking

Approximate, derived from this session's context-token budget (start: 15,000,000 tokens) and tool-call
history — not metered API accounting, but the closest available proxy:

| Phase | Regira MCP calls (approx.) | Context tokens consumed (approx.) |
|---|---|---|
| Back end (bootstrap guide, `Regira.Entities` docs, scaffolding, build/seed/verify) | ~16 | ~235,000 |
| Front end (bootstrap guide, `regira_modules.vue.entities`/`.ui` docs, scaffolding, build/browser verify, backend DTO fix) | ~30 | ~340,000 |
| **Total** | **~46** | **~575,000** |

Wall-clock time was not independently logged; based on the timestamps in the backend's own startup logs
(seeding runs, restarts) the build-and-verify portion alone spanned roughly 30–40 minutes, with front-end
scaffolding/customization/browser verification bringing the full session to somewhere in the 1.5–2 hour
range. Dollar cost was not computed — it depends on the caller's specific Claude pricing tier, which
wasn't available to check from inside the session; the token figures above are what a cost estimate would
need to be multiplied against.
