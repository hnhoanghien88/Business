# Research: Restaurant Ordering

## Transaction and idempotency

- **Decision**: Revalidate and create order, items, promotion snapshot and kitchen ticket in one EF
  transaction; persist a caller-generated request ID under a unique index.
- **Rationale**: Atomic confirmation and deterministic retry behavior survive a lost HTTP response.
- **Alternatives considered**: Separate order/send commands risk partial state; cache-only keys can disappear.

## Authority and cart

- **Decision**: Client sends variant IDs, quantities, notes, one optional code and its observed total.
  Server recomputes current totals and returns a conflict when acknowledgement is stale.
- **Rationale**: Browser data never decides price/discount, while staff consciously accepts repricing.
- **Alternatives considered**: Trusting cart prices violates FR-005; silent repricing violates FR-007.

## Reads

- **Decision**: Dapper returns active categories/foods/variants, including unavailable active variants
  with reasons. Client carts are keyed by session and identical in-flight menu reads are coalesced.
- **Rationale**: Lightweight live data, understandable sold-out choices and no cross-session leakage.
- **Alternatives considered**: Persistent server carts add scope; stale caching conflicts with availability.
