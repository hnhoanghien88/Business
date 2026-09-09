# Implementation Plan: Restaurant Layout

**Branch**: `[003-restaurant-layout]` | **Date**: 2026-09-09 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-restaurant-layout/spec.md`

## Summary

Deliver a permission-protected `/restaurant/layouts` workspace with Areas and Tables tabs. Extend
the existing `RestaurantArea`, `RestaurantTable`, and `TableSession` model into a CQRS vertical
slice: Dapper-backed stable list reads, EF-backed validated writes, optimistic concurrency and
session-aware safety rules, a secured REST contract, and an accessible responsive React UI. Identical
concurrent list reads are coalesced in the browser without caching completed results.

## Technical Context

**Language/Version**: C# / .NET 10; JavaScript ES modules with React 19  
**Primary Dependencies**: ASP.NET Core, MediatR 14, FluentValidation 12, EF Core 10,
MySql.EntityFrameworkCore, Dapper, MySqlConnector, React 19, Vite 8, Material UI 9, oxlint  
**Storage**: MySQL `restaurant_db`; existing `restaurant_areas`, `restaurant_tables`,
`restaurant_table_sessions`; one `BusinessDbContext`  
**Testing**: backend build; frontend lint/build; contract/manual integration scenarios (no test project exists)  
**Target Platform**: ASP.NET Core API and modern desktop/mobile browsers  
**Project Type**: Full-stack web application  
**Performance Goals**: interaction within 2 seconds at 10,000 tables; one request per overlapping identical list query  
**Constraints**: immutable case-insensitive codes; active-area requirement; capacity 1..1000;
configuration cannot overwrite operational state; open-session/status safety; live `Layouts.*`
authorization; cancellation; no hard delete  
**Scale/Scope**: one two-tab screen, two list resources, secured REST endpoints, max 100 rows/page

## Constitution Check

*GATE: Passed before research and re-checked after design.*

- **Layering — PASS**: Domain remains independent; Application owns requests/rules/abstractions;
  Infrastructure owns EF/Dapper; API only translates HTTP and dispatches.
- **CQRS/persistence — PASS**: Dapper serves reads and EF performs writes. Schema changes use a new migration.
- **Security/contracts — PASS**: every endpoint requires bearer access and matching `Layouts.*`;
  Identity remains the live source; Problem Details is preserved.
- **Validation/verification — PASS**: code, area, capacity, session, confirmation and concurrency
  rules precede commit. Builds and contract scenarios are required; missing test infrastructure is recorded.
- **Observability/safety — PASS**: correlation IDs, structured logs and cancellation remain intact.
- **Formatting — PASS**: modified C# and JSX use readable multi-line formatting.
- **List coalescing — PASS**: pending requests use every normalized condition and are removed in `finally`.

No constitutional exceptions are required. Phase 1 introduces no gate violation.

## Project Structure

### Documentation (this feature)

```text
specs/003-restaurant-layout/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/layouts.openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
Business-api/src/
├── Business.Domain/Entities/Restaurant/LayoutEntities.cs
├── Business.Application/{Abstractions/Persistence/Restaurant,Common/Authorization,Restaurant/Layouts}/
├── Business.Infrastructure/Persistence/{MySqlLayoutsRepository,DapperLayoutsReadRepository}.cs
├── Business.Infrastructure/Persistence/Configurations/Restaurant/LayoutConfigurations.cs
└── Business.Api/Controllers/Restaurant/LayoutsController.cs

Business-client/src/
├── app/{App,AppShell}.jsx
└── features/restaurant/layouts/{api,components,LayoutsPage.jsx}
```

**Structure Decision**: Add one Layouts vertical slice beside Categories and Foods, reusing the
existing layout entities/schema rather than creating parallel objects.

## Phase 0: Research Decisions

All decisions are recorded in [research.md](research.md); no clarification remains.

## Phase 1: Design and Contracts

- [data-model.md](data-model.md) defines fields, relationships, invariants and transitions.
- [contracts/layouts.openapi.yaml](contracts/layouts.openapi.yaml) defines secured list/write operations.
- [quickstart.md](quickstart.md) defines rule, concurrency, coalescing, accessibility and build validation.

## Implementation Approach

1. Add concurrency mapping/indexes without rewriting migrations.
2. Add DTOs, permissions, abstractions, repositories, handlers and validators.
3. Expose secured area/table endpoints with explicit validation and conflict semantics.
4. Build two-tab responsive UI, dialogs, conflict recovery and in-flight list coalescing; wire route/menu.
5. Generate/inspect migration if needed and execute all quality gates.

## Complexity Tracking

No constitution violations require justification.
