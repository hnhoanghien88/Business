# Tasks: Restaurant Table Operations

**Input**: Design documents from `/specs/004-restaurant-table-operations/`

## Phase 1: Setup

- [X] T001 Verify existing layout/session/order/payment schema and ignore patterns in `Business-api/src`, `Business-client/src`, and `.gitignore`
- [X] T002 Record current backend/frontend quality-gate baseline from `Business-api/Business-api.slnx` and `Business-client/package.json`

## Phase 2: Foundational

- [X] T003 [P] Add operational permission constants in `Business-api/src/Business.Application/Common/Authorization/TableOperationsPermissions.cs`
- [X] T004 [P] Add snapshot/session DTOs in `Business-api/src/Business.Application/Restaurant/TableOperations/Dtos/TableOperationDtos.cs`
- [X] T005 [P] Add read/write abstractions in `Business-api/src/Business.Application/Abstractions/Persistence/Restaurant/ITableOperationsReadRepository.cs` and `ITableOperationsRepository.cs`
- [X] T006 Register repositories and validators in `Business-api/src/Business.Infrastructure/DependencyInjection.cs` and `Business-api/src/Business.Api/Program.cs`

## Phase 3: User Story 1 - Monitor Table State (P1)

**Goal**: Staff see grouped, filterable authoritative table/session state and recover after disconnect.

**Independent Test**: Load mixed states, verify grouping/session timing and observe another client's change within three seconds without duplicates.

- [X] T007 [P] [US1] Implement stable Dapper snapshot/detail reads in `Business-api/src/Business.Infrastructure/Persistence/DapperTableOperationsReadRepository.cs`
- [X] T008 [US1] Implement list/detail queries in `Business-api/src/Business.Application/Restaurant/TableOperations/Queries/TableOperationQueries.cs`
- [X] T009 [US1] Expose protected snapshot/detail endpoints in `Business-api/src/Business.Api/Controllers/Restaurant/TableOperationsController.cs`
- [X] T010 [P] [US1] Add normalized coalesced snapshot API client in `Business-client/src/features/restaurant/tableOperations/api/tableOperationsApi.js`
- [X] T011 [US1] Build grouped responsive accessible floor cards with polling/reconnect state in `Business-client/src/features/restaurant/tableOperations/TableOperationsPage.jsx`
- [X] T012 [US1] Wire Identity menu and route in `Business-client/src/app/App.jsx`, `Business-client/src/app/AppShell.jsx`, and `Business-client/src/index.css`

## Phase 4: User Story 2 - Open Table (P1)

**Goal**: Staff atomically open exactly one session on an eligible table with controlled capacity override.

**Independent Test**: Concurrent opens yield one session; invalid state/count and unauthorized override fail without partial changes.

- [X] T013 [US2] Implement transactional open/session uniqueness and capacity override in `Business-api/src/Business.Infrastructure/Persistence/MySqlTableOperationsRepository.cs`
- [X] T014 [US2] Implement open command and validator in `Business-api/src/Business.Application/Restaurant/TableOperations/Commands/TableOperationCommands.cs`
- [X] T015 [US2] Add protected open endpoint in `Business-api/src/Business.Api/Controllers/Restaurant/TableOperationsController.cs`
- [X] T016 [US2] Add open-table dialog and command refresh in `Business-client/src/features/restaurant/tableOperations/components/OpenTableDialog.jsx` and `TableOperationsPage.jsx`

## Phase 5: User Story 3 - View and Continue Session (P1)

**Goal**: Staff inspect complete session totals and navigate to ordering/payment according to permissions.

**Independent Test**: Session with several orders/payments shows correct totals, blockers and permission-sensitive actions.

- [X] T017 [US3] Complete session order/payment projection in `Business-api/src/Business.Infrastructure/Persistence/DapperTableOperationsReadRepository.cs`
- [X] T018 [US3] Add session detail dialog and ordering/payment CTAs in `Business-client/src/features/restaurant/tableOperations/components/SessionDialog.jsx` and `TableOperationsPage.jsx`

## Phase 6: User Story 4 - Close and Clean (P2)

**Goal**: Eligible sessions close into Cleaning and cleaned tables return to Available; overrides remain audited.

**Independent Test**: Block obligations, close eligible/overridden session, then mark Cleaning table Available.

- [X] T019 [US4] Implement transactional close/override/mark-clean transitions in `Business-api/src/Business.Infrastructure/Persistence/MySqlTableOperationsRepository.cs`
- [X] T020 [US4] Add close and clean commands/validators in `Business-api/src/Business.Application/Restaurant/TableOperations/Commands/TableOperationCommands.cs`
- [X] T021 [US4] Expose protected close and mark-clean endpoints in `Business-api/src/Business.Api/Controllers/Restaurant/TableOperationsController.cs`
- [X] T022 [US4] Add close/override/clean UI actions in `Business-client/src/features/restaurant/tableOperations/components/SessionDialog.jsx` and `TableOperationsPage.jsx`

## Phase 7: Polish and Verification

- [X] T023 Verify identical concurrent snapshot calls coalesce, different keys separate and failures retry in `Business-client/src/features/restaurant/tableOperations/api/tableOperationsApi.js`
- [X] T024 Run backend build and frontend lint/build using `Business-api/Business-api.slnx` and `Business-client/package.json`
- [ ] T025 Execute and record live authorization, concurrency, reconnect and lifecycle scenarios from `specs/004-restaurant-table-operations/quickstart.md`

## Dependencies and Execution Order

- Setup → Foundational → US1 → US2 → US3 → US4 → Polish.
- T003–T005 can proceed in parallel. T007 and T010 can proceed together.
- US1 supplies the operational surface; US2 and US3 build on it; US4 requires detail and commands.
- Suggested MVP is US1+US2, followed by US3 because all are P1.

## Parallel Examples

- Snapshot read T007 can run with frontend API client T010.
- Permission, DTO and abstraction tasks T003–T005 touch separate files.

## Implementation Strategy

Build the authoritative snapshot first, then atomic opening, session detail and safe closing. Validate
polling/coalescing and all available build gates after the complete lifecycle works.
