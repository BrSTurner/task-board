# Architecture decisions

These are short ADRs (decision, context, consequences) to support defending the design in a code review.

## 1. Clean Architecture with four projects

**Decision:**
- `Domain` (no dependencies);
- `Application` (references Domain plus FluentValidation);
- `Infrastructure` (EF Core, Identity's `PasswordHasher`, JWT);
- `Api` (Minimal API).

Api references Infrastructure only to call `AddInfrastructure()` and to share the JWT conventions used for token validation.

**Why:** The brief asks for business logic that is independent of the data and API layers. Repository and service *ports* (`ITaskRepository`, `IUserRepository`, `IPasswordService`, `ITokenService`, `ICurrentUser`) live in Application. Their implementations live in Infrastructure, or in Api for `ICurrentUser`, which reads `HttpContext`.

**Consequences:**
- Use cases are unit-tested with small hand-written fakes.
- Persistence is tested against a real SQLite engine.
- The cost is a little indirection, which is acceptable at this size.

## 2. Use-case classes, CQRS-style, without MediatR

**Decision:** Each operation is a plain class with an `ExecuteAsync` method (`CreateTask`, `GetMyTasks`, `ChangeTaskStatus`, …), registered directly in DI. There is no interface per use case and no mediator.

**Why:**
- **CQRS vs. mediator.** CQRS (separating reads from writes) is already visible in how the classes are split; a mediator is only a dispatch mechanism.
- **Nothing for MediatR to do.** With ten handlers and no cross-cutting pipeline beyond validation (an endpoint filter already does that), MediatR would add concepts and indirection without solving a problem.
- **Licensing.** Recent MediatR versions need a license key.

## 3. Result pattern internally, one `ApiResult` envelope externally

**Decision:**
- **Expected failures** (validation, not found, conflict, unauthorized) are returned as `Result` / `Result<T>` carrying an `Error(code, message, type)`.
- **Mapping to HTTP.** `ApiResults.ToHttpResult` maps a result to the correct status code and the envelope `{ isSuccess, data, error: { code, message, validationErrors? } }`.
- **Responses with no body.** 204 has no body.
- **Everything else** goes through the same envelope:
  - `UseStatusCodePages`: JWT 401, unknown routes, 405, 415;
  - `IExceptionHandler`: malformed JSON (`ThrowOnBadRequest = true` in every environment), escaped `DomainException`s → 400, anything else → a 500 without internals.

**Why:**
- The project owner asked for a single result class so the frontend handles one error shape. It replaces ProblemDetails.
- Exceptions are reserved for exceptional cases.

## 4. FluentValidation at the boundary, invariants in the domain

**Decision:**
- **Request validators** live in Application and run through a generic Minimal API endpoint filter, `ValidationFilter<T>`, which returns 400 with camelCase field keys.
- **Entities** (`TaskItem`, `User`) still guard their invariants and throw `DomainException`.
- **DST gaps.** The one rule that needs the user's time zone is checked in the use case and returned as a validation `Result`.

**Details:**
- **OpenAPI contract.** Request DTO properties carry `[Required]` only so the OpenAPI document marks them required. ASP.NET's own DataAnnotations validation is not enabled.
- **Required messages.** Value types are nullable (`DateTime?`, `TaskItemStatus?`), so a missing field produces "Due date is required" rather than silently defaulting to `0001-01-01` / `Todo`.
- **Length checks** are applied after trimming, on both the frontend and the backend.

## 5. Manual mapping, no AutoMapper

**Decision:** `TaskDto.From(task, timeZone, utcNow)` and `UserDto.From(user)`.

**Why:**
- The task mapping needs runtime context (the user's time zone and "now" for `isOverdue`), which is simpler as a function than as mapping configuration.
- AutoMapper 15+ requires a commercial license key.
- Entities are never returned to clients.

## 6. Due dates: UTC storage, wall-clock input, both forms in output

**Decision:**
- **Storage.** `TaskItem.DueDateUtc` is a UTC `DateTime`. The domain rejects any other `Kind`.
- **Input.** The API accepts `dueDate` as a wall-clock time without an offset, interpreted in the user's IANA zone. A value with `Z` or an offset deserializes with a non-`Unspecified` `Kind` and is rejected.
- **Output.** Responses include `dueDateUtc` and `dueDateLocal`.
- **Source of the zone.** It comes from the token's `zoneinfo` claim (OIDC standard name). It was validated at registration and is immutable in this scope.

**Why:**
- US-001 Scenarios 21 and 22 require the backend to convert, and the display to use the user's configured zone, not the browser's.
- Doing every conversion in one place (`DueDates`) keeps it testable and consistent. Angular's `DatePipe` cannot format with IANA zones anyway.

**Edge cases:**
- **DST gap** → validation error.
- **Ambiguous time** → standard time, which is `TimeZoneInfo`'s rule.
- **Unchanged due date on edit** → the stored instant is kept.
- **Windows time zone ids** are rejected (`HasIanaId`).
- **ICU** is required, so `InvariantGlobalization` must stay off.

## 7. EF Core + SQLite specifics

**Decisions:**
- **Explicit `UserId` foreign key** with `DeleteBehavior.Restrict`: deleting a user who has tasks must be a deliberate operation.
- **Email.** A unique index, with emails stored normalized (trimmed, lower-case).
- **Registration race.** A unique-constraint violation (SQLite 19/2067) is translated into `TryAddAsync == false` → 409. The application's pre-insert check is only a fast path, so concurrent registrations still get 409.
- **UTC on read.** SQLite stores `DateTime` as text and loses its `Kind`, so a convention-wide value converter restores `DateTimeKind.Utc` when reading.
- **Status as text** (`HasConversion<string>()`), so the database stays readable.
- **Lengths.** `HasMaxLength` documents the schema, but SQLite does not enforce it. The domain and validators do.
- **Database path.** A relative SQLite path is resolved against the content root, so the API, `dotnet ef` and IDEs share one file.

## 8. Seeding with `HasData` (Code First migrations)

**Decision:** The two demo users and their tasks are model data in the `SeedDemoData` migration:
- fixed GUIDs;
- fixed UTC dates around 2026-10-21;
- a password hash precomputed once with the same `PasswordHasher`.

The data is inserted with anonymous objects, which bypass the entities' private constructors.

**Why:** The owner explicitly asked for Code First with migrations to create **and seed** the database. Migrations run exactly once per database, so restarts never duplicate or overwrite data.

**Consequences and safeguards:**
- **Constants only.** Every seed value must be a literal: anything computed at runtime would change the model and break `Migrate()` in EF 9+. `MigrationTests.Migrations_match_the_model` catches this.
- **Validity.** `SeedDataTests` verifies that the hash matches `DemoPass123!` and that the seeded values respect the domain limits, since anonymous objects skip domain validation.
- **Every environment.** The seed runs wherever migrations run, not only in Development, so it must be removed before a production deployment.
- **Shared with tests.** `EnsureCreated` in tests also inserts the seed, so tests never assert absolute counts and never reuse the demo emails.

## 9. Authentication

**Decisions:**
- **Tokens.** JWT HS256, issued with `JsonWebTokenHandler`.
- **Configuration.** `JwtOptions` is validated on startup (issuer, audience, a key of at least 32 bytes, expiry).
- **Claims.** `MapInboundClaims = false`, so `sub`, `email`, `name` and `zoneinfo` keep their issued names.
- **Passwords.** Identity's `PasswordHasher` sits behind `IPasswordService`, with no Identity tables or `UserManager`.
- **Login failures.** Unknown email and wrong password produce the same error and similar timing.
- **`/me`.** Returns 401 if the token's user no longer exists.

## 10. OpenAPI for NSwag

**Decisions:**
- **Generator.** Built-in `AddOpenApi()` emitting **OpenAPI 3.0**, since NSwag 14 does not fully support 3.1.
- **Transformers:**
  - add the `Bearer` scheme;
  - add a security requirement only on endpoints that have authorization metadata and no `AllowAnonymous`;
  - clear `servers`, so the generated client doesn't hard-code a host and port;
  - set `additionalProperties: false` on DTO schemas, so the TypeScript interfaces are strict.
- **Metadata.** Every endpoint declares `WithTags` (one client per module), `WithName` (operationId → method name) and `Produces…` (handlers return `IResult`, so metadata must be explicit).
- **Contract tests.** `OpenApiDocumentTests` pin these properties.

## 11. One deployable, two logical APIs

**Decision:** `/api/tasks` (task management) and `/api/auth` (user management) are endpoint groups in a single host.

**Why:** The brief's "second API" is read as a logical module. Two processes would double the operational surface without any benefit for this scope. If the interviewer means separate deployables, splitting is mechanical: the modules share only the token-validation configuration.

## 12. Frontend state and errors

**Decisions:**
- **Board state.** `TasksStore` is provided by the board page, not the root injector. It is destroyed on logout and navigation, so one user's tasks cannot leak into another user's session in the same tab.
  - Columns are `computed` signals that re-apply the backend ordering rule.
  - Moves are optimistic, rolled back on failure, and a task is locked while its move is pending.
- **Error display.** Only the HTTP error interceptor shows errors. It reads the envelope from NSwag's Blob responses and opens a single CDK dialog. Stores only keep state consistent.
  - A 401 on any request except login clears the session.
- **Modals** use `@angular/cdk/dialog`, which brings focus trapping, Escape handling and ARIA. The CDK is already a dependency for drag-and-drop, and Bootstrap's JavaScript is not needed.
