# Tasks: Restaurant Ordering

**Input**: Design documents from `/specs/005-restaurant-ordering/`

## Phase 1: Setup

- [X] T001 Verify restaurant schema, solution quality gates and ignore patterns in `Business-api/Business-api.slnx`, `Business-client/package.json`, and `.gitignore`

## Phase 2: Foundational

- [X] T002 [P] Add ordering permissions in `Business-api/src/Business.Application/Common/Authorization/OrderingPermissions.cs`
- [X] T003 [P] Add ordering DTOs in `Business-api/src/Business.Application/Restaurant/Ordering/Dtos/OrderingDtos.cs`
- [X] T004 [P] Add ordering persistence abstractions in `Business-api/src/Business.Application/Abstractions/Persistence/Restaurant/IOrderingReadRepository.cs` and `IOrderingRepository.cs`
- [X] T005 Add request-id persistence and unique index in `Business-api/src/Business.Domain/Entities/Restaurant/OrderEntities.cs`, `Business-api/src/Business.Infrastructure/Persistence/Configurations/Restaurant/OrderConfigurations.cs`, and a migration under `Business-api/src/Business.Infrastructure/Migrations/Restaurant/`
- [X] T006 Register ordering repositories and validators in `Business-api/src/Business.Infrastructure/DependencyInjection.cs` and `Business-api/src/Business.Api/Program.cs`

## Phase 3: User Story 1 - Select Items for an Open Table (P1)

**Goal**: Browse the live menu and maintain an isolated cart for one open session.

**Independent Test**: Load an open session, filter/search, add variants, edit quantities/notes and verify sold-out blocking and correct subtotal.

- [X] T007 [P] [US1] Implement current menu/session Dapper reads in `Business-api/src/Business.Infrastructure/Persistence/DapperOrderingReadRepository.cs`
- [X] T008 [US1] Add menu query and validation in `Business-api/src/Business.Application/Restaurant/Ordering/Queries/OrderingQueries.cs`
- [X] T009 [US1] Expose protected menu endpoint in `Business-api/src/Business.Api/Controllers/Restaurant/OrderingController.cs`
- [X] T010 [P] [US1] Add normalized coalesced menu API client and session carts in `Business-client/src/features/restaurant/ordering/api/orderingApi.js` and `cartStore.js`
- [X] T011 [US1] Build responsive accessible menu/cart workspace in `Business-client/src/features/restaurant/ordering/OrderingPage.jsx`
- [X] T012 [US1] Wire ordering route and navigation in `Business-client/src/app/App.jsx`, `Business-client/src/app/AppShell.jsx`, and `Business-client/src/index.css`

## Phase 4: User Story 2 - Confirm and Send to Kitchen (P1)

**Goal**: Revalidate and atomically commit a retry-safe order plus kitchen ticket.

**Independent Test**: Confirm a valid cart, retry the request ID, then exercise repricing/sold-out/session-close conflicts without partial rows.

- [X] T013 [US2] Implement authoritative preview, atomic commit and idempotent retry in `Business-api/src/Business.Infrastructure/Persistence/MySqlOrderingRepository.cs`
- [X] T014 [US2] Add confirm command and validators in `Business-api/src/Business.Application/Restaurant/Ordering/Commands/OrderingCommands.cs`
- [X] T015 [US2] Expose protected confirmation endpoint in `Business-api/src/Business.Api/Controllers/Restaurant/OrderingController.cs`
- [X] T016 [US2] Add price-change acknowledgement, submit guard and success/error handling in `Business-client/src/features/restaurant/ordering/OrderingPage.jsx`

## Phase 5: User Story 3 - Place Additional Orders (P1)

**Goal**: Show immutable order history/totals and allow another cart confirmation for the same session.

**Independent Test**: Submit twice for one session and verify two independent orders, unchanged snapshots and aggregate total.

- [X] T017 [US3] Implement session order-history projection in `Business-api/src/Business.Infrastructure/Persistence/DapperOrderingReadRepository.cs`
- [X] T018 [US3] Expose history query/endpoint in `Business-api/src/Business.Application/Restaurant/Ordering/Queries/OrderingQueries.cs` and `Business-api/src/Business.Api/Controllers/Restaurant/OrderingController.cs`
- [X] T019 [US3] Add history and session totals with post-confirm refresh in `Business-client/src/features/restaurant/ordering/OrderingPage.jsx`

## Phase 6: User Story 4 - Apply Promotion (P2)

**Goal**: Preview one promotion and revalidate it during commit.

**Independent Test**: Preview valid/invalid codes, alter conditions, and verify only the commit-time valid discount is snapshotted.

- [X] T020 [US4] Add promotion preview request/endpoint using authoritative repository evaluation in `Business-api/src/Business.Application/Restaurant/Ordering/Commands/OrderingCommands.cs` and `Business-api/src/Business.Api/Controllers/Restaurant/OrderingController.cs`
- [X] T021 [US4] Add promotion entry, reason and discounted totals in `Business-client/src/features/restaurant/ordering/OrderingPage.jsx`

## Phase 7: Polish and Verification

- [X] T022 Verify coalescing, changed-key separation, failed-load retry and per-session cart isolation in `Business-client/src/features/restaurant/ordering/api/orderingApi.js` and `cartStore.js`
- [X] T023 Run backend build and frontend lint/build, then record runnable scenarios in `specs/005-restaurant-ordering/quickstart.md`

## Dependencies and Execution Order

Setup -> Foundational -> US1 -> US2 -> US3 -> US4 -> Polish. T002-T004 are parallel; T007 and T010
are parallel. US1 provides the cart surface, US2 commits it, US3 projects results, and US4 extends pricing.
Suggested MVP is US1 + US2; US3 remains P1 and follows immediately.

## Parallel Examples

- Implement the Dapper menu read T007 while implementing the frontend request/cart utilities T010.
- Permission, DTO and abstraction tasks T002-T004 touch separate files and can proceed together.

## Implementation Strategy

Establish contracts and idempotency storage, deliver the independently usable menu/cart, then add the
atomic confirmation path. Add history and promotion without mutating the confirmed-order model, and
finish with build/lint plus concurrency and failure-retry validation.
