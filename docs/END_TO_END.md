# End-to-end verification

Verified on 11 October 2026 using the actual Angular screens, ASP.NET Core API, and SQL Server LocalDB. The browser session used client port 61010 and API port 5215 over local HTTP, with the connection overridden to `OIT_E2E_20261011` on `(localdb)\OITDevelopment`. This separate database protected the existing `OrderIntake` records. The test database is retained for repeat checks; temporary test services were stopped after verification.

## Task list: browser workflows

| Task | Observed result | Completed |
| --- | --- | :---: |
| Required-field validation | Empty order submission showed validation feedback and stayed on the form. | ☑ |
| Numeric validation | Fractional quantity and three-decimal price were rejected. | ☑ |
| Create order and client | New-client intake saved one client and one Pending order. | ☑ |
| Multiple lines and totals | Three items at 19.99 plus two at 5.50 produced line totals 59.97 and 11.00, and total 70.97. | ☑ |
| Edit Pending order | Increasing the first quantity to four recalculated the total to 90.96. | ☑ |
| Conflicting duplicate | Lowercase reference with surrounding spaces matched the saved reference; popup showed the saved overview and Notes/Line items differences. | ☑ |
| Duplicate modification | Modify action opened the saved order with its original values; submitted differences were not silently applied. | ☑ |
| Identical duplicate | Popup reported matching content; no second order was created. | ☑ |
| Directional statuses | Pending exposed Confirmed/Cancelled; Confirmed exposed Fulfilled/Cancelled. | ☑ |
| Fulfilment | Pending → Confirmed → Fulfilled succeeded; terminal actions and modification link disappeared. | ☑ |
| Pending cancellation | Dismissing confirmation preserved Pending; accepting it produced terminal Cancelled. | ☑ |
| Confirmed cancellation | Confirmed → Cancelled succeeded and became terminal. | ☑ |
| Locked edit route | Opening a Fulfilled order's edit URL directly displayed the editing lock. | ☑ |
| Client snapshots | Client rename appeared in the client list; an earlier order retained its original client name. | ☑ |
| Email matching | Uppercase matching email reused the existing client and retained the edited profile. | ☑ |
| Currency and zero price | ZAR decimal order saved total 0.20; zero-priced USD order saved total 0.00. | ☑ |
| Filters and reset | Status, client, partial reference, combined filters, reset, and no-match feedback returned the expected rows. | ☑ |
| Client validation and creation | Invalid email disabled Save; valid input created a client. | ☑ |
| Unreferenced client removal | Confirmation deleted the disposable test client. | ☑ |
| Referenced client removal | Client became Inactive while order history remained; editing allowed reactivation. | ☑ |
| Inactive client intake | Inactive client was excluded from new-order selection; email matching was rejected with reactivation guidance. | ☑ |
| Duplicate client email | Save returned clear feedback and retained the loaded client table after the correction below. | ☑ |
| Concurrent order editing | Second tab's stale save was rejected; refresh recovered the newer notes. | ☑ |
| Keyboard dismissal | Escape closed the cancellation dialog without changing status. | ☑ |
| Missing order | Unknown valid GUID showed Order not found without a permanent loading indicator. | ☑ |
| Reload and restart persistence | Browser refresh and API restart retained four orders, statuses, totals, and the winning edit. | ☑ |

## Task list: API and automated checks

| Task | Observed result | Completed |
| --- | --- | :---: |
| Invalid status jump | Direct Pending → Fulfilled request returned HTTP 409. | ☑ |
| Validation and atomic intake | Invalid quantity returned HTTP 400 and did not create its supplied new client. | ☑ |
| Invalid filter | Unknown status filter returned HTTP 400. | ☑ |
| Missing resources and malformed ID | Missing order, malformed order ID, and unknown API route returned HTTP 404. | ☑ |
| OpenAPI | Development OpenAPI document returned HTTP 200. | ☑ |
| No duplicate side effects | Final database held exactly four orders and one client. | ☑ |
| Backend suite | 33 xUnit cases passed, including SQL persistence and HTTP integration tests. | ☑ |
| Frontend suite | 22 Vitest cases passed, including the two new client-list recovery cases. | ☑ |
| Production Angular build | Build passed within configured budgets; initial bundle 290.77 kB. | ☑ |

## Correction made

The client list's `loadFailed` flag was incorrectly set by the save error handler instead of the list-loading error handler. A duplicate-email save consequently hid otherwise valid loaded rows. The flag now represents a failed read; failed writes leave the list available. Regression tests verify both duplicate-save recovery and loading failure/recovery, and the duplicate-email workflow was repeated successfully in the browser.

## Final test records

| Reference | Status | Currency | Total | Completed |
| --- | --- | --- | ---: | :---: |
| E2E-001 | Fulfilled | USD | 90.96 | ☑ |
| E2E-002 | Cancelled | ZAR | 0.20 | ☑ |
| E2E-003 | Cancelled | USD | 0.00 | ☑ |
| E2E-004 | Pending | USD | 10.00 | ☑ |

This pass covers the core business workflows. A fresh-checkout installation, comprehensive accessibility/responsive review, and browser certificate trust remain separate delivery checks. The user confirmed the normal Visual Studio HTTPS startup loaded the saved development order automatically before this isolated pass.
