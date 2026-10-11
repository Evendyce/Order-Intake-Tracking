# Order Intake & Tracking

A small internal sales application for recording purchase orders, preventing accidental duplicate submissions, and tracking order progress. Built for the Senior .NET/Angular technical assignment.

## Project status

The first end-to-end implementation is in place. Backend tests and the Angular production build pass. A browser walkthrough verified order creation, duplicate feedback, modification, status progression, filtering, and client add/edit/deactivation/reactivation.

Progress uses Markdown task lists and tables with a **Completed** column: `☐` outstanding, `☑` complete. Final delivery, fresh-checkout validation, and the narrated demo remain pending.

- [x] Create `OrderIntake.sln` with `OIT.Server`, `oit.client`, and `OIT.Tests`.
- [x] Configure repository ignores and reference the server from the xUnit project.
- [x] Implement the order/client API, LocalDB persistence, and initial migration.
- [x] Implement Angular components with views separated by domain.
- [x] Verify critical backend logic and API behaviour with tests.
- [x] Verify the main UI flow in a browser.
- [x] Verify the complete Visual Studio HTTPS startup experience and core end-to-end workflows.
- [x] Trust the development browser HTTPS certificate and verify HTTPS with certificate validation enabled.
- [ ] Verify setup on a fresh checkout.
- [ ] Complete final review and record the narrated demo.

## Technology and structure

| Area | Technology |
| --- | --- |
| Backend | C#, ASP.NET Core .NET 10, controllers and OpenAPI |
| Persistence | EF Core 10.0.12, SQL Server LocalDB |
| Frontend | Angular 22.2, TypeScript 6.0; exact dependencies in `package-lock.json` |
| Tests | xUnit 2.9.3, ASP.NET Core test host, real LocalDB integration tests |
| Development | Visual Studio 2026; Node.js 24.21.0, npm 11.19.0 |

```text
OrderIntake.sln
├── OIT/
│   ├── OIT.Server/
│   │   ├── Application/Orders/       Order use cases
│   │   ├── Contracts/Orders|Clients/ API request/response models
│   │   ├── Controllers/Orders|Clients/
│   │   ├── Domain/Orders|Clients/    Entities and business rules
│   │   ├── Infrastructure/          Consistent API errors
│   │   ├── Persistence/             EF Core context
│   │   └── Migrations/              Versioned database schema
│   └── oit.client/src/app/
│       ├── domains/orders/          models, services, components, views
│       ├── domains/clients/         models, services, components, views
│       └── shared/                  feedback, confirmation dialog, error formatting
└── OIT.Tests/
    ├── Orders/                      Business rules and persistence tests
    └── Api/                         HTTP-level tests
```

## Local setup

Use Windows with Visual Studio 2026 and the **ASP.NET and web development** workload, .NET 10 SDK, Node.js 24.21.0/npm 11.19.0, and SQL Server LocalDB. The Node.js development workload is optional. Install LocalDB through Visual Studio Installer's individual components if it is unavailable.

From the repository root:

```powershell
dotnet tool restore
dotnet restore OrderIntake.sln
Push-Location OIT/oit.client
npm ci
Pop-Location
```

Create the app-specific LocalDB instance once, then start it:

```powershell
# Run create only on first setup. Existing instances do not need recreation.
SqlLocalDB create OITDevelopment
SqlLocalDB start OITDevelopment
```

The configured database is `OrderIntake` on `(localdb)\OITDevelopment`, using Windows authentication. The API applies committed EF migrations on startup. To apply them explicitly:

```powershell
dotnet ef database update --project OIT/OIT.Server
```

The local EF tool is pinned in `dotnet-tools.json`. Database files are not committed. Override the connection with `ConnectionStrings__OrderIntake` if using a different SQL Server instance.

Run the instance creation command in a normal Windows terminal. The development machine uses `OITDevelopment`, registered through Visual Studio's environment and visible to both the IDE and command-line tooling. The previous instances were left untouched, and the existing `OrderIntake` database was backed up, verified, and restored into this instance. The API uses the standard instance-name connection; no named-pipe settings or build-time preparation are required.

### Visual Studio startup

1. Open `OrderIntake.sln`.
2. Trust the development HTTPS certificate with `dotnet dev-certs https --trust` if needed.
3. Configure `OIT.Server` and `oit.client` as startup projects, with the server's HTTPS launch profile selected.
4. Start debugging and open the client at `https://localhost:61009`.

| Service | Configured address |
| --- | --- |
| Client | `https://localhost:61009` |
| API HTTPS | `https://localhost:7145` |
| API HTTP | `http://localhost:5214` |
| OpenAPI document (Development) | `https://localhost:7145/openapi/v1.json` |

The Angular `prestart` script exports its local HTTPS certificate. `/api/**` and `/openapi/**` are proxied to the API. Initial API reads retry temporary connection failures while the server starts. On this development machine, `dotnet dev-certs https --trust` trusted the certificate used by Angular; the Orders page opened over HTTPS without a certificate-warning page.

HTTPS was verified with certificate validation enabled: the Angular page, API orders endpoint, and client-proxied orders endpoint returned HTTP 200. If automatic SPA startup cannot resolve `npm.cmd`, start the client separately with `npm start` and confirm Node/npm are on Visual Studio's PATH. Certificate trust is local to the Windows account; repeat the trust step on a fresh development machine.

### Verified command-line development workflow

The browser walkthrough used separate local HTTP processes. In one PowerShell terminal at the repo root:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project OIT/OIT.Server --no-launch-profile --urls http://localhost:5214
```

In a second terminal:

```powershell
Set-Location OIT/oit.client
$env:ASPNETCORE_URLS = 'http://localhost:5214'
npx ng serve --ssl=false --host=127.0.0.1
```

Open `http://127.0.0.1:61009`. This workflow is for local development only. The API may log a missing HTTPS redirect port when intentionally started with only HTTP. Stop running processes before rebuilding to avoid Windows executable file locks.

## Tests and build

```powershell
# All backend tests; requires the running OITDevelopment instance.
dotnet test OIT.Tests/OIT.Tests.csproj

# Business-rule tests only; no database needed.
dotnet test OIT.Tests/OIT.Tests.csproj --filter 'Category!=Integration'

Push-Location OIT/oit.client
npm test -- --watch=false
npm run build
Pop-Location
```

Visual Studio **Test > Test Explorer** also runs the xUnit tests. Integration tests create uniquely named `OIT_Tests_*` / `OIT_ApiTests_*` databases, apply migrations, and remove only their own databases during cleanup.

Verified on 11 October 2026: **33 backend tests passed** (21 business-rule cases and 12 persistence/HTTP cases), plus **20 frontend tests** covering read recovery, retry limits, immediate errors, write safety, cancellation, and orders-page loading/error states. The solution build passed with no warnings/errors and the Angular production build passed. The saved order survived API restarts. `npm audit` reported zero vulnerabilities after removing an unused template dependency. Frontend tests use Angular's Vitest runner; the template's mixed Jasmine/Karma settings were removed.

API reads retry network errors and HTTP 502/503/504 up to eight times, one second apart, so normal startup delays retain a loading state and recover automatically. Other errors and all writes are reported immediately. Persistent read failures show an error after retries rather than a misleading empty-list message.

## Task list: assignment requirements

| Task | Acceptance criteria | Completed |
| --- | --- | :---: |
| Create orders | Client information, external reference, and line items accepted with validation. | ☑ |
| Prevent duplicates | Existing references return the existing order; concurrent submissions create one order. | ☑ |
| Validate quantities and prices | Positive integer quantities, non-negative prices, supported precision and bounds. | ☑ |
| Calculate totals | Line totals, subtotal, and total calculated on the server using decimal. | ☑ |
| Retrieve orders | Order detail and newest-first listing. | ☑ |
| Change status | Allowed transitions enforced with useful rejection messages. | ☑ |
| Submit and review in Angular | Order form, list, and details show saved totals and status. | ☑ |
| Critical backend tests | Business rules, persistence, concurrency, and HTTP validation covered. | ☑ |
| Startup recovery | Bounded read retries, loading states, outage feedback, and frontend tests. | ☑ |
| End-to-end verification | Browser workflows, live API rejections, persistence after restart, 33 backend tests, and 22 frontend tests. See [verification report](docs/END_TO_END.md). | ☑ |

## Task list: agreed extensions and development conventions

| Task | Acceptance criteria | Completed |
| --- | --- | :---: |
| UI status updates | Directional progression with only allowed next actions. | ☑ |
| Duplicate overview and popup | Show saved order details, changed fields, and view/modify actions. | ☑ |
| Modify orders | Pending orders editable; reference fixed; server recalculates totals. | ☑ |
| LocalDB persistence | Committed migration, persisted clients/orders/items, and database-level uniqueness. | ☑ |
| Filtering | Reference, client, and status filters, newest first. | ☑ |
| Client management | Add/edit/remove; referenced clients deactivate; reactivation available through editing. | ☑ |
| Create client during intake | Reuse normalized email matches or save a new client with the order atomically. | ☑ |
| Component-based UI | Dedicated line editor, status flow, duplicate dialog, client editor, feedback, and confirmation components. | ☑ |
| Domain-separated views | Orders and clients have their own models, services, components, and view folders. | ☑ |
| REST API structure | Resource routes with HTTP verbs and consistent errors. | ☑ |
| Explain design choices | Brief comments explain uniqueness, snapshots, transaction boundaries, concurrency, and dialog semantics. | ☑ |

## Deliverables and remaining checks

| Task | Completed |
| --- | :---: |
| Application source and tests in the local repository. | ☑ |
| README setup instructions and SOLUTION design notes updated. | ☑ |
| Verify full Visual Studio HTTPS startup. | ☑ |
| Trust the development browser HTTPS certificate. | ☑ |
| Verify restoration and setup on a fresh checkout. | ☐ |
| Final accessibility, responsive layout, and edge-case review. | ☐ |
| Final GitHub submission review. | ☐ |
| Record a 5–10 minute narrated application/architecture demo. | ☐ |

See [SOLUTION.md](SOLUTION.md) for business policies, endpoint contracts, trade-offs, and verification details.
