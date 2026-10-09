# Order Intake & Tracking

A technical assignment for a small internal sales application that records purchase orders, prevents accidental duplicate submissions, and tracks order progress.

## Project status

Planning documentation and a blank Visual Studio solution are in place. Application projects, database setup, and tests have not yet been created.

Progress uses Markdown task lists and task tables with a **Completed** column. `☐` means outstanding; `☑` means complete. Check items only after their acceptance criteria have been verified.

- [x] Document the assignment requirements and agreed extensions.
- [x] Create the initial design and trade-off notes in [SOLUTION.md](SOLUTION.md).
- [x] Create the blank Visual Studio solution (`OrderIntake.sln`).
- [ ] Create the API, Angular client, and test projects.
- [ ] Implement and verify the backend and persistence.
- [ ] Implement and verify the Angular UI.
- [ ] Complete critical business-rule tests.
- [ ] Verify setup instructions on a fresh checkout.
- [ ] Record the narrated demo.

## Technology and planned structure

| Area | Technology |
| --- | --- |
| Backend | C# and ASP.NET Core, .NET 8 LTS or later |
| Frontend | Angular 18+ and TypeScript 5+ |
| Persistence | SQL Server LocalDB, accessed through Entity Framework Core |
| Unit tests | xUnit |
| Development | Visual Studio; API, Angular client, and tests in one solution |

Exact framework and tooling versions will be recorded when the projects are created.

```text
OrderIntake.sln
├── OrderIntake.Api
├── OrderIntake.Client
└── OrderIntake.Tests
```

The solution exists; the projects shown beneath it are planned and have not yet been created.

## Task list: assignment requirements

| Task | Acceptance criteria | Completed |
| --- | --- | :---: |
| Create orders | Accept client reference, client information, and line items with useful validation feedback. | ☐ |
| Prevent duplicate orders | Repeated external references never create another order, including concurrent submissions. | ☐ |
| Validate quantities and prices | Quantities are positive whole numbers; unit prices are non-negative. | ☐ |
| Calculate totals | The server calculates line totals, subtotal, and total. | ☐ |
| Retrieve orders | Retrieve one order and list orders newest first. | ☐ |
| Change status | Enforce allowed transitions and explain rejected changes. | ☐ |
| Submit orders in Angular | Provide a minimal order form with validation and submission feedback. | ☐ |
| Review orders in Angular | Show the order list and details including totals and status. | ☐ |
| Test business logic | Cover duplicate handling, validation, calculations, and status rules. | ☐ |

## Task list: agreed extensions

| Task | Acceptance criteria | Completed |
| --- | --- | :---: |
| UI status updates | Display the direction of the status flow and expose only allowed actions. | ☐ |
| Duplicate overview response | Return an overview of the existing order and meaningful differences from the submission. | ☐ |
| Duplicate popup | Identify the matching reference and offer viewing or modifying the existing order where permitted. | ☐ |
| Modify orders | Edit Pending orders and recalculate totals on the server; preserve the external reference. | ☐ |
| LocalDB persistence | Persist clients, orders, and line items across application restarts. | ☐ |
| Order filtering | Filter by external reference, client, and status while retaining newest-first ordering. | ☐ |
| Client management | Provide a simple add/edit/remove screen; deactivate clients referenced by orders. | ☐ |
| Create clients during order intake | Select an existing client or create one with the order; reuse an existing email match. | ☐ |

## Local setup and running

Setup documentation is pending implementation. The application cannot yet be run from this repository.

The intended environment is Visual Studio with the **ASP.NET and web development** workload, a compatible .NET SDK, Node.js with npm, Angular CLI, and SQL Server LocalDB. The Node.js development workload is optional. Node.js and Angular versions must be compatible with the selected Angular version.

| Setup documentation task | Completed |
| --- | :---: |
| Record exact SDK, Node.js, Angular, and Visual Studio requirements. | ☐ |
| Document LocalDB installation and connection configuration. | ☐ |
| Document database creation and migration commands. | ☐ |
| Document dependency restoration and API/client startup. | ☐ |
| Document Visual Studio startup configuration. | ☐ |
| Document test commands and Test Explorer usage. | ☐ |
| Validate instructions against a fresh checkout. | ☐ |

## Deliverables

| Deliverable | Completed |
| --- | :---: |
| All application source and tests in this GitHub repository. | ☐ |
| README with verified local setup and run instructions. | ☐ |
| SOLUTION with final design decisions and trade-offs. | ☐ |
| A 5–10 minute narrated demo showing the application, architecture, decisions, and interesting implementations. | ☐ |

Initial design notes are available in [SOLUTION.md](SOLUTION.md). Both documents will be updated as implementation and verification progress.
