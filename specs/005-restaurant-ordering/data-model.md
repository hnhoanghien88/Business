# Data Model: Restaurant Ordering

- **TableSession**: existing occupied-table visit; must be `Open`; owns many independent orders.
- **SellableMenuItem**: category/food/variant projection with current price, default flag, availability
  and sold-out reason; all three catalog levels must be active.
- **Cart**: client value keyed by session ID; lines contain variant ID, positive integer quantity and
  optional note (maximum 500). It clears only after success.
- **Order / OrderItem**: a new order per confirmation. `ClientRequestId` is required and unique. Items
  snapshot food code/name, variant name, current price, quantity, note and calculated totals.
- **OrderPromotion**: optional single current promotion snapshot with code/name/discount.
- **KitchenOrder / KitchenOrderItem**: one initial ticket per confirmed order containing full quantities.

## Invariants and transitions

- Open session + valid cart -> Pending order and Pending kitchen ticket in one commit.
- Same request ID -> original result; no new rows.
- Stale total, closed session, unavailable item or invalid promotion -> conflict; no rows.
- Later calls create new orders and never mutate old snapshots.
- Effective kitchen quantities must never exceed ordered quantity.
