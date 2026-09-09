# Quickstart Validation: Restaurant Layout

## Prerequisites

- Configure MySQL at `ConnectionStrings:RestaurantDatabase`.
- Use a Restaurant token/menu with applicable `Layouts.*` permissions and `/restaurant/layouts`.
- Install .NET 10 SDK and Node/npm.

## Build gates

```powershell
dotnet build Business-api/Business-api.slnx
npm --prefix Business-client run lint
npm --prefix Business-client run build
```

Inspect new migration SQL: it must be forward-only and preserve existing rows/history.

## End-to-end scenarios

1. With `Layouts.Read`, open both tabs and verify stable search/filter/page, keyboard, collapsed
   sidebar and mobile behavior. Without permission, verify menu/route/API are inaccessible.
2. Create an area/table. Verify immutable normalized unique codes, capacity, active-area and default
   Available rules; invalid/duplicate input writes nothing.
3. Update with the returned version, then repeat a stale update; verify 409 and no newer-state loss.
4. With an open session, verify table move/disable return 409. With Occupied/Cleaning, verify the
   configuration form cannot overwrite operational status.
5. Deactivate an impacted area: verify preview/confirmation, retention of tables/sessions and
   effective blocking of new service. Reactivate it.
6. Disable eligible Available table and reactivate it under an active area; verify status/state.
7. Trigger concurrent identical list loads on each tab: verify one request/equal result. Change a
   condition for a separate request. Force failure, retry, and verify a new request succeeds.
8. Verify response envelopes/Problem Details, correlation ID, cancellation and structured logging.

See [contracts/layouts.openapi.yaml](contracts/layouts.openapi.yaml) for payloads and status codes.
