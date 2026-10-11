# Order Intake Client

Angular views and components for orders and clients. See the [repository README](../../README.md) for prerequisites, LocalDB setup, API startup, proxy configuration, and test instructions.

The development server is configured on port **61009**, not Angular's default 4200. `npm start` uses the generated local HTTPS certificate; the root README also documents the verified local HTTP workflow.

```powershell
npm ci
npm run build
```

Views are lazy-loaded from `src/app/domains/orders/views` and `src/app/domains/clients/views`. Each domain keeps its models, API services, and reusable components together. Shared feedback and confirmation components live under `src/app/shared`.

The root application provides navigation and a router outlet. UI rendering uses Angular components with external HTML templates. Frontend automated tests are not yet implemented; backend test coverage and browser verification are documented in the root README and SOLUTION.
