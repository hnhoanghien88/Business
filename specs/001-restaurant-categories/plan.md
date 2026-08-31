# Implementation Plan: Restaurant Categories

**Branch**: `[001-restaurant-categories]` | **Date**: 2026-08-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-restaurant-categories/spec.md`

## Summary

Deliver permission-protected management of the restaurant category hierarchy at
`/restaurant/categories`. The existing `Category` entity and `restaurant_categories` table remain
the persistence foundation. Add explicit CQRS requests and FluentValidation, EF Core write and
Dapper recursive-tree read repositories, REST contracts, and a responsive React tree list with
search, status filtering, ancestor context, paging, create/edit/move, and safe activation changes.
Optimistic concurrency uses the existing microsecond `UpdatedDate` as a client-supplied version
token; a new migration adds only justified concurrency/index changes.

## Technical Context

**Language/Version**: C# / .NET 10; JavaScript with React 19

**Primary Dependencies**: ASP.NET Core, MediatR 14, FluentValidation 12, EF Core 10,
MySql.EntityFrameworkCore, Dapper, MySqlConnector; React 19, Vite 8, Material UI 9, Emotion

**Storage**: MySQL through the shared `BusinessDbContext`; existing
`restaurant_categories` adjacency-list table plus a forward-only migration

**Testing**: `dotnet build`, backend integration tests if test infrastructure is introduced,
`npm run lint`, `npm run build`, and [quickstart.md](quickstart.md). The repository currently has no
automated test project, so manual API/database verification is a required fallback and must be
recorded in the implementation handoff.

**Target Platform**: ASP.NET Core web API and evergreen desktop/mobile web browsers

**Project Type**: Full-stack web application (modular backend plus React SPA)

**Performance Goals**: Search/filter/page interactions return within 2 seconds under normal load;
users can find a category within 15 seconds with 10,000 categories; ordering is deterministic

**Constraints**: No hard delete; immutable code; case-insensitive code uniqueness; arbitrary-depth
acyclic hierarchy; effective sale status depends on all ancestors; stale writes return conflict;
Identity permissions are checked on every protected request; cancellation reaches database work

**Scale/Scope**: One management screen, four permission policies, three endpoint paths/five
operations, up to 10,000 category nodes, and compatibility with later food/promotion features

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1 design.*

- **Layering and modularity — PASS**: Domain remains dependency-free; Application owns CQRS,
  validation, DTOs, and persistence abstractions; Infrastructure implements EF/Dapper access; API
  and React coordinate transport and presentation only. Additions live under Restaurant/Categories.
- **CQRS and persistence — PASS**: Commands use EF; recursive list/detail queries use Dapper;
  controllers dispatch MediatR requests. Existing migrations remain immutable.
- **Security and contracts — PASS**: Endpoints require bearer authentication and exact
  `Categories.Read`, `Categories.Create`, `Categories.Update`, and `Categories.ViewMenu` strings.
  Identity remains the live permission source; errors retain Problem Details conventions.
- **Validation and verification — PASS**: Guards cover normalized values, lengths, uniqueness,
  parent validity, cycles, immutable code, and concurrency. Build, lint, migration inspection, and
  targeted scenarios are required.
- **Observability and safety — PASS**: Existing correlation, structured logging, exception handling,
  and performance behavior are reused. Recursive queries and writes accept cancellation.
- **Readable formatting — PASS**: Backend and JSX changes follow the multi-line formatting rule.

No constitutional exceptions are required. Post-design review introduces no new violation.

## Project Structure

### Documentation (this feature)

```text
specs/001-restaurant-categories/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── categories.openapi.yaml
└── tasks.md                         # Created later by speckit-tasks
```

### Source Code (repository root)

```text
Business-api/src/
├── Business.Domain/Entities/Restaurant/CatalogEntities.cs
├── Business.Application/
│   ├── Abstractions/Persistence/Restaurant/
│   │   ├── ICategoryRepository.cs
│   │   └── ICategoryReadRepository.cs
│   ├── Common/Authorization/CategoriesPermissions.cs
│   └── Restaurant/Categories/
│       ├── CreateCategory/
│       ├── UpdateCategory/
│       ├── GetCategoryByCode/
│       ├── GetCategories/
│       ├── Dtos/
│       └── CategoryRules.cs
├── Business.Infrastructure/
│   ├── Persistence/Configurations/Restaurant/CatalogConfigurations.cs
│   ├── Persistence/MySqlCategoriesRepository.cs
│   ├── Persistence/DapperCategoriesReadRepository.cs
│   └── Migrations/Restaurant/
└── Business.Api/Controllers/Restaurant/CategoriesController.cs

Business-client/src/
├── app/App.jsx
├── app/AppShell.jsx
└── features/restaurant/categories/
    ├── api/categoriesApi.js
    ├── components/CategoryFormDialog.jsx
    ├── components/CategoryStatusDialog.jsx
    ├── components/CategoryTreeTable.jsx
    └── CategoryPage.jsx
```

**Structure Decision**: Extend the existing full-stack structure and mirror the established
Restaurant Products vertical slice while keeping category logic in its own module feature.

## Phase 0: Research Decisions

Resolved decisions and rejected alternatives are in [research.md](research.md). All technical
questions needed for this plan are resolved.

## Phase 1: Design and Contracts

- [data-model.md](data-model.md) defines the adjacency-list entity, effective status, validation,
  relationships, and concurrency/state transitions.
- [contracts/categories.openapi.yaml](contracts/categories.openapi.yaml) defines list/detail/create/
  update operations, response envelopes, and Problem Details outcomes.
- [quickstart.md](quickstart.md) defines runnable validation for permissions, hierarchy behavior,
  concurrency, activation effects, accessibility, performance, and builds.

## Implementation Approach

1. Mark `UpdatedDate` as concurrency-checked and add only indexes justified by query plans; generate
   a new Restaurant migration and inspect its SQL.
2. Add Category permissions, DTOs, persistence abstractions, validators, CQRS handlers, and EF/Dapper
   repositories. Keep writes atomic and reject invalid/cyclic parents.
3. Expose secured endpoints through a thin controller. Return 409 for duplicate codes, invalid
   hierarchy races, and stale versions; use validation Problem Details for input errors.
4. Add frontend API bindings and the category UI. Keep query/filter/page and expanded-node state
   stable; reset page when search, status, or size changes.
5. Wire the Identity `categories` menu route and permission-controlled actions into the shell, then
   execute the quickstart and quality gates.

## Complexity Tracking

No constitution violations require justification.
