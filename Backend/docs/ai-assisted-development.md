# AI-assisted development log

This project was built with Claude Code (Claude Opus 5.5) acting as a pair programmer under the rules in
`Backend/CLAUDE.md` and `Frontend/CLAUDE.md`. This document records the TDD evidence (RED → GREEN → REFACTOR)
and what was reviewed or corrected along the way. Every RED step is a separate git commit (`test(red): …`)
whose test compiles and fails on an assertion; the matching `feat(green): …` commit makes it pass.

## TDD log

| # | Behaviour | RED — why it failed | GREEN — minimal change |
|---|-----------|---------------------|------------------------|
| 1 | `GET /api/public/ping` is anonymous and returns the `ApiResult` envelope | `Expected: OK, Actual: NotFound` (endpoint did not exist) | `ApiResult<T>` envelope + `PublicEndpoints.MapPublicEndpoints()` |
| 2 | OpenAPI document is 3.0, has operationIds, a Bearer scheme, no `servers`, no security on anonymous ops | `/openapi/v1.json` returned 404 | `AddTaskBoardOpenApi()` with `OpenApiVersion = OpenApi3_0`, document + operation transformers (Microsoft.OpenApi v2 API) |
| 3 | Unknown routes (404) and unhandled exceptions (500) return the envelope; no internals leak | 404 had no body (`ContentType` null); 500 rendered the developer exception page containing the exception message | `GlobalExceptionHandler` (`IExceptionHandler`) + `UseStatusCodePages` writer + `ThrowOnBadRequest = true` |
