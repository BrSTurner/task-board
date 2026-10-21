# CLAUDE.md — TaskBoard Frontend (Angular LTS)

> Instructions for Claude Code working inside `frontend/`. Read this file before changing code. Source brief: **".NET - Technical Interview Exercise", version 6 (3 pages)**. Backend contract is defined in `../backend/CLAUDE.md` and its **actual generated OpenAPI schema**.

## 0. Binding project decisions (override anything below that conflicts)

Agreed with the project owner; they reflect `../Documents/USER-STORY.md` (US-001) and `../Backend/CLAUDE.md` §0:

- **Register form:** name, email, timezone (IANA select, defaults to the browser's zone), password, confirm password.
- **Task fields:** title required (≤ 50, trimmed), description optional (≤ 400), **due date/time required** (`datetime-local`, interpreted in the **user's configured timezone**, not the browser's), status. Past dates are allowed.
- **Dates:** send `dueDate` as the local wall-clock string (`YYYY-MM-DDTHH:mm`); display `dueDateLocal` from the API. The frontend does **no timezone math**.
- **Overdue:** use the API's `isOverdue` flag; show a red "Overdue" pill; overdue cards first in each column (earliest due first).
- **API envelope:** every response body is `ApiResult` `{ isSuccess, data, error }`; DELETE returns 204 with no body. **All API errors are shown in a modal**; client-side field validation stays inline (red field + message).
- **Toasts:** `ngx-toastr` success toasts on task create / update (including moves) / delete.
- **Board interaction:** drag-and-drop between columns (CDK); **clicking a card opens the edit modal**; modals use `@angular/cdk/dialog` with Bootstrap modal styling.

## 1. Mission and stack

Build an elegant, responsive **Angular SPA** for a small personal task-management app. The exercise mainly evaluates .NET; the frontend must demonstrate professional, well-structured full-stack work without becoming a second oversized project.

**Required choices:**

- **Angular 21 LTS** at the time these instructions were written (October 2026). Use the latest compatible stable **21.x** patch and its officially supported Node/TypeScript versions; do not automatically upgrade to the newer *active* Angular 22, because the request is specifically **LTS**. Recheck support status if the project is started much later.
- **Standalone components**, modern Angular control flow (`@if`, `@for`), strict TypeScript, Angular Router, and typed reactive forms.
- **Bootstrap 5** for layout, spacing, forms, buttons, and responsive visual style, with a restrained amount of custom CSS/SCSS.
- **Angular Signals** (`signal`, `computed`, `effect` only when appropriate) for local state and derived views; RxJS/Observables for HTTP and asynchronous operations.
- **NSwag-generated Angular `HttpClient` API clients** based on the backend **OpenAPI document**. No hand-maintained copies of backend DTOs or endpoint URLs.
- **Angular CDK Drag & Drop**, not an extra Kanban UI package and not the HTML5 native drag-and-drop API.
- A clean, accessible task-management experience, optimized for **desktop, tablet, and mobile**.

**Informal user story:** As a registered user, I want to sign in, create and manage my own tasks, and drag them among To Do, In Progress, and Done to track my progress easily.

## 2. Scope and source requirements

The exercise asks for a responsive frontend using a framework of our choice, integrated with backend CRUD, and clean component/state organization. It does **not** prescribe Angular, Bootstrap, Signals, NSwag, or Kanban. Those are **our design choices** and must remain subordinate to task completeness and simplicity.

Create exactly these primary **routed pages**:

1. `/register` — **Create account**; inputs for name, email, timezone, password, and password confirmation; clear validation/errors; link to login.
2. `/login` — **Login**; email and password; show errors; link to create account.
3. `/tasks` — **Protected Kanban board**, displaying the current user's tasks, with create/edit/delete and drag-between-columns support.

Redirect from `/` to `/tasks` when logged in, otherwise `/login`. A not-found route can redirect or show a simple 404. CRUD dialogs, modals, or a side panel can live within `/tasks`; no need for separate routed detail/create pages.

**Do not add** teams, sharing, attachments, roles, notifications, realtime sync, dark-mode architecture, Redux/NgRx, or other speculative features.

## 3. Functional contract with backend

Backend supplies two **logical** endpoint modules in one API host. Verify the **real OpenAPI schema** before generating code; the table below is the intended contract, not permission to hardcode request methods.

| Interaction | Endpoint | Expected result |
| --- | --- | --- |
| Register | `POST /api/auth/register` | User created, `201`; duplicate email `409`. |
| Login | `POST /api/auth/login` | `200` with bearer token, expiry, user; invalid credentials `401`. |
| Current session | `GET /api/auth/me` | `200` with user, or `401`. |
| Public example | `GET /api/public/ping` | `200` anonymously; not required in the main UI. |
| List my tasks | `GET /api/tasks` | `200` with tasks belonging to current user. |
| Task detail | `GET /api/tasks/{id}` | `200` / `404`. |
| Create task | `POST /api/tasks` | `201`; default status `todo`. |
| Full task edit | `PUT /api/tasks/{id}` | `200`, validation `400`, not owned `404`. |
| Move Kanban card | `PATCH /api/tasks/{id}/status` | `200`, `404` if unavailable. |
| Delete task | `DELETE /api/tasks/{id}` | `204`, `404` if unavailable. |

**Task fields:** `id: Guid`, `title` (required, max 50), `description?` (max 400), `status` (`todo` | `inProgress` | `done`), `dueDateUtc`, `dueDateLocal`, `isOverdue`; requests send `dueDate` (required local wall-clock). Ownership is always determined server-side. Backend `UserId` must not be supplied from the browser.

**Dates:** Due dates are **instants stored in UTC** and displayed in the **user's configured timezone**. NSwag is configured with `dateTimeType: String`, so dates stay strings; display `dueDateLocal` without creating browser-local `Date` objects (see `shared/utils`).

**Kanban ordering caveat:** The brief has **no sort-order field**. Moving a task to another status column must persist; arbitrary reordering **within the same column is not a saved feature**. Use deterministic rendering (overdue first by due date, then due date, title, id). Don't imply users can permanently rearrange same-column cards without backend support.

## 4. Proposed structure (change only with a clear reason)

```text
frontend/
  src/
    app/
      app.config.ts
      app.routes.ts
      core/
        api/
          generated/              # NSwag output: do not hand-edit
          api.config.ts           # base URL configuration
        auth/
          auth.store.ts           # login state / token / current user signals
          auth.interceptor.ts     # bearer auth via HttpInterceptorFn
          auth.guard.ts           # protected routing
        errors/
      features/
        auth/
          pages/login/
          pages/register/
        tasks/
          pages/task-board/
          components/task-card/
          components/task-column/
          components/task-form/
          tasks.store.ts          # Signal state + computed column views
      shared/
        components/
        utils/date-only.ts
      app.component.ts
    styles.scss
  nswag.json                   # or one clearly documented equivalent config
  package.json                 # scripts: start, build, test, generate:api
  README.md
```

Favor **feature-first organization**. Keep components small, but do not split a five-line template into six files. Don't introduce a 'facade', a domain abstraction, a DTO mapping layer, and a repository on the frontend merely to mimic backend Clean Architecture.

## 5. NSwag: API clients are generated, not written by hand

1. Backend must first expose an accurate **OpenAPI** document (usually `/openapi/v1.json` in development).
2. Add NSwag configuration or an equivalent repeatable script using the **Angular TypeScript template** and modern Angular `HttpClient`; inspect available generator options in the installed NSwag version.
3. Generate strongly typed models and clients into `src/app/core/api/generated/`. Treat this folder as **read-only generated code**.
4. Add `npm run generate:api`, document backend URL/prerequisites, and commit the generation configuration. Generated output can be committed for an easy evaluator build if appropriate.
5. Use the generated clients inside an `AuthStore`/`TasksStore` or focused feature service, never directly in ten unrelated UI components.
6. Configure one base URL / dev proxy strategy, and use a functional HTTP interceptor to attach the token. No repeated `HttpClient` calls, repeated URLs, duplicated DTO interfaces, or anonymous `any` payloads.
7. If NSwag has a compatibility issue with .NET 10 OpenAPI **3.1**, coordinate with backend to emit OpenAPI **3.0** where needed; validate a real generated client and actual request, not just a successful code-generation command.
8. Don't manually edit generator output to 'fix' imports; fix generator settings or add an adapter outside the generated folder.

**Important:** Both the Angular client and the backend must agree on endpoint paths, bearer auth, task status serialization, `dueDate`, and 200/201/204 response handling. Update or regenerate clients after *any* contract change.

## 6. Signals, async boundaries, and component state

- `AuthStore`: private writable signals for token/currentUser/loading/error; expose readonly signals/computed `isAuthenticated`. Implement `login`, `register`, `logout`, and optional `loadCurrentUser` with the generated auth client.
- `TasksStore`: private writable signals for `tasks`, `loading`, `saving`, and `error`; computed `todoTasks`, `inProgressTasks`, `doneTasks` and task counts. Stable IDs in `@for (task of tasks(); track task.id)`.
- Use `signal.update`/`set` for immutable array updates. Do not mutate the array inside a signal and expect dependency tracking to work.
- Prefer `computed` for derived lists and counts; do not duplicate derivable state or abuse `effect` to synchronize signals. Use `effect` for a justified external side effect only.
- Use `async/await` or RxJS consistently at HTTP boundaries; handle failures; use `takeUntilDestroyed` for subscriptions where needed; avoid nested subscribes and leaks.
- Do not assume Signals eliminate all HTTP work or magically improve runtime performance. Use them where they simplify and localize reactivity.

For this **demo-only** JWT frontend, a small auth service + **sessionStorage** for the access token is acceptable, with a README note that JS-accessible storage is vulnerable to XSS and not the recommended general-purpose production auth architecture. Never log tokens or expose them in error messages. Don't implement refresh tokens unless the backend actually supports them. On `401`, clear the session and redirect to login; avoid redirect loops on the login request itself.

## 7. UI and interaction design

### Authentication screens

- Present a centered, polished Bootstrap card with clear title, labels, help text, and navigation links.
- Register: typed reactive form, email validation, password policy aligned with backend, password confirmation match, disabled submit while pending.
- Login: email/password, loading and incorrect-credentials feedback; safe, clear route to register.
- Accessible labels, visible focus states, password visibility toggle only if easy and correctly accessible.
- On successful registration: navigate to login with a friendly success message; **do not assume registration also logs the user in**.

### Kanban board

- Header/nav: product name, logged-in user email, logout, "New task" action.
- Three accessible columns: **To Do**, **In Progress**, **Done** with counts.
- Cards display title, optional description (truncated), due date, overdue indication, edit and delete actions.
- Use **Angular CDK** connected drop lists (`cdkDropList`, `cdkDrag`); a cross-column drop must call the generated `PATCH /status` client to persist state. Prefer optimistic local update with rollback and a visible error on failure (or a clear pending state if simpler).
- Same-column custom drag order is **not** a persisted capability. Disable or ignore same-column reordering unless a tested backend `sortOrder` feature is deliberately added.
- Accessible fallback for drag-and-drop: status select/menu on each card so keyboard and assistive-tech users can move tasks.
- Create/edit: one reusable typed form for title, description, due date, status on edit. Create defaults to To Do. Reuse the same validation rules as the backend; server remains authoritative.
- Delete: explicit confirmation before a destructive action; update UI only on confirmed deletion or restore on failure.
- Cover empty board, empty column, loading, failed request, invalid session, and form-validation states.

### Responsiveness and visual quality

- Use Bootstrap's grid and utility classes with custom design tokens only where needed; don't mix multiple CSS frameworks.
- Desktop: three side-by-side columns. Tablet: columns can wrap or scroll horizontally with clear affordances. Mobile: stacked columns or a deliberate horizontally scrollable board; controls must remain tappable and cards legible.
- Keep contrast, focus indicators, labels, live status feedback, and responsive spacing usable. Prefer subtle neutral styling over heavy animations and gradients.
- Never treat a beautiful screenshot as a substitute for real CRUD and responsive behavior.

## 8. Tests and quality gates

The backend is explicitly TDD-driven, and the frontend should also develop **test-first for meaningful behavior** without disproportionate ceremony.

Prioritize focused Angular tests for:

- Form validation and password confirmation; rejected login/registration handling.
- Protected route behavior and logout; unauthorized API responses.
- Signal store loading, derived status columns, create/edit/delete success and failure.
- Cross-column move sending correct status; optimistic rollback on failed PATCH.
- Empty/loading/error UI and the accessible status-change alternative.

Use Angular's supported test runner in the selected v21 workspace and `HttpTestingController` (or supported equivalent) to fake HTTP without depending on a running backend. Prefer tests exercising generated-client-facing behavior instead of mocking implementation details. Add E2E tests only if feasible after core requirements are complete.

After changing code, run **`npm run build`**, **`npm test`** (or documented CI equivalent), and regenerate the client when the contract changed. Inspect browser console for errors/warnings; the brief considers a warning-free console desirable.

## 9. Required workflow for Claude Code

1. Inspect the existing repo before scaffolding. Read `../backend/CLAUDE.md`, backend OpenAPI, and README. State any mismatches before making assumptions.
2. Create Angular 21 LTS app, Bootstrap theme/base layout, routing, and strict types. Verify a build.
3. Configure NSwag generation from a real backend OpenAPI document; inspect the generated signatures, especially enum and date handling. Verify a call.
4. Implement auth screen and `AuthStore` with tests; wire route guards and interceptor.
5. Implement `TasksStore` and CRUD with tests; connect to API.
6. Implement Kanban view, CDK moves, error rollback, mobile view, and keyboard fallback.
7. Run/build/test the UI and check actual frontend/backend flows using demo credentials.
8. Document setup, generator command, implementation decisions, trade-offs, and any known limitations in frontend README or root README.

**Work in small vertical slices and use KISS/YAGNI.** Don't add state libraries, charting, animation packages, or a complex design system. Prefer changes that are easy to review and demonstrate in under a few minutes.

## 10. Interview/demo checklist

- [ ] New user can register, then login, then log out.
- [ ] Authenticated user can create/read/update/delete **their own** tasks.
- [ ] Three Kanban status columns, responsive on desktop/tablet/mobile.
- [ ] Drag between columns persists through API; status change works without drag-and-drop.
- [ ] All API calls use **NSwag-generated clients**, not handwritten endpoint wrappers.
- [ ] Signals and computed state are used thoughtfully, without direct DOM manipulation or duplicated state.
- [ ] Clear errors, loading states, form validation, and no significant console warnings.
- [ ] README includes startup steps and dev seeded credentials (`demo@example.test` / `DemoPass123!`), plus notes about the backend’s persistent file-based SQLite database, idempotent demo seeding, and demo token storage.
- [ ] Can explain why Angular + Minimal API differs from the brief's ASP.NET MVC reference, and why the two APIs are logical groups in one host.
- [ ] Can describe what AI-generated code was verified and corrected, not merely say "Claude wrote it".

## 11. Useful official references

- Angular release / LTS policy: https://angular.dev/reference/releases
- Angular version compatibility: https://angular.dev/reference/versions
- Angular CDK drag-and-drop: https://angular.dev/guide/drag-drop
- NSwag Angular generator: https://github.com/RicoSuter/NSwag/wiki/TypeScriptClientGenerator-Angular
