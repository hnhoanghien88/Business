<!--
Sync Impact Report
- Version change: template (unratified) -> 1.0.0
- Added principles:
  - I. Modular Layered Architecture
  - II. CQRS and Persistence Boundaries
  - III. Secure Contracts and Authorization
  - IV. Validation and Verification
  - V. Observability and Operational Safety
  - VI. Readable Multi-line Code Formatting
- Added sections:
  - Technical Constraints
  - Development Workflow and Quality Gates
- Removed sections: none
- Follow-up TODOs: none
-->

# Business Platform Constitution

## Core Principles

### I. Modular Layered Architecture

Code MUST preserve the dependency direction `Domain <- Application <- Infrastructure/API`.
Business capabilities MUST be grouped by business module and feature. Domain entities, persistence
configuration, tables, and migrations MUST use module-specific namespaces and naming so that new
modules can be added without coupling or naming collisions. Dependencies MUST NOT bypass an
abstraction merely for convenience.

### II. CQRS and Persistence Boundaries

Commands and queries MUST remain explicit application requests with focused handlers. Write-side
operations MUST use Entity Framework Core repositories, while read-side operations SHOULD use
Dapper repositories when the established feature pattern applies. Controllers MUST coordinate HTTP
concerns and dispatch application requests; they MUST NOT contain domain or persistence logic.
Database schema changes MUST be delivered through new migrations, and committed migrations MUST NOT
be rewritten after they may have been applied.

### III. Secure Contracts and Authorization

Protected endpoints MUST validate bearer access tokens and enforce the permission policy appropriate
to the operation. Business MUST treat Identity as the source of truth for current permissions and
MUST NOT cache authorization results in a way that allows revoked or version-stale permissions to
remain effective. Input, error, and API response contracts MUST be explicit and preserve the
project's Problem Details conventions. Secrets and environment-specific credentials MUST NOT be
committed.

### IV. Validation and Verification

Business rules and request validation MUST be enforced before state changes. Every modification MUST
be verified in proportion to its risk: affected projects MUST build cleanly, relevant automated tests
MUST pass when present, and contract, authorization, migration, or distributed-state changes MUST
include targeted integration coverage when a test project is available. When coverage cannot be
added because the repository lacks test infrastructure, the limitation and manual verification MUST
be recorded in the implementation handoff.

### V. Observability and Operational Safety

Request processing MUST preserve correlation IDs, structured logging, and standardized Problem
Details responses. New external calls and performance-sensitive paths MUST support cancellation and
MUST expose failures without leaking tokens, credentials, or sensitive payloads. Distributed rate
limit state MUST use atomic operations. Failure-mode choices, caching behavior, and operational
tradeoffs MUST be explicit in configuration or documentation.

### VI. Readable Multi-line Code Formatting

Generated or modified code MUST use readable multi-line formatting whenever an expression contains
nested structure, child elements, multiple properties, or a non-trivial callback body. JSX parent
and child elements MUST be placed on separate lines with consistent indentation; nested UI trees
MUST NOT be compressed into a single line. Object literals and callbacks returning object literals
with multiple fields MUST place fields on separate lines. Multiple statements MUST NOT be joined on
one line.

For example:

```jsx
<Table>
  <TableHead>
    <TableRow />
  </TableHead>
</Table>
```

## Technical Constraints

- Backend changes MUST remain compatible with .NET 10 and the established ASP.NET Core, MediatR,
  FluentValidation, EF Core, Dapper, MySQL, Serilog, and Redis stack.
- Frontend changes MUST remain compatible with React 19, Vite 8, Material UI 9, Emotion, and oxlint.
- One `BusinessDbContext` MUST serve all modules; separation MUST be maintained through namespaces,
  folders, configurations, and module-prefixed database object names.
- Configuration MUST remain environment-aware. Local-only settings, generated build output, logs,
  and dependency folders MUST remain untracked.
- Simpler designs that satisfy the specification MUST be preferred over speculative abstractions.

## Development Workflow and Quality Gates

Every feature MUST begin with an approved specification that states user outcomes, constraints, and
acceptance criteria before implementation planning. Plans MUST identify affected layers, contracts,
data changes, security effects, and verification. Tasks MUST be dependency ordered and independently
verifiable where practical. Reviews MUST confirm constitutional compliance, readable formatting,
successful builds, relevant test results, migration safety, and updated documentation. Any justified
exception MUST be documented in the plan and called out in the final handoff.

## Governance

This constitution governs specifications, plans, tasks, implementation, and review for the Business
Platform repository. If another project document conflicts with it, this constitution takes
precedence.

Amendments MUST state the proposed change and rationale, update the Sync Impact Report, and receive
explicit project-owner approval. Versioning follows semantic versioning: MAJOR for incompatible
governance changes or principle removals, MINOR for new principles or materially expanded rules, and
PATCH for clarifications that do not change obligations. The amendment date MUST be updated whenever
the constitution changes.

Every implementation plan and code review MUST include a constitution compliance check. Violations
MUST be corrected before completion or documented as an explicitly approved, time-bounded exception
with a remediation path.

**Version**: 1.0.0 | **Ratified**: 2026-08-26 | **Last Amended**: 2026-08-26
