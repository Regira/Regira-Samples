# HelpDesk - support center

A support-ticket application: a .NET 10 **Regira Entities** API with JWT sign-in, and a Vue 3 SPA built on
**@regira/modules**. It has a support-center layout: a sidebar with work queues, a KPI dashboard, a
drag-and-drop **Kanban board**, and **conversation-style comment threads**.

| Part | Folder | URL |
|---|---|---|
| Back-end API (ASP.NET Core, SQLite) | `HelpDesk.Api/` | http://localhost:5841 (Scalar UI at `/scalar`, OpenAPI at `/openapi/v1.json`) |
| Front-end SPA (Vue 3 + Vite, Bootstrap 5) | `HelpDesk.Web/` | http://localhost:5842 (proxies `/api` to the API) |

## Features

- **Customers** sign in to a self-service portal. They can open tickets, choose categories (one or more) and urgency, attach files, reply in the thread, and follow the status. The API scopes them to their own tickets.
- **Agents** work queues (*My queue*, *Unassigned*, *Overdue*, *All open*, *All tickets*) and the Kanban board, where dropping a card changes its status. They can assign tickets (*Assign to me*), change status quickly, set due dates, and add **internal notes** that customers never see.
- **Administrators** also manage categories (with a routing team), priorities (with an SLA target in hours), statuses (Kanban columns, default and closed flags), support teams, people, and logins.
- **Tickets** have a sequential code (`HD-000123`), a priority, a status, a team, an assignee, an SLA due date (created + the priority's target hours), and timestamps for when they were closed and first answered. They also carry categories (m2m), comments and file attachments.
- **Dashboard** shows open, unassigned, overdue and closed-in-30-days counts, a 30-day created/closed chart, tickets by status and priority, team queues, and the average first-response time.
- **Search**: `?q=` matches the ticket code, subject and customer (name, company, e-mail). There are filters for status, priority, category, team, customer, assignee, closed/overdue/assigned/attachments, plus sort options.

## Running

```bash
# API (creates App_Data/helpdesk.db and seeds on first start)
cd HelpDesk.Api
dotnet run --launch-profile http                      # http://localhost:5841
dotnet run --launch-profile http -- --ResetDatabase=true   # drop + re-seed (Development)

# SPA
cd HelpDesk.Web
npm install
npm run dev                                           # http://localhost:5842 (strictPort)
npm run build                                         # vue-tsc -b + vite build
```

### Demo accounts

Seeding creates 37 logins, all with the demo password stored in `HelpDesk.Api/appsettings.Development.json`
(`Seed:DemoPassword`):

| Role | Login |
|---|---|
| Admin + Agent | `alex.morgan@helpdesk.test` |
| Agents | every employee (`<first>.<last>@helpdesk.test`; see *People*, filter *Employees*) |
| Customers | the first 12 active customers (*People*, filter *Has a login*), e.g. `ezekiel.cremin@emardmcdermott.test` |

The JWT signing secret for Development is in the same file. For any other environment, provide
`Authentication:Jwt:Secret` (64 characters or more) through user secrets or environment variables.

## Architecture

### Back-end (`HelpDesk.Api`)

- **Template**: `BasicApi`, plus the JWT registrations from `SelfHostingApiWithAuth` (Identity users in the same SQLite database, roles `Admin`/`Agent`/`Customer` issued as a `role` claim, refresh tokens). Also Serilog, OpenAPI with Scalar, and the central route prefix `api`.
- **Entity budget (free tier, 5 simple + 2 complex, fits exactly)**:

  | Entity | Registration |
  |---|---|
  | SupportTeam, Priority, Status | simple |
  | Category | simple (`For<Category, int, CategorySearchObject>`) |
  | TicketAttachment | simple (the `HasAttachments` join) |
  | Person | complex: customers **and** employees in one role-discriminated type (the budget-saving "stakeholder" move) |
  | Ticket | complex: typed `TicketSortBy` + `[Flags] TicketIncludes` (Categories, Comments, Attachments) |
  | TicketCategory | owned m2m join (`e.Related`) |
  | TicketComment | owned child with a single writer, the comment action |

- **Pipeline pieces**:
  - `TicketPrepper`: code minting, defaults (status, priority, routing team, SLA due date), `ClosedAt` bookkeeping, and customer write-restrictions.
  - `TicketQueryBuilder`: the domain filters.
  - `TicketProcessor`: comment and attachment counters; strips internal notes for customers.
  - `TicketNormalizer`: folds the customer into the ticket's `?q=`.
  - `TicketCodeGenerator`: a singleton, primed from the highest code.
- **Security**:
  - Global row filters: `TicketAccessFilter`, `TicketAttachmentAccessFilter`, `PersonAccessFilter`.
  - `WriteAuthorizationFilter`: a per-controller allow-list for writes and deletes.
  - `TicketOwnerScopeFilter`: closes the attachment-upload hole.
  - Everything requires authentication (`MapControllers().RequireAuthorization()`); the dashboard is staff-only.
- **Custom endpoints**:
  - `POST api/tickets/{id}/comments`: the conversation thread.
  - `GET api/me`: roles and linked person.
  - `POST api/persons/{id}/account`: admin creates a login.
  - `GET api/dashboard`: KPIs.
  - The standard `auth`, `auth/password` and `users` account endpoints.
- **Seeding** (`Data/Seeding`, Bogus, fixed seed) goes through the `IEntityService` implementations and `UserManager`:
  - 6 teams, 13 categories, 4 priorities, 6 statuses
  - 185 people (25 employees, 160 customers)
  - **520 tickets**, about 1,700 comments with correlated conversations, and about 130 attachments
  - Open tickets are dated relative to their own SLA window, so roughly a quarter of open work is overdue.

### Front-end (`HelpDesk.Web`)

- Full reference scaffold (`scaffold.mjs --shell` plus one slice per entity). Ticket was scaffolded with `--rel` for each relation, `--owns TicketCategory --picker Category`, and `--attachments`.
- Custom support-center shell: a dark sidebar (queues, plus entity navigation built from `config.json → navigation` through `useNavigation()`), a top bar with global ticket search, and role-aware home pages (staff dashboard, customer portal).
- The ticket page has tabs: **Conversation** (chat bubbles, internal notes in amber, composer with Ctrl+Enter), **Details** (form) and **Files**. A side panel shows the properties, quick status buttons and *Assign to me*.
- **Kanban board** (`/board`): one column per status, each column its own server-side search (capped, with a "show all" link). Drag and drop saves through the pooled ticket service; touch screens get a "move to" select.
- `infrastructure/access.ts` mirrors the API's write tiers: buttons are hidden or forms read-only per role.
- Deliberate deviations:
  - Small, bounded lookups (status, priority, team) use each slice's shipped `SelectorDropdown`; people use the scalable `InputSelector`.
  - Status and priority badges are plain coloured pills rather than record buttons, since they are lookups.
  - The four reference-data entities are hidden from agents' navigation (admin-only).
  - Single language (English); no language selector.

## Build status

- `dotnet build`: succeeds with 0 warnings. Startup logs `5 simple / 2 complex registered -> tier = free` and no validation warnings.
- `npm run build` (`vue-tsc -b && vite build`): succeeds.
- Verified at runtime:
  - Through the API: create, update twice, PATCH and re-read; customer row scoping (404 on foreign tickets, 403 on deletes, admin writes and the dashboard); internal notes hidden; attachment upload scope; attachment download.
  - In the browser: login, dashboard, drag and drop on the board, sending a reply, quick status change and a second save, customer ticket creation with a category chip and a second save, queues, lookups, and the mobile layout.

## Build statistics

| Phase | Regira MCP calls | Wall-clock time | Tokens (approx.) |
|---|---|---|---|
| Back-end (design, API, auth, seeding, API verification) | 25 | ~15 min | ~320k |
| Front-end (scaffold, shell, slices, board, conversation, browser verification) + README | 10 | ~23 min | ~230k |
| **Total** | **35** | **~38 min** | **~550k** (cumulative, mostly cached context) |

Estimated cost: about US$3-6 at list API prices, most of it cache reads. This is an estimate: the session
does not expose exact billing.

## Credits

Built by **Claude** (Anthropic), model *Claude Opus 5.5*, running as a Claude Code sub-agent (general-purpose
agent type) with **low reasoning effort**. It followed the Regira MCP documentation server (bootstrap guide, package cards,
entities/security/UI guides) and used no prior memory or other projects as reference.
