# Data Model: Restaurant Categories

## Category

Existing table: `restaurant_categories`

| Field | Type | Required | Rules |
|---|---|---:|---|
| `Id` | unsigned 64-bit integer | yes | Generated primary key and stable tree identity |
| `ParentId` | unsigned 64-bit integer | no | Null means root; cannot be self or a descendant |
| `Code` | string(50) | yes | Trimmed, nonblank, immutable, case-insensitively unique |
| `Name` | string(150) | yes | Trimmed and nonblank |
| `Description` | string(500) | no | Trimmed; blank becomes null |
| `DisplayOrder` | 32-bit integer | yes | Sibling order; ties sort by name then code |
| `IsActive` | boolean | yes | Node-local state; defaults true |
| `CreatedBy` / `UpdatedBy` | unsigned 64-bit integer | no | Authenticated subject audit fields |
| `CreatedDate` | datetime(6) | yes | Database creation time |
| `UpdatedDate` | datetime(6) | yes | Update time and optimistic-concurrency token |

### Derived read fields

- `depth`: edges from root.
- `ancestorPath`: ordered ancestors (`id`, `code`, `name`) from root to parent.
- `hasChildren`: whether a direct child exists.
- `directFoodCount` and `descendantCount`: deactivation impact.
- `isEffectivelyActive`: node and every ancestor are active.
- `version`: ISO-8601 `UpdatedDate` used as update precondition.

## Relationships

```text
Category (parent) 1 ─── 0..* Category (children)
Category          1 ─── 0..* Food
Category          1 ─── 0..* PromotionCategory
```

- Existing foreign keys restrict deletion; this feature exposes no hard delete.
- Moving changes only `ParentId`; descendant links remain unchanged.
- A category may contain both direct foods and child categories.
- Promotion coverage of descendant foods consumes this hierarchy but belongs to its own feature.

## Indexes and constraints

- Primary key on `Id` and unique case-insensitive `uk_categories_code` on `Code`.
- Parent traversal/order index begins with `ParentId`, then `DisplayOrder`, `Name`, and `Code`.
- Inspect representative `EXPLAIN` plans before adding any status/search index in a new migration.
- Self-reference `ParentId -> Id` uses restrict semantics.

## Validation invariants

1. Normalize code, name, and description before validation and persistence.
2. Code/name cannot be blank and all text respects maximum lengths.
3. A new category is a root or has an existing active parent.
4. Code is immutable and comparisons ignore case.
5. An updated parent exists and is neither the node nor a descendant.
6. Update `version` is required and matches the persisted token.
7. Validation, uniqueness, cycle, or concurrency failure writes nothing.
8. Effective sale eligibility is derived; status changes do not mutate descendants or foods.

## State transitions

```text
Create(active/inactive) ──> Active or Inactive
Active   ──deactivate──> Inactive
Inactive ──reactivate──> Active
Any state ──move───────> Same local state under a valid parent
```

Deactivation requires UI confirmation with impact counts. Reactivation preserves descendant/food
local state. A locally active node under an inactive ancestor remains effectively inactive. A stale
transition returns conflict and leaves current data unchanged.
