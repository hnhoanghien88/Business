# Implementation Plan: Restaurant Ordering

**Branch**: `[005-restaurant-ordering]` | **Date**: 2026-09-09 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/005-restaurant-ordering/spec.md`

**Note**: This template is filled in by the `$speckit-plan` command; its definition describes the execution workflow.

## Summary

Add a secured dine-in ordering slice for open table sessions. Dapper supplies current menu/session
projections; one EF transaction revalidates and snapshots the order plus kitchen ticket exactly once
using a stable request ID. React provides a responsive, session-isolated cart and coalesced live reads.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C#/.NET 10; JavaScript ES modules/React 19

**Primary Dependencies**: ASP.NET Core, MediatR, FluentValidation, EF Core, Dapper, MySQL, React, Vite, Material UI, oxlint

**Storage**: Existing restaurant tables plus an additive order request-id column/index

**Testing**: Backend build, frontend lint/build and quickstart scenarios; no automated test project exists

**Target Platform**: ASP.NET Core API and modern browsers

**Project Type**: Full-stack web application

**Performance Goals**: Five-item order under 90 seconds; availability visible within 3 seconds

**Constraints**: Server-authoritative totals; atomic/retry-safe commit; integer MVP quantities; one promotion

**Scale/Scope**: One ordering screen, menu/history reads, promotion preview and confirmation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

*GATE: Passed before research and re-checked after design.*

- Layering/CQRS: PASS — application abstractions, Dapper reads, EF writes and thin API.
- Security/contracts: PASS — `Ordering.*` policies protect every operation.
- Validation/transactions: PASS — commit re-reads authority data and writes one transaction.
- Observability/safety: PASS — cancellation and existing Problem Details/logging remain.
- List coalescing: PASS — normalized conditions share only an in-flight Promise removed in `finally`.

No exception is required; the additive migration has a safe down operation.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file ($speckit-plan command output)
├── research.md          # Phase 0 output ($speckit-plan command)
├── data-model.md        # Phase 1 output ($speckit-plan command)
├── quickstart.md        # Phase 1 output ($speckit-plan command)
├── contracts/           # Phase 1 output ($speckit-plan command)
└── tasks.md             # Phase 2 output ($speckit-tasks command - NOT created by $speckit-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
specs/005-restaurant-ordering/{plan,research,data-model,quickstart,tasks}.md
specs/005-restaurant-ordering/contracts/ordering.openapi.yaml
Business-api/src/Business.Application/Restaurant/Ordering/
Business-api/src/Business.Infrastructure/Persistence/{DapperOrderingReadRepository,MySqlOrderingRepository}.cs
Business-api/src/Business.Api/Controllers/Restaurant/OrderingController.cs
Business-client/src/features/restaurant/ordering/
```

**Structure Decision**: A vertical slice reuses existing restaurant entities and the shared context
while preserving application, infrastructure, API and client boundaries.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
No constitutional violations.
