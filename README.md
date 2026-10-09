# Order Intake & Tracking

A technical assignment for a small internal sales application that records purchase orders, prevents accidental duplicate submissions, and tracks order progress.

## Project status

The Visual Studio solution contains the generated API, Angular client, and xUnit test projects. The application still contains the weather forecast sample and an empty template test. Order features, LocalDB setup, and business-rule tests are pending.

Progress uses Markdown task lists and task tables with a **Completed** column. `☐` means outstanding; `☑` means complete. Check items only after their acceptance criteria have been verified.

- [x] Document the assignment requirements and agreed extensions.
- [x] Create the initial design and trade-off notes in [SOLUTION.md](SOLUTION.md).
- [x] Create the Visual Studio solution (`OrderIntake.sln`).
- [x] Create and include `OIT.Server`, `oit.client`, and `OIT.Tests` in the solution.
- [x] Configure `.gitignore` for Visual Studio, .NET, Angular, test output, and local database files.
- [ ] Add the `OIT.Tests` project reference to `OIT.Server`.
- [ ] Implement and verify the backend and persistence.
- [ ] Implement and verify the Angular UI.
- [ ] Complete critical business-rule tests.
- [ ] Verify setup instructions on a fresh checkout.
- [ ] Record the narrated demo.

## Technology and project structure

| Area | Technology |
| --- | --- |
| Backend | C# and ASP.NET Core, targeting .NET 10 (`net10.0`); controllers, HTTPS, and OpenAPI enabled |
| Frontend | Angular `^22.2.0`, Angular CLI `^22.2.2`, TypeScript `~6.0.2` as declared in `package.json` |
| Persistence | Planned: SQL Server LocalDB through Entity Framework Core; not configured yet |
| Unit tests | xUnit 2.9.3, Visual Studio runner 3.1.4, Microsoft.NET.Test.Sdk 17.14.1 |
| Development | Visual Studio 2026; three projects in one solution |
| Node tooling | Node.js 24.21.0 and npm 11.19.0 reported during setup; package manager declares npm 11.19.0 |

Frontend versions above are dependency ranges, not verified installed versions. Commit the generated `package-lock.json` when dependencies are installed to make restoration reproducible. Angular 22 and TypeScript 6 satisfy the assignment's Angular 18+ and TypeScript 5+ minimums.

```text
OrderIntake.sln
├── OIT/
│   ├── OIT.Server/       ASP.NET Core Web API
│   └── oit.client/       Angular application and Visual Studio .esproj
└── OIT.Tests/           xUnit test project
```

All three projects exist and are included in `OrderIntake.sln`. The API references the Angular project for template integration; the test project does not yet reference the API.

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

The following describes the generated scaffold. Startup, builds, and tests have not yet been verified; the order application and database are not implemented.

Use Visual Studio 2026 with the **ASP.NET and web development** workload, .NET 10 SDK, and Node.js/npm as listed above. Global Angular CLI 22.2.2 was installed for template creation; project scripts use the locally restored CLI. The Node.js development workload is optional. SQL Server LocalDB installation and connection setup remain pending.

### Restore and start the scaffold

From the repository root, restore the frontend dependencies:

```powershell
Push-Location OIT/oit.client
npm install
Pop-Location
dotnet restore OrderIntake.sln
dotnet dev-certs https --trust
```

After a lockfile is committed, use `npm ci` for clean dependency restoration.

Open `OrderIntake.sln` in Visual Studio. In solution startup settings, configure `OIT.Server` and `oit.client` to start, select the server's HTTPS launch profile, and start debugging. Verify this configuration before marking startup complete.

The generated configuration uses:

| Service | Configured address |
| --- | --- |
| Angular development server | `https://localhost:61009` |
| API HTTPS | `https://localhost:7145` |
| API HTTP | `http://localhost:5214` |
| OpenAPI document in Development | `https://localhost:7145/openapi/v1.json` |

The Angular `prestart` script exports the local HTTPS certificate. The current proxy only covers `/weatherforecast`; order and client routes will need to be added during implementation.

### Tests and frontend build

Use **Test > Test Explorer** in Visual Studio to discover and run xUnit tests, or run from the repository root:

```powershell
dotnet test OIT.Tests/OIT.Tests.csproj
```

The existing `UnitTest1.Test1` has no assertions and provides no business-rule coverage. Add the server reference before writing tests against API business logic.

To build the frontend:

```powershell
Push-Location OIT/oit.client
npm run build
Pop-Location
```

These commands are documented from the scaffold and remain unverified. Frontend test tooling generated by the template also needs verification before documenting a working test workflow.

| Setup documentation task | Completed |
| --- | :---: |
| Record selected framework, Node.js/npm, Angular dependency ranges, and Visual Studio version. | ☑ |
| Document LocalDB installation and connection configuration. | ☐ |
| Document database creation and migration commands. | ☐ |
| Document scaffold dependency restoration and API/client startup. | ☑ |
| Document intended Visual Studio startup configuration. | ☑ |
| Document backend test command and Test Explorer usage. | ☑ |
| Verify scaffold restoration, builds, startup, and tests. | ☐ |
| Validate instructions against a fresh checkout. | ☐ |

## Deliverables

| Deliverable | Completed |
| --- | :---: |
| All application source and tests in this GitHub repository. | ☐ |
| README with verified local setup and run instructions. | ☐ |
| SOLUTION with final design decisions and trade-offs. | ☐ |
| A 5–10 minute narrated demo showing the application, architecture, decisions, and interesting implementations. | ☐ |

Initial design notes are available in [SOLUTION.md](SOLUTION.md). Both documents will be updated as implementation and verification progress.
