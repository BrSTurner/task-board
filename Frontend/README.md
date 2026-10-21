# TaskBoard — Frontend

This is the Angular 21 LTS single-page app for TaskBoard. Setup, demo credentials and the overall architecture are in the [root README](../README.md).

## Commands

| Command | What it does |
|---|---|
| `npm ci` | Installs the exact dependency versions from the lockfile. With npm 10.9.x, use this instead of `npm install` (see the root README). |
| `npm start` | Runs the dev server on http://localhost:4200 and proxies `/api` to http://localhost:5080 (`proxy.conf.json`). |
| `npm test` | Runs the Vitest unit tests once (jsdom, zoneless). |
| `npm run build` | Production build into `dist/`. |
| `npm run generate:api` | Regenerates the NSwag clients from the running API's `/openapi/v1.json` into `src/app/core/api/generated/`. |

## Structure

```
src/app/
  core/api/generated/   NSwag output (AuthClient, TasksClient, PublicClient, DTOs). Never edit by hand.
  core/auth/            AuthStore (signals + sessionStorage), auth interceptor, guards
  core/errors/          Error interceptor, ApiResult parsing, single error modal
  features/auth/        /login, /register
  features/tasks/       /tasks board, TasksStore, task card/column/form dialog, ordering rule
  shared/               Validators that mirror the backend, date helpers, confirm/error dialogs
```

## Design notes

- **NSwag clients.**
  - Configured with `dateTimeType: String` and `enumStyle: StringLiteral`, so dates stay as strings and the task status is the union `'todo' | 'inProgress' | 'done'`.
  - Clients are only used inside stores. Components never call `HttpClient` directly.
- **Due dates.**
  - Sent as `YYYY-MM-DDTHH:mm`, the value of a `datetime-local` input, interpreted in the **user's configured time zone**.
  - Displayed from the API's `dueDateLocal`, with no `Date` objects. The browser's own time zone therefore never shifts a value.
- **Errors.**
  - The error interceptor turns every API error envelope into one modal.
  - Inline red fields and messages are for client-side validation only.
  - Toasts confirm creates, updates (including moves) and deletes.
- **Drag-and-drop.**
  - Built with Angular CDK. Only moves *between* columns are saved; sorting within a column is disabled because no order is stored.
  - Moves are optimistic, with rollback on failure.
  - Each card also has a status `<select>` so keyboard and assistive-technology users can move tasks.
- **Board state.** The task store is provided per board page, so its state is discarded on logout and never shared between users in the same tab.
