# Implementation Plan: Restaurant Table Operations

**Branch**: `[004-restaurant-table-operations]` | **Date**: 2026-09-09 | **Spec**: [spec.md](spec.md)

## Summary

Add a secured `/restaurant/table-operations` floor view backed by Dapper snapshots and EF commands.
Staff can filter table state, atomically open one session, inspect order/payment totals, safely close
eligible sessions into Cleaning, and mark cleaned tables Available. A two-second snapshot refresh
provides sub-three-second synchronization and reconnect recovery; identical in-flight loads coalesce.

## Technical Context

**Language/Version**: C#/.NET 10; JavaScript ES modules/React 19  
**Primary Dependencies**: ASP.NET Core, MediatR, FluentValidation, EF Core, Dapper, MySQL, React, Vite, Material UI, oxlint  
**Storage**: Existing area, table, session, order and payment tables/views  
**Testing**: backend build, frontend lint/build, manual contracts; no test project exists  
**Target Platform**: ASP.NET Core API and modern browsers  
**Project Type**: Full-stack web application  
**Performance Goals**: visible state changes within 3 seconds; usable with 300 tables  
**Constraints**: one open session/table; atomic transitions; capacity and close overrides require
permission/reason; live `TableOperations.*` authorization; cancellation; no stale cache  
**Scale/Scope**: one floor screen, table snapshot, session detail and four commands

## Constitution Check

*GATE: Passed before research and re-checked after design.*
- Layering/CQRS — PASS: Application requests/abstractions, Dapper reads, EF writes, thin API.
- Security/contracts — PASS: bearer auth and distinct read/open/close/clean/override policies.
- Validation — PASS: commands re-read state; transitions and obligations precede commit.
- Observability/safety — PASS: cancellation, Problem Details, correlation and logging remain.
- Formatting — PASS: nested code uses readable multi-line formatting.
- List coalescing — PASS: normalized snapshot conditions key a pending Promise removed in `finally`.

No exception is required. No migration is expected because the existing schema supports the flow.

## Project Structure

```text
specs/004-restaurant-table-operations/{plan,research,data-model,quickstart,tasks}.md
specs/004-restaurant-table-operations/contracts/table-operations.openapi.yaml
Business-api/src/Business.Application/Restaurant/TableOperations/
Business-api/src/Business.Infrastructure/Persistence/{DapperTableOperationsReadRepository,MySqlTableOperationsRepository}.cs
Business-api/src/Business.Api/Controllers/Restaurant/TableOperationsController.cs
Business-client/src/features/restaurant/tableOperations/
```

**Structure Decision**: A separate operational slice reuses 003 entities; configuration and live
transitions remain separate permission/API boundaries.

## Phase 0 and Phase 1

Decisions are in [research.md](research.md), entities/transitions in [data-model.md](data-model.md),
HTTP in [contracts/table-operations.openapi.yaml](contracts/table-operations.openapi.yaml), and
validation in [quickstart.md](quickstart.md). No clarification remains; post-design gate passes.

## Implementation Approach

1. Define permissions, DTOs, persistence boundaries and rules.
2. Implement Dapper snapshot/detail and EF atomic open/close/clean.
3. Expose secured endpoints with validators and standard responses.
4. Build grouped cards, dialogs, detail actions, polling/reconnect and coalescing.
5. Wire route/menu and run quality gates.

## Complexity Tracking

No constitutional violations.
