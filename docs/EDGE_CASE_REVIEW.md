# Edge cases and recovery review

Performed by Codex on 11 October 2026 against `main`, commit `fa29f7d`. All 25 rows in the [edge-case checklist](UI_REVIEW_CHECKLIST.md#edge-cases-and-recovery) passed the recorded scenarios. No application changes were needed.

## Environment and scope

The browser walkthrough used the Codex in-app browser and the existing development build. The client ran at `https://localhost:62010`, proxying to an API at `https://localhost:7246`. Certificate validation remained enabled for browser/API requests.

Writes used a new database, `OIT_EdgeReview_20261011`, on a separate `(localdb)\OITEdgeReview` instance. The normal `OrderIntake` database was not used for test writes. Two tabs exercised stale edits; stopping only the verification API exercised outages. Both verification services were stopped after review. Test records and the separate instance/database were retained for inspection.

The first attempt to start a test API against `OITDevelopment` failed before listening: LocalDB reported that its SQL Server process failed to start. A direct `SqlLocalDB start OITDevelopment` also failed. Creating and starting `OITEdgeReview` succeeded, allowing review to continue without modifying the normal instance. This environment issue remains separate from the passing UI scenarios; its cause was not established in this review.

Startup recovery was checked using the isolated ports: start the client before the API, load Orders while requests are retrying, start the API, and verify that records appear without another browser refresh. Visual Studio's normal launch sequence was not repeated in this review; its earlier verification is recorded in the existing documentation.

## Recorded evidence

| Task | Observed result | Completed |
| --- | --- | :---: |
| Empty states, filtering and reset | Empty order/client lists provided next actions; a nonmatching reference returned no results; Reset restored records. | ☑ |
| Validation and totals | UI rejected invalid email, quantities 0/-1/1.5, and negative prices. API rejected whitespace reference/client name, empty items, and fractional quantity with HTTP 400. Quantity 3 at 19.99 saved 59.97; zero price saved 0.00. | ☑ |
| Submission and duplicates | Double-clicking Submit created one order. Identical and changed submissions showed the existing overview; changed Notes were listed. Modification opened the saved order without automatically applying differences. | ☑ |
| Clients | Order creation auto-created a client; normalized matching reused it without profile changes. Duplicate-email errors during add/edit kept the list usable. Deactivation preserved history and excluded the client from new-order selection; reactivation restored it. | ☑ |
| Status rules | Pending allowed Confirm/Cancel; Confirmed allowed Fulfil/Cancel. Fulfilled and Cancelled were locked, including direct edit routes. An API terminal transition attempt returned HTTP 409. | ☑ |
| Concurrent edits | Two tabs loaded the same order version. The first saved Winning update; the second received a conflict message and did not overwrite it. | ☑ |
| Routing and cancellation | Refresh/direct navigation loaded saved details. Unknown GUID showed Order not found with an Orders link. Cancelling edits or a cancellation dialog preserved the saved state. | ☑ |
| Read outage recovery | Loading finished with Cannot reach the API feedback. Restarting the API and selecting Apply filters restored the list and clients. | ☑ |
| Failed writes | An offline submission returned connection feedback and re-enabled Submit. After restart, the API still held exactly two orders; EDGE-OFFLINE was absent. | ☑ |
| Startup recovery | Initial loading automatically populated when the test API became available during the read retry window. | ☑ |
| Existing frontend regression tests | `npm test -- --watch=false`: 22 tests passed across three files. | ☑ |

Final records: `EDGE-001` (Fulfilled, 59.97) and `EDGE-ZERO` (Cancelled, 0.00); two clients, including the second client created to test duplicate-email edits. No invalid-input or failed-write records were created.

The full backend test suite was not rerun during this review: its integration fixtures use `OITDevelopment`, whose startup failed in this shell. Backend boundary checks above were performed through the isolated running API instead. Earlier full-suite results remain documented in the fresh-checkout report.

## Remaining review work

The required-field indicator issue recorded in Accessibility remains outstanding. Screen-reader testing is intentionally skipped. These edge-case passes do not mark the complete accessibility/submission review finished, or establish accessibility compliance. Performance diagnostics and the final GitHub submission review remain separate tasks.
