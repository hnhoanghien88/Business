# Research: Restaurant Table Operations

## Synchronization

- **Decision**: Poll authoritative snapshots every two seconds and after commands.
- **Rationale**: Meets the three-second target and repairs missed updates after reconnect.
- **Alternatives considered**: SignalR/SSE adds infrastructure; manual refresh misses the target.

## Atomic transitions and uniqueness

- **Decision**: EF commands use a transaction and check table/session state before insertion.
- **Rationale**: Session and table changes never partially commit; competing opens have one winner.
- **Alternatives considered**: Client checks race; unique `(TableId, Status)` breaks Closed history.

## Closing policy

- **Decision**: Normal close requires zero remaining amount and Completed/Cancelled orders; override
  requires distinct permission and a reason stored in the session audit note.
- **Rationale**: Prevents unpaid or in-progress service from disappearing.
- **Alternatives considered**: Payment-only checks ignore order/kitchen obligations.

## Idempotency

- **Decision**: Invalid competing transitions return 409 after authoritative re-read.
- **Rationale**: Retries cannot duplicate sessions or corrupt state with the existing schema.
- **Alternatives considered**: A separate idempotency table is unnecessary for state commands.

## In-flight request coalescing

- **Decision**: Stable normalized URL keys a shared pending Promise removed in `finally`.
- **Rationale**: Meets constitution without stale caching.
- **Alternatives considered**: Debounce does not cover all concurrent callers.

No unresolved clarification remains.
