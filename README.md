# Groundwork

**A full-stack starter template — ASP.NET Core 10 Web API + React 19 — with authentication, auditing, and conventions already wired.**

Groundwork is the part you build before the building starts. Every project needs the
same first two weeks: a layered API, Identity with JWTs and refresh-token rotation,
audit columns, soft delete, validation, mapping, pagination, a themed React shell with
login and signup, and a written set of conventions so the code stays consistent. That
work is done here. Copy the folder, rename a handful of strings, and start on the
feature that is actually yours.

> The name is a one-line change — see [Naming your project](#1-name-the-project).

---

## Table of contents

- [What you get](#what-you-get)
- [Tech stack](#tech-stack)
- [Repository layout](#repository-layout)
- [Quick start](#quick-start)
- [Configuration](#configuration)
- [Backend architecture](#backend-architecture)
- [API reference](#api-reference)
- [Frontend architecture](#frontend-architecture)
- [AI skills (`.agent/`)](#ai-skills-agent)
- [Starting a new project](#starting-a-new-project)
- [Before production](#before-production)
- [Command reference](#command-reference)

---

## What you get

| Area | What's already built |
|---|---|
| **Auth** | Register, login, refresh, logout, current user. JWT access tokens + rotating refresh tokens, ASP.NET Core Identity with role tables. |
| **Layering** | Four projects with a one-way reference graph. Controllers validate and map; services hold the logic; only the data project knows EF Core. |
| **Auditing** | `CreatedAt`/`CreatedBy`/`UpdatedAt`/`UpdatedBy` stamped automatically inside `SaveChanges` — impossible to forget. |
| **Soft delete** | `IsActive` via `ISoftDeletable`, filtered explicitly at every call site (no invisible global query filters). |
| **Validation** | FluentValidation auto-wired; validators discovered from the assembly, no manual registration. |
| **Mapping** | Mapster with `IRegister` configs scanned at startup. Entities never leave the service layer. |
| **Pagination** | `PagedResult<T>` / `PagedResultResponse<T>`, `PaginationParams`, and a clamped `ToPagedResultAsync` extension. |
| **Seeding** | Idempotent role + user seeders driven from configuration, with passwords in user-secrets. |
| **Frontend shell** | Marketing pages, auth pages, 404, routed layout, UI primitives, and a session layer with single-flight refresh-on-401. |
| **Theming** | Semantic design tokens in one CSS file — light and dark, no `dark:` variants scattered through components. |
| **Conventions** | [`.agent/code-structure.md`](.agent/code-structure.md) — the written contract every file follows. |
| **AI skills** | Seven skills under [`.agent/skills/`](.agent/skills/) for documents, design, and skill authoring. |

---

## Tech stack

### Backend — `net10.0`

| Package | Version | Role |
|---|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.10 | Users, roles, password hashing |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.10 | Data access (SQL Server) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.10 | Bearer token authentication |
| `System.IdentityModel.Tokens.Jwt` | 8.22.0 | Token issuing |
| `Mapster` | 10.0.11 | Object mapping |
| `FluentValidation.AspNetCore` | 11.3.1 | Request validation |
| `Swashbuckle.AspNetCore` | 10.2.3 | Swagger / OpenAPI |

### Frontend

| Package | Version | Role |
|---|---|---|
| `react` / `react-dom` | 19.2 | UI |
| `react-router-dom` | 7.18 | Routing |
| `axios` | 1.19 | HTTP client |
| `dayjs` | 1.11 | Dates (with `utc`, `relativeTime`, `duration` plugins) |
| `tailwindcss` + `@tailwindcss/vite` | 4.3 | Styling |
| `vite` | 8.2 | Build & dev server |
| `typescript` | 6.0 | Types (`erasableSyntaxOnly` on) |
| `oxlint` | 1.75 | Linting |

---

## Repository layout

```
Groundwork/
├── .agent/
│   ├── code-structure.md          ← the coding contract (read this first)
│   └── skills/                    ← AI skills available to Claude
├── backend/                       ← solution folder AND the Web API project
│   ├── backend.slnx
│   ├── backend.csproj             ← ASP.NET Core Web API
│   ├── Program.cs                 ← ALL DI registration, pipeline, Swagger, mapping scan
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Controllers/               ← AuthController, SeedDataController
│   ├── Model/
│   │   ├── Requests/Auth/         ← the wire contract in
│   │   └── Responses/{Auth,Common,SeedData}/   ← the wire contract out
│   ├── Validation/Auth/           ← FluentValidation validators
│   ├── backend.Data/              ← entities, DbContext, migrations, constants
│   │   ├── Abstractions/          ← ICurrentUserProvider
│   │   ├── Entities/Common/       ← IAuditableEntity, ISoftDeletable, AuditableEntity
│   │   ├── Data/                  ← ApplicationDbContext (audit stamping lives here)
│   │   ├── Migrations/            ← InitialIdentity
│   │   └── migration.md           ← every EF command, written down
│   ├── backend.Services/          ← business logic + service models
│   │   ├── Services/{Auth,SeedData}/
│   │   ├── Model/{Auth,Common,SeedData}/
│   │   ├── Common/                ← PaginationExtensions
│   │   └── Mapping/               ← ServiceMappingConfig
│   └── backend.SeedData/          ← DatabaseSeeder + RoleSeeder + UserSeeder
└── frontEnd/                      ← React + TypeScript (Vite)
    ├── vite.config.ts             ← dev proxy /api → https://localhost:7157
    └── src/
        ├── components/{pages,layout,ui}/
        ├── services/              ← apiClient, authService, tokenStorage, seedDataService
        ├── context/               ← AuthContext, AuthProvider
        ├── hooks/                 ← useAuth
        ├── types/                 ← every interface and enum in the app
        ├── utils/                 ← apiErrors, dateUtils
        ├── config/                ← pagination defaults
        └── index.css              ← design tokens (the whole palette, light + dark)
```

The class libraries deliberately live **inside** the Web API's folder. That means the
SDK's default `**/*.cs` glob would compile them twice, so `backend.csproj` carries a
`Compile`/`Content`/`None`/`EmbeddedResource` `Remove` for each. **Any new project
added under `backend/` must be added to that exclusion list** or the build breaks
with confusing duplicate-type errors.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- SQL Server or SQL Server LocalDB
- `dotnet-ef` — `dotnet tool update --global dotnet-ef`

### 1. Backend

```bash
cd backend

# Nothing runs until the signing key is set — Program.cs throws at startup if it is empty.
dotnet user-secrets set "Jwt:SigningKey" "<a random string of 32+ characters>"
dotnet user-secrets set "SeedData:Users:0:Password" "<a password for the seeded admin>"

dotnet ef database update --project backend.Data --startup-project .
dotnet run
```

API: `https://localhost:7157` · Swagger UI (Development only): `https://localhost:7157/swagger`

### 2. Frontend

```bash
cd frontEnd
npm install
npm run dev
```

App: `http://localhost:5173`

The Vite dev server proxies `/api` to `https://localhost:7157` with `secure: false`.
That keeps the browser on a single origin, so there is no CORS preflight in development
and no need to trust the ASP.NET dev certificate. In a deployed build, set
`VITE_API_BASE_URL` to the real API origin instead — `apiClient` reads it and falls
back to `/api`.

### 3. Sign in

In Development the login page shows a **Seed data** panel listing the seeded accounts
with their passwords, filled in with one click. It is compiled out of production
builds via `import.meta.env.DEV`, and the endpoint behind it returns 404 outside
Development. See [Seed data endpoint](#seed-data-endpoint--development-only).

---

## Configuration

`appsettings.json` holds structure and non-secrets; `appsettings.Development.json`
overrides it locally; **secrets are never committed**.

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Issuer` / `Jwt:Audience` | Token issuer and audience, validated on every request |
| `Jwt:SigningKey` | **Secret.** Ships empty; startup throws if unset |
| `Jwt:AccessTokenMinutes` | Access token lifetime (default 60) |
| `Jwt:RefreshTokenDays` | Refresh token lifetime (default 7) |
| `SeedData:Users[]` | `Email` + `FullName` per seeded account — passwords excluded by design |
| `Cors:AllowedOrigins[]` | Origins allowed to call the API directly (not needed for the dev proxy) |

**Secrets in development** go to user-secrets, indexed by position for list entries:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<key>"
dotnet user-secrets set "SeedData:Users:0:Password" "<password>"
```

**Secrets in production** come from environment variables — `Jwt__SigningKey`,
`ConnectionStrings__DefaultConnection`, and so on.

---

## Backend architecture

### Four projects, one direction

```
backend  →  backend.Services  →  backend.Data
backend  →  backend.SeedData  →  backend.Services  →  backend.Data
```

`backend.Data` references nothing else in the solution. `backend.Services` never
references `backend` — which is exactly why **service models** exist: the service
layer needs its own data contracts so it never depends on the API's wire types.

| Project | Contains |
|---|---|
| `backend` | Controllers, request/response models, validators, `Program.cs` |
| `backend.Data` | Entities, enums, constants, `ApplicationDbContext`, migrations |
| `backend.Services` | Service interfaces + implementations, service models, mapping config |
| `backend.SeedData` | Startup seeders |

### The path a request takes

```
HTTP request
   ↓
Controller            validates input, catches exceptions, logs
   ↓ .Adapt<>()
CreateXxxModel        service model — the service layer's own contract
   ↓
Service               all business logic; returns a service model, never an entity
   ↓
ApplicationDbContext  the only thing that touches the database
   ↑
XxxModel
   ↑ .Adapt<>()
XxxResponse           the published wire contract
```

Three rules hold the shape together, and they are the ones most worth remembering:

1. **A controller never touches `DbContext`.** It validates, calls one service method, maps, and returns.
2. **A service never returns an entity.** EF types stop at the service boundary; the controller maps a service model to a response model.
3. **All DI registration happens in `Program.cs` and nowhere else.** No `AddApplicationServices()` extension hiding in a library. When a dependency fails to resolve, there is exactly one file to open.

### Authentication

`AddIdentityCore` is used rather than `AddIdentity` — this is a token API, and
`AddIdentity` would register cookie schemes that compete with JWT as the default.

The flow:

1. `POST /api/auth/login` verifies the password through `SignInManager` and returns an **access token** (JWT, 60 min) plus a **refresh token** (opaque, 7 days, stored in `RefreshTokens`).
2. `apiClient` attaches the access token to every request.
3. On a `401`, `apiClient` calls `POST /api/auth/refresh` once, retries the original request, and — critically — is **single-flight**: redeeming a refresh token revokes it, so concurrent 401s share one refresh instead of racing to spend the same token.
4. `POST /api/auth/logout` revokes the refresh token server-side.

Roles travel in the JWT as `ClaimTypes.Role` claims, so `[Authorize(Roles = ...)]`
resolves without a database hit. Role names are constants (`RoleNames.Admin`) — the
one sanctioned exception to the no-magic-strings rule, because attributes require a
compile-time constant.

### Auditing and soft delete

Two narrow interfaces, one base class:

```csharp
IAuditableEntity  → CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
ISoftDeletable    → IsActive
AuditableEntity   → abstract base implementing both, plus Id
```

`ApplicationDbContext` overrides the two-argument `SaveChanges`/`SaveChangesAsync`
overloads — every other signature funnels into them, so no write path can bypass the
stamp. **Services never assign audit fields by hand.** Deleting is
`IsActive = false` and nothing else; the same `Modified` branch records who did it
and when, which is why there is no separate `DeletedAt`/`DeletedBy` pair.

Soft-delete filtering is **explicit** (`.Where(x => x.IsActive)`), not an EF global
query filter. Global filters are invisible at the call site and force
`IgnoreQueryFilters()` into any code that legitimately needs inactive rows — at which
point the guarantee is gone anyway.

### Ownership scoping

The default model is one user per account: every user-owned entity carries a `UserId`,
and **ownership is part of the query, not a check afterwards**.

```csharp
var product = await dbContext.Products
    .AsNoTracking()
    .SingleOrDefaultAsync(p => p.Id == productId
                            && p.UserId == currentUser.UserId
                            && p.IsActive);
```

Someone else's record must be indistinguishable from one that does not exist — return
**404, never 403**, so IDs cannot be probed. The current user comes from
`ICurrentUserProvider`, which also feeds the audit stamper.

### Seeding

`DatabaseSeeder` runs at startup in Development only, inside a service scope, wrapped
in a try/catch that logs and continues — an un-migrated database must not stop the
host from starting. Seeders are idempotent and go through `UserManager`/`RoleManager`
so password hashes and security stamps are produced by Identity itself.

---

## API reference

Base URL: `https://localhost:7157`. All responses are JSON. Enums serialize as
strings via a global `JsonStringEnumConverter`.

| Method | Route | Auth | Purpose |
|---|---|---|---|
| `POST` | `/api/auth/register` | Anonymous | Create an account; the user becomes Admin of their own account |
| `POST` | `/api/auth/login` | Anonymous | Sign in; returns a token pair |
| `POST` | `/api/auth/refresh` | Anonymous | Exchange a refresh token for a new pair; the old one is revoked |
| `POST` | `/api/auth/logout` | Bearer | Revoke a refresh token |
| `GET` | `/api/auth/me` | Bearer | The signed-in user's profile and roles |
| `GET` | `/api/seeddata/users` | Anonymous | **Development only** — seeded accounts with passwords; 404 elsewhere |

### Shapes

```jsonc
// POST /api/auth/register
{ "fullName": "Ada Lovelace", "email": "ada@example.com", "password": "Str0ngPass" }

// POST /api/auth/login
{ "email": "ada@example.com", "password": "Str0ngPass" }

// POST /api/auth/refresh  ·  POST /api/auth/logout
{ "refreshToken": "…" }

// 200 — AuthResponse (register, login, refresh)
{
  "accessToken": "eyJ…",
  "accessTokenExpiresAt": "2026-01-01T12:00:00Z",
  "refreshToken": "…",
  "user": {
    "id": "…", "fullName": "Ada Lovelace", "email": "ada@example.com",
    "roles": ["Admin"], "createdAt": "2026-01-01T11:00:00Z"
  }
}
```

### Error shapes — there are two, on purpose

Most endpoints return **RFC 7807 Problem Details**, produced by `Problem(...)`:

```jsonc
{ "detail": "Product with ID … was not found.", "status": 404 }
```

Identity-driven endpoints (register, login, refresh) return an **error list**, because
`UserManager` can reject a request for several reasons at once and a single `detail`
string would throw information away:

```jsonc
{ "errors": ["Passwords must have at least one uppercase letter.", "Email is already taken."] }
```

The frontend's `extractApiError` reads whichever is present, so components handle one
call shape regardless.

### Paged endpoints

Any list endpoint that can grow without bound is paged from day one. Bind
`[FromQuery] PaginationParams paging` (`pageNumber`, `pageSize`, `search?`) and return:

```jsonc
{ "items": [ … ], "totalCount": 137, "pageNumber": 1, "pageSize": 20 }
```

`PaginationExtensions.ToPagedResultAsync` does the count, skip/take, and clamps
`pageSize` at 200.

### Seed data endpoint — development only

`GET /api/seeddata/users` returns seeded accounts **with plaintext passwords**. It is a
deliberate, contained exception that holds only because of four properties:

- **Nothing is persisted** — it reads the same configuration the seeder does. There is no credentials table to leak.
- **It is unauthenticated on purpose** — its job is to help someone sign in *before* they have a token.
- **The environment gate is absolute** — 404 outside Development, not an empty list.
- **The frontend button is compiled out** of production bundles.

If any of those is ever weakened, remove the feature rather than repair it. Delete it
before a production deployment regardless — see [Before production](#before-production).

---

## Frontend architecture

### Where things go

| Concern | Location |
|---|---|
| UI, pages, modals, forms | `src/components/` |
| Types, interfaces, enums | `src/types/` |
| API calls, token storage | `src/services/` |
| Context objects and providers | `src/context/` |
| Custom hooks | `src/hooks/` |
| Shared helpers (dates, errors) | `src/utils/` |
| App-wide constants | `src/config/` |
| Design tokens, global styles | `src/index.css` |

Three rules, each with a reason:

- **No `fetch`/`axios` in a component.** Every call goes through a service file, so a URL appears in exactly one place and adding TanStack Query later means wrapping services, not rewriting components.
- **No types declared in component or service files.** They live in `src/types/`, so the shape of a thing has one definition.
- **No raw colour utilities and no `dark:` variants.** Components use semantic tokens, so re-skinning the app is an edit to `index.css` alone.

### Routes

| Path | Page |
|---|---|
| `/` | HomePage |
| `/about` | AboutUsPage |
| `/contact` | ContactUsPage |
| `/login` | LoginPage |
| `/signup` | SignupPage |
| `/logout` | LogoutPage |
| `*` | NotFoundPage |

All render inside `PageLayout` (navbar + `<Outlet />` + footer), wrapped by `AuthProvider`.

### Session handling

- `context/AuthContext.ts` holds **only** the `createContext` call — no JSX, so fast refresh behaves.
- `context/AuthProvider.tsx` is the provider; `hooks/useAuth.ts` is the accessor and **throws** outside the provider rather than handing back `undefined`.
- `services/tokenStorage.ts` is the only module that touches `localStorage`, kept separate from `authService` so `apiClient` can read the token without importing the service that calls it.
- **Only tokens are persisted, never the user.** On boot the provider calls `/api/auth/me` — a stored token proves nothing on its own, since it may be expired or revoked. `isRestoringSession` covers that round trip so the UI never flashes a signed-out navbar at a signed-in user.

### Design tokens

The full palette lives in `src/index.css` as CSS variables, exposed to Tailwind v4 via
`@theme inline`, with a `prefers-color-scheme: dark` block redefining the same names:

`surface` · `surface-raised` · `surface-sunken` · `ink` · `ink-soft` · `muted` ·
`line` · `brand` · `brand-strong` · `brand-soft` · `on-brand` ·
`success`/`success-soft` · `danger`/`danger-soft` · `warning`/`warning-soft`

Used as `bg-surface`, `text-ink`, `border-line`, `bg-brand-soft`, and so on.

### TypeScript enums

`erasableSyntaxOnly` is on, which real TS `enum` declarations violate. Use a `const`
object plus a derived type, with **string values matching the backend's JSON**:

```ts
export const OrderStatus = {
  Draft: 'Draft',
  Submitted: 'Submitted',
} as const;

export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus];
```

---

## AI skills (`.agent/`)

The `.agent/` folder is what makes this template useful to an AI assistant rather than
just to a person. It has two parts.

### 1. `code-structure.md` — the coding contract

[`.agent/code-structure.md`](.agent/code-structure.md) is the full specification of
*how* code is written here: layering, naming, file conventions, validation, mapping,
auditing, pagination, error handling, workers, and every frontend rule. It opens with a
standing instruction that the standards are not suggestions and are not traded away for
speed — if a task seems to require breaking one, that is a conversation to have first.

Point your assistant at it at the start of a session. It is also the right place to
record new decisions: when you settle a convention, write it there rather than leaving
it implicit in one file that the next feature will diverge from.

### 2. `skills/` — packaged capabilities

Skills are folders containing a `SKILL.md` (plus any scripts and references) that an
assistant loads on demand when the work matches. Seven ship with the template:

| Skill | Use it for |
|---|---|
| **docx** | Creating, reading, and editing Word documents — reports, memos, letters, templates, tracked changes, comments |
| **pdf** | Reading and extracting text/tables, merging, splitting, watermarking, filling forms, OCR |
| **pptx** | Building and editing slide decks — layouts, speaker notes, charts, templates |
| **frontend-design** | Aesthetic direction when building or reshaping UI — palette, typography, layout that doesn't read as templated defaults |
| **theme-factory** | Applying a coherent theme to an artifact; ten presets plus on-the-fly generation |
| **brand-guidelines** | Applying Anthropic's brand colours and typography to an artifact |
| **skill-creator** | Authoring new skills, improving existing ones, and running evals on them |

**Adding your own.** Create `.agent/skills/<name>/SKILL.md` with YAML frontmatter:

```markdown
---
name: my-skill
description: One or two sentences saying exactly when to use this. The description is
  what the assistant matches against, so make it concrete and trigger-heavy.
---

# My Skill

Instructions the assistant should follow when this skill applies.
```

Keep the body focused on procedure rather than background. The `skill-creator` skill
walks through the full authoring and evaluation loop if you want to go further.

**A good first project-specific skill** is one that captures a workflow you repeat:
scaffolding a new feature end to end (entity → migration note → service → controller →
validator → frontend service → page), or your deployment checklist.

---

## Starting a new project

Copy the folder, then work through this list. The same checklist, with a little more
detail, closes [`.agent/code-structure.md`](.agent/code-structure.md).

### 1. Name the project

| File | Change |
|---|---|
| `README.md` | Title and description (this file) |
| `backend/appsettings.json` + `.Development.json` | Database name, `Jwt:Issuer`, `Jwt:Audience` |
| `backend/Program.cs` | Swagger `Title`, `Description`, endpoint name, `DocumentTitle` |
| `frontEnd/index.html` | `<title>` and the description meta tag |
| `frontEnd/src/components/layout/Navbar.tsx`, `Footer.tsx` | Brand name |
| `frontEnd/src/components/pages/{HomePage,AboutUsPage,ContactUsPage}.tsx` | Placeholder marketing copy |
| `frontEnd/src/services/tokenStorage.ts` | `SESSION_KEY` prefix — keeps `localStorage` separate from other apps on the same host |
| `frontEnd/public/favicon.svg` | App icon |
| `frontEnd/src/index.css` | `--brand*` tokens, if the brand colour differs |

Leave the *project* names (`backend`, `backend.Data`, `backend.Services`,
`backend.SeedData`) alone unless you have a real reason — renaming them touches every
namespace, the `.slnx`, and the `Compile Remove` globs in `backend.csproj`.

### 2. Set secrets and the database

```bash
cd backend
dotnet user-secrets set "Jwt:SigningKey" "<32+ chars>"
dotnet user-secrets set "SeedData:Users:0:Password" "<password>"
dotnet ef database update --project backend.Data --startup-project .
```

### 3. Build your first feature

Follow the vertical slice the conventions describe, in this order:

1. Entity in `backend.Data/Entities/` extending `AuditableEntity` — then **stop and tell the developer a migration is needed**. Claude never runs migrations; the commands live in [`backend/backend.Data/migration.md`](backend/backend.Data/migration.md).
2. Service models in `backend.Services/Model/<Feature>/`, one class per file.
3. Service in `backend.Services/Services/<Feature>/XxxService.cs` — interface and implementation in the same file.
4. Register it in `Program.cs`, the only place DI wiring lives.
5. Request/response models in `backend/Model/`, validator in `backend/Validation/<Feature>/`.
6. Controller in `backend/Controllers/`, with the three standard `[ProducesResponseType]` attributes and a `try/catch`.
7. Frontend types in `src/types/`, service in `src/services/`, then the component.

---

## Before production

- [ ] Delete the seed-data feature: `backend/Controllers/SeedDataController.cs`, `backend.Services/Services/SeedData/`, `backend/Model/Responses/SeedData/`, `frontEnd/src/services/seedDataService.ts`, and the login page's seed panel.
- [ ] Move every secret from user-secrets to environment variables.
- [ ] Set `Cors:AllowedOrigins` to the real frontend origin.
- [ ] Set `VITE_API_BASE_URL` to the real API origin.
- [ ] Confirm Swagger is Development-gated (it is, in `Program.cs` — verify it stayed that way).
- [ ] Review `Jwt:AccessTokenMinutes` / `RefreshTokenDays` against your risk appetite.

---

## Command reference

### Backend — run from `backend/`

```bash
dotnet run                                   # start the API
dotnet build                                 # build the solution
dotnet user-secrets list                     # inspect local secrets

# EF Core — developer only; see backend.Data/migration.md for the full set
dotnet ef migrations add <Name> --project backend.Data --startup-project .
dotnet ef database update      --project backend.Data --startup-project .
dotnet ef migrations remove    --project backend.Data --startup-project .
```

### Frontend — run from `frontEnd/`

```bash
npm install      # install dependencies
npm run dev      # dev server on http://localhost:5173
npm run build    # type-check (tsc -b) then production build
npm run preview  # serve the production build locally
npm run lint     # oxlint
```

### Ports

| Service | URL |
|---|---|
| API (HTTPS) | `https://localhost:7157` |
| API (HTTP) | `http://localhost:5247` |
| Swagger UI | `https://localhost:7157/swagger` (Development only) |
| Frontend dev server | `http://localhost:5173` |
