# Architectural Decisions & Engineering Rationale

This document records the architectural decisions, design trade-offs, and engineering rationale adopted during the development of the **Takwene Track Management API**.

---

## 1. Clean Architecture via Four Distinct Projects

* **Decision**: Decompose the backend into four isolated class libraries (`TakweneTask.Domain`, `TakweneTask.Application`, `TakweneTask.Infrastructure`, `TakweneTask.Api`) plus a dedicated test suite (`TakweneTask.Tests`).
* **Rationale**: Single-project folder-based architectures rely on discipline or external architecture testing harnesses (e.g. NetArchTest) to maintain boundaries. By using separate project files, the compiler strictly enforces the dependency inversion principle: `TakweneTask.Domain` has zero package or project references, preventing accidental leaks of ORM or presentation primitives into core business rules.
* **Trade-Off**: Slightly more csproj maintenance overhead in exchange for ironclad, compile-time architectural integrity.

---

## 2. Domain Lifecycle Governed by the State Pattern

* **Decision**: Implement the State Pattern in `TakweneTask.Domain` (`ITrackState`, `DraftTrackState`, `SubmittedTrackState`, `DistributedTrackState`) rather than embedding status `switch` statements across application services.
* **Rationale**: The lifecycle of a music track (`Draft` -> `Submitted` -> `Distributed`) has distinct operational rules:
  - Tracks in `Draft` cannot be distributed without at least one DSP, and cannot directly update status.
  - Tracks in `Submitted` can add additional DSPs or retry rejected DSPs, but cannot advance to `Distributed` unless at least one distribution is confirmed `Live`.
  - Tracks in `Distributed` represent a terminal state where status cannot be reversed.
  Encapsulating these rules inside concrete state objects isolates domain invariants, eliminates code duplication, and makes testing state transitions straightforward and exhaustive.

---

## 3. Persistence Mapping Strategy for the State Pattern

* **Decision**: The `Track` entity maintains an internal `TrackStatus Status` property mapped directly to a PostgreSQL string column (`draft`, `submitted`, `distributed`). An internal `ITrackState _state` instance is lazily initialized via `TrackStateFactory.Create(this, Status)`.
* **Rationale**: Avoids the significant complexity and impedance mismatch of Table-per-Hierarchy (TPH) inheritance or EF Core shadow state. Queries remain performant and readable in PostgreSQL, while the in-memory domain aggregate preserves full object-oriented polymorphism.

---

## 4. Fixed Version 7 UUIDs for Deterministic Seeding

* **Decision**: Utilize Version 7 UUIDs (`Guid.CreateVersion7()`) with fixed deterministic constants for database seeding in `HasData`.
* **Rationale**: Standard sequential integer primary keys cause primary key collisions (`duplicate key value violates unique constraint`) when seeded via EF Core's `HasData`, because PostgreSQL sequence counters are not advanced during raw `INSERT` migrations. UUIDv7 guarantees chronological ordering, index clustering, and eliminates sequence desynchronization entirely.

---

## 5. AutoMapper with Compile-Time and Test Assertions

* **Decision**: Centralize object-to-object projections in `TakweneTask.Application.Mappings.MappingProfile`, verified by an xUnit unit test calling `mapper.ConfigurationProvider.AssertConfigurationIsValid()`.
* **Rationale**: Manual mapping code is repetitive and prone to missed properties when contracts evolve. AutoMapper standardizes projections, while the automated configuration assertion test guarantees that every source and destination property is accounted for at build and test time, preventing silent runtime reflection failures.

---

## 6. Asynchronous FluentValidation Action Filter

* **Decision**: Implement a custom `FluentValidationActionFilter` executing `ValidateAsync` on each action argument before controller execution, avoiding the deprecated `FluentValidation.AspNetCore` auto-validation package.
* **Rationale**: Upstream maintainers deprecated `FluentValidation.AspNetCore` because synchronous action filters freeze threads on asynchronous database checks (e.g. `MustAsync` verifying that an `ArtistId` or `DspId` exists in the database). Our custom async filter resolves `IValidator<T>` dynamically and safely executes asynchronous validation rules.

---

## 7. RFC 9457 ProblemDetails with Trace Correlation

* **Decision**: Implement `IExceptionHandler` in `GlobalExceptionHandler` mapping domain, application, and database exceptions to standardized RFC 9457 `ProblemDetails` payloads containing embedded `traceId` values.
* **Rationale**: Clients and API consumers require deterministic error formats. Grouping validation errors, domain rule violations (409 Conflict), missing entities (404 Not Found), and uniqueness conflicts (409 Conflict) under standard `application/problem+json` envelopes ensures reliable client integration. Embedding `traceId` allows immediate correlation between client-received errors and Serilog log streams.

---

## 8. Timing-Attack Resistant Authentication

* **Decision**: Verify demo authentication credentials using `CryptographicOperations.FixedTimeEquals` across UTF-8 byte representations, validate JWT signing key length (>= 32 bytes) at startup, and set `MapInboundClaims = false`.
* **Rationale**: Standard string equality operators (`==`) terminate early upon finding the first mismatched character, opening APIs to timing side-channel attacks. Constant-time byte comparisons prevent timing analysis. Setting `MapInboundClaims = false` ensures claims like `sub` and `name` are not remapped to legacy SOAP XML schema URIs.

---

## 9. Structured Request Observability with Serilog & OpenTelemetry

* **Decision**: Configure Serilog to emit a single structured HTTP log line per request containing HTTP method, route, status code, elapsed milliseconds, authenticated identity, and W3C trace ID. OpenTelemetry is wired for distributed tracing and runtime metrics with an optional OTLP exporter.
* **Rationale**: High-volume APIs produce cluttered log streams if individual controller actions log repeatedly. Single-line summaries provide high signal-to-noise ratio for production monitoring, while OpenTelemetry enables standards-compliant telemetry export to Prometheus, Jaeger, or Datadog without code changes.

---

## 10. Dedicated Reviewer Simulation Endpoint

* **Decision**: Expose `PATCH /api/tracks/{id}/distributions/{dspId}` requiring JWT authentication to simulate external DSP callbacks (`live` or `rejected`).
* **Rationale**: In a local evaluation environment without live external DSP webhooks (Spotify, Apple Music, YouTube), submitted tracks would be permanently trapped in the `submitted` state, preventing evaluators from testing the final transition to `distributed`. This explicit test endpoint closes the testing loop cleanly.

---

## 11. Result Pattern Adoption (Railway Oriented Programming)

* **Decision**: Adopt the Result Pattern (`Result<T>`) across the Application and API presentation layers to eliminate exceptions as control flow.
* **Rationale**: Using exceptions (`NotFoundException`, `DomainException`, `UnauthorizedAccessException`) for expected business outcomes introduces expensive stack-unwinding overhead and violates the Open-Closed Principle in global exception middleware. With `Result<T>`, use-case services communicate expected outcomes (Success, NotFound, Conflict, Unauthorized) deterministically, while `GlobalExceptionHandler` is strictly preserved for genuine unhandled infrastructure crashes (500s) and database concurrency collisions.

---

## 12. Unit of Work & Repository Pattern with Centralized Pagination and Transactions

* **Decision**: Decouple application use-case services (`ArtistService`, `TrackService`) and async validators from Entity Framework Core's `DbContext` by introducing `IUnitOfWork` and repository abstractions (`IArtistRepository`, `ITrackRepository`, `IDspRepository`, and generic `IRepository<T>`). The `UnitOfWork` centralizes:
  - Atomic persistence (`SaveChangesAsync`).
  - Pagination execution (`PaginateAsync<T>`), computing count, total pages, and applying offset/limit queries.
  - Transaction orchestration (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`, `ExecuteInTransactionAsync`).
  - Resilient execution strategies (`ExecuteStrategyAsync`) for transient connection resilience.
  - Entity change-tracker inspection and detachment (`ClearChangeTracker`, `HasChanges`, `Detach`).
* **Rationale**: Direct injection of `DbContext` or `IAppDbContext` into services violates Clean Architecture's dependency isolation by coupling application orchestration to ORM querying primitives and change tracking. Delegating transactions, pagination, and persistence to `IUnitOfWork` ensures consistent transaction boundaries across aggregates and provides a clean mockable surface for unit tests.
* **Compatibility Strategy**: Endpoints support optional `pageNumber` and `pageSize` query parameters. When omitted, endpoints preserve complete backward compatibility returning flat collections; when provided, endpoints return standardized `PagedResult<T>` envelopes containing full pagination metadata.

---

## 13. AI Tooling & Pairing Disclosure

* **Tools Used**: Google Antigravity (Advanced Agentic Coding Assistant).
* **Scope of Usage**: Interactive architectural review, scaffold generation, unit test creation, documentation drafting, and commit staging.
* **Human Oversight**: Architectural boundaries, State Pattern behavioral invariants, security constraints, and database relational models were authored, verified, and audited by Mohamed Ayoub.
