# Research: Restaurant Categories

## Hierarchy storage and traversal

**Decision**: Retain the existing `ParentId` adjacency list and use MySQL 8 recursive CTEs for
ancestor/descendant traversal. Writes use EF Core; read projections use Dapper.

**Rationale**: The schema already uses this model. Recursive CTEs support arbitrary depth, ancestor
context, effective status, and cycle checks without a second hierarchy source.

**Alternatives considered**: Closure tables add transactional maintenance; materialized paths make
moves expensive; loading all 10,000 nodes into the API weakens paging and predictable performance.

## Search, filtering, context, and paging

**Decision**: Match nodes case-insensitively by code/name and status, compute ancestor paths with a
recursive CTE, and page matching nodes rather than context ancestors. Return flat ordered items with
`depth`, `ancestorPath`, `hasChildren`, and effective status. Unfiltered browsing uses deterministic
pre-order; the client controls branch expansion locally.

**Rationale**: Paging matches gives a stable total and prevents context rows consuming page slots.
Path metadata preserves context without requiring a full-tree download. Ties sort by display order,
name, then code.

**Alternatives considered**: Nested JSON makes paging ambiguous; paging every flattened row can
separate matches from context; client-only search requires the full hierarchy.

## Optimistic concurrency

**Decision**: Treat microsecond `UpdatedDate` as an EF concurrency token and expose it as ISO-8601
`version`. Updates echo the version; mismatches return `409 Conflict` with reload guidance.

**Rationale**: The field already exists and MySQL `CURRENT_TIMESTAMP(6)` provides declared precision.

**Alternatives considered**: An integer revision requires another field; ETags do not remove the
persistence token; last-write-wins violates the specification.

## Case-insensitive uniqueness and normalization

**Decision**: Trim code/name, preserve code casing, and enforce uniqueness with an explicit
case-insensitive MySQL collation plus an application pre-check and database-conflict translation.

**Rationale**: The database closes races while validation gives friendly errors.

**Alternatives considered**: Uppercasing changes display; application-only checks race; a normalized
column duplicates data when collation can enforce equality.

## Safe moves and activation

**Decision**: In one EF transaction, read the target and parent, reject self/descendant parents, then
save with concurrency checking. `IsActive` is local; `isEffectivelyActive` is local state AND every
ancestor state. Deactivation never rewrites descendants or foods.

**Rationale**: This preserves the subtree, prevents partial cycles, and lets reactivation restore
only descendants whose own state remains active.

**Alternatives considered**: Cascading false loses prior state; persisted effective state can drift;
triggers hide business behavior outside the application pattern.

## Authorization and UI integration

**Decision**: Use `Categories.ViewMenu`, `Categories.Read`, `Categories.Create`, and
`Categories.Update` under application `restaurant`. Discover the menu by code `categories` or route
`/restaurant/categories`; hide unauthorized UI actions and enforce every policy again in the API.

**Rationale**: These strings match the specification and dynamic Identity provider. Revocation takes
effect on the next request.

**Alternatives considered**: Foods permissions conflate resources; UI-only checks are insecure;
Business-side permission caching violates the constitution.

## Verification strategy

**Decision**: Require backend/frontend builds, frontend lint, migration inspection, and manual
contract scenarios. Add focused integration tests if a test project is created; otherwise record the
repository's missing test infrastructure.

**Rationale**: This is proportional to hierarchy, authorization, migration, and concurrency risk.

**Alternatives considered**: Build-only checks miss recursive-query behavior and races; mandating a
broad test platform would expand this feature's scope.
