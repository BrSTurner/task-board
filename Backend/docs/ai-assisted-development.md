# AI-assisted development log

This project was built with Claude Code (Claude Opus 5.5) acting as a pair programmer. It followed the rules in `Backend/CLAUDE.md` and `Frontend/CLAUDE.md`, the user story in `Documents/USER-STORY.md`, and the project owner's prompt.

This document records:
- the TDD evidence (RED → GREEN → REFACTOR);
- how the work was planned;
- what was verified and what was corrected along the way.

## Prompt and planning

The full session (prompts, replies, tool calls, clarifying questions and the approved plan) is exported in [docs/claude-code-session.md](../../docs/claude-code-session.md).

The project owner's prompt, summarized:

> Build a .NET Minimal API + Angular app to manage tasks with user registration and authentication. Follow the two CLAUDE.md files and the user story.
> - Seed two users with Code First / EF Core migrations.
> - Validate with FluentValidation.
> - Never return entities; use DTOs (AutoMapper optional).
> - Add an `IsOverdue` flag to the task output.
> - Use the Result pattern with a single result class sent to the frontend, and show errors in a modal.
> - Use ngx-toastr for create/update/delete.
> - Drag-and-drop between columns; clicking a task opens an edit modal.
> - Look for gaps and edge cases.

**Before writing code**, the assistant:
- read both CLAUDE.md files, the user story and the brief;
- identified the conflicts between them (CLAUDE.md said title ≤ 120, description ≤ 2000, optional date-only due date; US-001 says ≤ 50, ≤ 400, required UTC date/time with time zones);
- asked three product questions:
  - **Past due dates?** Allowed.
  - **TDD evidence?** A git commit for every RED and every GREEN step.
  - **DELETE response?** 204 with no body; the envelope is only used for responses that have a body.

A second planning pass reviewed the design for library-specific pitfalls before implementation. These became explicit tests or decisions:
- .NET 10 OpenAPI 3.1 vs. NSwag;
- the Microsoft.OpenApi v2 API changes;
- `ThrowOnBadRequest` defaulting to Development only;
- SQLite losing `DateTimeKind`;
- `HasData` determinism in EF 9+;
- zoneless Angular 21 with Vitest;
- NSwag's Blob error bodies.

Both CLAUDE.md files were then updated (§0) so later sessions don't revert the US-001 rules.

## TDD log (backend)

Every RED step is its own commit (`test(red): …`). Its test compiles against a minimal stub and fails on an assertion. The matching `feat(green): …` commit makes it pass, and `refactor: …` commits change structure only.

| # | Behaviour | RED: why it failed | GREEN: minimal change |
|---|-----------|---------------------|------------------------|
| 1 | `GET /api/public/ping` is anonymous and returns the `ApiResult` envelope | `Expected: OK, Actual: NotFound` (the endpoint did not exist) | `ApiResult<T>` envelope + `PublicEndpoints.MapPublicEndpoints()` |
| 2 | The OpenAPI document is 3.0, has operationIds and a Bearer scheme, no `servers`, and no security on anonymous operations | `/openapi/v1.json` returned 404 | `AddTaskBoardOpenApi()` with `OpenApiVersion = OpenApi3_0`, plus document and operation transformers (Microsoft.OpenApi v2 API) |
| 3 | Unknown routes (404) and unhandled exceptions (500) return the envelope; no internals leak | The 404 had no body (`ContentType` null); the 500 rendered the developer exception page, including the exception message | `GlobalExceptionHandler` (`IExceptionHandler`) + a `UseStatusCodePages` writer + `ThrowOnBadRequest = true` |
| 4 | `User.Create` trims and normalizes, generates ids, and guards required fields and max lengths | The stub returned an empty `User`: empty ids, no `DomainException` thrown (11 failures) | `Guard` helper + `User.Create` / `NormalizeEmail` (trim + `ToLowerInvariant`), `Guid.CreateVersion7()` |
| 5 | Register: validator messages, password policy, IANA-only time zones; a duplicate email (case/whitespace variants) or a unique-index race → Conflict | The empty validator accepted everything; the `TryFindIana` stub returned false; the stub use case returned a fixed failure (20 failures) | `RegisterUserRequestValidator` (+ `RequiredText` rule), `TimeZones.TryFindIana` (`HasIanaId`), `RegisterUser` with a pre-check + `TryAddAsync` |
| 6 | `UserRepository` persists and reads users; a unique email index violation makes `TryAddAsync` return `false` | The stub repository returned nulls / `true` | EF `UserConfiguration` (unique index on `Email`) + `SqliteErrors.IsUniqueConstraintViolation` (SQLite 19/2067) |
| 7 | `POST /api/auth/register` → 201 safe DTO / 400 camelCase `validationErrors` / 409 duplicate / 400 malformed JSON / 415 wrong content type; `PasswordService` salts and verifies | The endpoint was missing (404); the stub password service returned plaintext and always verified | `AuthEndpoints`, `ValidationFilter<T>`, `ApiResults` (Result → HTTP), `PasswordService` over `PasswordHasher`, `AddInfrastructure` (content-root-relative SQLite path), `InitialUsers` migration, dev `MigrateAsync` |
| 8 | Login (token + expiry + user; identical failure for unknown email and wrong password), `/me` (401 without or with an invalid token, 401 when the user row no longer exists), JWT claims and expiry, JWT options validation | The stub use cases returned a fixed failure; the stub token service returned an empty token; the options validator accepted everything (15 failures) | `LoginUser`, `GetCurrentUser`, `JwtTokenService` (`sub`, `email`, `name`, `zoneinfo`), `JwtOptionsValidator` + `ValidateOnStart`, JwtBearer with `MapInboundClaims = false`, `HttpContextCurrentUser` |
| 9 | `TaskItem`: defaults to To Do, trims title and description (blank → null), 50/400 limits after trimming, UTC-only due dates, any status move (including backwards), overdue = due < now and not Done | The stub entity had empty values, never threw and was never overdue (16 failures) | `TaskItem.Create/UpdateDetails/ChangeStatus/IsOverdue` with a validate-then-assign `SetDetails` |
| 10 | Task use cases: create (owner from the token, To Do default, local → UTC; Sc. 8/9/21), list (only mine, overdue-first ordering, local display; Sc. 17–19/22/23), get/update/move/delete with 404 for tasks you don't own (Sc. 13/14/16/20/24), DST gap rejected, ambiguous time stable on edit, stale token → 401; validators with the exact Scenario 10/11 messages | Stub use cases, validators and conversion (28 failures) | `DueDates`, `TaskOrdering`, `TaskDto.From`, validators with `LocalDueDate`/`ValidStatus` rules, one class per use case |
| 11 | Task persistence: UTC round-trip (`Kind` preserved), owner-scoped queries, update/delete, status stored as text, FK rejects an unknown user, restrict-delete of users who have tasks | The stub repository persisted nothing (6 failures) | `TaskItemConfiguration` (FK `Restrict`, string status), a global UTC `ValueConverter` via `ConfigureConventions`, `TaskRepository`, `AddTaskItems` migration |
| 12 | Task endpoints: 201 + `Location`; 400 with exact messages and nothing persisted; offsets rejected; overdue-first list; PATCH → done clears overdue and persists; invalid/integer/null status → 400; PUT; DELETE → 204 with an empty body; cross-user 404 for all verbs; non-GUID id → 404; every route 401 without a token; OpenAPI security, string enum and required DTO fields | No `/api/tasks` routes (24 failures) | `TaskEndpoints` group with `RequireAuthorization`, a generic `ProducesApiError` for groups |
| 13 | `HasData` seed: two demo users (normalized emails, IANA zones, hash verifies `DemoPass123!`), tasks covering Sc. 18/19/21–23, migrations match the model, a restart keeps data and doesn't duplicate or overwrite the seed | The stub `SeedData` was empty (4 failures) | `SeedData` constants (fixed GUIDs and UTC dates, a precomputed hash) via `HasData`, `SeedDemoData` migration; repository test emails changed to avoid the seeded ones |
| 14 | An escaped `DomainException` returns a 400 envelope (code `DomainRule`) instead of 500 | It returned 500 | `GlobalExceptionHandler` maps `DomainException` → 400 |
| 15 | DTO schemas declare `additionalProperties: false`, so NSwag emits strict interfaces (no `[key: string]: any`) | The property was absent | A schema transformer setting `AdditionalPropertiesAllowed = false` on object schemas |

One `refactor:` commit made the register request fields `[Required]` in the OpenAPI contract while keeping the field-level messages, and added tests for both behaviours.

**Frontend.** Tests were written next to each unit rather than as separate RED commits: 53 Vitest specs covering:
- validators and date formatting;
- `AuthStore`, `TasksStore` (optimistic move, rollback, 404 handling);
- the auth and error interceptors (with Blob bodies, as NSwag receives them);
- guards (including open-redirect protection);
- the register, login and task-form pages (Scenarios 2, 5, 10, 11);
- the board (Scenarios 6, 7, 17–19) and drop handling.

## Verification that was actually run

- `dotnet build` (warnings are errors) and `dotnet test`: all backend tests green.
- `npm run build` (no warnings) and `npm test`: all frontend specs green.
- NSwag generation against the running API; the generated TypeScript was inspected:
  - required/nullable properties;
  - `TaskItemStatus` as a string union;
  - 204 handling;
  - typed error results for declared status codes.
- **An end-to-end run in Microsoft Edge** (Playwright driving the real API and dev proxy, outside the repository). 29 checks passed:
  - redirect to login;
  - invalid-login modal;
  - board columns and overdue ordering;
  - São Paulo display time;
  - drag In Progress → Done (pill removed, toast, no stray edit modal);
  - persistence after refresh;
  - create/edit validation;
  - card click → edit modal;
  - keyboard status select;
  - delete confirmation;
  - logout;
  - register validation (5 fields), duplicate-email modal, prefilled login;
  - empty board for a new user;
  - tampered token → "Session expired" and redirect.

  The browser console had no warnings or errors beyond the network log lines for the deliberate 401/409 responses.

## What review caught and corrected

| Finding | Correction |
|---|---|
| CLAUDE.md defaults conflicted with US-001 (lengths, due-date type, user fields) | Both CLAUDE.md files got a binding §0; US-001 values are used everywhere |
| Leaving `dueDate` out of a request would bind `default(DateTime)` (`0001-01-01`) and `Todo` silently | Request value types are nullable; "Due date is required" / "Status is required" come from FluentValidation |
| A `dueDate` with `Z` or an offset is silently shifted by System.Text.Json | The validator requires `DateTimeKind.Unspecified`; API test with `Z` and `+02:00` |
| SQLite drops `DateTimeKind`, so UTC values came back `Unspecified` | Convention-wide UTC value converter; test asserts `Kind == Utc` |
| `ThrowOnBadRequest` is Development-only, so malformed JSON would return an empty 400 elsewhere | Set to `true` in every environment; `UseStatusCodePages` covers the empty 401/404/405/415 responses |
| Existing API tests (via plain `WebApplicationFactory`) started writing `taskboard.db` into the source folder once migrations ran on startup | All API tests use `TaskBoardApiFactory`, which uses an isolated temporary SQLite file |
| `HasData` puts the demo users into every test database | Repository tests changed to non-demo emails; tests never assert absolute counts |
| `JwtTokenService` was internal but needed by the API for validation parameters | Extracted the public `JwtConventions` (claim name, signing key) and kept the service internal |
| The generated TS interfaces had `[key: string]: any` | `additionalProperties: false` on DTO schemas (TDD #15) |
| **Browser check:** dialogs rendered transparent, with no padding | Bootstrap 5.3 defines `--bs-modal-*` on `.modal`, which the CDK overlay replaces; the variables are now declared on the dialog panel |
| npm 10.9.7 crashed with `edgesOut` on a fresh `npm install` | Installed with npm 11; `npm ci` with npm 10 works from the lockfile (documented in the README) |
| Error modals could stack on parallel failures (e.g. several 401s) | `ErrorDialogService` keeps a single modal open |
| A post-login `returnUrl` could be an absolute or protocol-relative URL (open redirect) | `safeReturnUrl` accepts only app-relative paths, with a spec |

## Representative generated code

The time-zone conversion at the heart of BR-008 / Scenario 21. It was reviewed for DST gaps, ambiguous times and `DateTimeKind` handling:

```csharp
public static Result<DateTime> ToUtc(DateTime local, TimeZoneInfo timeZone)
{
    var wallClock = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
    if (timeZone.IsInvalidTime(wallClock))
    {
        return Error.Validation("dueDate", "This time does not exist in your time zone (daylight saving time change). Choose a different time.");
    }

    // Ambiguous times (clocks falling back) resolve to standard time, as documented by TimeZoneInfo.
    return TimeZoneInfo.ConvertTimeToUtc(wallClock, timeZone);
}
```

Its tests (`DueDatesTests`):
- São Paulo 15:00 → 18:00Z;
- New York 2026-03-08 02:30 → validation error;
- New York 2026-11-01 01:30 → 06:30Z.

`UpdateTask` keeps the stored instant when the submitted local time is unchanged, so editing only the title of a task with an ambiguous due time doesn't move it by an hour.
