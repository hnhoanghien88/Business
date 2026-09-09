# Quickstart: Restaurant Table Operations

## Gates

```powershell
dotnet build Business-api/Business-api.slnx
npm --prefix Business-client run lint
npm --prefix Business-client run build
```

Use configured MySQL and an Identity token/menu for `/restaurant/table-operations`.

## Validation

1. Verify grouping, labels, filters, duration, responsive layout and keyboard use with mixed states.
2. Start two concurrent opens for one Available table; exactly one creates an Open session.
3. Reject invalid guest count, inactive/disabled table, and excess capacity without override.
4. Verify capacity/close overrides require permission and a reason that remains auditable.
5. Verify session order/payment totals and permission-sensitive CTAs.
6. Reject normal close with obligations; eligible close produces Closed/Cleaning; MarkClean returns Available.
7. Keep two clients open and verify updates within three seconds; reconnect and verify snapshot wins.
8. Fire identical concurrent snapshot queries: verify one network call/equal result; changed conditions
   create another call, and a failed call can be retried.

See [contracts/table-operations.openapi.yaml](contracts/table-operations.openapi.yaml).
