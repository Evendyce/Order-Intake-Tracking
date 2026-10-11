# Solution Design and Trade-offs

## Status and scope

The first implementation covers order intake, duplicate prevention, order review/editing/status changes, filtering, and client management with automatic client creation. The backend has 33 passing tests and the Angular production build passes. The main flows have been checked in a local browser; final review, fresh-checkout setup, and the video demo are outstanding.

The assignment's required backend/frontend/tests are supplemented by the agreed extensions: UI status changes, duplicate overview and modification flow, LocalDB storage, filters, client CRUD, and automatic client creation. Authentication, deployment, inventory, payments, taxes, and discounts remain outside scope.

## Architecture and conventions

`OrderIntake.sln` contains `OIT.Server` (ASP.NET Core .NET 10), `oit.client` (Angular 22/TypeScript 6), and `OIT.Tests` (xUnit). The server references the client's `.esproj`; the test project references the server. EF Core and the local EF CLI are pinned to 10.0.12. Frontend dependency resolution is committed in `package-lock.json`.

Backend folders separate API controllers/contracts, domain entities/rules, order application services, persistence, and error handling. `OrderRules` has no HTTP or database dependency. `OrderService` orchestrates persistence and business rules. Client CRUD is deliberately small and remains in its domain controller; it can move into a service if client workflows grow.

Angular is component-based, with external HTML templates and lazy-loaded standalone views. Each domain owns its `models`, `services`, `components`, and `views`. The root component provides navigation only. Reusable components own line-item editing, status progression, duplicate feedback, client editing, error feedback, and confirmations; screens do not construct HTML strings.

Comments explain choices where code alone cannot convey the reason: database uniqueness, client snapshots, atomic saves, explicit EF state for replacement lines, lazy loading, native modal focus containment, and local startup migrations. Comments avoid narrating obvious assignments.

| Implementation task | Completed |
| --- | :---: |
| Domain-separated backend and frontend folders. | ☑ |
| Component-based UI and API services. | ☑ |
| Resource-oriented REST routes and consistent errors. | ☑ |
| SQL persistence and initial migration. | ☑ |
| Critical unit, database, and API tests. | ☑ |
| Final reviewer walkthrough and fresh-checkout validation. | ☐ |

## Data and persistence

| Entity | Stored data |
| --- | --- |
| Order | ID, original and normalized reference, client ID and name/email snapshot, UTC timestamps, status, currency, notes, totals, version, items |
| Client | ID, name, original and normalized email, active flag, version |
| Order line | ID, order ID, position, SKU, name, quantity, unit price, line total |

Client is the application's name for the assignment's Customer concept. Line items retain their own product details; no product catalogue is needed. Client name/email snapshots preserve order history when a client profile changes. An explicit Pending order edit refreshes the snapshot from the selected client.

EF Core uses SQL Server LocalDB with unique indexes for normalized references and emails, a creation-date index for listing, restricted client deletion, and cascading line deletion. `decimal(18,2)` stores money. GUID version fields are optimistic concurrency tokens on orders and clients; clients submit the version they last loaded, and stale writes receive HTTP 409.

The database is `OrderIntake` on the dedicated `(localdb)\OITDevelopment` instance. The previous app instance's registry registration was visible to the agent's command environment but absent from Visual Studio's environment, even under the same Windows account. A new instance was created through Visual Studio's environment and confirmed visible to both. The existing database was restored from a verified backup and checked with `DBCC CHECKDB`; previous instances were preserved.

The API uses a standard LocalDB instance-name connection. Temporary named-pipe configuration, keep-alive connections, and build hooks were removed. Initial Angular GET requests retry transient proxy/network failures during server startup, without replaying writes.

The API applies committed migrations on startup for a short local setup path. A production deployment should apply migrations separately. LocalDB is Windows-specific and adds installation work; this is an accepted trade-off for persistent relational storage.

## REST endpoints

| Method | Resource | Behaviour |
| --- | --- | --- |
| GET | `/api/orders` | List newest first; optional `reference`, `clientId`, `status` filters. |
| GET | `/api/orders/{id}` | Retrieve details, totals, version, edit eligibility, and allowed statuses. |
| POST | `/api/orders` | Create or return the duplicate overview. |
| PUT | `/api/orders/{id}` | Replace Pending order details using its version; reference fixed. |
| PATCH | `/api/orders/{id}/status` | Apply one allowed status transition using its version. |
| GET | `/api/clients` | List client profiles, including inactive clients. |
| GET | `/api/clients/{id}` | Retrieve a client profile. |
| POST | `/api/clients` | Add a unique client email. |
| PUT | `/api/clients/{id}` | Edit profile/active flag using its version. |
| DELETE | `/api/clients/{id}?version={version}` | Delete an unreferenced client or deactivate a referenced client. |

Created resources return HTTP 201 and a Location header. Invalid input returns HTTP 400 validation feedback; missing records return 404; invalid transitions, stale versions, and uniqueness conflicts return 409. Business and persistence errors are translated into Problem Details with helpful messages. Unexpected errors are logged and return a generic 500 message. Status values are strings; numeric enum values are rejected in JSON.

The generated `.http` file includes sample order/client requests. Development OpenAPI is available at `/openapi/v1.json`. The Angular dev proxy forwards `/api/**` and `/openapi/**` to the backend.

## Repeat submissions and duplicate overview

The external reference is the duplicate key; similar clients/products alone are not duplicates. References are trimmed and normalized to uppercase for case-insensitive matching, while the readable original is retained.

| Outcome | Response |
| --- | --- |
| New reference | 201 with `{ isDuplicate: false, overview, differences: [] }`. |
| Existing reference with equivalent content | 200 with `{ isDuplicate: true, overview, differences: [] }`. |
| Existing reference with changed content | 409 with `{ isDuplicate: true, overview, differences }`; saved data unchanged. |

The overview contains order ID/reference, client snapshot, timestamps, status, currency, notes, totals, version, line details, edit eligibility, and allowed transitions. Differences identify client, currency, notes, or line items. Email and currency matching is normalized; notes/SKU/name are trimmed; line order is meaningful. Server IDs/timestamps/status are not part of submitted-content equivalence. Matching an existing client by email does not overwrite its name.

A duplicate popup explains the matching reference, displays the saved overview and differences, and offers viewing or modifying the saved order. Modification opens the saved values for explicit review; retry content is never silently applied. Comparison is against current saved order content, so a replay of an older pre-edit payload may produce a conflict.

A database unique index guarantees only one order per reference across simultaneous requests. A single `SaveChanges` transaction saves a new client and order together. When a SQL unique-key race occurs, the rolled-back context is cleared and the reference/client is resolved again, with at most three attempts. A duplicate does not create a new client. Tests verify five concurrent submissions produce one order and one client.

## Status and editing rules

```text
Pending -> Confirmed -> Fulfilled
   |            |
   +-> Cancelled <-+
```

Orders start Pending. Pending may become Confirmed or Cancelled; Confirmed may become Fulfilled or Cancelled. Fulfilled and Cancelled are terminal. Repeating the current status is a no-op, preserving timestamp/version. Invalid transitions receive a useful 409 message.

The UI shows the progression and only exposes allowed actions supplied by the API. Cancellation requires confirmation through a reusable modal component. The server enforces transitions independently of the UI.

Only Pending orders can change client, items, currency, or notes. The external reference stays fixed. Edits require an existing client selection, recalculate totals, replace old lines, and update timestamp/version. The server explicitly tracks replacement lines as Added because their application-generated GUIDs would otherwise be interpreted as existing rows by EF change tracking.

## Validation and money

| Rule | Policy |
| --- | --- |
| Order reference | Required, maximum 100 characters. |
| Client | Exactly one existing ID or new client profile; name and valid email required. |
| Currency | USD, ZAR, EUR, or GBP. All supported currencies use two decimal places. |
| Lines | Between 1 and 100; required SKU/name with length bounds. |
| Quantity | Whole number from 1 through 100000. Fractional JSON quantities fail binding. |
| Unit price | Decimal from 0 through 1000000, with at most two decimal places. |
| Line total | Quantity × unit price, rounded to two decimals using AwayFromZero. |
| Subtotal / total | Sum of server line totals; total equals subtotal because tax/discount rules are out of scope. |

The server uses C# decimal rather than binary floating point. Client-supplied totals are not part of the input contract. Supported ranges bound totals within database precision. The UI displays persisted totals after saving and provides input feedback; the API remains authoritative.

## Client lifecycle and filtering

Clients can be added, edited, deactivated, reactivated, and removed. Removal physically deletes only clients without orders; referenced clients deactivate so order relationships remain valid. Inactive clients cannot be used for new orders, but their historical orders remain visible. A Pending order may keep its existing inactive client during edits; it cannot be reassigned to another inactive client.

New-client intake trims and normalizes email for matching. An existing active email match is reused without changing its profile. Inactive matches require explicit reactivation. New client/order insertion is atomic; failed inserts roll back together.

Order filters use a case-insensitive reference substring, exact client ID, and status. Results sort by creation timestamp descending, then ID descending for deterministic ties. Listing is unpaginated for this small assignment; pagination would be needed with growing data volume.

## Verification and progress

Verified on 11 October 2026:

| Check | Evidence | Completed |
| --- | --- | :---: |
| Business rules | 21 passing cases for decimal totals, bounds, validation, allowed/invalid/no-op statuses, duplicate comparisons, and editing locks. | ☑ |
| Database behaviour | 6 passing cases for duplicate outcomes, simultaneous submissions, no orphan client on duplicate, line replacement/stale versions, client removal, and snapshots. | ☑ |
| HTTP contracts | 6 passing cases for creation/Location, duplicate 200/409, fractional/invalid quantities/prices, filtering/status rules, email validation, and client CRUD. | ☑ |
| Database schema | Initial EF migration successfully applied to LocalDB. | ☑ |
| Angular build | Production build passes; domain views are lazy chunks. | ☑ |
| Frontend recovery | 22 Vitest cases cover transient read recovery, bounded failure, non-retryable errors, write safety, cancellation, orders-page loading, and client-list read/write error recovery. | ☑ |
| Browser order flow | Create, duplicate conflict popup, modify saved order, recalculate totals, confirm status, show cancellation confirmation, and filter matching/nonmatching orders. | ☑ |
| Browser client flow | Add, edit, delete unused client, deactivate referenced client, reactivate, and reject duplicate email without hiding loaded rows. | ☑ |
| Core end-to-end review | Isolated browser/API checks covered duplicate outcomes, terminal statuses, client policies, filters, concurrent edits, and restart persistence. See [verification report](docs/END_TO_END.md). | ☑ |
| Dependency audit | `npm audit` reports zero vulnerabilities after removing unused `jest-editor-support`. | ☑ |
| Restart persistence | The saved demo order and totals were retrieved after API restarts, including through the HTTPS client proxy. | ☑ |
| Command-line HTTPS startup | Starting the client separately with `npm start`, then the API HTTPS launch profile, served proxied orders and the OpenAPI document. | ☑ |
| IDE database connection | Visual Studio build succeeded and connected by instance name after LocalDB was stopped, automatically starting it and reading the preserved order. | ☑ |
| Visual Studio HTTPS startup | User confirmed the saved order appeared automatically after startup without a manual refresh. | ☑ |
| Browser certificate trust | Trusted the exported Angular certificate with `dotnet dev-certs https --trust`; browser opened HTTPS Orders without a warning page, and client/API/proxy requests passed normal certificate validation. | ☑ |
| Fresh-checkout setup | Commit 2d0507d restored/built/tested in a separate clone; fresh database/certificate exports, HTTPS startup, order/client creation, and restart persistence passed. Installed prerequisites and global caches were shared. See [process and results](docs/FRESH_CHECKOUT.md). | ☑ |
| Final accessibility/responsive review | Not yet completed. | ☐ |

Integration tests run against uniquely named LocalDB databases and remove their own databases in teardown; they never delete the app database. Unit-only runs can exclude the `Integration` trait. Frontend tests use Angular's Vitest runner; the generated mixed Jasmine/Karma configuration was removed.

Angular can serve the page before the API completes startup migrations. A shared HTTP interceptor retries only `/api/` GET requests for network errors and HTTP 502/503/504, with at most eight retries spaced one second apart. Writes and other failures are never retried. The orders page waits for both order rows and client filter options, keeps its loading indicator during retries, and cancels reads on navigation. List-loading failures show an error without claiming that the database contains no matching records.

The browser check used the documented local HTTP workflow. Synthetic demo clients and `DEMO-001` remain in the development database; they are not seeds or committed application data.

The latest HTTPS check used normal certificate validation for the Angular page, API, and proxy. The Angular PEM certificate was valid but initially absent from the current user's trusted root store; trusting the matching ASP.NET Core development certificate resolved the warning. The template's automatic SPA launch could not resolve `npm.cmd` in the agent environment; starting the client separately worked. README documents that fallback and the need for Node/npm on Visual Studio's PATH.

## Remaining delivery tasks

- [x] Implement agreed domains, components, REST routes, comments, and business policies.
- [x] Add migration, critical tests, and dependency lockfile.
- [x] Update setup and design documentation with verified results.
- [x] Verify application restart persistence and separate-process HTTPS startup.
- [x] Verify full Visual Studio HTTPS startup and core browser workflows.
- [x] Trust the development browser HTTPS certificate.
- [x] Verify setup from a fresh checkout with isolated application data; document the process and shared-environment limits.
- [ ] Complete final accessibility, responsive layout, and edge-case review.
- [ ] Review all source and limitations before GitHub submission.
- [ ] Record a 5–10 minute narrated demo covering the working flow, architecture, key decisions, trade-offs, and test evidence.
