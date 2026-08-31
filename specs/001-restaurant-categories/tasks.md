# Tasks: Restaurant Categories

**Input**: Design documents from `/specs/001-restaurant-categories/`

## Phase 1: Setup

- [X] T001 Verify category feature paths and existing category schema in `Business-api/src` and `Business-client/src`
- [X] T002 Verify required .NET/Node ignore patterns in `.gitignore`

## Phase 2: Foundational

- [X] T003 [P] Add category permission constants in `Business-api/src/Business.Application/Common/Authorization/CategoriesPermissions.cs`
- [X] T004 [P] Add category DTOs and paging/filter contracts in `Business-api/src/Business.Application/Restaurant/Categories/Dtos/CategoryDtos.cs`
- [X] T005 [P] Add category read/write persistence abstractions in `Business-api/src/Business.Application/Abstractions/Persistence/Restaurant/ICategoryRepository.cs` and `ICategoryReadRepository.cs`
- [X] T006 Configure category concurrency and hierarchy indexes in `Business-api/src/Business.Infrastructure/Persistence/Configurations/Restaurant/CatalogConfigurations.cs`
- [X] T007 Register category repositories and validators in `Business-api/src/Business.Infrastructure/DependencyInjection.cs` and `Business-api/src/Business.Api/Program.cs`

## Phase 3: User Story 1 - View and Find Categories (P1) MVP

**Goal**: Authorized users can browse, search, filter, page, expand, and retry a category tree with ancestor context.

**Independent Test**: Load a three-level mixed-status tree, expand branches, search by partial code/name, filter status, and verify stable hierarchy/path output.

- [X] T008 [P] [US1] Implement recursive category read repository in `Business-api/src/Business.Infrastructure/Persistence/DapperCategoriesReadRepository.cs`
- [X] T009 [US1] Implement list/detail queries in `Business-api/src/Business.Application/Restaurant/Categories/GetCategories/GetCategories.cs` and `GetCategoryByCode/GetCategoryByCode.cs`
- [X] T010 [US1] Expose secured list/detail API operations in `Business-api/src/Business.Api/Controllers/Restaurant/CategoriesController.cs`
- [X] T011 [P] [US1] Add frontend category API bindings in `Business-client/src/features/restaurant/categories/api/categoriesApi.js`
- [X] T012 [US1] Build accessible category tree list/search/filter/page UI in `Business-client/src/features/restaurant/categories/CategoryPage.jsx` and `components/CategoryTreeTable.jsx`
- [X] T013 [US1] Wire Identity menu route and responsive navigation in `Business-client/src/app/App.jsx`, `Business-client/src/app/AppShell.jsx`, and `Business-client/src/index.css`

## Phase 4: User Story 2 - Create Categories (P1)

**Goal**: Authorized users create root or child categories with normalized, validated, unique data.

**Independent Test**: Create one root and two child levels, then reload and verify parent and stable order; invalid and duplicate codes write nothing.

- [X] T014 [US2] Implement category rules, EF repository, create command, and validator in `Business-api/src/Business.Application/Restaurant/Categories/CategoryRules.cs`, `CreateCategory/CreateCategory.cs`, and `Business-api/src/Business.Infrastructure/Persistence/MySqlCategoriesRepository.cs`
- [X] T015 [US2] Add create endpoint mapping in `Business-api/src/Business.Api/Controllers/Restaurant/CategoriesController.cs`
- [X] T016 [US2] Add create form with parent selection and duplicate-submit protection in `Business-client/src/features/restaurant/categories/components/CategoryFormDialog.jsx` and `CategoryPage.jsx`

## Phase 5: User Story 3 - Update and Move Categories (P2)

**Goal**: Authorized users edit mutable fields and atomically move a subtree without cycles or stale overwrites.

**Independent Test**: Move a category with descendants to a valid parent, reject self/descendant parents, and reject a stale second update.

- [X] T017 [US3] Implement atomic move and optimistic-concurrency update command in `Business-api/src/Business.Application/Restaurant/Categories/UpdateCategory/UpdateCategory.cs` and `Business-api/src/Business.Infrastructure/Persistence/MySqlCategoriesRepository.cs`
- [X] T018 [US3] Add update endpoint mapping in `Business-api/src/Business.Api/Controllers/Restaurant/CategoriesController.cs`
- [X] T019 [US3] Add edit/move UX with immutable code and conflict recovery in `Business-client/src/features/restaurant/categories/components/CategoryFormDialog.jsx` and `CategoryPage.jsx`

## Phase 6: User Story 4 - Safe Activation Changes (P2)

**Goal**: Users confirm deactivation impact while descendant/food local states remain unchanged.

**Independent Test**: Deactivate and reactivate a parent, verifying effective state changes and preservation of all local child/food states.

- [X] T020 [US4] Return descendant/food impact and effective status from category queries in `Business-api/src/Business.Infrastructure/Persistence/DapperCategoriesReadRepository.cs`
- [X] T021 [US4] Add impact confirmation and activation controls in `Business-client/src/features/restaurant/categories/components/CategoryStatusDialog.jsx` and `CategoryPage.jsx`

## Phase 7: Polish and Verification

- [X] T022 Generate and inspect a forward-only category migration in `Business-api/src/Business.Infrastructure/Migrations/Restaurant/`
- [X] T023 Run backend build and frontend lint/build using `Business-api/Business-api.slnx` and `Business-client/package.json`
- [X] T024 Validate contract and manual scenarios from `specs/001-restaurant-categories/quickstart.md`

## Dependencies and Execution Order

- Setup → Foundational → US1 → US2 → US3 → US4 → Polish.
- T003–T005 can run in parallel. T008 and T011 can run in parallel after foundational work.
- US1 is the suggested MVP. Later stories depend on its read/controller/page surfaces but remain independently testable through their stated scenarios.

## Parallel Examples

- US1 backend read work (T008) can proceed with frontend API bindings (T011).
- Foundational permission, DTO, and persistence interface work (T003–T005) touches separate files.

## Implementation Strategy

Complete the browse/search MVP first, then add create, update/move, and safe activation in priority order. Validate each story at its checkpoint and finish with migration, build, lint, and quickstart evidence.
