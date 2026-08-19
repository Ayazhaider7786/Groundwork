# Code Structure & Conventions
#latest
---

This document defines **how** code is written in this project. It does not define
**what** gets built — that lives in the project's PRD. When the two disagree on a
structural matter, this document wins; when they disagree on a product matter, the
PRD wins.

> ## Write all code according to the coding standards in this document.
>
> This applies to **every** file — backend and frontend, new code and edits to
> existing code. Before writing anything, check it against the rules here; after
> writing it, check it again.
>
> The standards are not suggestions and are not traded away for speed. If a task
> seems to require breaking one, that is a conversation to have first — say which
> rule is in the way and why, and get the exception agreed. Silently deviating,
> or matching a nearby file that already deviates, is not acceptable.

> **About the examples.** This is a starter template. The only features it ships are
> authentication (register, login, refresh, current user), the development-only seed
> data helper, and shared pagination plumbing. Code samples below use a placeholder
> `Product` feature purely to show the *shape* of a feature — no such feature exists
> in the template. Substitute your own domain when you build the real thing.

---

## Core Principles (apply to ALL code, backend and frontend)

These are non-negotiable and outrank convenience. Every rule further down this
document is a concrete application of one of them.

### 1. Well-structured code

- **One responsibility per unit.** A class, service, component, or function does
  one thing. If you need "and" to describe it, split it.
- **Small, focused files.** One class per file (see *Model File Conventions*), one
  feature per service folder, one component per file. A file that has grown past a
  few hundred lines is a signal to extract, not to keep appending.
- **No god-objects.** Do not funnel unrelated logic into a shared "helper",
  "manager", or "utils" dump. Each feature owns its own service folder
  (`Auth/`, and one per feature you add); shared building blocks live in `Common/`
  only when genuinely cross-cutting.
- **Depth over breadth in layering.** Controller → Service → DbContext. A
  controller never touches `DbContext`; a service never returns an entity.

### 2. SOLID

| Principle | What it means here |
|---|---|
| **S**ingle Responsibility | One service per feature area. `IJwtTokenService` owns issuing and reading tokens and nothing else. A change to one feature must not force an edit to an unrelated service. |
| **O**pen/Closed | Extend via new types/implementations, not by adding flags to existing branches. Add a new rule type, don't scatter `if (status == …)` across services. |
| **L**iskov Substitution | Any implementation of an interface must be safely swappable. Don't throw `NotImplementedException` from a method the interface promises. |
| **I**nterface Segregation | Keep interfaces narrow and purpose-built (`IAuthService`, `IJwtTokenService`, `ICurrentUserProvider`) rather than one fat `IAppService`. Consumers depend only on what they use. |
| **D**ependency Inversion | Depend on abstractions, injected via primary constructors. Services take an interface, never a concrete implementation. Nothing news-up its own dependencies. |

### 3. Meaningful naming (international coding standards)

- **Names reveal intent.** `GetActiveProductsAsync` — not `GetData`, `Process`,
  `DoWork`, `Handle`, or `Temp`.
- **No cryptic abbreviations.** `productId`, not `pId`/`prodId`. Well-known short
  forms (`Id`, `Url`, `Html`, `Db`, `Http`, `Ssl`) are fine.
- **No single-letter names** except conventional loop/lambda locals (`i`, and short
  lambda params like `p => p.Id`).
- **Casing — C#:** `PascalCase` for types, methods, properties, constants;
  `camelCase` for locals and parameters; `_camelCase` for private fields;
  interfaces prefixed `I`; async methods suffixed `Async`.
- **Casing — TypeScript/React:** `PascalCase` for components and types,
  `camelCase` for variables/functions/props, `SCREAMING_SNAKE_CASE` for module-level
  constants. Component files match the component name.
- **Booleans read as assertions** — `isEmailConfirmed`, `hasPendingChanges`,
  `canEditProfile`. Not `flag`, `status2`, `check`.
- **Collections are plural** — `products`, not `productList`/`arr`.
- **Say the same thing the same way everywhere.** One concept, one word: it is a
  *refresh token*, not `refreshToken` here and `token` there. It is a *customer*,
  not `customer` here and `client` there.
- **English only**, in names *and* comments.
- **Comments explain WHY, not WHAT.** If a comment is needed to explain what the
  code does, rename things until it isn't.

---

## Solution Layout

All four projects live under `backend/`. The Web API project sits at that folder's
root and the three class libraries are nested inside it.

```
<ProjectName>/
  .agent/
    code-structure.md          ← this document
  backend/                     ← solution folder AND the Web API project
    backend.slnx
    backend.csproj             ← ASP.NET Core Web API (net10.0)
    Program.cs                 ← all DI registration, pipeline, Swagger, mapping scan
    appsettings.json
    appsettings.Development.json
    Controllers/
    Model/
      Requests/
      Responses/
    Validation/
    backend.Data/              ← EF Core entities, enums, DbContext, migrations
    backend.Services/          ← business logic, service models
    backend.SeedData/          ← startup data seeders
  frontEnd/                    ← React + TypeScript (Vite)
    src/
```

**Folders created on demand** — they are not in the template because nothing needs
them yet, but when the need arises they go exactly here, with these names:

| Folder | Created when |
|---|---|
| `backend/Mapping/WebApiMappingConfig.cs` | The first Request → Service Model or Service Model → Response mapping needs custom rules |
| `backend/Workers/` | The first `BackgroundService` is added — see **Background Workers** |
| `frontEnd/src/components/modals/` | The first modal is added |

`backend.Data/Enums/` was on this list until the file upload service added
`FileStorageProvider`; it now exists — see **Enums**.

> **Consequence of nesting.** Because the libraries sit inside the Web API's own
> folder, the SDK's default `**/*.cs` glob would compile their sources into
> `backend.dll` as well, producing duplicate-type errors. `backend.csproj` therefore
> carries a `Compile`/`Content`/`None`/`EmbeddedResource` `Remove` for the three
> library folders. **Any new project added under `backend/` must be added to that
> exclusion list**, or the build breaks in a confusing way.

| Project | Type | Purpose |
|---|---|---|
| `backend` | Web API | Controllers, request/response contracts, validators, Mapster config, background workers, `Program.cs` |
| `backend.Data` | Class library | EF Core entities, enums, constants, `ApplicationDbContext`, migrations |
| `backend.Services` | Class library | Service interfaces/implementations, service models, business logic |
| `backend.SeedData` | Class library | Startup seeders (roles, admin user, reference data) |

**Project references:**

```
backend  →  backend.Services  →  backend.Data
backend  →  backend.SeedData  →  backend.Services  →  backend.Data
```

`backend.Data` references nothing else in the solution. `backend.Services` never
references `backend` — that is the entire reason service models exist (see
**Service Models** below).

There is a single data project. Do **not** split enums/models across a "shared" vs
"shared.data" project. Everything data/entity-related lives in `backend.Data`.

---

## What the Template Ships

The backend that exists today, and which every convention below is already applied to:

| Area | Files |
|---|---|
| Auth | `Controllers/AuthController.cs`, `backend.Services/Services/Auth/{AuthService,JwtTokenService,CurrentUserProvider}.cs` |
| Seed data helper | `Controllers/SeedDataController.cs`, `backend.Services/Services/SeedData/SeedDataService.cs` |
| File uploads | `Controllers/FilesController.cs`, `backend.Services/Services/Files/{FileUploadService,IFileStorage,LocalFileStorage}.cs`, `backend.Services/Common/FileUploadLocation.cs` — see **File Uploads** |
| Entities | `ApplicationUser`, `ApplicationRole`, `RefreshToken`, plus `Entities/Common/` (`IAuditableEntity`, `ISoftDeletable`, `AuditableEntity`) |
| Seeders | `backend.SeedData/{DatabaseSeeder,Seeders/RoleSeeder,Seeders/UserSeeder}.cs` |
| Shared plumbing | `RoleNames`, `FileUploadLocation`, `ICurrentUserProvider`, `PagedResult<T>`, `PagedResultResponse<T>`, `PaginationParams`, `PaginationExtensions` |
| Migrations | One `InitialIdentity` migration covering the Identity tables and `RefreshToken`. File uploads add none — they own no table |

Everything else described in this document is a convention for code you are about
to write.

---

## Controllers

**Location:** `backend/Controllers/`

### Rules

- Every action method **must** have these three `[ProducesResponseType]` attributes as a minimum:
  ```csharp
  [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
  ```
  Plus the success response type specific to the endpoint, e.g.:
  ```csharp
  [ProducesResponseType(typeof(PagedResultResponse<ProductResponse>), StatusCodes.Status200OK)]
  ```

- Route attribute: `[Route("api/[controller]")]` on the controller, relative `[HttpGet("...")]`/`[HttpPost("...")]` on actions. Use a leading `/` on the action route only when it needs to escape the controller's base route (see **Absolute Route Override** below).

- Every action method wraps its body in a `try/catch`.

- **Two standard catch blocks:**
  ```csharp
  catch (KeyNotFoundException ex)
  {
      logger.LogError(ex, "...");
      return Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
  }
  catch (Exception ex)
  {
      logger.LogError(ex, "...");
      return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
  }
  ```
  - `KeyNotFoundException` → 404. Only include this catch if the service layer can realistically throw it (record looked up by ID/key).
  - `Exception` → 500. Always present.

- **Input validation happens in the controller**, before calling the service layer:
  - Simple params (string, int, Guid) → inline `if` checks in the controller method, return `Problem(...)` with `Status400BadRequest`.
  - Request model inputs → a FluentValidation validator runs automatically via `AddFluentValidationAutoValidation()` (registered in `Program.cs`) — no manual validator call needed in the controller.

- **Log at every significant step** using `logger.LogInformation`, `logger.LogWarning`, `logger.LogError`.

- **Authorization is explicit.** Every controller carries `[Authorize]` at the class level. Endpoints opt out individually with `[AllowAnonymous]` — in the template that is register, login, refresh, and the development-only seed data endpoint. Admin-only endpoints use `[Authorize(Roles = RoleNames.Admin)]` — see **Identity & Roles**.

- **Exception:** Identity-driven endpoints (register/login) don't fit the single-message `Problem()` shape — `UserManager` can return several validation errors at once. These return `BadRequest(new { errors })` / `Unauthorized(new { errors })` with the error list from an `AuthResult`-style result object (see **Result Object Pattern** below) instead of throwing. Use `Problem()` for everything else.

### Example Pattern

```csharp
[ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(PagedResultResponse<ProductResponse>), StatusCodes.Status200OK)]
[HttpGet("{categoryId:guid}/products")]
public async Task<IActionResult> GetProducts(Guid categoryId, [FromQuery] PaginationParams paging)
{
    try
    {
        if (categoryId == Guid.Empty)
        {
            logger.LogWarning("Invalid category ID supplied for product listing.");
            return Problem(detail: "Category ID must be provided.", statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("Fetching products for category {CategoryId}: page={PageNumber}, size={PageSize}", categoryId, paging.PageNumber, paging.PageSize);
        var result = await productService.GetByCategoryAsync(categoryId, paging);

        if (result is null)
        {
            logger.LogWarning("Category with ID {CategoryId} was not found.", categoryId);
            return Problem(detail: $"Category with ID {categoryId} was not found.", statusCode: StatusCodes.Status404NotFound);
        }

        var response = result.Adapt<PagedResultResponse<ProductResponse>>();

        logger.LogInformation("Fetched {Count} products for category {CategoryId}.", response.Items.Count, categoryId);
        return Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while fetching products for category {CategoryId}.", categoryId);
        return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
    }
}
```

---

## Request & Response Models

**Location:** `backend/Model/`

```
backend/Model/
  Requests/
    <Feature>/
      CreateXxxRequest.cs
      UpdateXxxRequest.cs
  Responses/
    <Feature>/
      XxxResponse.cs
      XxxDetailsResponse.cs
    Common/
      PagedResultResponse.cs
```

### These are a public contract

Both projects call their data folder `Model/`, but the two are not equivalent, and
the difference is the one thing to keep in mind here.

**Everything under `backend/Model/` is part of the API's published surface.**
Renaming a property, tightening a type, or removing a field is a **breaking change**
for every client already calling the endpoint. Service models in
`backend.Services/Model/` carry no such promise — they are an internal boundary and
can be refactored at will.

So: change a service model freely; change a request or response only deliberately,
and expect the frontend to need updating in the same commit.

Which layer a type belongs to is readable from its name — `LoginRequest` and
`AuthResponse` are the wire; `LoginModel` and `AuthResult` are internal. Keep those
suffixes rigid, because the folder name no longer draws the line.

Feature folders track the PRD's bounded contexts. The template ships `Auth/`,
`SeedData/`, and `Common/`; add one folder per feature as it lands.

- **Requests** — models the controller receives from the client (`[FromBody]`, `[FromQuery]`, `[FromRoute]`).
- **Responses** — models returned to the client. **Never return raw EF entities** — always map to a response model (Mapster `.Adapt<>()`).
- **One class per file, with a blank line between every property** — see **Model File Conventions** below.

---

## Validators (FluentValidation)

**Location:** `backend/Validation/`

```
backend/Validation/
  <Feature>/
    CreateXxxRequestValidator.cs
    UpdateXxxRequestValidator.cs
```

- Use **FluentValidation** (`AbstractValidator<T>`) for validating **request models**.
- For **simple scalar params** (int, Guid, string directly on the route/query) — validate inline in the controller with an `if` check, not a validator.
- Validators are picked up automatically via `AddValidatorsFromAssemblyContaining<Program>()` + `AddFluentValidationAutoValidation()` in `Program.cs` — no manual registration needed.

### Example

```csharp
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.");
    }
}
```

Password rules in a validator must stay in step with the `IdentityOptions.Password`
settings configured in `Program.cs` — two sources of truth that disagree produce a
400 the user cannot act on.

---

## When to Use What for Validation

| Input type | Where to validate |
|---|---|
| Simple scalar on route/query (`int`, `Guid`, `string`) | Inline `if` in the controller method |
| Request body model (`[FromBody] CreateXxxRequest`) | FluentValidation validator in `Validation/<Feature>/` |

---

## Services

**Location:** `backend.Services/Services/<Feature>/`

Every service lives under the single `Services/` folder, grouped into a feature
sub-folder. **Interface and implementation live in the same file, named after the
service** — `Services/Auth/AuthService.cs` contains both `IAuthService` and
`AuthService`. The file is named for the class, not the interface: there is no
`IAuthService.cs`.

```
backend.Services/
  Services/
    <Feature>/
      XxxService.cs          ← interface + implementation together
  Model/
    <Feature>/
      XxxModel.cs            ← that feature's inputs and outputs, one class per file
    Common/
      XxxModel.cs            ← models used by more than one feature
  Common/
    PaginationExtensions.cs  ← shared helpers that are neither services nor models
  Mapping/
    ServiceMappingConfig.cs
```

`Model/` mirrors `Services/`: a feature with a folder under one gets a folder under
the other. Three folders, three answers to "where does this go?":

| It is… | Folder |
|---|---|
| A service — behaviour, injected, has an interface | `Services/<Feature>/` |
| A data class — service input, output, or bound options | `Model/<Feature>/` |
| A data class used by more than one feature | `Model/Common/` |
| Neither — an extension method or shared helper | `Common/` |

**Existing feature folders:** `Auth/`, `SeedData/`, `Files/`. Add one per feature.

**Examples:**
- `backend.Services/Services/Auth/AuthService.cs`
- `backend.Services/Services/Auth/JwtTokenService.cs`
- `backend.Services/Services/Auth/CurrentUserProvider.cs` — the one implementation whose interface lives elsewhere (`backend.Data/Abstractions/`), because the DbContext depends on it; see **Audit Fields & Soft Delete**

Namespaces follow folders, so a service sits in `backend.Services.Services.<Feature>`
and a model in `backend.Services.Model.<Feature>`.

### Rules

- All business logic lives in the service layer — controllers only validate input, call the service, and map the result.
- Services are injected via **primary constructor** parameters (e.g. `AuthService(UserManager<ApplicationUser> userManager, ...)`).
- **All DI registration happens in `backend/Program.cs` and nowhere else.** The class libraries contain no `ServiceCollectionExtensions`, no `AddApplicationServices()`, no `AddSeedData()`. A new service is one `AddScoped<IXxxService, XxxService>()` line in `Program.cs`. When a dependency fails to resolve there is exactly one file to open, and no chance of a registration living in a library that the host forgot to call.
- Services throw `KeyNotFoundException` to signal a not-found condition when the operation is a straight lookup-by-ID (the controller catches this and returns 404) — see **Service Return Patterns** below for the alternative (no-exception) approaches.
- Services do **not** map to API response models — they return **service models** (see below). The controller maps service models → response models.

---

## Service Models

**Location:** `backend.Services/Model/<Feature>/`

Service models are the data contracts **between the service layer and the controller**. They mirror the shape of request/response models but live in `backend.Services` so the service layer has no dependency on `backend` (the API project).

Every service-layer data class lives here — inputs, outputs, result objects,
pagination wrappers, and options classes bound from configuration — grouped by
feature so a feature's models stay together as the project grows. **Add a folder
when a feature's first model appears**, matching the folder name used under
`Services/`. Anything genuinely shared across features goes in `Model/Common/`.

```
backend.Services/
  Model/
    Auth/
      AccessTokenModel.cs
      AuthResult.cs
      JwtOptions.cs
      LoginModel.cs
      RegisterModel.cs
      UserModel.cs
    Files/
      FileDownloadModel.cs
      FileStorageOptions.cs
      FileUploadModel.cs
      FileUploadResult.cs
      LocalFileStorageOptions.cs
      StoredFileModel.cs
    SeedData/
      SeedDataOptions.cs
      SeedUserModel.cs
      SeedUserOptions.cs
    Common/
      PagedResult.cs
      PaginationParams.cs
```

Namespaces follow the folders: `backend.Services.Model.Auth`,
`backend.Services.Model.Common`, and so on.

**Promote to `Common/` only when a second feature actually needs the model** — not
in anticipation. `UserModel` sits in `Auth/` because auth is its only consumer; the
day another feature needs it, it moves.

**Naming convention:** same name as the corresponding API request/response but with `Model` suffix instead of `Request`/`Response` (e.g. `RegisterRequest` → `RegisterModel`).

- **One class per file, with a blank line between every property** — see **Model File Conventions** below. There is **no** bundled `AuthModels.cs`; every model gets its own file from day one.

**Example pairing:**

| API Layer (`backend/Model`) | Service Layer (`backend.Services`) |
|---|---|
| `RegisterRequest.cs` | `RegisterModel` |
| `LoginRequest.cs` | `LoginModel` |
| `AuthResponse.cs` | `AuthResult` |
| `CreateProductRequest.cs` | `CreateProductModel` |
| `ProductResponse.cs` | `ProductModel` |

### Data Flow

```
Controller receives:   CreateXxxRequest   (backend/Model/Requests/)
         ↓  map (Adapt)
Service receives:      CreateXxxModel     (backend.Services/Model/<Feature>/)
         ↓  business logic
Service returns:       XxxModel           (backend.Services/Model/<Feature>/)
         ↓  map (Adapt)
Controller returns:    XxxResponse        (backend/Model/Responses/)
```

---

## Model File Conventions — One Class Per File & Property Spacing

Applies to **Request models** (`backend/Model/Requests/`), **Response models** (`backend/Model/Responses/`), and **Service models** (`backend.Services/Model/<Feature>/`).

### One class per file

Never define multiple classes/records in the same file — including small nested or line-item types. Each class gets its own file named after the class.

```csharp
// ❌ Wrong — CreateProductRequest.cs containing two classes
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public List<ProductAttributeRequest> Attributes { get; set; } = [];
}

public class ProductAttributeRequest
{
    public string Name { get; set; } = string.Empty;
}
```

```csharp
// ✅ Correct — CreateProductRequest.cs
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public List<ProductAttributeRequest> Attributes { get; set; } = [];
}
```

```csharp
// ✅ Correct — ProductAttributeRequest.cs
public class ProductAttributeRequest
{
    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
```

### Blank line between properties

Within any model/class, leave a blank line between each property — never stack properties on consecutive lines.

```csharp
// ✅ Correct
public class ExampleModel
{
    public int FieldOne { get; set; }

    public int FieldTwo { get; set; }

    public int FieldThree { get; set; }
}
```

---

## backend.Data — Entities, Enums & DbContext

### Entities

**Location:** `backend.Data/Entities/`

EF Core entity classes use **plain names** — no `Dto` suffix (e.g. `Product`, `RefreshToken`, not `ProductDto`).

```
backend.Data/
  Abstractions/
    ICurrentUserProvider.cs     ← supplies the acting user's ID to the audit stamper
  Entities/
    Common/
      IAuditableEntity.cs       ← CreatedAt / CreatedBy / UpdatedAt / UpdatedBy
      ISoftDeletable.cs         ← IsActive
      AuditableEntity.cs        ← abstract base implementing both
    ApplicationUser.cs          ← extends IdentityUser<Guid>, implements both interfaces
    ApplicationRole.cs          ← extends IdentityRole<Guid>
    RefreshToken.cs
  Enums/                        ← added with the first enum
  Constants/
    RoleNames.cs
  Data/
    ApplicationDbContext.cs
  Migrations/
```

**Entities the template ships:** `ApplicationUser`, `ApplicationRole`, `RefreshToken`.
Add domain entities feature by feature as the PRD's phases land — **do not create an
entity before the feature that needs it.**

- `ApplicationUser` extends the ASP.NET Core Identity base class (`IdentityUser<Guid>`); `ApplicationRole` extends `IdentityRole<Guid>`.
- **Every user-configured entity extends `AuditableEntity`** — it carries `CreatedAt`/`CreatedBy`/`UpdatedAt`/`UpdatedBy` and `IsActive`. See **Audit Fields & Soft Delete** below for the interfaces, the stamping mechanism, and which entities are exempt.
- **All timestamps are UTC.** Store `DateTime.UtcNow`, never `DateTime.Now`. The frontend converts to local time for display. A timestamp recorded in server-local time is worthless the moment the app is deployed anywhere but the developer's machine.
- Navigation properties use EF Fluent API configuration in `ApplicationDbContext.OnModelCreating`, not `[ForeignKey]` data annotations.
- Collections are initialised to `[]` (empty list) or `new List<T>()` by default.
- Soft-delete flag is a plain PascalCase `IsActive` boolean — not a lowercase `isActive`. See **Soft-Delete Pattern** for the naming trap around domain "enabled" flags.

---

### Audit Fields & Soft Delete — `AuditableEntity`

Every entity a user creates or edits carries a full audit trail *and* a soft-delete
flag. Two narrow interfaces express the two concerns separately; one abstract base
class implements both for the common case.

```csharp
// backend.Data/Entities/Common/IAuditableEntity.cs
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    Guid? CreatedBy { get; set; }

    DateTime? UpdatedAt { get; set; }

    Guid? UpdatedBy { get; set; }
}
```

```csharp
// backend.Data/Entities/Common/ISoftDeletable.cs
public interface ISoftDeletable
{
    bool IsActive { get; set; }
}
```

```csharp
// backend.Data/Entities/Common/AuditableEntity.cs
public abstract class AuditableEntity : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsActive { get; set; } = true;
}
```

There is no `DeletedAt`/`DeletedBy` pair. A soft delete is a modification, so
`UpdatedAt`/`UpdatedBy` already record who deactivated the row and when — a
dedicated pair would be two columns storing a copy of what the audit fields hold at
that moment, and two more chances for the copy to disagree with the original.

**Why two interfaces rather than one.** Some entities must be audited but must
never be soft-deleted — append-only history (an audit log, a delivery log) is the
usual case, where hiding a row would falsify the very record it exists to provide.
Keeping the concerns separate lets those entities implement `IAuditableEntity`
alone. Entities needing both — the overwhelming majority — just extend
`AuditableEntity` and inherit everything.

#### Which entities get what

| Group | Treatment | Reason |
|---|---|---|
| **User-configured entities** — anything a person creates and edits through the UI | Extend `AuditableEntity` | A person created it and can change or remove it — so who, when, and whether it still counts all matter |
| `ApplicationUser` | Implements `IAuditableEntity` **and** `ISoftDeletable` directly | It already extends `IdentityUser<Guid>` and C# has no multiple inheritance — the interfaces exist precisely for this case |
| `ApplicationRole` | Neither | Identity owns this table; leave it as the framework defines it |
| **System-emitted records** — `RefreshToken`, and any log/telemetry/event row | Neither — purpose-built lifecycle fields instead | Nobody "creates" them, so `CreatedBy` is always null and `UpdatedAt` duplicates a field that already exists (`IssuedAt`, `RevokedAt`, `RecordedAt`). See **Hard-Delete Exception** |

The test for a new entity: **does a user create or edit it, or does the system emit
it as a record of something that happened?** Created → `AuditableEntity`. Emitted →
its own explicit timestamps, no base class.

#### Stamping is automatic — never set audit fields by hand

Audit fields are populated by `ApplicationDbContext.SaveChangesAsync`, not by
services. A service that assigns `CreatedAt` itself is a bug waiting to happen: the
one place someone forgets is the one row that can't be traced.

```csharp
public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserProvider currentUser)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    // Override the two-argument overloads, not the convenience ones: every other
    // SaveChanges signature funnels into these, so nothing can bypass the stamp.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInformation();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var timestamp = DateTime.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = timestamp;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = timestamp;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }
}
```

One loop covers deletion too: setting `IsActive = false` puts the entity in
`EntityState.Modified`, so the same branch stamps `UpdatedAt`/`UpdatedBy` with the
deleting user and time.

Rules that follow from this:

- **Services set `IsActive = false` and nothing else** when deleting. The audit stamp follows automatically.
- `CreatedAt` is non-nullable and always set. `UpdatedAt`/`UpdatedBy` stay **null until the first edit** — "never modified" is information, and defaulting them to the creation time destroys it.
- `CreatedBy` is nullable because seeded and worker-created rows have no acting user. Nullable means "the system did it", not "we forgot".
- Background workers run outside any HTTP request, so `ICurrentUserProvider.UserId` returns `null` there — which is the correct record of what happened.
- Both `Guid?` audit columns reference `ApplicationUser.Id` conceptually but are **not** configured as foreign keys. An audit trail must survive the deletion of the user it names.

#### `ICurrentUserProvider` — a deliberate layering exception

`ApplicationDbContext` lives in `backend.Data`, which references nothing else in the
solution — so the abstraction it depends on must live there too:

```csharp
// backend.Data/Abstractions/ICurrentUserProvider.cs
public interface ICurrentUserProvider
{
    Guid? UserId { get; }
}
```

The implementation reads the `NameIdentifier` claim from `IHttpContextAccessor` and
lives in `backend.Services/Services/Auth/CurrentUserProvider.cs`. This is the **one place**
where an interface and its implementation live in different projects — everywhere
else they share a file (see **Services**). The split exists because the dependency
must point downward: `backend.Services` → `backend.Data`, never the reverse.

There is exactly one current-user abstraction in the codebase. Do not add an
`ICurrentUserService` alongside it — one concept, one word.

---

### Identity & Roles

This project uses **full ASP.NET Core Identity, including the role tables.**

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    // ...
}
```

- `AddIdentityCore` is used rather than `AddIdentity` — this is a token API, and `AddIdentity` would register cookie schemes that compete with JWT as the default authentication scheme.
- `ApplicationRole : IdentityRole<Guid>` is a real entity. `AspNetRoles` and `AspNetUserRoles` exist in the database.
- Role assignment goes through `RoleManager<ApplicationRole>` and `UserManager<ApplicationUser>.AddToRoleAsync(...)` — never by writing to the join table directly.
- The JWT carries the user's roles as `ClaimTypes.Role` claims, populated from `UserManager.GetRolesAsync(user)`. `[Authorize(Roles = ...)]` resolves against those claims.

#### Role names are constants, not magic strings

`[Authorize(Roles = ...)]` requires a compile-time constant, so a C# enum cannot be
used there. This is the **one sanctioned exception** to the no-magic-strings rule —
and it is handled with a constants class, never a bare literal:

```csharp
// backend.Data/Constants/RoleNames.cs
public static class RoleNames
{
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [Admin];
}
```

```csharp
// ✅ Correct
[Authorize(Roles = RoleNames.Admin)]
public class ProductController : ControllerBase { }

await roleManager.CreateAsync(new ApplicationRole { Name = RoleNames.Admin });

// ❌ Wrong — bare literal, a typo silently locks everyone out
[Authorize(Roles = "Admin")]
```

`RoleNames.All` is what the seeder iterates over, so adding a role is a one-line
change in one file.

#### Current role model

**Admin is the only role the template ships**, and every registered user is created
as an `Admin` of their own account. Add roles only when the feature that needs them
is actually being built — adding `Member`/`Viewer`/`Owner` "for later" creates
authorization branches that are never exercised and never tested.

---

### Account & Ownership Model

The template's default is **one user per account**. There is no `Organization`,
`OrganizationMember`, or `Invitation` entity. A user registers, becomes an `Admin`,
and owns their own records directly. If the PRD calls for teams, that model is
introduced deliberately — not assumed.

The practical consequence, and it applies to **every domain entity**:

- Every user-owned entity carries a `UserId` foreign key to `ApplicationUser`.
- **Every read query filters by the current user's ID.** A record belonging to
  someone else must be indistinguishable from a record that does not exist —
  return 404, never 403, so IDs cannot be probed.
- The current user's ID is resolved through `ICurrentUserProvider` (reads the
  `NameIdentifier` claim), injected into services. Controllers do not read
  `User.Claims` and pass IDs down; services do not read `IHttpContextAccessor`
  directly. The same abstraction feeds the audit stamper — see **Audit Fields &
  Soft Delete**.

```csharp
// ✅ Correct — ownership is part of the query, not a check afterwards
var product = await dbContext.Products
    .AsNoTracking()
    .SingleOrDefaultAsync(p => p.Id == productId
                            && p.UserId == currentUser.UserId
                            && p.IsActive);

return product is null ? null : product.Adapt<ProductModel>();
```

```csharp
// ❌ Wrong — fetches another user's row before deciding, and leaks existence
var product = await dbContext.Products.SingleOrDefaultAsync(p => p.Id == productId);
if (product.UserId != currentUser.UserId) return Forbid();
```

If teams are eventually introduced, this scoping predicate is the single place that
changes — which is exactly why it must never be duplicated as an ad-hoc `if`.

---

### Enums

**Location:** `backend.Data/Enums/`

There is a single data project — **all enums live in `backend.Data/Enums/`**,
whether they're stored on an entity or only used in service/business logic. The
folder does not exist yet; create it with the first enum.

### Never Use Magic Strings for Status/Category Fields

Any field with a fixed, known set of values (status, type, category, cause, etc.) is a **C# enum** — never a raw `string` compared against literal values. This holds even though the API ultimately sends it to the client as a string (via the global `JsonStringEnumConverter`, see **Mapping**) — the enum is what keeps the *backend* code type-safe and typo-proof; the JSON string is just the wire format, not the source of truth.

```csharp
// ❌ Wrong — magic string, no compile-time safety, typos slip through silently
public string Status { get; set; } = "draft";

if (order.Status == "Draft") // case mismatch — silently never matches, no compiler error
{
    ...
}
```

```csharp
// ✅ Correct
public enum OrderStatus
{
    Draft,
    Submitted,
    Approved,
    Cancelled,
}

public OrderStatus Status { get; set; } = OrderStatus.Draft;

if (order.Status == OrderStatus.Draft)
{
    ...
}
```

- Applies equally to entity fields, service models, and request/response models — not just database columns.
- There are exactly **two** sanctioned exceptions, both handled by a constants class with an `All` list rather than bare literals:
  - **Identity role names** (`RoleNames`) — `[Authorize(Roles = ...)]` forces a `const string`.
  - **Upload locations** (`FileUploadLocation`) — the value is a directory name on disk, so the string is the stored artefact rather than a label for one. See **File Uploads**.

  Both recover the lost compile-time safety through a runtime membership check against `All`. Anything that is neither an attribute argument nor a path segment is an enum.
- This is a one-way door in the other direction too: never *add* a `string`-typed status/type field later "for flexibility" — extend the enum instead, even if it means a migration.

### DbContext

**Location:** `backend.Data/Data/ApplicationDbContext.cs`

Single EF Core context for the whole application, extending
`IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`.

- **Provider: SQL Server** (`Microsoft.EntityFrameworkCore.SqlServer`), configured in `Program.cs` from `ConnectionStrings:DefaultConnection`. If a project needs a different provider, it changes here and in `Program.cs` only.
- `OnModelCreating` must call `base.OnModelCreating(builder)` **first** — Identity's own table configuration depends on it.
- All relationships, indexes, and column constraints are configured in `OnModelCreating`, not via data annotations.
- `SaveChangesAsync` is overridden to stamp audit fields — see **Audit Fields & Soft Delete**. Both the async and synchronous `SaveChanges` overloads must apply it, so no write path can bypass the audit trail.
- Index anything queried on a hot path, and `UserId` on every user-owned entity.
- Because **every** read on a user-configured table filters on `IsActive`, put it in the index rather than leaving it as a residual predicate — the natural shape is a composite `(UserId, IsActive)` index on each such table.

### Migrations

**Location:** `backend.Data/Migrations/`

**Never create or run migrations.** The developer handles all migrations manually. When entity changes are needed:
- Make the entity/model changes only.
- Leave a note stating that a migration is needed and what it covers.
- Do **not** run `dotnet ef migrations add` or `dotnet ef database update`.

The commands themselves are written down in **`backend.Data/migration.md`** — add,
apply, roll back, remove, list, and script. Keep that file current; it is the
developer's reference, not a Claude one.

The template ships one migration, `InitialIdentity`, covering the Identity tables
and `RefreshToken`.

### GUIDs in Seed Scripts and SQL Files

**Never generate or invent GUIDs yourself** — this applies to seeder C# files (`backend.SeedData/Seeders/`), staging/production SQL scripts, CSV seed data, and any other place where an ID needs to be hardcoded as a stable value.

- Fabricated GUIDs will diverge between dev seeder and staging SQL, making IDs inconsistent across environments.
- **Always ask the user to provide the required GUIDs** before writing seed data or SQL insert statements.
- State clearly how many GUIDs are needed and what each one represents.
- `Guid.NewGuid()` in runtime application code (normal entity creation paths, e.g. `RefreshToken.Id`) is fine — this rule only applies to **hardcoded values** in seed scripts and SQL files.

---

## backend.SeedData — Seeders

**Location:** `backend.SeedData/`

```
backend.SeedData/
  Seeders/
    RoleSeeder.cs
    UserSeeder.cs
  DatabaseSeeder.cs        ← orchestrates seeders in order
```

The options it binds (`SeedDataOptions`, `SeedUserOptions`) live in
`backend.Services/Model/SeedData/`, not here, because `SeedDataService` reads the same
configuration to serve the login screen — see **Seed Data Endpoint** below. That is
why `backend.SeedData` references `backend.Services`, and never the reverse.

Like `backend.Services`, this project carries **no DI wiring of its own** — the
seeders and their options are registered in `backend/Program.cs`.

### Rules

- **One seeder per concern**, each a small class with a single `SeedAsync` method. `DatabaseSeeder` calls them in dependency order (roles before users) and is the only thing `Program.cs` invokes.
- **Seeders are idempotent.** Every seeder checks for existence before inserting, so running the app twice never duplicates or throws. Use `RoleManager.RoleExistsAsync(...)` / `UserManager.FindByEmailAsync(...)`, not a blind insert.
- **Seeders go through `UserManager`/`RoleManager`**, never raw `DbContext.Add` for Identity entities — password hashing, security stamps, and normalized names must be produced by Identity itself.
- **Credentials come from configuration**, never from a literal in the C# file. The section is a list, so adding an account is a config edit rather than a code change:
  ```json
  "SeedData": {
    "Users": [
      { "Email": "admin@example.com", "FullName": "Administrator" }
    ]
  }
  ```
  Passwords are **not** in that file — they go in user-secrets, indexed by position:
  ```
  dotnet user-secrets set "SeedData:Users:0:Password" "<password>"
  ```
  Bind with the options pattern. A hardcoded seed password is a credential leak the moment the repo is shared.
- Seeding runs at startup inside a scoped service scope, and **only in Development** unless a deliberate flag says otherwise. It is wrapped in a try/catch that logs and continues — an un-migrated database must not stop the host from starting.
- Reference data with stable IDs needs GUIDs — see the GUID rule above and ask before writing them.

---

## Seed Data Endpoint — Development Only

`GET /api/seeddata/users` lists the seeded accounts **with their passwords**, so the
login screen's "Seed data" button can fill the form in one click.

This is a deliberate, contained exception to every instinct about credentials, and
it holds only because of how it is built:

- **Nothing is persisted.** `SeedDataService` reads the same `SeedData:Users`
  configuration the seeder does. Identity stores only password hashes, and the one
  copy of the plaintext stays in user-secrets. There is no credentials table to leak,
  back up, or forget to drop. (Some codebases solve this with a `SeedCredential`
  table — do not add one here; it puts plaintext at rest for no gain.)
- **The endpoint is unauthenticated on purpose.** Its whole job is to help someone
  sign in *before* they have a token, so `[Authorize]` would defeat it.
- **The environment gate is therefore the only protection, and it is absolute.**
  `SeedDataController` returns **404** outside Development — not an empty list, not
  403.
- **The frontend button is compiled out of production.** The panel is wrapped in
  `import.meta.env.DEV`, so a production bundle contains neither the button nor the
  URL.

If any of those four properties is ever weakened, the feature must be removed rather
than repaired. Delete this controller — and `frontEnd/src/services/seedDataService.ts` —
before a production deployment regardless.

---

## Mapping

- Uses **Mapster** — always use Mapster for mapping; never write manual mapping loops in controllers or services.
- Prefer `.Adapt<T>()` for simple mappings.
- Custom mappings are registered via Mapster's `IRegister` interface, auto-discovered at startup:
  ```csharp
  // Program.cs
  TypeAdapterConfig.GlobalSettings.Scan(
      typeof(Program).Assembly,
      typeof(AuthService).Assembly);
  ```
  The second argument is only there to reach the `backend.Services` assembly — any
  type from that project would do.
- Enums serialize to the client as **strings** via a global `JsonStringEnumConverter` registered in `Program.cs`.
- Always map **entities → response models** before returning from the controller. Never serialize EF entities directly (circular reference risk).
- Custom mapping configs live in:
  - **API layer:** `backend/Mapping/WebApiMappingConfig.cs` (Request → Service Model, Service Model → Response) — create this file when the first custom rule is needed
  - **Service layer:** `backend.Services/Mapping/ServiceMappingConfig.cs` (Entity → Service Model)

### Example

```csharp
public class ServiceMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductModel>()
            .Map(dest => dest.CategoryName, src => src.Category.Name);
    }
}
```

Only register mappings where property names **differ** or need custom logic — Mapster handles identical names automatically.

---

## General Patterns

- Services are injected via **primary constructor** parameters.
- `KeyNotFoundException` is the conventional exception thrown by the service layer to signal a "not found" condition for simple lookups — caught at the controller level and returned as 404. For operations with multiple possible business failures (not just "not found"), prefer the **Result Object Pattern** below instead of throwing.
- All responses use `return Ok(...)`, `return Problem(...)` — no raw `return StatusCode(...)` calls (except the Identity `{ errors }` shape noted under **Controllers**).
- Pagination uses `PagedResult<T>` wrapper with `Items`, `TotalCount`, `PageNumber`, `PageSize` (service layer) mapped to `PagedResultResponse<T>` (API layer) — see **Pagination** below.
- **Never generate or invent GUIDs** for seed scripts, SQL insert statements, or CSV seed data — always ask the user to provide them.
- **Secrets never live in `appsettings.json` committed to the repo** — the JWT signing key, any third-party API key, and the seed account passwords come from user-secrets in development and environment variables in production. `Jwt:SigningKey` ships empty and `Program.cs` throws at startup if it is still empty, so a missing secret fails loudly instead of silently issuing unsigned-looking tokens.

---

## Soft-Delete Pattern

Configuration records are **never hard-deleted**. The `IsActive` flag comes from
`ISoftDeletable`, inherited via `AuditableEntity` — do not redeclare it on an
entity that already extends the base.

- **Delete operations** set `IsActive = false` and call `SaveChangesAsync()`. `UpdatedAt`/`UpdatedBy` are stamped automatically and are the record of who deleted it — see **Audit Fields & Soft Delete**.
- **All read queries** must filter by `&& x.IsActive` (or `.Where(x => x.IsActive)`).
- Hard `DELETE` statements are never used on configuration entities.
- A `DELETE` endpoint on an already-inactive record returns the same result as one on a missing record. Deleting twice is not an error.

### Filtering is explicit, not a global query filter

EF Core's `HasQueryFilter` would apply `IsActive` automatically. We deliberately do
**not** use it. Global filters are invisible at the call site, behave surprisingly
across required navigations, and force `IgnoreQueryFilters()` into any code that
legitimately needs inactive rows — at which point the guarantee is gone anyway. The
predicate stays visible in the query, next to the ownership predicate it always
travels with.

If this is ever revisited, it is an all-or-nothing change across every
`ISoftDeletable` entity. Half the tables filtering silently and half filtering
explicitly is worse than either choice alone.

### Naming trap — `IsActive` is soft delete, nothing else

`IsActive` means **"this record has not been deleted."** It never doubles as a
domain on/off switch. When an entity also needs a user-facing enabled/disabled,
paused/running, or published/draft concept, that is a **second, separately named
field**:

```csharp
public class Subscription : AuditableEntity
{
    // IsActive is inherited from ISoftDeletable via AuditableEntity — do NOT
    // redeclare it here. False means the record is deleted and must never be returned.

    // The domain switch — false means paused, but the user still sees the record
    public bool IsPaused { get; set; }
}
```

Collapsing the two into one boolean means a paused record disappears from the UI, or
a deleted record keeps being processed. Keep them distinct, and name the domain
field for what it actually controls.

---

## Hard-Delete Exception — High-Volume Telemetry

Soft delete is the rule for configuration. It is the **wrong** choice for
append-only, high-volume tables — event logs, audit trails, metrics, and any table
that gains millions of rows a month and is purged on a retention schedule.

When a project introduces such a table:

- The entity extends **neither `AuditableEntity` nor any audit interface**. A
  soft-delete flag on a table that is purged wholesale is a bug, not a safety net —
  and `CreatedBy`/`UpdatedBy` on a row no human ever touched is four dead columns
  multiplied by every row ever written. Give it one purpose-built timestamp
  (`RecordedAt`, `OccurredAt`) and nothing more.
- Deletion happens in bounded batches (`ExecuteDeleteAsync` with a `Take`), never
  one unbounded `DELETE` over the whole table.
- If historic aggregates must survive the purge, roll rows up into summary records
  **before** deleting them.
- The rule stays intact everywhere else: every configuration entity is
  soft-deleted only.

If you are unsure which side a new entity falls on, ask: *does a user configure it,
or does the system emit it?* Configured → soft-delete. Emitted → retention.

---

## EF Core Query Conventions

### AsNoTracking on read queries

Every query that does **not** mutate data must call `.AsNoTracking()`:

```csharp
// Read — no tracking needed
return await dbContext.Products
    .AsNoTracking()
    .Where(p => p.UserId == currentUser.UserId && p.IsActive)
    .ToListAsync();

// Write — tracking required (entity is mutated then saved)
var product = await dbContext.Products
    .SingleOrDefaultAsync(p => p.Id == productId && p.UserId == currentUser.UserId);
product.IsActive = false;
await dbContext.SaveChangesAsync();
```

### SingleOrDefaultAsync for primary key lookups

When fetching a single record by its primary key (`Id`), use `SingleOrDefaultAsync` — not `FirstOrDefaultAsync`. A primary key guarantees at most one row; `SingleOrDefaultAsync` enforces this at the query level.

```csharp
var product = await dbContext.Products
    .SingleOrDefaultAsync(p => p.Id == productId && p.IsActive)
    ?? throw new KeyNotFoundException($"Product with ID {productId} was not found.");
```

The same applies to any column with a unique index — look it up with
`SingleOrDefaultAsync`, not `FirstOrDefaultAsync`.

---

## Pagination — Two Wrappers

There are two pagination wrapper classes that work together:

| Class | Project | Used by |
|---|---|---|
| `PagedResult<T>` | `backend.Services/Model/Common/` | Returned by the **service layer** |
| `PagedResultResponse<T>` | `backend/Model/Responses/Common/` | Returned by the **controller** to the client |

The controller maps `PagedResult<T>` → `PagedResultResponse<T>` via Mapster before returning. Two reusable helpers sit either side of the model/helper split:

- **`PaginationParams`** (`backend.Services/Model/Common/`) — the single paging input (`PageNumber`, `PageSize`, `Search?`). Controllers bind it directly (`[FromQuery] PaginationParams paging`) and pass it straight to the service; endpoints needing extra filters (status, date range, type) **inherit** it rather than re-declaring page fields.
- **`PaginationExtensions.ToPagedResultAsync(pageNumber, pageSize)`** (`backend.Services/Common/`) — the count + skip/take + clamp (`MaxPageSize = 200`) behind every paged query. An extension class, so it belongs with the helpers rather than the models.

**Convention:** where a list endpoint also has "load all" consumers (e.g. a dropdown that needs every option), keep the plain endpoint and add a companion **`GET …/paged`**; otherwise convert the endpoint in place. Frontend page-size defaults live in one file — `frontEnd/src/config/pagination.ts` (`DEFAULT_PAGE_SIZE`, `PAGE_SIZE_OPTIONS`) — consumed by a shared `components/ui/Pagination.tsx` bar (add it with the first paged list).

Any endpoint returning a list that grows without bound is paged from day one.

---

## Absolute Route Override in Controllers

When an endpoint needs a route that does **not** start with the controller's `[Route("api/[controller]")]` base prefix, use a leading `/` in the `[HttpGet]`/`[HttpPost]` attribute to override it completely:

```csharp
[Route("api/[controller]")]
[ApiController]
public class WebhookController : ControllerBase
{
    // Full URL: POST /hooks/{token}  (overrides base route — public, no /api prefix)
    [AllowAnonymous]
    [HttpPost("/hooks/{token}")]
    public async Task<IActionResult> Receive(string token) { }

    // Full URL: GET /api/webhook/history/{id}  (uses base route)
    [HttpGet("history/{id:guid}")]
    public async Task<IActionResult> GetHistory(Guid id) { }
}
```

Use this only for genuinely public, non-`/api` surface (webhooks, short links, badges).
Everything else stays under the controller's base route.

---

## Result Object Pattern (Auth-style)

When an operation can fail for several distinct business reasons and the caller needs to relay a message (not just a true/false or 404), return a **result object** with a `Succeeded` flag and an error list, rather than throwing:

```csharp
public class AuthResult
{
    public bool Succeeded { get; set; }

    public IReadOnlyList<string> Errors { get; set; } = [];

    // ... payload fields (only meaningful when Succeeded) ...

    public static AuthResult Fail(params string[] errors) => new() { Succeeded = false, Errors = errors };
}
```

**Service:**
```csharp
public async Task<AuthResult> LoginAsync(LoginModel model)
{
    var user = await userManager.FindByEmailAsync(model.Email);
    if (user is null || !user.IsActive)
    {
        return AuthResult.Fail("Invalid email or password.");
    }
    // ...
}
```

**Controller:**
```csharp
var result = await authService.LoginAsync(model);
if (!result.Succeeded)
{
    return Unauthorized(new { errors = result.Errors });
}
return Ok(result.Adapt<AuthResponse>());
```

Use this pattern for multi-reason business operations — registration, login, and any
rule where "it failed, and here is why" is more useful than a status code alone
(quota limits, state-machine violations). Use the simpler **Service Return Patterns**
below (null/bool) for straightforward CRUD.

---

## Service Return Patterns — Not-Found Handling

Two accepted patterns for a not-found condition in the service layer, in addition to the **Result Object Pattern** above:

### Pattern A — Throw exception

Used when the caller always expects the record to exist and its absence is truly exceptional.

```csharp
var product = await dbContext.Products
    .SingleOrDefaultAsync(p => p.Id == productId)
    ?? throw new KeyNotFoundException($"Product with ID {productId} was not found.");
```

Controller catches `KeyNotFoundException` and returns 404.

### Pattern B — Return `null` / `bool` (preferred for new CRUD services)

Avoids using exceptions for normal control flow. More semantically correct for simple record lookups and mutations.

**GetById — return `null` when not found:**
```csharp
// Interface
Task<ProductModel?> GetByIdAsync(Guid productId);

// Implementation
public async Task<ProductModel?> GetByIdAsync(Guid productId)
{
    var product = await dbContext.Products
        .AsNoTracking()
        .SingleOrDefaultAsync(p => p.Id == productId
                                && p.UserId == currentUser.UserId
                                && p.IsActive);

    return product?.Adapt<ProductModel>();
}
```

**Update / Delete — return `bool`:**
```csharp
// Interface
Task<bool> UpdateAsync(Guid productId, UpdateProductModel model);
Task<bool> DeleteAsync(Guid productId);

// Implementation
public async Task<bool> UpdateAsync(Guid productId, UpdateProductModel model)
{
    var product = await dbContext.Products
        .SingleOrDefaultAsync(p => p.Id == productId
                                && p.UserId == currentUser.UserId
                                && p.IsActive);
    if (product is null) return false;

    // ... apply changes ...
    await dbContext.SaveChangesAsync();
    return true;
}
```

**Controller — check return value, no catch needed for not-found:**
```csharp
var result = await productService.GetByIdAsync(productId);
if (result is null)
{
    logger.LogWarning("Product with ID {ProductId} was not found.", productId);
    return Problem(detail: $"Product with ID {productId} was not found.", statusCode: StatusCodes.Status404NotFound);
}
return Ok(result.Adapt<ProductResponse>());
```

- No `KeyNotFoundException` catch block needed in the controller when using Pattern B.
- The `catch (Exception ex)` → 500 block is still always present.
- Note that "not found" and "belongs to another user" produce the **same** 404 — see **Account & Ownership Model**.

---

## Update Endpoints — Route ID, Not Body ID

For `PUT` endpoints, **do not include `Id` in the request body or service model** — the ID is already on the route. Pass it as the first parameter to the service method.

```csharp
// ✅ Correct
public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    // No Id here
}

[HttpPut("{productId:guid}")]
public async Task<IActionResult> Update(Guid productId, [FromBody] UpdateProductRequest request)
{
    var model = request.Adapt<UpdateProductModel>();
    var updated = await productService.UpdateAsync(productId, model);
    ...
}

// Interface
Task<bool> UpdateAsync(Guid productId, UpdateProductModel model);
```

```csharp
// ❌ Wrong — don't put Id in the body
public class UpdateProductRequest
{
    public Guid Id { get; set; }   // redundant — already in route

    public string Name { get; set; } = string.Empty;
}
```

---

## Validators — Numeric Bounds

For `decimal` or `int` properties that represent intervals, timeouts, quantities, or ranges, always add both a lower and upper bound:

```csharp
RuleFor(x => x.Quantity)
    .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.")
    .LessThanOrEqualTo(1000).WithMessage("Quantity must not exceed 1000.");

RuleFor(x => x.EndsAt)
    .GreaterThan(x => x.StartsAt).WithMessage("The end date must be after the start date.");
```

- Apply each single-field bound **before** any cross-field comparison so the rules fire independently and the user gets the specific message.
- Agree the upper limit with the domain before hardcoding it. Where a bound depends on the current user (a plan limit, a role allowance), it is **not** a validator concern — enforce it in the service layer via the Result Object Pattern, because the validator has no access to the current user.

---

## `[Flags]` Bitwise Enum Pattern

Use the `[Flags]` attribute when an entity field can hold **multiple combined values** (bitfield stored as `int`) — a notification rule that fires on any combination of events is the canonical case.

```csharp
[Flags]
public enum NotificationEvents : int
{
    None        = 0,
    Created     = 1,
    Updated     = 2,
    Deleted     = 4,
    Completed   = 8,
    Failed      = 16,
    // All = 31  (1 | 2 | 4 | 8 | 16 — no separate member needed)
}
```

- **Location:** `backend.Data/Enums/`.
- The combined value (e.g. `31`) is stored as an `int` column in the database.
- Never add a dedicated enum member for every combination — let bitwise OR handle it.

**Reading flags (C#):**
```csharp
bool notifiesOnDeleted = (rule.Events & NotificationEvents.Deleted) != 0;
```

**Handling flags in the frontend (TypeScript):**

```ts
const NOTIFICATION_EVENTS = {
  None: 0,
  Created: 1,
  Updated: 2,
  Deleted: 4,
  Completed: 8,
  Failed: 16,
} as const;

const notifiesOnDeleted = (events & NOTIFICATION_EVENTS.Deleted) !== 0;
```

- The API sends and receives the flags field as a plain `number` (it is an `int`, not a named enum value, so the string converter does not apply).
- Mirror the C# enum values as a plain `const` object in the frontend — do not use a TypeScript `enum` for bitflags.

---

## Background Workers

**Location:** `backend/Workers/` — created with the first worker.

Scheduled and long-running work runs as hosted background workers, not as request
handlers. This is the one place where code executes outside a controller, and it
follows the same layering discipline.

### Rules

- **One worker, one job.** Each worker extends `BackgroundService` and overrides `ExecuteAsync`. If a worker needs "and" to describe it, split it.
- **Workers hold no business logic.** A worker is a loop and a clock. It resolves a service and calls one method. All decision-making lives in `backend.Services`, where it can be unit-tested without a host.
  ```csharp
  public class CleanupWorker(IServiceScopeFactory scopeFactory, ILogger<CleanupWorker> logger)
      : BackgroundService
  {
      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
          while (!stoppingToken.IsCancellationRequested)
          {
              try
              {
                  using var scope = scopeFactory.CreateScope();
                  var cleanupService = scope.ServiceProvider.GetRequiredService<ICleanupService>();
                  await cleanupService.RemoveExpiredRecordsAsync(stoppingToken);
              }
              catch (Exception ex)
              {
                  logger.LogError(ex, "Cleanup iteration failed.");
              }

              await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
          }
      }
  }
  ```
- **Always create a scope.** `BackgroundService` is a singleton; `DbContext` is scoped. Resolve scoped services through `IServiceScopeFactory` inside the loop, never by injecting `ApplicationDbContext` into the worker's constructor.
- **A failed iteration never kills the worker.** Wrap each iteration in try/catch, log, and continue. An unhandled exception in `ExecuteAsync` silently stops the host's background processing.
- **Honour the `CancellationToken`** — pass it to every async call and to `Task.Delay`, so shutdown is prompt.
- **Workers do not talk to each other.** State changes are written to the database by one service and picked up by the next worker.
- Workers are registered with `AddHostedService<T>()` in `Program.cs`, and each one is individually switchable via configuration so a developer can run the API without them.

---

## File Uploads

Uploading is a shared service, not something a feature implements for itself. Bytes
go to a storage provider; the **storage key** that comes back is the only thing any
other part of the system ever holds.

### The pieces

| File | Role |
|---|---|
| `backend.Services/Common/FileUploadLocation.cs` | The upload locations, as constants. One entry per feature that uploads |
| `backend.Services/Services/Files/FileUploadService.cs` | `IFileUploadService` + implementation. Owns every rule — ownership, location, size, extension |
| `backend.Services/Services/Files/IFileStorage.cs` | Where bytes physically live. The extension point |
| `backend.Services/Services/Files/LocalFileStorage.cs` | The one implementation today: the server's own disk |
| `backend.Services/Model/Files/` | `FileUploadModel`, `FileUploadResult`, `StoredFileModel`, `FileDownloadModel`, `FileStorageOptions`, `LocalFileStorageOptions` |
| `backend/Controllers/FilesController.cs` | `POST upload`, `GET download`, `DELETE` |
| `backend.Data/Enums/FileStorageProvider.cs` | `Local`, `Azure` — which provider is configured |

`IFileStorage` is the **one interface in the solution that gets its own file** rather
than sharing one with its implementation (see **Services**). It has more than one
implementation and so belongs to none of them. Do not take this as licence to split
other interfaces out.

### The storage key is the whole contract

```
App_Data/Upload/8f9b3599-…-4636e5930ea6/Profile/bcd95f0c63734f3cbfc08e0da43aab61.png
       └─root─┘└──────── UserId ────────┘└Location┘└──── generated file name ────┘
        │       │                          │         │
        │       │                          │         └ generated GUID + the original extension
        │       │                          └ a FileUploadLocation constant
        │       └ taken from the token, never from the caller
        └ FileStorage:Local:RootPath — keep it outside wwwroot
```

The key returned by an upload is `Upload/{userId}/{location}/{file}` — a
provider-independent path, never an absolute one and never a blob URL. That is what
makes a provider switch invisible: the same string resolves under a disk root or
inside a container.

**The file name is generated, never the uploaded one.** A user-supplied name can be
a path; a GUID cannot. The original name is returned for display only.

### Using it — the normal flow

The client uploads first, then sends the returned key to the feature endpoint that
owns it:

```
POST /api/files/upload?location=Profile   →  { "storageKey": "Upload/…/Profile/….png", … }
PUT  /api/profile                          →  { "profileImagePath": "Upload/…/Profile/….png" }
```

The feature stores that string on its own entity. **`Files` owns no table** — there
is no `UploadedFile` entity and no migration for uploads themselves. The column
lives wherever the file logically belongs:

```csharp
public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, ISoftDeletable
{
    /// <summary>Storage key from the upload service, or null when no picture is set.</summary>
    public string? ProfileImagePath { get; set; }
}
```

Adding that column **does** need a migration — see **Migrations**; the developer runs it.

### Using it — from another service

A service that needs to store a file as part of a larger operation injects
`IFileUploadService` and passes the location constant:

```csharp
public class ProfileService(
    IFileUploadService fileUploadService,
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider) : IProfileService
{
    public async Task<ProfileResult> SetPictureAsync(FileUploadModel picture, CancellationToken cancellationToken = default)
    {
        var upload = await fileUploadService.UploadAsync(picture, FileUploadLocation.Profile, cancellationToken);

        if (!upload.Succeeded)
        {
            return ProfileResult.Fail([.. upload.Errors]);
        }

        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == currentUserProvider.UserId, cancellationToken);

        if (user is null)
        {
            return ProfileResult.Fail("The account no longer exists.");
        }

        // Replacing a picture must remove the old bytes, or every change leaks a file
        // nothing points at any more.
        if (!string.IsNullOrWhiteSpace(user.ProfileImagePath))
        {
            await fileUploadService.DeleteAsync(user.ProfileImagePath, cancellationToken);
        }

        user.ProfileImagePath = upload.File!.StorageKey;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ProfileResult.Success(upload.File.StorageKey);
    }
}
```

`UploadAsync` returns a `FileUploadResult` rather than throwing, because an upload
fails for several distinct reasons at once — see **Result Object Pattern**. Relay
`Errors`; do not flatten them to a bare 400.

### Adding an upload location

One line, plus the `All` entry:

```csharp
public static class FileUploadLocation
{
    public const string RootFolder = "Upload";

    public const string Profile = "Profile";

    public const string Attachment = "Attachment";   // ← new

    public static readonly IReadOnlyList<string> All = [Profile, Attachment];   // ← and here
}
```

Forgetting `All` means the service rejects every upload to the new location, which is
the safe direction to fail but easy to misread as a broken endpoint.

**These values are persisted data.** Renaming one orphans every file already stored
under the old value — the saved paths keep pointing at a folder nothing reads any
more. Add freely; rename only by moving the folders to match.

> **Why strings and not an enum.** This is a deliberate, named exception to
> **Never Use Magic Strings for Status/Category Fields** — the second one in the
> codebase, alongside `RoleNames`. The value *is* a directory name on disk, so the
> string is the stored artefact rather than a label for one, and `All` plus the
> service's membership check recover the safety the compiler would otherwise give.
> Do not extend this exception to anything that is not a path segment.

### Adding a storage provider

1. Write `AzureFileStorage : IFileStorage` in `backend.Services/Services/Files/`.
2. Add an `Azure` object beside `Local` in the `FileStorage` configuration section, bound by a new `AzureFileStorageOptions` in `backend.Services/Model/Files/`.
3. Add one arm to the `switch` in `Program.cs`.

Nothing that consumes `IFileUploadService` changes. Selecting a provider with no
implementation throws **at startup**, naming the file to write — not at the first
upload in production.

Implementations handle bytes only. Size limits, extension whitelists and ownership
stay in `FileUploadService`, so every provider enforces them identically and a new
provider cannot forget one.

### Rules

- **Persist the storage key, never a filesystem path or a URL.** A path ties the row to one provider and one machine.
- **Never build a key by hand.** `Upload/{user}/{location}/…` is composed in exactly one method. A key that did not come from `UploadAsync` is not a key.
- **Never parse a key to decide anything** — least of all ownership. See below.
- **The content type served back is derived from the whitelisted extension**, never from the client's declared type. A browser acts on that value, so the uploader must not choose it.
- **Downloads are `Content-Disposition: attachment` with `X-Content-Type-Options: nosniff`.** Serving inline is a per-endpoint decision to be taken deliberately, not a default.
- **`.svg` is not whitelisted.** An SVG is a document that can carry script; allowing it means hosting attacker-authored script on the API's own origin the moment anything is served inline.
- **`FileStorage:Local:RootPath` stays outside `wwwroot`.** Static file middleware serves whatever it can reach, which would hand out other users' uploads without ever consulting the ownership check.
- **Deleting the owning row must delete the file.** Nothing sweeps orphans; a soft-deleted row still holds the only reference to those bytes.

### The ownership check — shape, not scan

There is no database row behind an upload, so authorisation reads the key itself.
That is only safe because the key's **whole shape** is validated:

```csharp
var segments = storageKey.Split(['/', '\\']);

return segments.Length == StorageKeySegmentCount
    && string.Equals(segments[0], FileUploadLocation.RootFolder, StringComparison.Ordinal)
    && Guid.TryParse(segments[1], out var ownerId)
    && ownerId == userId.Value
    && FileUploadLocation.All.Contains(segments[2], StringComparer.Ordinal)
    && IsSafeFileName(segments[3]);
```

Reading the user id out of segment 1 and stopping there is the obvious
implementation, and it is broken:

```
❌ Upload/{mine}/Profile/../../{theirs}/Profile/x.png
```

Segment 1 really is the caller's own id, so a scan-style check passes it. The
provider then calls `Path.GetFullPath`, which collapses the `..` and lands inside
another user's folder — still under the storage root, so the provider's own
"did it escape the root?" check sees nothing wrong either. Both guards pass and
another user's file is served or deleted.

Demanding exactly four segments, splitting on **both** separators and **keeping
empty entries** means a `..`, a backslash or a doubled slash fails here and never
reaches the point where it would be normalised away.

The same reasoning applies to any future identifier that encodes a path: validate
its shape against what you issue, rather than scanning it for the part you care
about.

### Known gap

`MaxFileSizeBytes` is enforced after ASP.NET has already buffered the request, so an
oversized upload costs the disk and bandwidth before it is refused. Closing it
properly needs an `IResourceFilter` — the only filter that runs *before* model
binding — because `[RequestSizeLimit]` takes a compile-time constant and cannot read
configuration, and raising Kestrel's global limit would loosen every JSON endpoint
too. There is no rate limiting on the upload endpoint either. Both are worth
addressing before a public deployment; neither is wired up today.

---

---

# Frontend Code Structure & Conventions

**Framework:** React + TypeScript (Vite)
**Root:** `frontEnd/src/`

**What the template ships:** the public marketing pages (home, about us, contact us),
the auth pages (log in, sign up, log out), a 404, the shared layout (`Navbar`,
`Footer`, `PageLayout`), the UI primitives (`Button`, `TextField`, `TextAreaField`,
`Alert`), and the session plumbing — `apiClient` with JWT attachment and
refresh-on-401, `authService`, `seedDataService`, `tokenStorage`, and an
`AuthProvider`/`useAuth` pair. The marketing page copy is placeholder content:
replace it, don't build around it.

**Dev server talks to the API through a Vite proxy** (`/api` → `https://localhost:7157`,
`secure: false` in `vite.config.ts`; the dev server itself runs on port `5173`). That
keeps the browser on one origin, so there is no CORS preflight and no need to trust
the ASP.NET dev certificate. A deployed build sets `VITE_API_BASE_URL` to the real
API origin instead; `apiClient` reads that and falls back to `/api`. The backend's
`Cors:AllowedOrigins` setting is the matching list for non-proxied origins.

> **Styling convention:** components use **semantic design tokens** —
> `bg-surface`, `bg-surface-raised`, `bg-surface-sunken`, `text-ink`, `text-ink-soft`,
> `text-muted`, `bg-brand`, `bg-brand-strong`, `bg-brand-soft`, `text-on-brand`,
> `border-line`, `bg-danger-soft`/`text-danger`, `bg-success-soft`/`text-success`,
> `bg-warning-soft`/`text-warning` — defined as CSS variables in `index.css` and
> exposed via Tailwind v4 `@theme inline`. **Do not use raw `gray-*`/`red-*`/`green-*`
> utilities or `dark:` variants** in new UI; the tokens re-skin across light and dark
> automatically, and re-branding the app is then an edit to `index.css` alone.
> Shared primitives live in `components/ui/`.
>
> If a component library (e.g. shadcn/ui) is adopted, its components must be
> re-skinned onto these tokens on the way in. A library is not a licence to ship raw
> colour utilities or `dark:` variants.

---

## Components

**Location:** `frontEnd/src/components/`

All UI lives here. Every visual element, page, modal, tab, and form is a component.

```
frontEnd/src/components/
  pages/           ← one file per route
  layout/          ← Navbar, Footer, PageLayout (the routed <Outlet /> shell)
  ui/              ← shared primitives: Button, TextField, TextAreaField, Alert
  <Feature>/       ← sub-folder per feature once it has multiple files
  modals/
```

### Rules

- Components contain **only UI logic** — rendering, local state, user interactions.
- No raw `fetch`/`axios` calls inside components — all API calls go through a service function.
- No type/interface/enum definitions inside component files — all types live in `frontEnd/src/types/`.
- Group related components into a sub-folder when a feature has multiple files.

---

## Types

**Location:** `frontEnd/src/types/`

All TypeScript types, interfaces, and enums live here. Organised by domain/feature.
The template ships `api.ts`, `auth.ts`, `contact.ts`, `seedData.ts`, and `ui.ts`; add
one file per feature.

### Rules

- **Every** type, interface, and enum used across the app is defined here — never inline in a component or service file.
- This project's tsconfig has `erasableSyntaxOnly` on, which real TS `enum` declarations violate (they're not purely erasable — they emit a runtime object). Use a `const` object + derived type instead, with **string values** matching the backend's JSON output (the API serializes C# enums as strings via a global `JsonStringEnumConverter`):
  ```ts
  export const OrderStatus = {
    Draft: 'Draft',
    Submitted: 'Submitted',
    Approved: 'Approved',
    Cancelled: 'Cancelled',
  } as const;

  export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus];
  ```
- **Never hardcode the string literal in component/service code** — reference the const object's property so a typo is a compile error instead of a silent runtime bug:
  ```ts
  // ❌ Wrong — magic string, a typo here compiles fine and just silently fails
  if (order.status === 'Submited') { ... }

  // ✅ Correct
  if (order.status === OrderStatus.Submitted) { ... }
  ```
- Group related types into the same file by feature (e.g. all auth-related types in `auth.ts`).

---

## Services

**Location:** `frontEnd/src/services/`

All API calls and seed/static data live here. One service file per domain/feature.

```
frontEnd/src/services/
  apiClient.ts       ← shared axios client with base URL + auth headers
  tokenStorage.ts    ← the only module that touches localStorage
  authService.ts
  seedDataService.ts ← development-only; delete before production
```

### Rules

- All HTTP calls (`GET`, `POST`, `PUT`, `PATCH`, `DELETE`) are made **only** from service files — never directly in a component.
- Services import types from `frontEnd/src/types/` — never define their own types inline.
- Seed data or static lookup data also lives in the relevant service file, not in components.
- `apiClient.ts` is the shared HTTP client (base URL, JWT auth header attachment, and refresh-token retry on 401) — individual service files call through it.
- If TanStack Query is adopted, it **wraps** these service functions — hooks call `productService.getAll()`, they do not call `axios` themselves. The service layer stays the only place a URL appears.

---

## Frontend Data Flow

```
Component (UI + local state)
    ↓  calls
Service function  (frontEnd/src/services/)
    ↓  HTTP request
Backend API  (backend)
    ↑  response
Service function  (maps/returns typed response)
    ↑  typed data
Component  (renders result)
```

---

## Frontend Summary Table

| Concern | Location |
|---|---|
| UI, pages, modals, forms | `frontEnd/src/components/` |
| TypeScript types, interfaces, enums | `frontEnd/src/types/` |
| API calls, token storage, seed/static data | `frontEnd/src/services/` |
| React context objects and providers | `frontEnd/src/context/` |
| Custom hooks | `frontEnd/src/hooks/` |
| Shared helpers (dates, errors) | `frontEnd/src/utils/` |
| App-wide constants (page sizes) | `frontEnd/src/config/` |
| Design tokens, global styles | `frontEnd/src/index.css` |

---

## Session State — Context, Not a Store

Authentication state lives in a React context, not in a global store and not in
component state duplicated across pages.

- `context/AuthContext.ts` holds **only** the `createContext` call. No JSX, so the
  provider file stays a pure component module and fast refresh behaves.
- `context/AuthProvider.tsx` is the provider component.
- `hooks/useAuth.ts` is the accessor, and throws if called outside the provider —
  a missing provider should fail loudly at the call site, not hand back `undefined`.
- The context holds React state. **Persistence is a service concern**:
  `services/tokenStorage.ts` owns `localStorage`, separately from `authService` so
  `apiClient` can read the token without importing the service that calls it.
- **Only tokens are persisted, never the user.** On boot the provider calls
  `/api/auth/me`; a stored token proves nothing on its own, since it may be expired
  or revoked. `isRestoringSession` covers that round trip so the UI does not flash
  a signed-out navbar at a signed-in user.
- Refresh-on-401 lives in `apiClient` and is **single-flight** — redeeming a refresh
  token revokes it, so several concurrent 401s must share one refresh, not race to
  spend the same token.

---

## Types Must Not Live in Service Files

All TypeScript interfaces, types, and enums **must** be defined in `frontEnd/src/types/` — never inside a service file or component file. Service files may re-export types from the types folder for convenience, but must not define them inline.

```ts
// ✅ Correct — type defined in types/, imported into service
import { AuthResponse } from '../types/auth';

// ❌ Wrong — type defined inline in the service file
export interface AuthResponse { ... }
```

---

## Modal Pattern — `createPortal` + Body Scroll Lock

All modals that overlay the full UI **must** use `createPortal` to render at `document.body` level and lock body scroll while open. This prevents z-index conflicts caused by CSS stacking contexts (e.g. `transform`, `overflow`) on ancestor elements — a plain `z-[9999]` alone does **not** fix it when the modal is rendered inside a transformed parent.

### Required pattern

```tsx
import { createPortal } from 'react-dom';
import { useEffect } from 'react';

export const MyModal: React.FC<{ onClose: () => void }> = ({ onClose }) => {
  // Lock body scroll while modal is open
  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = ''; };
  }, []);

  return createPortal(
    <>
      {/* Backdrop — z-[9998] */}
      <div className="fixed inset-0 bg-black/50 z-[9998]" onClick={onClose} />

      {/* Panel — z-[9999] */}
      <div className="fixed z-[9999] top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-surface rounded-xl shadow-2xl ...">
        {/* content */}
      </div>
    </>,
    document.body
  );
};
```

### Rules

- Always use `z-[9998]` for the backdrop and `z-[9999]` for the panel — never `z-40`/`z-50` for full-page modals.
- The `useEffect` scroll lock must restore `overflow` in its cleanup function so the body is never left locked.
- Use `createPortal(..., document.body)` — **not** an inline render inside the component tree.

---

## `extractApiError` — Frontend Error Utility

All components that call service functions must use `extractApiError` (in `frontEnd/src/utils/apiErrors.ts`) to surface error messages from API responses. It reads the `detail` (Problem response) or `errors` (Identity `{ errors }` shape) field from the backend response.

```ts
import { extractApiError } from '../../utils/apiErrors';

try {
  await productService.create(request);
} catch (err) {
  setError(extractApiError(err, 'Failed to create product. Please try again.'));
}
```

- First argument: the caught error.
- Second argument: fallback string shown for unexpected (5xx / network) errors.
- For 4xx errors the backend's `detail`/`errors` message is shown directly to the user.

---

## Date & Time Handling (Frontend) — dayjs

All date/time formatting, parsing, and arithmetic in the UI **must** use `dayjs` — never raw `Date` methods (`.setDate()`, `.getMonth()`, `Intl.DateTimeFormat`, etc.) beyond simple read-only property access. `dayjs` ships with the template, and `utils/dateUtils.ts` already registers the `utc`, `relativeTime`, and `duration` plugins.

- Shared helpers live in `frontEnd/src/utils/dateUtils.ts` — it starts with `formatDateShort`, `formatDateTime`, and `formatRelative`. Add to it as date logic is needed, instead of duplicating date logic in components.
- **The API returns UTC.** Parse with the `utc` plugin and convert to local time for display — `dayjs.utc(value).local()`. Never render a raw UTC string as if it were local.
- Relative phrasing ("2 min ago") and durations ("14 days, 3 hours") come from the `relativeTime` and `duration` plugins — do not hand-roll them.
- Use dayjs format tokens (`'ddd'`, `'dddd'`, `'MMM'`, `'MMMM'`, `'YYYY-MM-DD'`, `'D MMM'`, `'HH:mm'`, etc.) instead of hand-rolled day/month name lookup arrays.
- **Gotcha:** when building a "first of month" date for a given year/month, call `.date(1)` **before** `.year()`/`.month()` — otherwise dayjs can roll the date into the wrong month if the current day-of-month doesn't exist in the target month:
  ```ts
  const firstOfMonth = dayjs().date(1).year(targetYear).month(targetMonth);
  ```

---

---

# Starting a New Project From This Template

Everything below is per-project setup. Nothing in it is a coding convention — it is
the list of places that still carry template defaults.

### 1. Name the project

| File | What to change |
|---|---|
| `backend/appsettings.json`, `backend/appsettings.Development.json` | `ConnectionStrings:DefaultConnection` database name, `Jwt:Issuer`, `Jwt:Audience` |
| `backend/Program.cs` | Swagger `Title`, `Description`, `SwaggerEndpoint` display name, `DocumentTitle` |
| `frontEnd/index.html` | `<title>` and the `description` meta tag |
| `frontEnd/src/components/layout/Navbar.tsx`, `Footer.tsx` | Brand name |
| `frontEnd/src/components/pages/{HomePage,AboutUsPage,ContactUsPage}.tsx` | Placeholder marketing copy |
| `frontEnd/src/services/tokenStorage.ts` | `SESSION_KEY` prefix — keeps localStorage separate from other apps on the same host |
| `frontEnd/public/favicon.svg` | App icon |
| `frontEnd/src/index.css` | `--brand*` tokens, if the brand colour differs |

The project folders themselves (`backend`, `backend.Data`, `backend.Services`,
`backend.SeedData`) are deliberately generic — renaming them means touching every
namespace, the `.slnx`, and the `Compile Remove` globs in `backend.csproj`. Leave
them unless there is a real reason not to.

### 2. Set the secrets

Nothing runs until `Jwt:SigningKey` is set — `Program.cs` throws at startup if it is
empty, by design.

```
dotnet user-secrets init                                    # only if UserSecretsId needs regenerating
dotnet user-secrets set "Jwt:SigningKey" "<32+ char key>"
dotnet user-secrets set "SeedData:Users:0:Password" "<password>"
```

### 3. Database

Update the connection string, then apply the shipped `InitialIdentity` migration —
the commands are in `backend.Data/migration.md`. Remember that **Claude never runs
migrations**; the developer does.

### 4. Before the first production deployment

- Delete `backend/Controllers/SeedDataController.cs`, `backend.Services/Services/SeedData/`, `backend/Model/Responses/SeedData/`, and `frontEnd/src/services/seedDataService.ts`, plus the login screen's seed-data panel.
- Move every secret from user-secrets to environment variables.
- Set `Cors:AllowedOrigins` to the real frontend origin and `VITE_API_BASE_URL` to the real API origin.
