# Data Model: Restaurant Table Operations

## Operational table snapshot

Projection over active areas/tables: table and area identity, capacity, `Status`, current Open
session ID, guest count, opened time and version. Stable order is area order/name then table name/code.

## TableSession detail

Existing session fields are retained. Derived fields include orders, `totalAmount` from non-cancelled
orders, `paidAmount` from Paid payments, `remainingAmount`, and close blockers. One table may have
history but at most one current Open session.

## Transitions

```text
Available active table --OpenSession--> Occupied + one Open session
Eligible Open session --CloseSession--> Closed + table Cleaning
Open session + override permission/reason --OverrideClose--> Closed + table Cleaning
Cleaning --MarkClean--> Available
```

Guest count must be positive; capacity excess requires override/reason. Commands re-read current
state. Invalid or stale transitions return 409. Ordering/payment features retain ownership of records.
