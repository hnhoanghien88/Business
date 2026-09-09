# Tasks: Restaurant Layout

**Input**: Design documents from `/specs/003-restaurant-layout/`

## Phase 1: Setup

- [X] T001 Verify existing layout entities, schema, route surfaces, and ignore patterns in `Business-api/src`, `Business-client/src`, and `.gitignore`
- [X] T002 Record current backend/frontend build baselines using `Business-api/Business-api.slnx` and `Business-client/package.json`

## Phase 2: Foundational

- [X] T003 [P] Add layout permission constants in `Business-api/src/Business.Application/Common/Authorization/LayoutsPermissions.cs`
- [X] T004 [P] Add area/table DTO, paging, filter, and request contracts in `Business-api/src/Business.Application/Restaurant/Layouts/Dtos/LayoutDtos.cs`
- [X] T005 [P] Add layout read/write persistence abstractions in `Business-api/src/Business.Application/Abstractions/Persistence/Restaurant/ILayoutReadRepository.cs` and `ILayoutRepository.cs`
- [X] T006 Configure timestamp concurrency and layout indexes in `Business-api/src/Business.Infrastructure/Persistence/Configurations/Restaurant/LayoutConfigurations.cs`
- [X] T007 Register layout repositories and validators in `Business-api/src/Business.Infrastructure/DependencyInjection.cs` and `Business-api/src/Business.Api/Program.cs`

## Phase 3: User Story 1 - Manage Areas (P1)

**Goal**: Authorized managers browse, create, edit, deactivate, and reactivate ordered areas safely.

**Independent Test**: Create/edit/deactivate/reactivate an area, verify stable list/search/filter and impact confirmation while all table/session rows remain.

- [X] T008 [P] [US1] Implement stable paged area projections and impact detail in `Business-api/src/Business.Infrastructure/Persistence/DapperLayoutsReadRepository.cs`
- [X] T009 [US1] Implement area rules and EF writes with immutable code, uniqueness, confirmation, and concurrency in `Business-api/src/Business.Infrastructure/Persistence/MySqlLayoutsRepository.cs`
- [X] T010 [US1] Implement area list/detail/create/update requests and validators in `Business-api/src/Business.Application/Restaurant/Layouts/Areas/AreaRequests.cs`
- [X] T011 [US1] Expose protected area endpoints in `Business-api/src/Business.Api/Controllers/Restaurant/LayoutsController.cs`
- [X] T012 [P] [US1] Add normalized, coalesced area API bindings in `Business-client/src/features/restaurant/layouts/api/layoutsApi.js`
- [X] T013 [US1] Build accessible area list/form/status UI in `Business-client/src/features/restaurant/layouts/LayoutsPage.jsx` and `Business-client/src/features/restaurant/layouts/components/AreaDialogs.jsx`
- [X] T014 [US1] Wire the Identity layouts menu and responsive route in `Business-client/src/app/App.jsx`, `Business-client/src/app/AppShell.jsx`, and `Business-client/src/index.css`

## Phase 4: User Story 2 - Manage Tables (P1)

**Goal**: Authorized managers browse/create/edit tables and move eligible tables between active areas without overwriting live operations.

**Independent Test**: Create and edit a table, move it when idle, reject invalid capacity/inactive area/duplicate code, and reject stale or open-session moves.

- [X] T015 [P] [US2] Add stable paged table projections, filters, safety flags, and detail to `Business-api/src/Business.Infrastructure/Persistence/DapperLayoutsReadRepository.cs`
- [X] T016 [US2] Add table create/update/move EF rules with session and optimistic-concurrency checks in `Business-api/src/Business.Infrastructure/Persistence/MySqlLayoutsRepository.cs`
- [X] T017 [US2] Implement table list/detail/create/update requests and validators in `Business-api/src/Business.Application/Restaurant/Layouts/Tables/TableRequests.cs`
- [X] T018 [US2] Expose protected table list/detail/create/update endpoints in `Business-api/src/Business.Api/Controllers/Restaurant/LayoutsController.cs`
- [X] T019 [P] [US2] Add normalized, coalesced table API bindings in `Business-client/src/features/restaurant/layouts/api/layoutsApi.js`
- [X] T020 [US2] Build accessible table list/filter/form/move UI in `Business-client/src/features/restaurant/layouts/LayoutsPage.jsx` and `Business-client/src/features/restaurant/layouts/components/TableDialogs.jsx`

## Phase 5: User Story 3 - Safe Table Activation (P2)

**Goal**: Eligible idle tables can be disabled and later restored to Available while live tables remain protected.

**Independent Test**: Disable idle Available table, reject open/Occupied/Cleaning table, then reactivate Disabled table under an active area.

- [X] T021 [US3] Implement dedicated session-aware activation write in `Business-api/src/Business.Infrastructure/Persistence/MySqlLayoutsRepository.cs`
- [X] T022 [US3] Implement activation request/validator and endpoint in `Business-api/src/Business.Application/Restaurant/Layouts/Tables/TableRequests.cs` and `Business-api/src/Business.Api/Controllers/Restaurant/LayoutsController.cs`
- [X] T023 [US3] Add impact/status dialog and activation actions in `Business-client/src/features/restaurant/layouts/components/TableDialogs.jsx` and `Business-client/src/features/restaurant/layouts/LayoutsPage.jsx`

## Phase 6: Polish and Cross-Cutting Verification

- [X] T024 Generate and inspect a forward-only layout migration in `Business-api/src/Business.Infrastructure/Migrations/Restaurant/` only if model comparison requires it
- [X] T025 Verify identical concurrent area/table list loads coalesce, changed keys separate, and failed calls retry using `Business-client/src/features/restaurant/layouts/api/layoutsApi.js`
- [X] T026 Run backend build and frontend lint/build using `Business-api/Business-api.slnx` and `Business-client/package.json`
- [ ] T027 Execute and record manual contract, authorization, concurrency, accessibility, and safety scenarios from `specs/003-restaurant-layout/quickstart.md`

## Dependencies and Execution Order

- Setup → Foundational → US1 → US2 → US3 → Polish.
- T003–T005 can run independently. T008 and T012 can proceed together after foundational work.
- US1 supplies active-area data needed by table creation; US2 supplies the table surface used by US3.
- Suggested MVP: US1 plus its shared foundations, followed immediately by US2 because both are P1.

## Parallel Examples

- US1 backend read work T008 can run with frontend binding T012.
- US2 backend projection T015 can run with frontend API binding T019.
- Permission, DTO and persistence-abstraction work T003–T005 touch distinct files.

## Implementation Strategy

Establish shared contracts and persistence first; deliver the independently verifiable Area slice,
then Table create/move, then activation safety. Finish by proving coalescing and all build/manual gates.
