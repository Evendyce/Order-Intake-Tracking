# Design decisions and source walkthrough

This guide explains why the application is structured as it is. [SOLUTION.md](../SOLUTION.md) contains the detailed business policies and API outcomes; [README.md](../README.md) contains setup and verification commands.

The `main` branch is the intended assignment review branch. `main-ui-overhaul` adds the optional VoidGlass presentation. API and business policies remain the same.

## Backend decisions

| Decision | Reason and consequence | Source |
| --- | --- | --- |
| Keep rules separate from persistence. | `OrderRules` validates details, computes totals, compares submissions, and controls transitions without HTTP or database dependencies. `OrderService` coordinates multi-step writes. Rules can be tested without starting SQL Server. | [Rules](../OIT/OIT.Server/Domain/Orders/OrderRules.cs), [service](../OIT/OIT.Server/Application/Orders/OrderService.cs) |
| Use resource-oriented HTTP routes. | POST creates orders, PUT replaces editable details, and PATCH targets status. A separate status route prevents ordinary detail edits from bypassing lifecycle policy. Client CRUD remains in its controller because the operations are small. | [Orders controller](../OIT/OIT.Server/Controllers/Orders/OrdersController.cs), [clients controller](../OIT/OIT.Server/Controllers/Clients/ClientsController.cs) |
| Compute money on the server. | The input contract contains quantities and prices, with no subtotal/total fields. C# decimal and SQL decimal(18,2) keep arithmetic consistent for the four supported two-decimal currencies. Total currently equals subtotal because tax/discount policy is outside scope. | [Contracts](../OIT/OIT.Server/Contracts/Orders/OrderContracts.cs), [rules](../OIT/OIT.Server/Domain/Orders/OrderRules.cs), [schema](../OIT/OIT.Server/Persistence/IntakeDbContext.cs) |
| Use normalized external reference as the duplicate key. | Trimmed, case-insensitive matching prevents accidental repeat insertion. An equivalent repeat returns 200; changed content returns 409. Both return the saved overview, and neither silently modifies the order. Line sequence is part of content equivalence. | [Create flow](../OIT/OIT.Server/Application/Orders/OrderService.cs), [comparison](../OIT/OIT.Server/Domain/Orders/OrderRules.cs) |
| Enforce uniqueness in SQL as well as application checks. | An initial lookup improves feedback but cannot prevent concurrent requests from passing together. Unique indexes are the final guarantee. SQL unique-key races clear the rolled-back change tracker and re-resolve the order/client, with at most three attempts. | [Schema](../OIT/OIT.Server/Persistence/IntakeDbContext.cs), [service](../OIT/OIT.Server/Application/Orders/OrderService.cs) |
| Save automatic client creation and order insertion atomically. | One SaveChanges transaction prevents an orphan client when insertion fails. Duplicate lookup happens before insertion, and email matching reuses an existing profile without overwriting its name. | [Service](../OIT/OIT.Server/Application/Orders/OrderService.cs) |
| Retain client snapshots on orders. | Existing orders remain readable after profile edits. An explicit Pending-order edit refreshes the snapshot from the selected client. Referenced clients deactivate instead of being deleted, preserving the relationship as well as the snapshot. | [Order entity](../OIT/OIT.Server/Domain/Orders/Order.cs), [client removal](../OIT/OIT.Server/Controllers/Clients/ClientsController.cs) |
| Use optimistic concurrency. | GUID version tokens travel with loaded records and subsequent writes. An early version check explains stale edits; EF also checks the token at save time to catch a race after that check. Conflicts require reloading rather than overwriting another change. | [Service](../OIT/OIT.Server/Application/Orders/OrderService.cs), [schema](../OIT/OIT.Server/Persistence/IntakeDbContext.cs), [error mapping](../OIT/OIT.Server/Infrastructure/ApiExceptionHandler.cs) |
| Publish lifecycle capabilities with the order. | The API supplies `canEdit` and `allowedStatuses`, keeping Angular aligned with server policy. Pending allows confirmation/cancellation; Confirmed allows fulfilment/cancellation; terminal states have no next actions. Repeating the current status preserves timestamp/version. | [Projection and policy](../OIT/OIT.Server/Domain/Orders/OrderRules.cs) |
| Replace lines during Pending edits. | The small bounded line collection is simpler to replace than diff by child ID. EF must explicitly mark replacement lines Added because application-generated GUIDs can otherwise imply existing rows. | [Update flow](../OIT/OIT.Server/Application/Orders/OrderService.cs) |
| Translate recoverable failures into Problem Details. | Business errors, concurrency conflicts, unique-key conflicts, and client relationship conflicts become useful HTTP responses. Unexpected errors are logged internally and return a generic 500 message. Duplicate overview responses deliberately use their own envelope. | [Exception handler](../OIT/OIT.Server/Infrastructure/ApiExceptionHandler.cs), [submission response](../OIT/OIT.Server/Controllers/Orders/OrdersController.cs) |
| Use LocalDB and startup migrations for local review. | Persistent SQL storage exercises real constraints while keeping setup small. The API applies committed migrations before serving requests. Production should apply migrations through a controlled deployment step. TimeProvider is injected for testable timestamps. | [Startup](../OIT/OIT.Server/Program.cs) |

## Frontend decisions

| Decision | Reason and consequence | Source |
| --- | --- | --- |
| Separate views by domain and lazy-load routes. | Orders and clients own their views, models, services, and reusable domain components. Shared controls handle common concerns. External HTML templates keep presentation reviewable and avoid constructing markup strings. | [Routes](../OIT/oit.client/src/app/app.routes.ts), [client guide](../OIT/oit.client/README.md) |
| Use template-driven forms and reusable field feedback. | The small forms need no larger form-state framework. Native validators plus custom whitespace/quantity/precision rules provide immediate feedback; the API still validates independently. Errors appear after interaction or submission. Default change detection accommodates mutable NgModel state. | [Order form](../OIT/oit.client/src/app/domains/orders/views/order-form/order-form.ts), [validators](../OIT/oit.client/src/app/shared/forms/field-rules.ts), [feedback](../OIT/oit.client/src/app/shared/components/field-feedback/field-feedback.ts) |
| Check line-editor validity explicitly. | Standalone line controls are not registered in the parent NgForm. The parent checks aggregate line validity before writing. JavaScript precision tolerance is for input feedback; persisted money is calculated with server decimal arithmetic. | [Line editor](../OIT/oit.client/src/app/domains/orders/components/line-items-editor/line-items-editor.ts) |
| Retry only transient API reads. | Angular can render before API migrations finish. GET network/502/503/504 failures retry at most eight times, one second apart. Writes and validation/business failures are never automatically replayed. | [Read interceptor](../OIT/oit.client/src/app/shared/http/read-retry-interceptor.ts) |
| Keep loading/failure distinct from empty data. | The orders view waits for rows and client filter options together. Failed refreshes retain loaded rows; an error is not evidence of an empty database. List reads unsubscribe when the view is destroyed. | [Orders view](../OIT/oit.client/src/app/domains/orders/views/orders-list/orders-list.ts), [clients view](../OIT/oit.client/src/app/domains/clients/views/clients-list/clients-list.ts) |
| Review duplicates explicitly. | Equivalent repeats arrive through HTTP success; changed repeats arrive through HTTP error. Both open the saved overview. Modify navigates to saved values, allowing the user to decide what to change. | [Form handling](../OIT/oit.client/src/app/domains/orders/views/order-form/order-form.ts), [duplicate dialog](../OIT/oit.client/src/app/domains/orders/components/duplicate-dialog/duplicate-dialog.ts) |
| Use native modal dialogs. | Native focus containment and Escape handling avoid a custom overlay implementation. Cancellation and client removal require an explicit action before sending the write. | [Confirmation dialog](../OIT/oit.client/src/app/shared/components/confirm-dialog/confirm-dialog.ts), [detail view](../OIT/oit.client/src/app/domains/orders/views/order-detail/order-detail.ts) |
| Keep client drafts separate from loaded rows. | Cancelling an edit leaves the displayed profile intact. Version tokens accompany edits/removals. Successful writes reload authoritative results, including whether removal deleted or deactivated the client. | [Editor](../OIT/oit.client/src/app/domains/clients/components/client-editor/client-editor.ts), [client view](../OIT/oit.client/src/app/domains/clients/views/clients-list/clients-list.ts) |

## Verification and limits

Pure xUnit tests cover business rules; SQL-backed tests cover constraints, transactions, and concurrency; HTTP tests exercise binding, validation, serialization, routes, and error contracts. Integration fixtures create uniquely named databases, and API fixtures verify the database override before writes. Vitest covers read recovery, list state, and field feedback. These layers test different failure modes rather than repeating the same implementation.

Recorded main-branch results are 33 backend and 25 frontend tests. See [end-to-end review](END_TO_END.md), [fresh-checkout validation](FRESH_CHECKOUT.md), [edge-case review](EDGE_CASE_REVIEW.md), and [UI checklist](UI_REVIEW_CHECKLIST.md) for evidence and environment limits. Screen-reader testing was intentionally skipped. This documentation pass changes comments and documentation only; it does not claim a new functional test run.

Authentication is outside scope: `UseAuthorization` alone does not secure these endpoints. Listing is unpaginated and returns full order projections; growing datasets would need pagination and a lighter list DTO. Client CRUD could move into an application service as workflows expand. Supporting other currencies would require explicit minor-unit/rounding policies. Production would also need authentication, deployment configuration, controlled migrations, and operational monitoring.

## Suggested reading order

1. Read the contracts and `OrderRules` for accepted inputs and policy.
2. Read `OrderService` and `IntakeDbContext` for transaction, uniqueness, and concurrency handling.
3. Read controllers and the exception handler for HTTP outcomes.
4. Follow Angular routes into the order/client views, then their API services and reusable components.
5. Read the tests and verification reports for the behavior and limits checked.

| Documentation task | Completed |
| --- | :---: |
| Explain backend policy, persistence, HTTP outcomes, and trade-offs. | ☑ |
| Explain frontend forms, components, recovery, and confirmation behavior. | ☑ |
| Link design decisions to their implementation. | ☑ |
| Add focused source comments explaining non-obvious choices. | ☑ |
| Align solution progress notes with recorded verification. | ☑ |
