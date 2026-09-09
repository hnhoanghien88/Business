# Quickstart: Restaurant Ordering Validation

## Run

```powershell
dotnet run --project Business-api/src/Business.Api/Business.Api.csproj
Set-Location Business-client
npm run dev
```

Use an occupied table with an open session and `Ordering.Read/Create` permissions.

## Scenarios

1. Open `/restaurant/ordering?sessionId=<id>`; verify active menu data and sold-out reasons.
2. Trigger identical concurrent menu loads: verify one request/shared result. Change a condition for a
   separate request; force failure then verify retry sends a new request.
3. Build carts for two sessions and verify isolation, quantity/note editing and keyboard operation.
4. Change a price before confirmation; verify conflict returns the new preview and acknowledgement retry commits.
5. Retry one successful request ID; verify the same numbers and exactly one order/ticket; cart clears.
6. Close session or sell out a variant before confirm; verify no partial data and cart remains.
7. Place a second order for the session; verify history/totals include both without changing the first.
8. Exercise valid/invalid promotion codes and change conditions before commit; verify revalidation/snapshot.

## Quality gates

```powershell
dotnet build Business-api/Business-api.slnx
Set-Location Business-client
npm run lint
npm run build
```
