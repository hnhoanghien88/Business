# Research: Restaurant Foods

## Canonical naming and migration boundary

**Decision**: Rename the current Product vertical slice in place to Foods. Use `/restaurant/foods`,
`/api/restaurant/foods`, and `Foods.*`; keep only browser redirects for `/product` and
`/restaurant/products`.

**Rationale**: Domain/schema already use Food and `restaurant_foods`. A second Product surface would
create conflicting catalogs and permissions.

**Alternatives considered**: Parallel Product/Foods APIs create drift; keeping Product internally
forever obscures ownership; immediate removal of legacy browser routes breaks saved links.

## Aggregate and write transactions

**Decision**: Treat Food as the aggregate root for presentation data and variants. Execute changes
to default variant and price history in explicit EF transactions with optimistic concurrency.

**Rationale**: Exactly-one-default and continuous price history span multiple rows and must commit
atomically. EF matches the project's write-side boundary.

**Alternatives considered**: Independent row updates permit two defaults or history gaps; database
triggers hide business rules; last-write-wins violates concurrency requirements.

## Read projections, filters, and paging

**Decision**: Use Dapper read projections from `restaurant_foods`, categories, variants, and current
price data. Apply search/category/status/availability filters server-side with deterministic order
by display order, name, then code, and page matches rather than child rows.

**Rationale**: A food appears once regardless of variant count, totals remain stable, and the design
supports the 10,000-food target without loading the catalog into the browser.

**Alternatives considered**: Client filtering downloads excessive data; joining variants without
aggregation duplicates foods; ordering only by name is unstable for ties.

## Default variant invariant

**Decision**: A food with active variants has exactly one active default. Creating the first active
variant makes it default; selecting another clears the prior default atomically; the current default
cannot be deactivated without a replacement unless the food is deactivated.

**Rationale**: Ordering requires deterministic selection while preserving operator intent.

**Alternatives considered**: Multiple defaults are ambiguous; no default pushes inconsistent rules
to consumers; silently choosing the cheapest or first variant surprises operators.

## Price history

**Decision**: Price changes take effect immediately in the MVP. In one transaction, close the open
history row at the change instant, create the new open row, and update `CurrentPrice`. Reject stale
versions and negative prices.

**Rationale**: This preserves a continuous auditable timeline and keeps current-price reads simple.

**Alternatives considered**: Updating only current price loses history; overlapping effective rows
are ambiguous; future scheduling is outside MVP scope.

## Active versus available

**Decision**: `IsActive` is lifecycle configuration; `IsAvailable` is temporary sellability.
Unavailable variants may record a normalized reason. Ordering eligibility also requires active
category ancestry, an active food, and an active variant.

**Rationale**: Temporary sold-out handling must not erase catalog configuration or history.

**Alternatives considered**: One status conflates retirement and sold-out; deleting variants breaks
history; extra free-form states complicate ordering without a current need.

## Authentication and authorization

**Decision**: Use exact case-sensitive application code/audience `Restaurant`; enforce
`Foods.Read`, `Foods.Create`, `Foods.Update`, and `Foods.Delete` at the API and hide corresponding
UI actions. Changing variants, prices, and availability is covered by `Foods.Update`.

**Rationale**: Identity-issued JWT claims use `Restaurant`; exact alignment prevents authentication
failure. Identity remains the source of current permissions for every protected request.

**Alternatives considered**: Case-insensitive token checks conceal configuration errors; UI-only
authorization is insecure; Business-side authorization caching delays revocation.

## In-flight request coalescing

**Decision**: Key list requests by normalized query string and share only the pending Promise.
Remove it after success or failure; do not persist response or authorization caches.

**Rationale**: Concurrent initialization yields one network call while retries and changed filters
remain fresh. Development diagnostics that intentionally repeat effects remain enabled.

**Alternatives considered**: Removing diagnostic mode hides defects; permanent caching risks stale
catalog/permissions; abort/restart still emits duplicate requests.

## Verification strategy

**Decision**: Require backend build, frontend lint/build, migration inspection, OpenAPI validation,
and manual scenarios for default/price/concurrency/authorization. Add focused integration coverage
when compatible test infrastructure is available.

**Rationale**: Cross-row invariants, stale writes, permission boundaries, and schema evolution are
the highest risks; build-only validation cannot prove them.

**Alternatives considered**: UI-only checks miss direct API authorization and races; adding a broad
test platform would exceed planning scope if no project exists.
