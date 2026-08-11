# Migrations

**Run every command from the `backend/` folder** (the one holding `backend.csproj`).
`--project` names where the migration files are written; `--startup-project` names
the app whose configuration and DI supply the connection string.

Migrations are created and applied **by the developer only**. Claude changes
entities and says a migration is needed; it never runs these.

---

## Install the tool (once per machine)

```
dotnet tool update --global dotnet-ef
```

## Create a migration

```
dotnet ef migrations add <Name> --project backend.Data --startup-project .
```

First one for this project:

```
dotnet ef migrations add InitialIdentity --project backend.Data --startup-project .
```

## Apply to the database

```
dotnet ef database update --project backend.Data --startup-project .
```

## Remove the last migration

Only works while it is still unapplied. If it has already been applied, roll the
database back first (see below), then remove.

```
dotnet ef migrations remove --project backend.Data --startup-project .
```

## Roll back to an earlier migration

```
dotnet ef database update <PreviousMigrationName> --project backend.Data --startup-project .
```

To undo everything:

```
dotnet ef database update 0 --project backend.Data --startup-project .
```

## Inspect

```
dotnet ef migrations list --project backend.Data --startup-project .
dotnet ef dbcontext info --project backend.Data --startup-project .
```

## Generate a SQL script instead of applying

For staging and production, where `database update` is not run against a live server.

```
dotnet ef migrations script --idempotent --output migrate.sql --project backend.Data --startup-project .
```

`--idempotent` guards every step with an existence check, so the script is safe to
run against a database at an unknown migration level.

---

## Notes

- Migrations land in `backend.Data/Migrations/`.
- The connection string comes from the startup project — `appsettings.Development.json`
  unless `ASPNETCORE_ENVIRONMENT` says otherwise. Confirm the target with
  `dotnet ef dbcontext info` before running `database update`.
- `ApplicationDbContext` takes an `ICurrentUserProvider`, so design-time commands
  build the app's DI container to resolve it. That is why `--startup-project` is
  required and cannot be omitted.
- Seeding runs at app startup, not as part of a migration. After
  `database update`, restart the API and the `Admin` role plus the configured seed
  users are created.
