# CLAUDE.md — TaskBoard Backend (.NET 10)

> Instructions for Claude Code working inside `backend/`. Read this file before changing code. The source brief is **".NET - Technical Interview Exercise", version 6 (3 pages)**. This is a small, interview-ready project, not a production SaaS platform.

## 0. Binding project decisions (override anything below that conflicts)

These were agreed with the project owner and reflect `../Documents/USER-STORY.md` (US-001), which **supersedes** the original defaults in this file:

- **Task fields:** `Title` required, trimmed, **≤ 50** chars; `Description` optional, **≤ 400** chars (blank → `null`); `DueDate` **required**, stored as a **UTC `DateTime`** (column `DueDateUtc`), past dates allowed; `Status` `todo | inProgress | done` (C# enum `TaskItemStatus`).
- **User fields:** `Name` (required, ≤ 100), `Email` (required, ≤ 254, trimmed + lower-cased, unique), `TimeZone` (required, valid **IANA** id), `PasswordHash`.
- **Timezones:** clients send `dueDate` as a **local wall-clock** value (no offset) in the user's configured timezone; the backend converts it to UTC. Task responses include `dueDateUtc` (UTC instant), `dueDateLocal` (wall-clock in the user's timezone) and a server-computed **`isOverdue`** (`status != done && dueDateUtc < now UTC`).
- **Ordering:** overdue tasks first (earliest due first), then due date ascending, title, id.
- **Response envelope:** every response **with a body** is an `ApiResult` (`{ isSuccess, data, error: { code, message, validationErrors? } }`), including 400/401/404/409/500. `204 No Content` responses (DELETE) have no body. This replaces ProblemDetails.
- **Validation:** FluentValidation validators (Application layer) run through a Minimal API endpoint filter; domain entities still guard invariants.
- **Mapping:** manual `ToDto()` mapping; no AutoMapper (licensing + KISS). **No MediatR.**
- **Seeding:** EF Core Code First **`HasData` in migrations** seeds two users (`demo@example.test`, `bob@example.test`, both `DemoPass123!`) and their tasks with fixed GUIDs/dates and a precomputed password hash. Migrations are applied on Development startup (`MigrateAsync`) or via `dotnet ef database update`.
- **TDD evidence:** git commits `test(red): …` → `feat(green): …` → `refactor: …`.

## 1. Mission and scope

Implement an ASP.NET Core **.NET 10** REST API for a personal task-management application, using **Clean Architecture, EF Core 10 with file-based SQLite, xUnit, and test-driven development**. Favor code that a senior engineer can explain clearly in a live code review.

**Informal user story:** As a registered user, I want to securely manage my own tasks and move them between To Do, In Progress, and Done, so I can see and maintain my work progress.

Two **logical API modules** live in **one deployable Web API**:

1. **Task Management:** authenticated CRUD operations for tasks, including changing status.
2. **User Management / Authentication:** user registration, login, current user, and a demonstrably public endpoint.

Do **not** create two microservices or two host processes unless the interviewer explicitly clarifies that "a second API" requires separate deployments. Document this interpretation in `README.md`.

## 2. Non-negotiable engineering rules

- Target **.NET 10**, ASP.NET Core Minimal APIs, C#, and an aligned **EF Core 10 SQLite provider** (`Microsoft.EntityFrameworkCore.Sqlite`). Do **not** use `Microsoft.EntityFrameworkCore.InMemory`.
- **Clean Architecture is mandatory:** dependencies point inward; Domain never references EF Core, HTTP, ASP.NET, or Infrastructure.
- **TDD is mandatory for our implementation:** write a failing xUnit test, implement the minimum production code to pass it, then refactor. Show the **RED → GREEN → REFACTOR** steps, not just tests added afterward.
- Enforce **SOLID**, **Clean Code**, **KISS**, and **YAGNI** pragmatically. Simplicity beats extra patterns.
- Put **entity invariants and state transitions** in domain entities when appropriate; coordinate cross-entity/application workflows in the Application layer.
- Use **repository interfaces** to keep application logic storage-independent. Infrastructure implements repositories using EF Core.
- Prefer async I/O, cancellation tokens, dependency injection, nullable reference types, and explicit errors.
- Keep Minimal API endpoint handlers thin: HTTP binding, authentication/authorization, dispatch to application use cases, and HTTP result mapping only.
- Expose accurate **OpenAPI** definitions so the Angular app can generate NSwag TypeScript clients. Treat the OpenAPI schema as the client/server contract.
- Never claim tests passed or the application ran unless the corresponding commands were actually executed.

## 3. Exercise brief: important interpretations and deviations

| Brief | Implementation and caveat |
| --- | --- |
| "ASP.NET MVC, Web API" in overview | We choose a **Minimal API + Angular SPA**; this satisfies Web API and separate frontend requirements, but **does not literally implement server-rendered ASP.NET MVC**. Flag for the reviewer rather than hide it. |
| "A second API" for users | Interpret as a second **endpoint group/module** (`/api/auth`) rather than another executable project. This is a potentially ambiguous requirement. |
| Database **or other data storage**, separate users and data | Use a **file-based SQLite relational database** with separate `Users` and `TaskItems` tables. Data persists across application restarts. Seed a known demo user and tasks idempotently on development startup. |
| Business logic layer independent of data and API | Domain + Application must not import EF, HTTP types, or endpoint DTOs. |
| All components tested | Include domain, application, repository/infrastructure, and HTTP endpoint tests. |
| Task system described under "Generative AI tools" | Use that task-management example as the actual exercise use case. Explicitly note this interpretation in the README. |
| User story, presentation, AI evidence | Provide an informal user story and document actual prompts, a representative code sample, review decisions, fixes, edge cases, authentication, and validation. |

**SQLite is relational**, supports foreign keys, unique indexes, and transactions, and can persist to a local `.db` file. Enforce email uniqueness with a **database unique index** as well as appropriate application validation; configure and verify foreign-key enforcement. Explain SQLite's provider-specific limitations when relevant instead of claiming it behaves identically to SQL Server. Do not equate **SQLite in-memory mode used in tests** with the non-relational EF Core InMemory provider.

## 4. Proposed structure (adapt to existing repository if present)

```text
backend/
  TaskBoard.sln
  src/
    TaskBoard.Domain/
      Entities/TaskItem.cs
      Entities/User.cs
      Enums/TaskStatus.cs
      Exceptions/
    TaskBoard.Application/
      Abstractions/ITaskRepository.cs
      Abstractions/IUserRepository.cs
      Abstractions/IPasswordService.cs
      Abstractions/ITokenService.cs
      Abstractions/ICurrentUser.cs
      Tasks/                       # Use cases and request/response models
      Auth/                        # Use cases and request/response models
    TaskBoard.Infrastructure/
      Persistence/TaskBoardDbContext.cs
      Persistence/Configurations/
      Persistence/Migrations/
      Persistence/Repositories/
      Authentication/
      Seeding/
      DependencyInjection.cs
    TaskBoard.Api/
      Endpoints/TaskEndpoints.cs
      Endpoints/AuthEndpoints.cs
      Endpoints/PublicEndpoints.cs
      Program.cs
      appsettings.Development.json
  tests/
    TaskBoard.Domain.Tests/
    TaskBoard.Application.Tests/
    TaskBoard.Infrastructure.Tests/
    TaskBoard.Api.Tests/
  README.md
  docs/
    ai-assisted-development.md
    architecture-decisions.md
```

**Dependency direction:** Domain → nothing; Application → Domain; Infrastructure → Application + Domain; API → Application and Infrastructure **only at the composition root**. Do not let Infrastructure implementations leak into handlers or use cases. Keep packages and projects lean; combine tests if that makes the solution more maintainable.

Avoid generic repositories, `IUnitOfWork` abstractions added without a reason, unnecessary base entities, event buses, microservices, mapping libraries, distributed caches, and elaborate DDD machinery.

## 5. Domain model and business rules

### `TaskItem`

- `Id`: `Guid`, generated by the server, never client-assigned.
- `UserId`: `Guid`, assigned from the **authenticated principal**; never trust a user ID sent by the frontend.
- `Title`: required, trimmed, non-whitespace, at most **50 characters**.
- `Description`: optional, at most **400 characters**.
- `Status`: string-serialized enum with values **`todo`, `inProgress`, `done`** (C# `TaskItemStatus`).
- `DueDateUtc`: **required** UTC `DateTime`; converted from the user's local wall-clock input using the user's IANA timezone (see §0).

Methods such as `Create`, `UpdateDetails`, and `ChangeStatus` should protect invariants. A user may move a task to any of the three statuses (including backwards); don't invent approval workflows. Allow past due dates (overdue tasks are valid). Add timestamps, priority, ordering, tags, or pagination **only when justified**; they are not in the exercise.

### `User`

- `Id`: `Guid`.
- `Name`: required, at most 100 characters.
- `Email`: required, normalized for **case-insensitive uniqueness**; avoid returning it in unnecessary task responses.
- `TimeZone`: required, valid IANA timezone id (e.g. `America/Sao_Paulo`).
- `PasswordHash`: never store or log plaintext passwords; do not expose the hash in API responses.

Use ASP.NET Core's maintained password hashing facilities (for example `PasswordHasher<TUser>`) behind an Application abstraction. Register/login validation is application logic; crypto and JWT implementation are infrastructure concerns. Use a sensible demo password policy and state it in README.

### Ownership and security

- All `/api/tasks` operations require a valid bearer token.
- Query by current authenticated `UserId` or enforce ownership *before* returning or mutating data.
- Return **404** for nonexistent **or someone else's** task to prevent ID enumeration.
- User registration and login are anonymous; `/api/auth/me` is protected; `/api/public/ping` is explicitly anonymous to satisfy the brief's authorized and non-authorized endpoint demonstration.
- Validate JWT signature, issuer, audience, expiry, and signing-key configuration. Do not hardcode deployable secrets. Local demo values belong in development config, environment variables, or user secrets.
- Never accept `PasswordHash`, `UserId`, or privileged fields in client request DTOs.

## 6. REST contract — frontend/backend shared agreement

Prefer `/api` routes, JSON camelCase, stable DTOs, and explicit typed/declared response metadata. **Do not rename fields or routes without regenerating clients and updating the frontend.**

| Method | Route | Auth | Success | Notes |
| --- | --- | --- | --- | --- |
| POST | `/api/auth/register` | Anonymous | **201 Created** | `{ name, email, timeZone, password }`; return a safe user DTO; include `Location` only if it points to an actual implemented resource; duplicate email **409**. |
| POST | `/api/auth/login` | Anonymous | **200 OK** | `{ email, password }` → `{ accessToken, expiresAt, user }`; bad credentials **401**. |
| GET | `/api/auth/me` | Required | **200 OK** | Current user DTO; missing/invalid token **401**. |
| GET | `/api/public/ping` | Anonymous | **200 OK** | Small public test endpoint; no user data. |
| GET | `/api/tasks` | Required | **200 OK** | **Current user's** tasks only; return `[]` when empty. |
| GET | `/api/tasks/{id}` | Required | **200 OK** | **404** if absent/not owned. |
| POST | `/api/tasks` | Required | **201 Created** | `{ title, description?, dueDate }` (`dueDate` = local wall-clock); status defaults to `todo`; return `Location` and task DTO. |
| PUT | `/api/tasks/{id}` | Required | **200 OK** | Replace editable fields `{ title, description?, dueDate, status }`; **404** absent/not owned. |
| PATCH | `/api/tasks/{id}/status` | Required | **200 OK** | `{ status }`; persisted Kanban column change; **404** absent/not owned. |
| DELETE | `/api/tasks/{id}` | Required | **204 No Content** | **404** absent/not owned. |

Common response rules: every response with a body uses the `ApiResult` envelope (§0). **400** for malformed or invalid request data (with `error.validationErrors`), **401** unauthenticated, **409** registration conflict, **500** unexpected failure (never expose stack traces). Use `403` only if a real authenticated-but-forbidden policy is introduced; do not pretend ownership checks imply 403. Specify `operationId`s, DTO schemas, error responses, and bearer security in OpenAPI.

**Task response DTO:** `id`, `title`, `description`, `status`, `dueDateUtc`, `dueDateLocal`, `isOverdue`. Do not expose password data or private internals. The choice of **PATCH** is an intentional, small UX addition for drag-and-drop; full updates remain available via PUT.

**No persisted ordering:** Kanban allows moving between **status columns**, but the required model does not contain `sortOrder`. Do not promise stable custom ordering within a column. Use deterministic title/due-date/ID sorting, or add persisted ordering only after explicitly documenting and testing that extension.

## 7. CQRS + MediatR: explicit decision gate

**Decision by default: use separate read/write use cases (CQRS-style) but NO MediatR dependency initially.** A small CRUD project can be clearer with ordinary application services such as `CreateTask`, `GetMyTasks`, `UpdateTask`, `ChangeTaskStatus`, `DeleteTask`, `RegisterUser`, `LoginUser`.

Before introducing MediatR:

1. Record which **actual problem** it solves (many handlers, cross-cutting pipeline behaviors, mediator decoupling) and what extra concepts/tests it adds.
2. Compare a direct `ITaskService`/use-case approach with `IRequest`/`IRequestHandler` and explain the difference between **CQRS** (separating commands and queries) and a **mediator** (dispatch pattern).
3. Check current **MediatR version/licensing requirements** before adding it; newer releases use license-key/commercial/OSS licensing rules. Do not suppress licensing issues or introduce unverified dependencies.
4. Only adopt it if benefits visibly exceed complexity. If adopted, keep handlers thin and domain rules in entities; don't create a command/query/handler folder for every single property read.

Document the final choice and rationale in `docs/architecture-decisions.md` so it can be defended in the interview.

## 8. SQLite persistence, EF Core migrations, and demo data

### Application database — relational and persistent

- Use **`Microsoft.EntityFrameworkCore.Sqlite`**, version aligned with **EF Core 10**, and `TaskBoardDbContext` with `DbSet<TaskItem>` and `DbSet<User>`. **Do not use the EF Core InMemory provider.**
- Use a local **file-based SQLite database** (for example `Data Source=taskboard.db`), configured through `ConnectionStrings:Default` in `appsettings.Development.json`. Document the working-directory-dependent path or use an explicit stable application path. Keep the database file and SQLite journal files out of Git (`.gitignore`).
- Keep `DbContext`, Fluent API mappings, EF queries, provider configuration, and **EF Core migrations** in Infrastructure. Create and commit the initial migration; document `dotnet ef database update` and how to apply migrations. Apply pending migrations on development startup only if explicitly chosen and documented; **do not use `EnsureCreated()` together with migrations**.
- Configure a real **one-to-many relationship** `User` → `TaskItem` via `TaskItem.UserId`, a required foreign key, and a deliberate delete behavior (favor `Restrict`/`NoAction` unless a user-deletion use case explicitly requires cascade). Ensure SQLite foreign-key constraints are enabled and test them.
- Define appropriate indexes, especially a **unique index on normalized user email**. Normalize consistently for lookup and uniqueness checks; map unique constraint failures to a suitable application error/HTTP **409 Conflict** even under concurrent registration attempts. Never rely only on a pre-insert `AnyAsync` check.
- Map required columns and length constraints using EF Fluent API, but retain validation/domain invariants in Domain/Application. SQLite type behavior and length enforcement may differ from SQL Server: test critical rules rather than assuming `HasMaxLength` enforces a length limit in SQLite.
- Use focused `ITaskRepository` and `IUserRepository` interfaces. Do not abstract EF with a generic repository framework. Keep `SaveChangesAsync` ownership clear and avoid extra transaction/unit-of-work layers without demonstrated need.
- Seed `demo@example.test` and `bob@example.test` (both `DemoPass123!`) plus tasks distributed across statuses through **`HasData` in migrations** (fixed GUIDs, fixed UTC dates, a password hash precomputed with the standard `PasswordHasher`). Because it lives in a migration, the seed runs exactly once per database (in every environment where migrations are applied); document this. Display credentials in README as **demo-only**. Existing records must survive restarts and must not be duplicated or overwritten.
- Explain the persistent local `.db` lifecycle: restarting the app **does not reset tasks**, but deleting the file intentionally resets local demo data (after recreating schema and seed). No SQL Server installation or separate database server is required.

### Tests — SQLite in-memory, not EF Core InMemory

- Use **SQLite in-memory** (`Data Source=:memory:`) for repository/database integration tests so actual relational SQL semantics, foreign keys, and unique indexes are exercised.
- Open and **keep one `SqliteConnection` alive for each test's database lifetime**; an SQLite in-memory database disappears when the connection closes. Construct the EF DbContext with that open connection, create schema using `EnsureCreated()` **for isolated integration test databases only**, and dispose the connection after the test. For tests specifically covering migrations, create an isolated file-based database and run actual migrations instead.
- Each test must have isolated DB state: no reused shared open connections across parallel tests unless deliberately coordinated. Avoid mocking EF Core LINQ/repositories when testing persistence.
- Test relational foreign-key rejection, normalized email uniqueness, repository CRUD/query behavior, transaction-dependent cases where relevant, idempotent seeding, and persistence across **app restarts** against a temporary file-based database when appropriate.

## 9. TDD operating procedure

For **every behavior**:

1. State the smallest testable behavior and choose the right level (unit vs repository/integration vs HTTP integration).
2. Write a meaningful **failing xUnit test** and run it. Record why it failed (RED).
3. Add only the code needed to pass; rerun it (GREEN).
4. Refactor production/test code with no behavior changes; run the test suite (REFACTOR).
5. Update relevant documentation and OpenAPI if the public contract changed.

Prioritize tests for:

- Domain title/description validation, default status, valid/invalid status transitions and ownership identity invariants.
- App-level registration duplicate email normalization, invalid login, task ownership filtering, attempted cross-user read/update/delete, status changes.
- Infrastructure repository CRUD with **SQLite in-memory**; verify foreign-key constraints, unique indexes, real SQL queries, and test isolation. Also test app restart persistence with a temporary file-based SQLite database.
- API endpoint **status codes, response bodies/locations, 400/401/404/409**, auth requirements, public endpoint, and OpenAPI availability.
- Demo seed works, passwords are hashed, and a logged-in demo user can manage only their own tasks.

Use **xUnit**, `dotnet test`, minimal test builders/fakes and, when appropriate, `WebApplicationFactory` for real HTTP integration tests. Do not mock EF LINQ behavior to 'prove' repository correctness. Don't chase an arbitrary 100% coverage metric; focus on the acceptance criteria and critical error paths. Preserve evidence of the TDD sequence in commits or `docs/ai-assisted-development.md`.

## 10. OpenAPI, NSwag interoperability, and CORS

- Use .NET 10 built-in `AddOpenApi()` / `MapOpenApi()` (or a clearly documented compatible alternative), with stable names and response schemas. Expose the OpenAPI JSON for local generation; document its URL.
- **Test actual NSwag generation** against the emitted document. .NET 10 defaults to OpenAPI 3.1; if the chosen NSwag version cannot consume it correctly, configure compatible OpenAPI 3.0 output **and document why**. Never assume generation succeeds.
- Make the auth token schema visible to generated clients. Keep API DTOs public-contract-focused and separate from EF entities.
- Restrict dev CORS to the actual Angular origin; no permissive wildcards with credentials. Prefer Angular's development proxy for local API routing if straightforward.
- Include an easy-to-run `dotnet run` and `dotnet test` sequence in README, **SQLite migration/init/reset instructions** (including `dotnet ef database update` as applicable), plus the demo API base URL and regeneration instructions.

## 11. Delivery workflow: small, reviewable slices

1. Read existing files and the exercise brief; state the proposed architecture and missing assumptions **before** major code changes.
2. Scaffold the solution and dependencies. Verify `dotnet build`.
3. TDD Domain (`TaskItem`, `User`), then Application ports and use cases.
4. TDD SQLite Infrastructure repositories, EF Core schema/migrations, auth implementation, and idempotent dev-only seeding.
5. TDD auth endpoints and task endpoints with correct auth/status semantics.
6. Generate and inspect OpenAPI. Coordinate with frontend NSwag generation.
7. Run complete tests and build; document warnings and outstanding limitations.
8. Finish README and interview evidence: informal user story, architecture/dependency diagram, AI prompt(s), representative generated code, what was verified/corrected, and security/edge-case notes.

**When working autonomously:** propose/implement one vertical slice at a time; do not manufacture a large speculative architecture. If you change domain/API decisions, call out the changes and sync `frontend/CLAUDE.md` and generated client expectations.

## 12. Definition of done

- [ ] Two logical modules: user registration/login + task CRUD; anonymous and protected routes are visible.
- [ ] File-based SQLite relational database with separate `Users` and `TaskItems` tables, valid FK, unique normalized email index, migrations, and requested task fields.
- [ ] Clean Architecture dependencies verified; domain rules are not accidentally in controllers/repositories.
- [ ] Every task operation is scoped to the logged-in user.
- [ ] Appropriate HTTP verbs/statuses and accurate OpenAPI metadata.
- [ ] Seeded development user/tasks work after a restart without duplicates or overwriting changes; **persistent** local database lifecycle documented.
- [ ] TDD evidence and meaningful unit/integration/endpoint tests; `dotnet build` and `dotnet test` actually run successfully.
- [ ] Angular frontend can generate and use NSwag clients.
- [ ] README and presentation material address deviations, AI usage, design decisions, and limitations.

## 13. Useful official references

- .NET support: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- ASP.NET Core OpenAPI: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0
- EF Core SQLite provider: https://learn.microsoft.com/en-us/ef/core/providers/sqlite/
- Testing with SQLite in-memory: https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database#sqlite-in-memory
- EF Core migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- MediatR repository/licensing information: https://github.com/LuckyPennySoftware/MediatR
