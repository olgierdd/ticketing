# Build Plan - Event Ticketing System

## 1. Key assumptions

1. .NET SDK 10 is available locally and is acceptable for this project template.
2. PostgreSQL is the runtime database for the application (no in-memory runtime database).
3. API versioning is represented in route prefix (`/api/v1/...`) and OpenAPI document version (`v1`).
4. `PATCH` support is optional by requirement and may be deferred while ensuring full `POST/GET/PUT/DELETE` coverage.
5. Integration tests use a real containerized PostgreSQL instance (Testcontainers) for deterministic database behavior.
6. Local startup applies EF migrations automatically; CI can still run migrations explicitly.

## 2. Proposed folder structure

```text
ticketing-system/
  Ticketing.slnx
  docker-compose.yml
  .dockerignore
  README.md
  dotnet-tools.json

  Ticketing.Api/
    Controllers/
      EventsController.cs
      TicketsController.cs
      ReportsController.cs

    Contracts/
      Common/
      Events/
      Tickets/
      Reports/

    Domain/
      Entities/

    Services/
      Interfaces/
      Implementations/
      Mapping/

    Persistence/
      ApplicationDbContext.cs
      Seed/
      Migrations/

    Validation/
    Infrastructure/
      Exceptions/
      ExceptionHandling/

    Program.cs
    appsettings.json
    appsettings.Development.json
    Dockerfile

  Ticketing.Api.Tests/
    Infrastructure/
    Unit/
    EventManagementTests.cs
    TicketManagementTests.cs
    ReportingTests.cs
```

## 3. Domain and database design

### Entities

- `Event`
  - `Id`, `Name`, `Description`, `Venue`, `EventDate`, `StartTime`
  - `TotalTicketCapacity`, `CreatedAtUtc`, `UpdatedAtUtc`
  - Collection: `PricingTiers`, `Purchases`

- `PricingTier`
  - `Id`, `EventId`, `Name`, `Price`, `Capacity`, `TicketsSold`

- `TicketPurchase`
  - `Id`, `EventId`, `PricingTierId`
  - `CustomerName`, `CustomerEmail`, `Quantity`
  - `UnitPrice` (captured at purchase time), `TotalPrice`
  - `PurchaseDateUtc`, `Status`, `BookingReference`

### Key EF Core constraints/configuration

- PK/FK relationships between `Event`, `PricingTier`, `TicketPurchase`.
- Required fields and max lengths for human-entered text.
- Decimal precision for prices/revenue.
- Unique index on `BookingReference`.
- Indexes for common query paths (`EventDate`, purchase lookups).
- Delete restrictions to protect historical purchase data.
- Concurrency tokens for entities (`xmin`) and explicit locking strategy for purchases.

### Core validation/business rules

- Event name and venue required.
- Event capacity > 0.
- Tier name required, tier capacity > 0, tier price >= 0.
- Sum of tier capacities <= event capacity.
- `TicketsSold <= Capacity` for each tier.
- Event capacity cannot be reduced below sold tickets.
- Tier capacity cannot be reduced below sold tickets.

## 4. Endpoint catalogue

### Events API (`/api/v1/events`)

- `POST /api/v1/events`
- `GET /api/v1/events?pageNumber=&pageSize=`
- `GET /api/v1/events/{eventId}`
- `PUT /api/v1/events/{eventId}`
- `DELETE /api/v1/events/{eventId}`
- `PATCH /api/v1/events/{eventId}` (optional)

### Ticket Management API (`/api/v1/tickets`)

- `GET /api/v1/tickets/availability/{eventId}`
- `POST /api/v1/tickets/purchases`
- `GET /api/v1/tickets/purchases/{purchaseId}`
- `GET /api/v1/tickets/purchases/reference/{bookingReference}`

### Reporting API (`/api/v1/reports`)

- `GET /api/v1/reports/events/{eventId}/sales-summary`
- `GET /api/v1/reports/events/sales-summary`

## 5. Overselling-prevention strategy

Selected approach: **database transaction + PostgreSQL row locking**.

Flow:

1. Start transaction with repeatable-read isolation.
2. Lock the event row using `SELECT ... FOR UPDATE`.
3. Validate event timing, tier ownership, tier availability, and event total capacity.
4. Update inventory and insert purchase in the same transaction.
5. Commit once both updates succeed.

Why this approach:

- Prevents race conditions under concurrent purchases.
- Keeps inventory and purchase creation atomic.
- Uses native PostgreSQL behavior and is fully supported by EF Core via SQL queries and tracked entities.

## 6. Implementation phases

### Phase A - Foundation

- Create solution with exactly two projects.
- Add required packages (EF Core, Npgsql, Swagger, FluentValidation, NUnit, Moq, Testcontainers).
- Configure startup pipeline: controllers, DI, validation, ProblemDetails, exception handling, Swagger.

### Phase B - Domain and persistence

- Implement entities and DTO contracts.
- Configure `ApplicationDbContext` and EF model rules.
- Generate and commit initial migration.
- Add development seeding for 3 sample events with 3 tiers each.

### Phase C - Business services and controllers

- Implement event lifecycle service.
- Implement purchase service with concurrency-safe transactional logic.
- Implement reporting service with aggregation.
- Keep controllers thin and asynchronous with cancellation tokens.

### Phase D - Quality, docs, and operations

- Implement centralized exception mapping with safe errors.
- Improve Swagger docs (summaries, status codes, XML docs).
- Add Dockerfile + docker-compose (health checks, volume, network, env-based connection).
- Write README with run/test/migration examples and overselling explanation.

### Phase E - Testing

- Unit tests (Moq) for controller/service contracts.
- Integration tests for all event/ticket/report scenarios.
- Concurrency integration test to ensure no overselling.
- Ensure test isolation via fresh database per test fixture.

## 7. Verification checklist

- [ ] Solution builds successfully.
- [ ] API starts and migrations apply.
- [ ] Seed data appears only when database is empty.
- [ ] Swagger UI is reachable in development.
- [ ] All listed routes are implemented and return expected status codes.
- [ ] Purchase flow rejects invalid tiers and over-capacity purchases.
- [ ] Concurrency test demonstrates one success / one conflict at boundary.
- [ ] Reporting totals match sold quantities and revenues.
- [ ] `docker compose up --build` starts DB then API with health dependency.
- [ ] `dotnet test` runs unit and integration suites.

