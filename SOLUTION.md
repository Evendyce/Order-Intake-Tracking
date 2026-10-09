# Solution Design and Trade-offs

## Status and scope

This document records the current scaffold and intended design for Order Intake & Tracking. The three projects have been generated and included in the solution. Business behaviour below remains planned; builds, startup, and tests have not yet been verified.

The application serves an internal sales team: create purchase orders, review them, and track status without accidentally duplicating orders when a submission is retried.

The assignment requires an ASP.NET Core backend, Angular frontend, critical backend unit tests, simple local setup, design notes, and a narrated demo. The agreed extensions are UI status updates, duplicate overview and modification flow, LocalDB persistence, filtering, client management, and automatic client creation during order intake.

Authentication, deployment, inventory, payments, tax, and discount rules are outside the current scope.

## Architecture

`OrderIntake.sln` contains `OIT/OIT.Server/OIT.Server.csproj`, `OIT/oit.client/oit.client.esproj`, and `OIT.Tests/OIT.Tests.csproj`. The Angular and ASP.NET Core template created separate frontend and backend projects; the xUnit project was added separately. Keep business rules separated from HTTP handling and persistence so they can be tested directly.

### Scaffold progress

| Setup task | Current evidence | Completed |
| --- | --- | :---: |
| Create solution and include all three projects | Project entries exist in `OrderIntake.sln`. | ☑ |
| Create API scaffold | `OIT.Server` targets .NET 10 and uses controllers, HTTPS, and Development OpenAPI. | ☑ |
| Create Angular scaffold | `oit.client` declares Angular 22.2, CLI 22.2, and TypeScript 6.0 dependency ranges. | ☑ |
| Create xUnit project | `OIT.Tests` targets .NET 10 and references xUnit 2.9.3 and the Visual Studio runner. | ☑ |
| Configure repository ignores | Build output, dependencies, editor state, coverage, LocalDB data, and local certificate files are ignored. | ☑ |
| Reference server from tests | No API project reference currently exists in `OIT.Tests.csproj`. | ☐ |
| Verify scaffold execution | Restore, build, startup, and tests remain unverified. | ☐ |
| Replace template sample | Weather forecast code and the empty xUnit test remain. | ☐ |

The development environment is Visual Studio 2026, with Node.js 24.21.0 and npm 11.19.0 reported during setup. The selected .NET 10, Angular 22, and TypeScript 6 versions meet the assignment's minimum versions. Installed frontend package versions will be recorded through the lockfile; current `package.json` entries are ranges.

The server references the Angular `.esproj` and configures the SPA proxy to launch `npm start`. The Angular development server uses HTTPS on port 61009; the API HTTPS profile uses port 7145. Its current frontend proxy covers only the weather forecast sample. Order/client routes and the startup configuration still need end-to-end verification.

The generated frontend contains Vitest dependencies, a `karmaConfig` test option, and a Jasmine declaration in the `.esproj`. This tooling needs review before relying on frontend tests; frontend tests remain optional within the assignment.

| Component | Responsibility | Completed |
| --- | --- | :---: |
| API endpoints | Accept requests, expose orders and clients, and translate outcomes into consistent responses. | ☐ |
| Business logic | Validate orders, calculate totals, enforce status and editing rules, and handle repeat submissions. | ☐ |
| EF Core and LocalDB | Persist data, enforce unique references, and save related changes transactionally. | ☐ |
| Angular client | Provide order intake, list/detail views, status controls, duplicate feedback, and client management. | ☐ |
| xUnit tests | Verify critical business rules independently of the UI. | ☐ |

## Data model

| Entity | Planned fields |
| --- | --- |
| Order | ID, external reference, client ID, created/updated UTC timestamps, status, currency, notes, line items, subtotal, total |
| Client | ID, name, email, normalized email, active flag |
| Line item | ID, order ID, SKU, name, positive integer quantity, decimal unit price, computed line total |

Client is the application's name for the assignment's illustrative Customer concept. Separate product management is unnecessary: an order stores its line-item details.

## Status flow and editing

```text
Pending -> Confirmed -> Fulfilled
   |            |
   +-> Cancelled <-+
```

Orders start Pending. Pending orders may become Confirmed or Cancelled; Confirmed orders may become Fulfilled or Cancelled. Fulfilled and Cancelled are terminal states. Pending cannot move directly to Fulfilled.

The API is authoritative. The UI displays the progression and allowed actions, and explains why a rejected transition is invalid. Requesting the current status is a successful no-op that does not update timestamps.

Commercial details (client, line items, currency, and notes) may be modified while Pending. Confirmed and terminal orders cannot be edited. The external reference remains fixed. Editing recalculates totals on the server.

## Repeat submissions and duplicate overview

An existing external reference identifies a duplicate. Matching clients or line items alone do not identify duplicate orders.

Trim external references and compare them case-insensitively. Preserve a readable reference and enforce uniqueness on its normalized value in the database. Duplicate detection must be atomic: checking first in application code is insufficient when submissions arrive concurrently.

| Submission outcome | Intended behaviour |
| --- | --- |
| New reference | Create the order and return HTTP 201 with its identifier and details. |
| Existing reference with equivalent content | Return HTTP 200 with the existing order overview and a duplicate indicator; create no new records. |
| Existing reference with different content | Return HTTP 409 with an explanatory message, the existing overview, and meaningful differences; leave the order unchanged. |

The overview includes order ID, external reference, client summary, creation timestamp, current status, currency, subtotal, total, and whether editing is allowed. Differences identify changed client, currency, notes, or line items without treating server-generated IDs and timestamps as submitted content.

The Angular popup explains that the reference already exists, displays the overview and differences, and offers **View existing order** and, when allowed, **Modify existing order**. A retry never implicitly edits an order. Modification is a separate, explicit save action.

Detailed request/response contracts and content-equivalence rules will be finalized during implementation.

## Validation and money

- [ ] Require a non-empty external reference, valid client information, currency, and at least one line item.
- [ ] Reject zero, negative, or fractional quantities and negative prices.
- [ ] Use C# `decimal` for money and calculate totals exclusively on the server.
- [ ] Define supported currencies, price precision, rounding mode, and calculation limits explicitly.
- [ ] Calculate each line total from quantity and unit price; calculate subtotal from line totals.
- [ ] Set total equal to subtotal because taxes and discounts are outside the current scope.
- [ ] Return consistent validation errors with messages suitable for the UI.

The rounding policy is intentionally pending rather than implying that every currency has two decimal places. Client-side previews, if added, remain informational.

## Client management and automatic creation

The client screen supports adding, editing, and removing clients. Clients referenced by orders are deactivated instead of physically deleted, preserving order relationships. Inactive clients remain visible on historical orders but cannot be selected for new orders. Unreferenced clients may be deleted.

The order form supports selecting an existing client or entering a new client's information. Trim email addresses and compare them case-insensitively through a unique normalized email value. If an email already exists, reuse that client without silently changing their name. An inactive email match requires explicit reactivation rather than creating a second client.

Validate and resolve duplicate order references before creating a client. When a new client is needed, save the client and order in the same transaction so a failed submission does not leave an orphan client. Concurrent matching client submissions must also respect email uniqueness.

Historical behaviour when a client's details change still needs a decision: display the current client record or retain a client snapshot on the order. This will be finalized before implementing client edits.

## Persistence and filtering

Use EF Core with SQL Server LocalDB to retain data across restarts. Include migrations and document local database initialization. Enforce unique normalized order references and client emails at the database level.

Filter orders by external reference, client, and status. Apply filters before sorting results by creation timestamp descending, with a deterministic ID tie-breaker. Keep the initial UI minimal.

LocalDB adds installation and database setup work compared with in-memory storage and makes the intended local environment Windows-specific. This is an accepted trade-off for persistent relational storage; clear setup documentation is part of the deliverable.

### Source control

Keep solution/project files, application configuration, migrations, and dependency lockfiles in Git. The root `.gitignore` covers nested .NET build directories, Angular dependencies and caches, Visual Studio user state, test output, local environment overrides, LocalDB database files, and private certificate files. The Angular template's nested `.gitignore` also remains in place. Shared `.vscode` launch and task files are retained.

Database files will not be distributed through Git; migrations and documented initialization will reproduce the schema. EF Core packages, migrations, and connection configuration have not yet been added.

## Verification task list

| Verification | Expected result | Completed |
| --- | --- | :---: |
| Valid order creation | Correct initial status, client link, line totals, and overall totals. | ☐ |
| Identical repeat submission | Existing overview returned; order count unchanged. | ☐ |
| Conflicting repeat submission | Clear conflict and differences; existing data unchanged. | ☐ |
| Concurrent repeated reference | Exactly one order created. | ☐ |
| Quantity and price validation | Invalid values rejected with useful messages. | ☐ |
| Decimal calculations | Totals follow the documented precision and rounding policy. | ☐ |
| Status transitions | Valid changes succeed; invalid changes fail; terminal states remain terminal. | ☐ |
| Order modification | Pending edits recalculate totals; locked orders reject edits. | ☐ |
| Listing and filtering | Correct matches, newest first, with deterministic ordering. | ☐ |
| Automatic client creation | New client and order saved together; email matches reused. | ☐ |
| Failure during creation | Transaction leaves no partially created order or client. | ☐ |
| Client removal | Referenced clients deactivated and historical orders remain accessible. | ☐ |
| Persistence | Orders and clients survive an application restart. | ☐ |
| UI walkthrough | Intake, duplicate popup, modification, status updates, filters, and client CRUD work end to end. | ☐ |

Use xUnit for calculations, validation, status rules, editing rules, and repeat-submission outcomes. Database uniqueness, transactions, and concurrent writes need integration verification against the actual storage provider. Frontend automated tests are optional in the assignment; the full UI flow still requires verification.

## Documentation and demo task list

- [x] Record the initial scope, design, and trade-offs.
- [ ] Finalize currency/rounding, content-equivalence, and historical client-detail policies.
- [x] Record the actual project structure, framework selections, and source-control conventions.
- [ ] Record the implemented order/client API contracts.
- [ ] Update README with exact dependencies, database setup, startup, and test instructions.
- [ ] Record test results and remaining limitations.
- [ ] Verify setup from a fresh checkout.
- [ ] Prepare a 5–10 minute demo covering new orders, duplicate handling, totals, status rules, filtering, and clients.
- [ ] Include an architecture walkthrough, design decisions, trade-offs, and test evidence in the demo.

The assignment estimates approximately four hours but allows more time. The additional features increase scope; complete the core order flow and its tests before polishing extensions.
