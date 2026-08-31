# Quickstart Validation: Restaurant Categories

## Prerequisites

- .NET 10 SDK, Node.js, MySQL, and configured Identity/Business application `restaurant`.
- Test users with read-only, create, update, and no-category permissions.
- A three-level tree with active/inactive nodes, equal display orders, and foods on multiple levels.

## Build and schema checks

```powershell
dotnet build Business-api/Business-api.slnx
npm --prefix Business-client run lint
npm --prefix Business-client run build
dotnet ef migrations list `
  --project Business-api/src/Business.Infrastructure `
  --startup-project Business-api/src/Business.Api
```

Inspect the new migration before applying it. It must not drop category/food data or edit a committed
migration. Apply it to a disposable database and seed the hierarchy above.

## Run locally

```powershell
dotnet run --project Business-api/src/Business.Api
npm --prefix Business-client run dev
```

Use the contract in [contracts/categories.openapi.yaml](contracts/categories.openapi.yaml) and the
browser route `/restaurant/categories`.

## Validation scenarios

1. **Authorization**: A read user sees the Categories menu and can list/detail. Create/update actions
   are hidden without permission. Direct unauthorized requests return 403; missing view/read access
   prevents screen/data access.
2. **Tree/order**: Verify three levels, stable sibling order (`displayOrder`, name, code), and that
   expanding one branch preserves other branch states.
3. **Search/filter/page**: Search partial mixed-case code and Vietnamese names; matches include their
   ancestor path. Verify status filters and page-one reset after search/filter/page-size changes.
4. **Create**: Create a root and child; verify trimming, parent, order, duplicate-submit prevention,
   and errors for blank, overlong, and case-only duplicate codes.
5. **Update/move**: Code is read-only. Rename/reorder a node and move its subtree to a valid active
   parent; descendant relationships remain intact.
6. **Cycle safety**: Select self and a deep descendant as parent through UI and direct API; both fail
   without partial writes.
7. **Concurrency**: Open one category twice, save once, then submit the stale form. Expect 409, kept
   form input, and reload/retry guidance.
8. **Activation**: Deactivate a parent after impact confirmation. Descendant local states stay intact
   while the branch becomes effectively inactive. Reactivation restores only locally active items.
9. **Operational states**: Verify loading, empty, retryable error, saving, success, expired session,
   revoked permission, and one in-flight request despite repeated save clicks.
10. **Accessibility/responsiveness**: Use keyboard navigation, visible focus, meaningful labels, an
    expanded/collapsed sidebar, and a narrow drawer layout.

## Performance evidence

Seed 10,000 varied-depth categories. Capture API timings and MySQL `EXPLAIN` for list, partial search,
status filter, and hierarchy traversal. Record whether normal-load interaction meets two seconds.

## Test-infrastructure note

The repository currently has no automated test project. If implementation does not add one, record
that limitation and completed manual scenarios in the handoff; do not claim automated coverage.
