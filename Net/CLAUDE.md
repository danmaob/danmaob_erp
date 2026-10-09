# DANMAOB_ERP — Backend (`Net/`) — Permanent instructions for the executor

This file contains only permanent rules and verified facts. The work for each task arrives in a prompt. The prompt always defines exactly what to do. When this file and a prompt differ, the prompt wins.

## 1. Scope restriction (always in force)

- You may only read and write inside `/Users/luisortiz/Desarrollo/DANMAOB/DANMAOB_ERP/Net/`. Never read, list or search anything outside it: not `../prompts/`, `../Docs/`, `../React/`, `../MAUI/` or any parent folder.
- Inside `Net/`, never read, list or search `bin/`, `obj/` or `docs/` unless a prompt names a specific file there.
- Do only what the "Task" section of the prompt says. Do not investigate, explore or improve anything on your own. If something is ambiguous or a fact is missing, STOP and report it in Spanish.
- Use only the facts written in the prompt ("Repository facts" and "Task"). Never reconstruct a path, namespace, type or member name from memory or from a pattern you expect.
- Never rewrite a whole existing file. Edit only the lines the prompt describes, at the anchors it quotes.
- If a fix fails 3 times, STOP and report in Spanish the exact lines you would change and the exact code you intended. Do not redesign code to make it compile.

## 2. Permanent hard rules

- Do not run `dotnet clean`. Do not delete `bin/` or `obj/`.
- The only git command you may run is the `git status` requested in a prompt's Verification section.
- Do not run `dotnet ef` unless a prompt explicitly asks for a migration.
- Do not install, remove or update NuGet packages unless a prompt explicitly asks for it.
- Do not add comments, XML documentation, logging, validation, attributes or `using` directives beyond what the prompt specifies.
- When a build or a test fails, paste the literal output and stop. Do not diagnose or fix unless the prompt says so.
- Your loaded skills and rules are general guidance. When they suggest something the prompt does not ask for, the prompt wins.
- Every report you write is in Spanish. It includes the step-by-step table with literal evidence and the literal `git status` output requested by the prompt.

## 3. Stack (verified)

- .NET 10 (`net10.0`), `LangVersion latest`. SDK pinned in `global.json` (10.0.400, roll forward to the latest feature band). Never use APIs or patterns from earlier .NET, ASP.NET Core or EF Core versions.
- ASP.NET Core Web API with controllers.
- xUnit for tests, NetArchTest.Rules for architecture tests.
- EF Core 10 (packages 10.0.12) with SQL Server. Persistence tests use the EF Core in-memory provider; no test connects to SQL Server.
- `DanmaobErp.Infrastructure` references the ASP.NET Core shared framework (`FrameworkReference` `Microsoft.AspNetCore.App`, without version) for localization, file providers and logging. Never add a version or a NuGet package for those namespaces.
- No MediatR, no CQRS, no mediator library of any kind.

## 4. Repository layout (verified)

| Path | Content |
|---|---|
| `DanmaobErp.slnx` | Solution |
| `global.json` | SDK version |
| `Directory.Build.props` | Settings shared by every project: `LangVersion latest`, `Nullable` enabled, `ImplicitUsings` enabled, nullable warnings as errors |
| `src/DanmaobErp.Domain/` | Domain layer |
| `src/DanmaobErp.Application/` | Application layer |
| `src/DanmaobErp.Infrastructure/` | Infrastructure layer |
| `src/DanmaobErp.Api/` | ASP.NET Core Web API (controllers) |
| `tests/DanmaobErp.ArchitectureTests/` | Architecture tests |
| `tests/DanmaobErp.Api.Tests/` | API tests: unit tests of API classes and integration tests that host the API in memory with `WebApplicationFactory<Program>` |
| `tests/DanmaobErp.Infrastructure.Tests/` | Infrastructure tests: persistence model and query filters, with the EF Core in-memory provider |
| `docs/adr/` | Architecture Decision Records (read only when a prompt names one) |

The folder name, project name and root namespace of every project are identical. A namespace always follows the folder path below its project folder.

## 5. Allowed dependencies between layers (enforced by tests)

| Project | May reference |
|---|---|
| `DanmaobErp.Domain` | Nothing |
| `DanmaobErp.Application` | `DanmaobErp.Domain` |
| `DanmaobErp.Infrastructure` | `DanmaobErp.Application`, `DanmaobErp.Domain` |
| `DanmaobErp.Api` | `DanmaobErp.Application`, `DanmaobErp.Infrastructure` |

Domain and Application never reference Entity Framework Core or ASP.NET Core packages. The architecture tests in `tests/DanmaobErp.ArchitectureTests/` fail if any of these rules is broken. Never modify those tests to make a build pass.

## 6. Code conventions (verified)

- File-scoped namespace declarations. A block-scoped namespace is a build error (IDE0161, enforced by `.editorconfig`).
- Indentation with 4 spaces, never tabs.
- Every type and member declares its accessibility modifier explicitly (`public`, `private`, `internal`, `protected`). A missing modifier is a build error (IDE0040, enforced by `.editorconfig`).
- `Nullable` is enabled and every nullable warning is a compile error. Never suppress nullable warnings with `!` or `#pragma` unless a prompt explicitly says so.
- `ImplicitUsings` is enabled. Never add `using` directives for `System`, `System.IO`, `System.Linq`, `System.Collections.Generic`, `System.Threading` or `System.Threading.Tasks`.
- The test project has a global using for `Xunit`. Never add `using Xunit;`.

## 7. Configuration (verified)

- Settings arrive as environment variables named `Section__Key`, read as the configuration key `Section:Key`. There are no user-secrets. Sensitive or environment-specific values never go in `appsettings*.json`.
- The keys the API needs to start are listed in `src/DanmaobErp.Api/Configuration/RequiredConfigurationKeys.cs` (property `All`). `RequiredConfiguration.EnsurePresent` stops the API at startup and names every missing variable, never its value. Add a key to that list only when a prompt says so.
- Database: `ConnectionStrings__Erp` (key `ConnectionStrings:Erp`, constant `RequiredConfigurationKeys.ErpConnectionString`) holds the SQL Server connection string. It is required at startup and is used only in `AddPersistence`. Tests supply it with `UseSetting`; its value is never used to connect.
- CORS: `Cors__AllowedOrigins` holds the allowed origins separated by commas, validated by `CorsOriginsParser`. The policy is `DefaultCors`: explicit origins, methods and headers, no credentials.
- The API exposes `GET /health`.
- In .NET 10 the `Program` class is public for the test project. Never declare a `Program` class.
- Tests that need configuration supply it with `UseSetting` on the web host builder, never with real environment variables.

## 8. Environment

- macOS, shell zsh. Use `curl`, never `curl.exe`.
- Port 5000 is taken by AirPlay Receiver. Never configure the API on port 5000. Locally the API listens on `https://localhost:7015` and `http://localhost:5116`.
- SQL Server runs in Docker and is normally off. Never run a command that needs the database and never design a test that connects to it.
- Build and test always from `Net/`: `dotnet build DanmaobErp.slnx` and `dotnet test DanmaobErp.slnx`.

## 9. Persistence (verified)

- There is one `DbContext`: `ErpDbContext`, in `src/DanmaobErp.Infrastructure/Persistence/ErpDbContext.cs`. Never modify it unless a prompt says so. It is not sealed: tests derive `TestErpDbContext` from it. Its constructor takes `DbContextOptions<ErpDbContext>` and `ITenantContext`.
- SQL Server schemas: `DatabaseSchemas.Platform` (`platform`, platform catalog) and `DatabaseSchemas.Erp` (`erp`, operational data of each company), in `src/DanmaobErp.Infrastructure/Persistence/DatabaseSchemas.cs`.
- Every entity derives from `DanmaobErp.Domain.Common.Entity`: `Guid Id` with `private set`, assigned in the constructor with `Guid.CreateVersion7()`. Never generate an `Id` anywhere else.
- Entities with logical deletes derive from `DanmaobErp.Domain.Common.SoftDeletableEntity`: `IsActive`, `Deactivate()` and `Reactivate()`. There are no physical deletes.
- `OnModelCreating` applies the configurations of each entity first and the model-wide configurations last: `EntityKeyConfiguration` (non-clustered primary key on `Id` and a shadow `ClusterKey` identity column with a unique clustered index) and `SoftDeleteQueryFilter` (named query filter `SoftDelete`). Never map `ClusterKey` and never call `HasKey` in an entity configuration.
- Query filters are named. To include deactivated entities, ignore only the `SoftDelete` filter by name. Never call `IgnoreQueryFilters()` without filter names.
- Every property of an entity is mapped explicitly in its configuration. For a relationship with only a foreign key and no navigation property, use `HasOne<TRelated>()` without arguments.
- The connection string is registered only in `AddPersistence` (`src/DanmaobErp.Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`).
- Migrations live in `src/DanmaobErp.Infrastructure/Persistence/Migrations/` and are created only with the exact `dotnet ef migrations add` command a prompt gives. They are generated code for `.editorconfig`: never edit them by hand. Never run `dotnet ef database update`.

## 10. Tenant isolation (verified)

- Operational entities (the data of one company) implement `DanmaobErp.Domain.Common.ITenantOwned`: a `Guid TenantId` property with `private set`. The entity never assigns `TenantId`: `TenantWriteGuard` assigns it when the entity is saved for the first time.
- Platform catalog entities (accounts, companies, users, Control Plane) do not implement `ITenantOwned` and must be listed in `PlatformCatalog.EntityTypes` (`src/DanmaobErp.Infrastructure/Persistence/PlatformCatalog.cs`). A test fails if an entity of the model is neither tenant-owned nor in that list.
- `DanmaobErp.Application.Tenancy.ITenantContext.RequireTenantId()` is the only source of the current tenant. It returns the tenant or throws `TenantNotResolvedException`. In the API, `HttpTenantContext` (scoped) reads the claim `tenant_id` (`TenantClaimTypes.TenantId`). Never add a default tenant, a tenant header, a query string value or any fallback.
- The named query filter `Tenant` (`TenantQueryFilter`) is the last statement of `OnModelCreating`. Ignoring `SoftDelete` by name keeps it. Never ignore the `Tenant` filter in operational code.
- `ErpDbContext` overrides `SaveChanges(bool)` and `SaveChangesAsync(bool, CancellationToken)`; both call `TenantWriteGuard.Apply` before saving. Never remove these overrides and never replace them with an interceptor. A write of a tenant-owned entity of another tenant throws `CrossTenantWriteException`.
- Infrastructure tests build contexts with `FixedTenantContext` (a fixed tenant, or `null` for a request without tenant), never with the API classes.

## 11. Errors and messages (verified)

- Every error response is `ProblemDetails` (`application/problem+json`) with `status`, `code` (`ApiErrorCodes`, stable, English), `title` (translated) and `traceId`; validation errors add `errors`. `ApiExceptionHandler` (`src/DanmaobErp.Api/Errors/`) builds them: never build an error response by hand in a controller.
- To reject input for a business rule, throw `DanmaobErp.Application.Errors.ValidationFailedException` with field names and message keys (422). `TenantNotResolvedException` maps to 401 and `CrossTenantWriteException` to 403. Anything else is a 500 that never exposes the exception message, type or stack trace.
- Never write user-facing text in code. Every message has a key in `DanmaobErp.Application.Localization.MessageKeys` and a text in the dictionary `src/DanmaobErp.Api/Localization/es.json`. Prompts never edit `es.json`: the user does. A test fails if a key of `MessageKeys` is missing in the dictionary.
- Input models declare every required field with `[Required(ErrorMessage = MessageKeys.Validation.Required)]`, and every other validation attribute with a key of `MessageKeys.Validation`. Messages never include the field name. The implicit required rule of non-nullable properties is disabled.
- `IStringLocalizer` (from `Microsoft.Extensions.Localization`) is the only way to get a message. The language of a request (header `Accept-Language`) changes only the messages: number and date formats of the API stay invariant.
- Logs are always in English, use message templates (never string interpolation) and never contain request bodies, passwords or fiscal data.
