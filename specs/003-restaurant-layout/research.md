# Research: Restaurant Layout

## Reuse existing schema

- **Decision**: Extend `RestaurantArea`, `RestaurantTable` and their configurations.
- **Rationale**: They already own the domain and are referenced by sessions/orders.
- **Alternatives considered**: Parallel management tables would create duplicate truth.

## CQRS persistence

- **Decision**: Dapper projections for list/detail; one EF repository for writes.
- **Rationale**: Matches established Categories/Foods architecture and permits transactional session checks.
- **Alternatives considered**: EF-only reads and stored procedures add inconsistency or needless complexity.

## Optimistic concurrency

- **Decision**: Expose microsecond `UpdatedDate` as `version` and compare before write.
- **Rationale**: Existing tables and sibling features already use this timestamp contract.
- **Alternatives considered**: MySQL has no SQL Server rowversion; numeric versions need broader schema work.

## Operational-state protection

- **Decision**: General updates never accept `Status`; activation is a dedicated operation that
  re-reads status/sessions and permits only Available→Disabled or Disabled→Available.
- **Rationale**: A stale configuration form cannot overwrite Occupied/Cleaning.
- **Alternatives considered**: A broad PUT payload violates FR-006 and FR-013.

## Area deactivation

- **Decision**: Return impact counts, require explicit confirmation when tables exist, change only
  area state, retain children/history and never close sessions.
- **Rationale**: Meets safe-deactivation requirements without destructive cascading.
- **Alternatives considered**: Cascading table status changes can corrupt live operations.

## In-flight coalescing

- **Decision**: Normalize all list conditions into a deterministic key, share the pending Promise,
  and delete it in `finally`.
- **Rationale**: Satisfies the constitution without stale-result or authorization caching.
- **Alternatives considered**: Debounce and completed-result caches do not meet the required behavior.

## Permission boundary

- **Decision**: `Layouts.Read`, `Layouts.Create`, `Layouts.Update`, menu code `layouts`, route
  `/restaurant/layouts`, Identity application `restaurant`.
- **Rationale**: One resource family maps directly to requested actions.
- **Alternatives considered**: Separate Areas/Tables permissions are not requested.

No unresolved technical questions remain.
