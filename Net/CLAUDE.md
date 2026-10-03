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
- CORS: `Cors__AllowedOrigins` holds the allowed origins separated by commas, validated by `CorsOriginsParser`. The policy is `DefaultCors`: explicit origins, methods and headers, no credentials.
- The API exposes `GET /health`.
- In .NET 10 the `Program` class is public for the test project. Never declare a `Program` class.
- Tests that need configuration supply it with `UseSetting` on the web host builder, never with real environment variables.

## 8. Environment

- macOS, shell zsh. Use `curl`, never `curl.exe`.
- Port 5000 is taken by AirPlay Receiver. Never configure the API on port 5000. Locally the API listens on `https://localhost:7015` and `http://localhost:5116`.
- SQL Server runs in Docker and is normally off. Never run a command that needs the database and never design a test that connects to it.
- Build and test always from `Net/`: `dotnet build DanmaobErp.slnx` and `dotnet test DanmaobErp.slnx`.
