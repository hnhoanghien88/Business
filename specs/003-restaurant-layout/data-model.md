# Data Model: Restaurant Layout

## RestaurantArea

| Field | Type | Rules |
|---|---|---|
| Id | ulong | Generated primary key |
| Code | string(50) | Required, uppercase-normalized, case-insensitive unique, immutable |
| Name | string(150) | Required and trimmed |
| Description | string?(500) | Trimmed or null |
| DisplayOrder | int | Stable ascending order |
| IsActive | bool | True by default; deactivation with tables requires confirmation |
| CreatedBy/UpdatedBy | ulong? | Audit actor |
| CreatedDate/UpdatedDate | datetime(6) | Audit; UpdatedDate is concurrency version |

One area has many tables. Delete is restricted; hard delete is outside scope.

## RestaurantTable

| Field | Type | Rules |
|---|---|---|
| Id | ulong | Generated primary key |
| AreaId | ulong | Active area required on create/move |
| Code | string(50) | Required, uppercase-normalized, case-insensitive unique, immutable |
| Name | string(150) | Required and trimmed |
| Capacity | int | 1..1000 |
| Status | string(30) | Available, Occupied, Reserved, Cleaning, Disabled |
| IsActive | bool | Kept consistent with Disabled configuration transition |
| CreatedBy/UpdatedBy | ulong? | Audit actor |
| CreatedDate/UpdatedDate | datetime(6) | Audit; UpdatedDate is concurrency version |

A table belongs to one area and has many sessions. Deletes are restricted.

## TableSession dependency

Layout configuration only reads sessions to calculate impact and block unsafe moves/disables. It
never closes, cancels, or edits a session. An open session has `Status = Open`.

## Derived projections

- Area: `tableCount`, `openSessionCount`, stable `(DisplayOrder, Name, Code, Id)`, `version`.
- Table: area identity/state, `hasOpenSession`, `canMove`, `canDisable`, `version`.
- Effective availability requires active area, active table and Available status.

## State transitions

```text
Area Active --confirmed if impacted--> Inactive --activate--> Active
Table Available --no open session--> Disabled --active area--> Available
Table Occupied/Cleaning/Reserved: configuration preserves Status
```

Move additionally requires no open session and active destination. Stale version/duplicate/safety
conflicts return 409 with no write; input failures return 400.
