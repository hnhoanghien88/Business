# Quickstart Validation: Restaurant Foods

## Prerequisites

- .NET 10 SDK, Node.js, MySQL, and configured Identity/Business application `Restaurant`.
- Application code and token audience must match `Restaurant` exactly, including casing.
- Test users with `Foods.Read`, `Foods.Create`, `Foods.Update`, `Foods.Delete`, and no-Foods permissions.
- Active/inactive categories and foods with multiple variants, prices, and availability states.

## Build and schema checks

```powershell
dotnet build Business-api/Business-api.slnx
npm --prefix Business-client run lint
npm --prefix Business-client run build
```

Verify that food persistence targets `restaurant_foods`, variants target
`restaurant_food_variants`, and price histories target `restaurant_food_price_histories`.
Inspect any new migration before applying it; committed migrations must remain unchanged and no
parallel Product table may be introduced. Use [data-model.md](data-model.md) for invariants.

## Run locally

```powershell
dotnet run --project Business-api/src/Business.Api
npm --prefix Business-client run dev
```

Use the canonical browser route `/restaurant/foods`. Verify that `/product` and
`/restaurant/products` redirect to it without creating duplicate navigation entries.
Use [contracts/foods.openapi.yaml](contracts/foods.openapi.yaml) for request/response validation.

## Validation scenarios

1. **Authorization**: A read user sees the Foods menu and can list/detail. Restricted actions are
   hidden without their matching permission and direct unauthorized requests are rejected.
2. **List/search/filter/page**: Search by partial code/name, filter by category, active state, and
   availability, then verify page reset and total count.
3. **Create/update**: Create a food in an active category, reject duplicate codes and inactive
   categories, then update mutable presentation data without changing its code.
4. **Variants/default**: Create multiple variants and verify exactly one active default variant.
5. **Price history**: Change a price and verify continuous, non-overlapping history while existing
   transaction snapshots remain unchanged.
6. **Availability**: Mark a variant unavailable with a reason and verify it cannot be selected for
   a new transaction; restore availability and verify it can be selected again.
7. **Concurrency**: Submit stale updates and concurrent default-variant changes; verify conflicts
   are reported without silent overwrite or partial state.
8. **Operational states**: Verify loading, empty, retryable error, saving, success, expired session,
   revoked permission, and duplicate-submit prevention.
9. **Concurrent initial load**: Open `/restaurant/foods` in development diagnostics and verify that
   concurrent initial list loads with identical search, filter, page, and page-size values produce
   one network request. Change any condition and verify that a distinct request is made.

## Test-infrastructure note

Record automated coverage when available. Otherwise record the manual scenarios completed and do
not claim automated coverage.
