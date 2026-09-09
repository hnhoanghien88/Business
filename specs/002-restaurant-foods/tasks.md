# Tasks: Restaurant Foods

**Input**: Design documents from `/specs/002-restaurant-foods/`

## Phase 1: Setup

- [X] T001 Verify canonical Foods paths, existing schema, migration history, and ignore patterns in `Business-api/src`, `Business-client/src`, and `.gitignore`
- [X] T002 Rename the frontend Product feature directory and files to Foods under `Business-client/src/features/restaurant/foods/`
- [X] T003 Rename Product application abstractions, feature files, and infrastructure repositories to Foods under `Business-api/src/Business.Application` and `Business-api/src/Business.Infrastructure`

## Phase 2: Foundational

- [X] T004 Define complete food, variant, price-history, paging, filter, and version DTOs in `Business-api/src/Business.Application/Restaurant/Foods/Dtos/FoodDtos.cs`
- [X] T005 Define Food read/write persistence contracts in `Business-api/src/Business.Application/Abstractions/Persistence/Restaurant/IFoodRepository.cs` and `IFoodReadRepository.cs`
- [X] T006 Add shared normalization and mapping rules in `Business-api/src/Business.Application/Restaurant/Foods/FoodRules.cs`
- [X] T007 Configure Food and FoodVariant optimistic concurrency and required catalog constraints in `Business-api/src/Business.Infrastructure/Persistence/Configurations/Restaurant/CatalogConfigurations.cs`
- [X] T008 Register renamed Foods repositories and validators in `Business-api/src/Business.Infrastructure/DependencyInjection.cs` and `Business-api/src/Business.Api/Program.cs`

## Phase 3: User Story 1 - Browse Foods Catalog (P1)

**Goal**: Authorized users browse, search, filter, page, and inspect Foods without duplicate concurrent list requests.

**Independent Test**: Query a mixed catalog by search/category/status/availability, verify stable paging/detail output, and observe one network call for identical concurrent loads.

- [X] T009 [US1] Implement filtered Food list/detail queries in `Business-api/src/Business.Application/Restaurant/Foods/GetFoods/GetFoods.cs` and `GetFoodByCode/GetFoodByCode.cs`
- [X] T010 [US1] Implement aggregated Dapper Food projections in `Business-api/src/Business.Infrastructure/Persistence/DapperFoodsReadRepository.cs`
- [X] T011 [US1] Expose secured Food list/detail operations in `Business-api/src/Business.Api/Controllers/Restaurant/FoodsController.cs`
- [X] T012 [P] [US1] Implement canonical Food API bindings and identical in-flight list coalescing in `Business-client/src/features/restaurant/foods/api/foodsApi.js`
- [X] T013 [US1] Build the Foods list/search/filter/page UI in `Business-client/src/features/restaurant/foods/FoodsPage.jsx`
- [X] T014 [US1] Wire canonical Foods navigation and legacy redirects in `Business-client/src/app/App.jsx` and `Business-client/src/app/AppShell.jsx`

## Phase 4: User Story 2 - Create and Update Foods (P1)

**Goal**: Authorized users create and update complete Food metadata with immutable codes, valid categories, and stale-write protection.

**Independent Test**: Create a Food in an active category, reject invalid/duplicate input, update presentation/category/state, and reject a stale version.

- [X] T015 [US2] Implement create/update/deactivate Food commands and validators in `Business-api/src/Business.Application/Restaurant/Foods/CreateFood/`, `UpdateFood/`, and `DeactivateFood/`
- [X] T016 [US2] Implement atomic Food writes and conflict translation in `Business-api/src/Business.Infrastructure/Persistence/MySqlFoodsRepository.cs`
- [X] T017 [US2] Add Food create/update/deactivate endpoints in `Business-api/src/Business.Api/Controllers/Restaurant/FoodsController.cs`
- [X] T018 [US2] Build complete Food create/edit/deactivation dialogs in `Business-client/src/features/restaurant/foods/components/` and integrate them in `FoodsPage.jsx`

## Phase 5: User Story 3 - Manage Variants and Prices (P1)

**Goal**: Authorized users manage variants, exactly one active default, current prices, and continuous price history.

**Independent Test**: Create two variants, switch default, reject invalid default deactivation, change price, and verify one open non-overlapping history interval.

- [X] T019 [US3] Implement variant create/update commands and default invariant in `Business-api/src/Business.Application/Restaurant/Foods/Variants/`
- [X] T020 [US3] Implement atomic price-change command and history rules in `Business-api/src/Business.Application/Restaurant/Foods/Prices/`
- [X] T021 [US3] Implement variant/default/price transactions in `Business-api/src/Business.Infrastructure/Persistence/MySqlFoodsRepository.cs`
- [X] T022 [US3] Add variant and price endpoints in `Business-api/src/Business.Api/Controllers/Restaurant/FoodsController.cs`
- [X] T023 [US3] Build variant/default/price-history management UI in `Business-client/src/features/restaurant/foods/components/`

## Phase 6: User Story 4 - Manage Availability (P2)

**Goal**: Authorized users mark variants sold out or available and ordering consumers receive correct eligibility.

**Independent Test**: Mark a variant unavailable with a reason, verify it is excluded from new ordering, then restore it without re-login.

- [X] T024 [US4] Implement availability command, validation, and write behavior in `Business-api/src/Business.Application/Restaurant/Foods/Availability/` and `MySqlFoodsRepository.cs`
- [X] T025 [US4] Add the Foods availability endpoint in `Business-api/src/Business.Api/Controllers/Restaurant/FoodsController.cs`
- [X] T026 [US4] Build availability controls and permission states in `Business-client/src/features/restaurant/foods/components/`

## Phase 7: Polish and Verification

- [X] T027 Generate and inspect a forward-only migration under `Business-api/src/Business.Infrastructure/Migrations/Restaurant/` only if model comparison requires it
- [X] T028 Verify all `Foods.*` authorization paths, exact Identity application/audience `Restaurant`, Problem Details, cancellation, and operational logging
- [X] T029 Run backend build, frontend lint/build, and the scenarios in `specs/002-restaurant-foods/quickstart.md`
- [X] T030 Update repository documentation and remove remaining active Product naming references in `PROJECT_OVERVIEW.md`, `Business-api/src`, and `Business-client/src`

## Dependencies and Execution Order

- Setup → Foundational → US1 → US2 → US3 → US4 → Polish.
- US1 is the MVP and establishes read contracts/UI consumed by later stories.
- US2 depends on foundational Food writes. US3 depends on Food detail/write support. US4 depends on variant support.
- Tasks marked [P] touch independent client/server files and may run concurrently after their phase prerequisites.

## Parallel Examples

- T012 can proceed alongside T009–T011 after DTO and persistence contracts stabilize.
- Within verification, backend build and frontend lint/build can run in parallel after implementation.

## Implementation Strategy

Deliver canonical read-only Foods management first, then Food writes, variants/prices, and availability. Preserve migration history and historical transaction data throughout. Validate each story independently before advancing.
