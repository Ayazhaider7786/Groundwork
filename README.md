# Groundwork

**Full-stack starter — ASP.NET Core 10 Web API + React 19 — with auth, auditing, file uploads and conventions already wired.**

Every project needs the same first two weeks: a layered API, Identity with JWTs and
refresh rotation, audit columns, soft delete, validation, mapping, pagination, and a
themed React shell with login and signup. That work is done here. Copy the folder,
rename a handful of strings, and start on the feature that is actually yours.

> **[`.agent/code-structure.md`](.agent/code-structure.md) is the coding contract** —
> layering, naming, file conventions, validation, mapping, auditing, file uploads,
> and every frontend rule. Read it before writing code, and point your AI assistant
> at it at the start of a session. This README is only how to run and rename things.

---

## What's inside

| Area | Built |
|---|---|
| **Auth** | Register, login, refresh, logout, current user. JWT access tokens + rotating refresh tokens, Identity with role tables |
| **Layering** | Four projects, one-way references. Controllers validate and map; services hold the logic; only the data project knows EF Core |
| **Auditing** | `CreatedAt`/`CreatedBy`/`UpdatedAt`/`UpdatedBy` stamped inside `SaveChanges` — impossible to forget |
| **Soft delete** | `IsActive`, filtered explicitly at every call site — no invisible global query filters |
| **File uploads** | Pluggable storage behind `IFileStorage` (local disk today), per-user paths, extension and size whitelist |
| **Plumbing** | FluentValidation auto-wired, Mapster with scanned configs, pagination helpers, idempotent seeders |
| **Frontend** | Marketing and auth pages, routed layout, UI primitives, session layer with single-flight refresh-on-401 |
| **Theming** | Semantic design tokens in one CSS file — light and dark, no `dark:` variants scattered about |
| **AI skills** | Seven skills under [`.agent/skills/`](.agent/skills/) — docx, pdf, pptx, frontend-design, theme-factory, brand-guidelines, skill-creator |

**Stack:** .NET 10 · EF Core 10 (SQL Server) · Mapster · FluentValidation · Swashbuckle — React 19 · Vite 8 · Tailwind 4 · axios · dayjs. Exact versions live in `backend.csproj` and `package.json`.

---

## Quick start

**Prerequisites:** .NET 10 SDK · Node 20+ · SQL Server or LocalDB · `dotnet tool update --global dotnet-ef`

```bash
cd backend

# Nothing runs until the signing key is set — Program.cs throws at startup if it is empty.
dotnet user-secrets set "Jwt:SigningKey" "<a random string of 32+ characters>"
dotnet user-secrets set "SeedData:Users:0:Password" "<a password for the seeded admin>"

dotnet ef database update --project backend.Data --startup-project .
dotnet run
```

```bash
cd frontEnd
npm install
npm run dev
```

| Service | URL |
|---|---|
| Frontend | `http://localhost:5173` |
| API | `https://localhost:7157` · `http://localhost:5247` |
| Swagger | `https://localhost:7157/swagger` (Development only) |

Vite proxies `/api` to `https://localhost:7157` with `secure: false`, keeping the
browser on one origin — no CORS preflight in development and no need to trust the
ASP.NET dev certificate. Deployed builds set `VITE_API_BASE_URL` to the real API
origin instead.

**Signing in:** in Development the login page shows a **Seed data** panel that fills
the form with a seeded account in one click. It is compiled out of production bundles
and the endpoint behind it returns 404 outside Development.

---

## Layout

```
Groundwork/
├── .agent/
│   ├── code-structure.md      ← the coding contract (read this first)
│   └── skills/                ← AI skills
├── backend/                   ← solution folder AND the Web API project
│   ├── Program.cs             ← ALL DI registration, pipeline, Swagger, mapping scan
│   ├── Controllers/  Model/{Requests,Responses}/  Validation/
│   ├── backend.Data/          ← entities, DbContext (audit stamping), enums, migrations
│   │   └── migration.md       ← every EF command, written down
│   ├── backend.Services/      ← business logic, service models, mapping config
│   └── backend.SeedData/      ← DatabaseSeeder + RoleSeeder + UserSeeder
└── frontEnd/
    └── src/{components,services,context,hooks,types,utils,config}/
        └── index.css          ← design tokens, light + dark
```

The class libraries deliberately sit **inside** the Web API's folder, so `backend.csproj`
carries a `Compile`/`Content`/`None`/`EmbeddedResource` `Remove` for each. **Any new
project added under `backend/` must go on that list**, or the build fails with
confusing duplicate-type errors.

---

## API

Base URL `https://localhost:7157`. JSON throughout; enums serialize as strings.
Swagger has the full shapes.

| Method | Route | Auth | Purpose |
|---|---|---|---|
| `POST` | `/api/auth/register` | Anonymous | Create an account; the user becomes Admin of their own account |
| `POST` | `/api/auth/login` | Anonymous | Sign in; returns a token pair |
| `POST` | `/api/auth/refresh` | Anonymous | Exchange a refresh token for a new pair; the old one is revoked |
| `POST` | `/api/auth/logout` | Bearer | Revoke a refresh token |
| `GET` | `/api/auth/me` | Bearer | The signed-in user's profile and roles |
| `POST` | `/api/files/upload?location=` | Bearer | Store a file; returns the storage key to save on your own entity |
| `GET` | `/api/files/download?storageKey=` | Bearer | Fetch a stored file |
| `DELETE` | `/api/files?storageKey=` | Bearer | Delete a stored file |
| `GET` | `/api/seeddata/users` | Anonymous | **Development only** — seeded accounts with passwords; 404 elsewhere |

**Two error shapes, on purpose.** Most endpoints return RFC 7807 Problem Details
(`{ "detail": …, "status": … }`). Identity-driven endpoints return `{ "errors": [ … ] }`,
because `UserManager` can reject a request for several reasons at once and one `detail`
string would throw that away. The frontend's `extractApiError` reads whichever is present.

---

## Configuration

`appsettings.json` holds structure; **secrets are never committed**.

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Issuer` / `Jwt:Audience` | Validated on every request |
| `Jwt:SigningKey` | **Secret.** Ships empty; startup throws if unset |
| `Jwt:AccessTokenMinutes` / `RefreshTokenDays` | Token lifetimes (60 min / 7 days) |
| `SeedData:Users[]` | `Email` + `FullName` per account — passwords excluded by design |
| `FileStorage` | `Provider`, `MaxFileSizeBytes`, `AllowedExtensions`, `Local:RootPath` |
| `Cors:AllowedOrigins[]` | Origins allowed to call the API directly (not needed for the dev proxy) |

Development secrets go to user-secrets, indexed by position for list entries
(`SeedData:Users:0:Password`). Production secrets come from environment variables —
`Jwt__SigningKey`, `ConnectionStrings__DefaultConnection`.

Uploads are written to `backend/App_Data/` by default, which is gitignored — user
files must never reach the repository. Keep that path outside `wwwroot`.

---

## Starting a new project

Copy the folder, then change these:

| File | Change |
|---|---|
| `README.md` | Title and description (this file) |
| `backend/appsettings.json` + `.Development.json` | Database name, `Jwt:Issuer`, `Jwt:Audience` |
| `backend/Program.cs` | Swagger `Title`, `Description`, endpoint name, `DocumentTitle` |
| `frontEnd/index.html` | `<title>` and the description meta tag |
| `frontEnd/src/components/layout/{Navbar,Footer}.tsx` | Brand name |
| `frontEnd/src/components/pages/{HomePage,AboutUsPage,ContactUsPage}.tsx` | Placeholder copy |
| `frontEnd/src/services/tokenStorage.ts` | `SESSION_KEY` prefix — keeps `localStorage` separate from other apps on the same host |
| `frontEnd/public/favicon.svg` | App icon |
| `frontEnd/src/index.css` | `--brand*` tokens, if the brand colour differs |

Leave the *project* names (`backend`, `backend.Data`, …) alone unless you have a real
reason — renaming touches every namespace, the `.slnx`, and the `Compile Remove` globs.

Then set secrets and the database as in [Quick start](#quick-start), and build your
first feature in the order [`.agent/code-structure.md`](.agent/code-structure.md)
describes: entity → service models → service → `Program.cs` registration →
request/response models + validator → controller → frontend types → service →
component.

> **Migrations are the developer's job.** Claude and other assistants never run
> `dotnet ef migrations add` or `database update` — they make the entity change and
> say what the migration needs to cover. Commands live in
> [`backend/backend.Data/migration.md`](backend/backend.Data/migration.md).

---

## Before production

- [ ] Delete the seed-data feature: `Controllers/SeedDataController.cs`, `backend.Services/Services/SeedData/`, `Model/Responses/SeedData/`, `frontEnd/src/services/seedDataService.ts`, and the login page's seed panel.
- [ ] Move every secret from user-secrets to environment variables.
- [ ] Set `Cors:AllowedOrigins` and `VITE_API_BASE_URL` to the real origins.
- [ ] Confirm Swagger is still Development-gated in `Program.cs`.
- [ ] Add rate limiting and a pre-buffering size cap to the upload endpoint — see **File Uploads → Known gap** in the contract.
- [ ] Review token lifetimes against your risk appetite.

---

## Commands

```bash
# backend/
dotnet run                                   # start the API
dotnet build                                 # build the solution
dotnet user-secrets list                     # inspect local secrets

# EF Core — developer only; full set in backend.Data/migration.md
dotnet ef migrations add <Name> --project backend.Data --startup-project .
dotnet ef database update      --project backend.Data --startup-project .

# frontEnd/
npm run dev        # dev server on http://localhost:5173
npm run build      # type-check (tsc -b) then production build
npm run lint       # oxlint
```
