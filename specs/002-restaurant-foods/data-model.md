# Data Model: Restaurant Foods

## Food

Existing table: `restaurant_foods`

| Field | Type | Required | Rules |
|---|---|---:|---|
| `Id` | unsigned 64-bit integer | yes | Generated primary key |
| `CategoryId` | unsigned 64-bit integer | yes | Existing active category for create/move |
| `Code` | string(50) | yes | Trimmed, immutable, case-insensitively unique |
| `Name` | string(200) | yes | Trimmed and nonblank |
| `Description` | text | no | Trimmed; blank becomes null |
| `ImageUrl` | string(1000) | no | Valid approved reference; blank becomes null |
| `DisplayOrder` | 32-bit integer | yes | Stable order; ties use name then code |
| `IsActive` | boolean | yes | Lifecycle state; defaults true |
| `CreatedBy` / `UpdatedBy` | unsigned 64-bit integer | no | Authenticated actor audit fields |
| `CreatedDate` | datetime(6) | yes | Creation timestamp |
| `UpdatedDate` | datetime(6) | yes | Update timestamp and concurrency version |

Derived list fields include category code/name/effective state, active variant count, default
variant, minimum/maximum active price, and effective sellability.

## Food Variant

Existing table: `restaurant_food_variants`

| Field | Type | Required | Rules |
|---|---|---:|---|
| `Id` | unsigned 64-bit integer | yes | Generated primary key |
| `FoodId` | unsigned 64-bit integer | yes | Owning food |
| `Code` | string(50) | yes | Trimmed, immutable, unique within food ignoring case |
| `Name` | string(100) | yes | Trimmed and nonblank |
| `CurrentPrice` | decimal(18,2) | yes | Nonnegative; changed only through price action |
| `IsDefault` | boolean | yes | Exactly one among active variants when any exist |
| `IsAvailable` | boolean | yes | Temporary sellability; defaults true |
| `SoldOutReason` | string(500) | no | Null when available; normalized when unavailable |
| `DisplayOrder` | 32-bit integer | yes | Stable sibling order |
| `IsActive` | boolean | yes | Lifecycle state |
| audit/version fields | timestamps and actor IDs | yes/no | `UpdatedDate` is concurrency version |

## Food Price History

Existing table: `restaurant_food_price_histories`

| Field | Type | Required | Rules |
|---|---|---:|---|
| `Id` | unsigned 64-bit integer | yes | Generated primary key |
| `FoodVariantId` | unsigned 64-bit integer | yes | Owning variant |
| `Price` | decimal(18,2) | yes | Nonnegative |
| `EffectiveFrom` | datetime(6) | yes | Inclusive interval start |
| `EffectiveTo` | datetime(6) | no | Exclusive end; null means current |
| `CreatedBy` | unsigned 64-bit integer | no | Actor changing price |
| `CreatedDate` | datetime(6) | yes | Audit timestamp |

For each variant, intervals do not overlap, at most one row is open, and `CurrentPrice` equals the
open row price. MVP changes are immediate, so a successful change closes and opens rows at one time.

## Relationships

```text
Category    1 ─── 0..* Food
Food        1 ─── 0..* FoodVariant
FoodVariant 1 ─── 0..* FoodPriceHistory
Food/FoodVariant 1 ─── 0..* historical transaction references
```

All catalog foreign keys restrict destructive deletion. This feature deactivates referenced records.

## Indexes and constraints

- Unique `restaurant_foods.Code` using case-insensitive business comparison.
- Food list index begins with category/status/order fields used by filters and stable ordering.
- Unique `(FoodId, Code)` for variants using case-insensitive business comparison.
- Variant availability/order index supports ordering-menu reads.
- Price history index covers `(FoodVariantId, EffectiveFrom, EffectiveTo)`.
- Inspect the existing schema and representative query plans before adding a forward-only migration.
- Where MySQL/model conventions support it cleanly, add protection against multiple active defaults
  and multiple open history rows; application transactions remain mandatory.

## Validation invariants

1. Normalize text before validation and persistence.
2. Food and variant codes are immutable and uniqueness is closed by the database.
3. A food belongs to one valid category; new/moved food cannot target an inactive category.
4. A food with active variants has exactly one active default.
5. Current default cannot be deactivated without replacement unless the food is deactivated.
6. Price is nonnegative and price intervals are continuous and non-overlapping.
7. Available variants have no sold-out reason; unavailable reasons respect normalization/length.
8. Stale versions, constraint races, or validation errors commit no partial changes.
9. Effective sellability requires active category ancestry, active food, active variant, and
   available variant.

## State transitions

```text
Food:    Active ──deactivate──> Inactive ──reactivate──> Active
Variant: Active ──deactivate──> Inactive ──reactivate──> Active
Sale:    Available ──sold out(reason)──> Unavailable ──restock──> Available
Default: Variant A ──select B atomically──> Variant B
Price:   Current P1 ──change atomically──> closed P1 history + open P2 history
```

Deactivation never deletes history or changes transaction snapshots. Reactivation does not bypass
category ancestry, food state, or availability checks.
