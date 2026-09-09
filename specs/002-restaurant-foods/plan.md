# Implementation Plan: Restaurant Foods

**Branch**: `[002-restaurant-foods]` | **Date**: 2026-09-03 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-restaurant-foods/spec.md`

## Summary

Evolve the current minimal Product vertical slice into the canonical Foods catalog at
`/restaurant/foods` and `/api/restaurant/foods`. Retain the existing `Food`, `FoodVariant`, and
`FoodPriceHistory` domain/schema foundation, rename application/client concepts to Foods, and add
search/filter/paging, full food metadata, variant/default invariants, atomic price history,
availability, optimistic concurrency, permission enforcement, and in-flight list-request
coalescing. No parallel Product catalog or table is introduced.

## Technical Context

**Language/Version**: C# / .NET 10; JavaScript ES modules with React 19  
**Primary Dependencies**: ASP.NET Core, MediatR 14, FluentValidation 12, EF Core 10,
MySql.EntityFrameworkCore, Dapper, MySqlConnector, React 19, Vite 8, Material UI 9, oxlint  
**Storage**: MySQL `restaurant_db`; `restaurant_foods`, `restaurant_food_variants`,
`restaurant_food_price_histories`; existing `BusinessDbContext`  
**Testing**: `dotnet build`, frontend lint/build, OpenAPI/manual integration scenarios; add focused
integration tests if a suitable test project exists when implementation begins  
**Target Platform**: ASP.NET Core web API and modern desktop/mobile web browsers  
**Project Type**: Full-stack web application  
**Performance Goals**: Search and catalog interaction usable within 2 seconds under normal load;
10,000-food validation dataset; identical concurrent list reads produce one network request  
**Constraints**: Case-sensitive Identity application/audience `Restaurant`; `Foods.*` policies;
one active default variant per food; nonnegative price; immutable codes; no hard delete of referenced
catalog data; cancellation propagated; no authorization-result caching  
**Scale/Scope**: One Foods management screen, food/variant/price-history aggregate, canonical and
legacy browser routes, secured REST contract, up to 100 list rows per page

## Constitution Check

*GATE: Passed before research and re-checked after design.*

- **Layering — PASS**: Domain entities remain independent; Application owns commands, queries,
  validation, DTOs, and persistence abstractions; Infrastructure implements EF/Dapper; API remains
  a thin transport layer.
- **CQRS/persistence — PASS**: Reads use Dapper projections; writes use EF repositories and explicit
  MediatR requests. Atomic multi-row variant/price changes use transactions. Existing migrations
  remain immutable; any schema delta receives a new migration.
- **Security/contracts — PASS**: Every operation validates bearer access and its matching `Foods.*`
  permission through Identity. Application code/audience is exactly `Restaurant`. OpenAPI retains
  response envelopes and Problem Details without exposing credentials.
- **Validation/verification — PASS**: Codes, category state, price, default variant, concurrency,
  and availability invariants are validated before commit. Builds, lint, contract scenarios, and
  migration inspection are required.
- **Observability/safety — PASS**: Existing correlation IDs, structured logging, performance
  behavior, exception mapping, and cancellation are preserved. Request coalescing applies only to
  in-flight catalog reads and never caches authorization results.
- **Formatting — PASS**: New C# and JSX follow the constitution's readable multi-line formatting.

No constitutional exceptions are required. Phase 1 introduces no new gate violation.

## Project Structure

### Documentation (this feature)

```text
specs/002-restaurant-foods/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── foods.openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
Business-api/src/
├── Business.Domain/Entities/Restaurant/CatalogEntities.cs
├── Business.Application/
│   ├── Abstractions/Persistence/Restaurant/
│   │   ├── IFoodRepository.cs
│   │   └── IFoodReadRepository.cs
│   ├── Common/Authorization/FoodsPermissions.cs
│   └── Restaurant/Foods/
│       ├── CreateFood/
│       ├── UpdateFood/
│       ├── GetFoodByCode/
│       ├── GetFoods/
│       ├── Variants/
│       ├── Prices/
│       ├── Availability/
│       ├── Dtos/
│       └── FoodRules.cs
├── Business.Infrastructure/
│   ├── Persistence/Configurations/Restaurant/CatalogConfigurations.cs
│   ├── Persistence/MySqlFoodsRepository.cs
│   ├── Persistence/DapperFoodsReadRepository.cs
│   └── Migrations/Restaurant/
└── Business.Api/Controllers/Restaurant/FoodsController.cs

Business-client/src/
├── app/App.jsx
├── app/AppShell.jsx
└── features/restaurant/foods/
    ├── api/foodsApi.js
    ├── components/
    └── FoodsPage.jsx
```

**Structure Decision**: Refactor the existing Product slice in place into Foods. Preserve domain
tables and migration history; do not maintain Product and Foods implementations in parallel.

## Phase 0: Research Decisions

All decisions and alternatives are recorded in [research.md](research.md). There are no unresolved
technical questions.

## Phase 1: Design and Contracts

- [data-model.md](data-model.md) defines Food, Food Variant, Price History, relationships, invariants,
  concurrency tokens, and state transitions.
- [contracts/foods.openapi.yaml](contracts/foods.openapi.yaml) defines canonical list/detail/write,
  variant, price, and availability operations with permission and failure contracts.
- [quickstart.md](quickstart.md) provides end-to-end validation for schema, authorization, legacy
  redirects, catalog operations, concurrency, request coalescing, and quality gates.

## Implementation Approach

1. Rename Product application abstractions, namespaces, repositories, client feature folders, DTOs,
   and visible text to Foods while keeping the existing Food domain objects and database tables.
2. Extend read projections and filters for category, active/effective state, availability, variants,
   price range, stable ordering, and pagination. Coalesce only identical in-flight client list reads.
3. Extend food create/update commands with immutable code, category validity, presentation fields,
   activation, and optimistic concurrency. Translate uniqueness/stale writes to explicit conflicts.
4. Add variant commands enforcing code uniqueness and exactly one active default. Add atomic price
   changes that close the prior history interval and open the new interval in one transaction.
5. Add availability commands with reason normalization and expose changes to ordering reads without
   requiring a new login. Deactivation preserves historical rows and transaction snapshots.
6. Secure all operations with `Foods.*`, keep Identity as the live permission authority, wire the
   canonical menu/route, and retain client redirects from `/product` and `/restaurant/products`.
7. Generate a forward-only migration only if design/model comparison requires new constraints or
   concurrency metadata; inspect SQL and execute the quickstart quality gates.

## Complexity Tracking

No constitution violations require justification.
