# 1. Solution structure for Event Ticketing System

Create one solution containing exactly two projects:

Ticketing.Api
- ASP.NET Core Web API application
- Controllers, business services, Entity Framework Core persistence, validation, exception handling, configuration, and Swagger/OpenAPI documentation

Ticketing.Api.Tests
- Automated unit and integration tests for the Web API
- Use NUnit as the testing framework
- Follow Arrange, Act, and Assert structure. Tests must be deterministic and independent.
- Use Moq for mocking dependencies
- Include [TestFixture], [SetUp], and [TearDown] attributes
- Use a real containerized test database where practical, or clearly document the selected testing approach

Do not create unnecessary additional projects.

# 2. Technology requirements

Use:

- ASP.NET Core Web API with controllers
- Entity Framework Core
- PostgreSQL
- Npgsql.EntityFrameworkCore.PostgreSQL
- Swagger/OpenAPI
- Docker
- Docker Compose
- NUnit
- FluentValidation or ASP.NET Core validation
- ProblemDetails for standardized API errors
- Async database and API operations
- Dependency injection
- Structured logging

Do not use an in-memory database for the running application.

# 3. Domain model

## Event

Each event must contain:

- Id
- Name
- Description
- Venue
- EventDate
- StartTime
- TotalTicketCapacity
- CreatedAt
- UpdatedAt
- Collection of pricing tiers

Validate that:

- Name is required
- Venue is required
- EventDate is valid
- TotalTicketCapacity is greater than zero
- The combined ticket inventory does not exceed the event capacity
- An event with purchased tickets cannot have its capacity reduced below the number of sold tickets

## PricingTier

Each pricing tier must contain:

- Id
- EventId
- Name, for example, Standard, Premium or VIP
- Price
- Capacity
- TicketsSold

Validate that:

- Name is required
- Price cannot be negative
- Capacity must be greater than zero
- TicketsSold cannot exceed Capacity

## TicketPurchase

Each purchase must contain:

- Id
- EventId
- PricingTierId
- CustomerName
- CustomerEmail
- Quantity
- UnitPrice
- TotalPrice
- PurchaseDate
- Status
- Unique booking reference

The price stored with the purchase must represent the price at the time of purchase.

# 4. API controllers

Implement three separate controllers representing three API areas.

## Events API

Base route:

/api/v1/events

Endpoints:

- POST /api/v1/events
  Create an event with its pricing tiers.

- GET /api/v1/events
  Retrieve all events. Support pagination.

- GET /api/v1/events/{eventId}
  Retrieve a single event and its pricing tiers.

- PUT /api/v1/events/{eventId}
  Replace an existing event.

- PATCH /api/v1/events/{eventId}
  Optionally support partial updates.

- DELETE /api/v1/events/{eventId}
  Delete an event only when business rules permit deletion.

Use appropriate HTTP status codes, including:

- 200 OK
- 201 Created
- 204 No Content
- 400 Bad Request
- 404 Not Found
- 409 Conflict

## Ticket Management API

Base route:

/api/v1/tickets

Endpoints:

- GET /api/v1/tickets/availability/{eventId}
  Return event capacity, total tickets sold, total tickets available and availability by pricing tier.

- POST /api/v1/tickets/purchases
  Purchase one or more tickets for a selected event and pricing tier.

- GET /api/v1/tickets/purchases/{purchaseId}
  Retrieve purchase details.

- GET /api/v1/tickets/purchases/reference/{bookingReference}
  Retrieve a purchase using its booking reference.

Ticket purchasing must:

- Verify that the event exists
- Verify that the pricing tier belongs to the event
- Verify that the event has not already occurred
- Verify sufficient event and pricing-tier capacity
- Calculate the total price on the server
- Execute inventory updates and purchase creation in one database transaction
- Prevent race conditions and overselling during concurrent purchases
- Return 409 Conflict when inventory is no longer available
- Never trust the price or total amount supplied by the client

Use an appropriate concurrency strategy supported by Entity Framework Core and PostgreSQL,
such as optimistic concurrency, atomic conditional updates or database-level row locking.
Explain the selected approach.

## Reporting API

Base route:

/api/v1/reports

Endpoints:

- GET /api/v1/reports/events/{eventId}/sales-summary
  Generate a sales summary for one event.

- GET /api/v1/reports/events/sales-summary
  Generate sales summaries for all events.

Each report should contain:

- Event ID
- Event name
- Total capacity
- Total tickets sold
- Remaining tickets
- Total revenue
- Sales and revenue grouped by pricing tier

Perform reporting aggregation efficiently in the database using Entity Framework Core queries.

# 5. DTOs and API contracts

Do not expose Entity Framework entities directly from controllers.

Create separate request and response DTOs, including:

- CreateEventRequest
- UpdateEventRequest
- EventResponse
- PricingTierRequest
- PricingTierResponse
- PurchaseTicketsRequest
- TicketPurchaseResponse
- TicketAvailabilityResponse
- EventSalesSummaryResponse

Map DTOs explicitly or with a lightweight mapping solution.

# 6. Error handling and validation

Implement centralized exception handling.

Handle at least:

- Validation errors
- Resource not found
- Insufficient ticket availability
- Concurrency conflicts
- Invalid event or pricing-tier relationships
- Database errors
- Unexpected server errors

Errors must not expose stack traces, database credentials or internal implementation details.

Validation errors should identify invalid fields and provide understandable messages.

# 7. Swagger and OpenAPI

Expose every public endpoint through Swagger/OpenAPI.

Swagger documentation must include:

- Endpoint summaries and descriptions
- Request and response schemas
- Expected HTTP status codes
- Validation responses
- Example requests where practical
- API version information
- XML documentation comments

Group endpoints using the Events, Tickets and Reports tags.

Swagger UI must be available in the development environment.

# 8. Entity Framework Core

Create an ApplicationDbContext with suitable DbSet properties.

Configure:

- Primary and foreign keys
- Required fields
- Maximum string lengths
- Decimal precision for prices and revenue
- Unique booking-reference constraint
- Useful database indexes
- Delete behaviours
- Concurrency handling

Generate an initial Entity Framework Core migration.

Apply migrations automatically during local Docker startup, or provide a clearly documented migration command.

Seed three sample events with a concert name and three pricing tiers for development if the database is empty.

# 9. Docker and Docker Compose

Create:

- A multi-stage Dockerfile for Ticketing.Api
- A docker-compose.yml file
- An optional .dockerignore file

Docker Compose must orchestrate:

- ticketing-api
- PostgreSQL database

Requirements:

- Configure connection strings through environment variables
- Do not store real credentials in source code
- Persist PostgreSQL data using a Docker volume
- Add a PostgreSQL health check
- Make the API wait for a healthy database
- Expose the API and Swagger UI through documented local ports
- Configure restart behaviour appropriately
- Put the services on a dedicated Docker network

The complete application must start using:

docker compose up --build

# 10. Automated testing

Implement tests for:

## Event management

- Creating a valid event
- Rejecting invalid event input
- Retrieving an existing event
- Returning 404 for a missing event
- Updating an event
- Deleting an event
- Preventing invalid capacity changes

## Ticket management

- Purchasing available tickets
- Calculating the correct total price
- Rejecting an invalid pricing tier
- Rejecting purchases exceeding tier capacity
- Rejecting purchases exceeding event capacity
- Preventing overselling during concurrent requests
- Returning the correct ticket availability
- Generating a unique booking reference

## Reporting

- Correct number of tickets sold
- Correct remaining capacity
- Correct revenue calculation
- Correct grouping by pricing tier

# 11. Code-quality requirements

- Use cancellation tokens
- Use async/await consistently
- Keep controllers thin
- Put business rules in services
- Use meaningful names
- Avoid generic repository abstractions over Entity Framework Core unless they add clear value
- Do not use static global state
- Do not silently catch exceptions
- Do not return database entities directly
- Use UTC for persisted timestamps
- Include nullable reference types
- Treat compiler warnings seriously

# 12. Deliverables

Generate the complete implementation, including:

1. Solution and project files
2. Folder structure
3. Domain entities
4. DTOs
5. DbContext and Entity Framework configurations
6. Services and interfaces
7. API controllers
8. Validation
9. Global exception handling
10. Swagger configuration
11. Database migration
12. Development seed data
13. Unit and integration tests
14. Dockerfile
15. docker-compose.yml
16. appsettings configuration
17. README.md

The README must explain:

- Prerequisites
- Solution architecture
- How to start the application
- How to run migrations
- How to run tests
- Swagger URL
- Database configuration
- Example API requests
- How overselling is prevented

# 13. Expected output format

Start by presenting:

1. Key assumptions
2. Proposed folder structure
3. Domain and database design
4. Endpoint catalogue
5. Overselling-prevention strategy

Then generate every required file with:

- Its relative file path as a heading
- The complete file content
- No placeholder comments such as “implementation omitted”
- No pseudo-code for essential functionality

Ensure the final solution builds successfully, tests are logically consistent, Docker Compose configuration matches the application configuration, and all documented endpoints are implemented.
