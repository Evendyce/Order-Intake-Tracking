# Order Intake Client

Angular views and components for orders and clients. See the [repository README](../../README.md) for prerequisites, LocalDB setup, API startup, proxy configuration, and test instructions.

The development server is configured on port **61009**, not Angular's default 4200. `npm start` uses the generated local HTTPS certificate; the root README also documents the verified local HTTP workflow.

```powershell
npm ci
npm test -- --watch=false
npm run build
```

Views are lazy-loaded from `src/app/domains/orders/views` and `src/app/domains/clients/views`. Each domain keeps its models, API services, and reusable components together. Shared feedback and confirmation components live under `src/app/shared`.

The root application provides navigation and a router outlet. UI rendering uses Angular components with external HTML templates. Vitest tests cover bounded read retries and orders-page loading/error states. Unavailable API reads retry up to eight times, one second apart; writes are never replayed. Backend test coverage and browser verification are documented in the root README and SOLUTION.

## Frontend design notes

Template-driven forms suit the small intake/profile screens. Reusable field validators and feedback components add whitespace, whole-quantity, and price-precision checks without changing the user's input. Default change detection is used where mutable `NgModel` state needs to stay visible. The line editor owns standalone controls, so the order form checks its aggregate validity before submitting.

API services own HTTP contracts; views own loading, draft, confirmation, and error state. Order edit eligibility and status actions come from the server response. After writes, views use the returned resource or reload the list to obtain current versions and persisted values. Client editing uses a local draft so cancellation leaves loaded profiles intact.

Native dialogs provide modal focus containment and Escape handling. Duplicate feedback opens the saved order for explicit review rather than applying submitted differences automatically. Failed reads and failed writes remain distinct from valid empty results.

See the [design-decision guide](../../docs/DESIGN_DECISIONS.md) for source links, trade-offs, and extension points. The optional `main-ui-overhaul` branch changes presentation using VoidGlass; `main` remains the assignment review branch.
