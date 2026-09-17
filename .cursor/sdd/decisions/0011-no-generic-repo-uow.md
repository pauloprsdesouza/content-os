# ADR-0011 — No generic repository / UoW / base service

Status: Accepted · Date: 2026-09-17

## Decision

Prohibit `IRepository<T>`, custom `IUnitOfWork`, `BaseService`, `CrudService`, Manager/Utils dumping grounds.

## Consequences

Aggregate-specific repos; purpose-specific queries; EF+Wolverine for commit.
