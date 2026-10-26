# Code Review — Order Inventory Management API

## Scope

This review covers the order controller and service implementation shared during development. The assessment's original review/debugging snippet was not available in the supplied requirements, so findings about that specific snippet remain outstanding.

## Findings

### 1. Customer identity is hardcoded

**Severity: High**

The order controller calls the service with a fixed customer ID of `1`.

**Impact:** Orders may be associated with the wrong customer. This is not safe for a multi-customer application.

**Recommendation:** Derive the customer ID from the authenticated user's claims after implementing JWT authentication. Never trust a customer ID supplied by an unauthenticated request.

### 2. Order endpoints need authentication and authorization

**Severity: High**

The shared controller does not show authentication or authorization attributes.

**Impact:** A caller may be able to create orders or change order status without the required permissions.

**Recommendation:** Require authentication for customer operations and restrict administrative actions to users with the administrator role. Customers should only be able to access their own orders and cancel eligible orders.

### 3. Concurrency exceptions need consistent handling

**Severity: High**

The service uses database transactions and a row-version token for concurrency. The order-creation code also converts a concurrency exception into an `InvalidOperationException`.

**Impact:** Mapping exceptions inconsistently can produce incorrect HTTP status codes or hide the original cause of a conflict.

**Recommendation:** Handle `DbUpdateConcurrencyException` explicitly and return HTTP 409 Conflict for a genuine concurrency conflict. Preserve useful structured logs without exposing internal exception details to clients.

### 4. Idempotency must remain reliable under concurrent requests

**Severity: High**

The service checks for an existing idempotency record before creating an order and uses a unique index on the customer and key.

**Impact:** Two simultaneous requests with the same key can race. One may fail with a database uniqueness exception rather than returning the original order.

**Recommendation:** Keep the unique database constraint and handle duplicate-key violations safely. A repeated request from the same customer with the same key should return the original order, without reserving stock twice.

### 5. Status changes require strict validation

**Severity: High**

Order status transitions must follow the allowed state machine.

**Impact:** Invalid transitions can allow a shipped or cancelled order to be changed again or stock to be restored incorrectly.

**Recommendation:** Validate the current status and requested status inside the service and transaction. Permit only the required transitions, and restore stock exactly once when a valid cancellation occurs.

### 6. API error responses should be consistent

**Severity: Medium**

The controller currently returns anonymous objects for some errors, while the assessment requires a global ProblemDetails approach.

**Impact:** Clients may receive inconsistent error formats.

**Recommendation:** Configure centralized exception handling and ProblemDetails responses. Return appropriate status codes for validation failures, missing records, conflicts, and unexpected server errors.

### 7. Automated tests are needed

**Severity: High**

The available code alone does not demonstrate that the order workflow passes automated tests. An order-creation request returned HTTP 500 during development.

**Impact:** The actual cause of the failure and the reliability of the workflow remain unverified.

**Recommendation:** Capture the server-side exception and add unit and integration tests for stock validation, discount rules, idempotency, status transitions, cancellation, and concurrent requests.

## Summary

The project has a starting structure with controllers, services, Entity Framework Core models, migrations, and database seeding. Before the implementation can be considered assessment-complete, authentication and authorization, robust error handling, race-safe idempotency, automated tests, and the remaining required endpoints must be implemented and verified.
