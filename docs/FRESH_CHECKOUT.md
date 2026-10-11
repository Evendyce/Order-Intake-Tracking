# Fresh-checkout setup verification

Completed on 11 October 2026 against commit `2d0507d5cd27cde5334ad18bafa866823071e69b`.

This check proves that the committed source can restore, build, migrate, test, and run without the original checkout's generated files or development database. It used a separate clone, an empty database, and newly exported Angular certificate files. No application fixes were needed.

## Scope and prerequisites

| Item | Verification scope | Completed |
| --- | --- | :---: |
| Separate source checkout | Local Git clone using `--no-local`; only committed files were copied. | ☑ |
| No existing project outputs | No `node_modules`, server `bin`/`obj`, `.vs`, or local pipe configuration existed initially. | ☑ |
| Empty database | Used `OIT_FreshCheckout_20261011_120232` on the existing `(localdb)\OITDevelopment` instance. | ☑ |
| Fresh Angular certificate export | Exported PEM/key into a separate temporary profile directory; existing Angular exports were not reused. | ☑ |
| Installed prerequisites | Windows, .NET SDK 10.0.401, Node 24.21.0, npm 11.19.0, LocalDB 17.0.4025.3. | ☑ |
| Clean operating-system installation | Not performed; installed tools, package caches, the LocalDB instance, and Windows certificate trust were shared. | ☐ |

The actual clone directory was `C:\Users\Evendyce\AppData\Local\Temp\oit fresh checkout 20261011-120232`. Its spaces also exercised project-path quoting. The temporary certificate profile was `C:\Users\Evendyce\AppData\Local\Temp\oit fresh checkout profile 20261011-120232`.

Node 24 was selected in the verification shell's PATH because the agent's default shell selected a different Node installation. A normal reviewer should ensure `node --version` and `npm --version` report the documented prerequisites before restoring packages. No machine-wide PATH changes were made.

## Reproduction process

Run the following in PowerShell on Windows with the README prerequisites installed. Stop other copies of this application before using the default ports. Replace the example source path with the repository location, or use its GitHub clone URL when checking the submitted repository.

### 1. Clone committed source

```powershell
$sourceRepository = 'D:\Solutions\Order-Intake-Tracking'
$checkSuffix = Get-Date -Format 'yyyyMMdd-HHmmss'
$checkoutPath = Join-Path $env:TEMP "oit fresh checkout $checkSuffix"
git clone --no-local $sourceRepository $checkoutPath
Set-Location $checkoutPath
git rev-parse HEAD
git status --short

dotnet --version
node --version
npm --version
SqlLocalDB versions
```

Initially, `git status --short` should be empty. Verify that `OIT/oit.client/node_modules`, `OIT/OIT.Server/bin`, `OIT/OIT.Server/obj`, and `.vs` do not exist. A local clone excludes uncommitted edits; commit the intended submission before repeating this check.

### 2. Restore tools and dependencies

```powershell
dotnet tool restore
dotnet restore OrderIntake.sln
Push-Location OIT/oit.client
npm ci
Pop-Location
```

`dotnet-ef` should restore at version 10.0.12. `npm ci` uses the committed lockfile and recreates dependencies rather than copying the original checkout's `node_modules`.

### 3. Prepare an isolated database

```powershell
# Use a normal Windows terminal. Create the instance only if it is absent.
$instances = SqlLocalDB info
if ($instances -notcontains 'OITDevelopment') {
    SqlLocalDB create OITDevelopment
}
SqlLocalDB start OITDevelopment

$checkDatabase = "OIT_FreshCheckout_$checkSuffix"
$env:ConnectionStrings__OrderIntake = "Server=(localdb)\OITDevelopment;Database=$checkDatabase;Trusted_Connection=True;TrustServerCertificate=True"
dotnet ef database update --project OIT/OIT.Server
Write-Host "Checkout: $checkoutPath"
Write-Host "Database: $checkDatabase"
```

The connection override protects the normal `OrderIntake` database. This check explicitly ran the documented EF command and applied migration `20261011072306_InitialOrderIntake` to the new database. Normal API startup also applies committed migrations, so the explicit EF step is optional for ordinary development.

Integration tests use their own uniquely named databases and remove those test databases during teardown.

### 4. Build and test

```powershell
dotnet build OrderIntake.sln --no-restore
dotnet test OIT.Tests/OIT.Tests.csproj

Push-Location OIT/oit.client
npm test -- --watch=false
npm run build
Pop-Location
git status --short
```

Expected results: zero solution-build warnings/errors, 33 backend cases, 22 frontend cases, and a successful production Angular build. Generated outputs are ignored; restoring/building should not change tracked files.

### 5. Verify certificate export and launch HTTPS

Trust the ASP.NET Core development certificate for the current Windows account using the README command. If Windows presents a trust confirmation, accept it yourself.

```powershell
dotnet dev-certs https --trust
```

To prove the Angular export does not depend on existing PEM/key files, use a separate temporary profile in the **client terminal only**:

```powershell
Set-Location $checkoutPath
$certificateProfile = Join-Path $env:TEMP "oit fresh checkout profile $checkSuffix"
$env:APPDATA = $certificateProfile
Set-Location OIT/oit.client
npm start
```

`prestart` creates the export directory and writes `oit.client.pem` and `oit.client.key` automatically when absent. The verification additionally invoked `node aspnetcore-https.js` with `npm_package_name=oit.client` to inspect the newly generated files before starting `npm start`; this extra invocation is not required for setup.

In a second PowerShell terminal, paste the checkout path and database name printed in step 3 when prompted, then start the server:

```powershell
$checkoutPath = Read-Host 'Checkout path from step 3'
$checkDatabase = Read-Host 'Database name from step 3'
Set-Location $checkoutPath
$env:ConnectionStrings__OrderIntake = "Server=(localdb)\OITDevelopment;Database=$checkDatabase;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run --project OIT/OIT.Server --launch-profile https
```

PowerShell variables are terminal-local, so the second terminal needs these values again. Leave its APPDATA unchanged. Starting the client separately exercises the README's documented fallback and avoids relying on an already configured Visual Studio startup selection.

### 6. Smoke-test the running application

Open `https://localhost:61009/orders` without bypassing a certificate warning. Confirm the initial database has no orders or clients. Create an order with:

| Field | Test value |
| --- | --- |
| Reference | FRESH-001 |
| New client name | Fresh Checkout Client |
| New client email | fresh-checkout@example.com |
| Currency | USD |
| SKU / item name | FRESH-SKU / Fresh checkout item |
| Quantity / unit price | 3 / 19.99 |

The saved order should be Pending and show subtotal/total **59.97**. Verify the new client is available and the order appears in the list. Confirm HTTPS responses with normal certificate validation:

```powershell
Invoke-WebRequest https://localhost:61009/orders
Invoke-RestMethod https://localhost:61009/api/orders
Invoke-RestMethod https://localhost:61009/api/clients
Invoke-WebRequest https://localhost:7145/openapi/v1.json
```

Stop the API with Ctrl+C, restart it using the same database connection, and refresh the saved order URL. The order, client, and total must remain. Stop both processes when finished and close the verification browser tab.

## Recorded results

| Task | Result | Completed |
| --- | --- | :---: |
| Tool and solution restore | `dotnet-ef` 10.0.12 and .NET packages restored successfully. | ☑ |
| Frontend restore | `npm ci` installed 281 packages; audit reported zero vulnerabilities. | ☑ |
| Database migration | New database and schema created from the committed migration. | ☑ |
| Solution build | Passed with zero warnings and zero errors. | ☑ |
| Backend tests | 33 passed, zero failed/skipped. | ☑ |
| Frontend tests | 22 passed across three test files. | ☑ |
| Production Angular build | Passed; initial bundle 290.77 kB, within budgets. | ☑ |
| Certificate export | Fresh PEM/key generated; certificate was already trusted in the shared Windows account. | ☑ |
| HTTPS client/API/proxy | Browser opened the app normally; API/proxy/OpenAPI requests succeeded with validation enabled. | ☑ |
| Empty-database startup | No orders or clients appeared before smoke-test creation. | ☑ |
| Order/client smoke test | Exactly one client and one Pending order, FRESH-001, total 59.97. | ☑ |
| Restart persistence | Order and total remained after API restart and browser refresh. | ☑ |
| Checkout cleanliness | `git status --short` was empty after restores, builds, tests, and startup. | ☑ |
| Cleanup | Verification processes stopped; separate clone/profile/database retained for inspection. | ☑ |

Observed non-blocking messages: npm reported install-script policy warnings and a `run-script-os` deprecation warning; restores, builds, and tests still succeeded. The API reported a missing `wwwroot` during development, where the Angular development server serves the UI. These messages did not block the documented development workflow.

The original checkout and its `OrderIntake` database were not used for smoke-test writes. A clean VM would be the next level of verification for installing prerequisites and creating Windows-level trust/LocalDB configuration from scratch.
