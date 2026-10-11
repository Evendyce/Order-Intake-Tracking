# UI review checklist

Review the assignment on `main` before starting the UI redesign.

Reviewer: Donovan Minnie
Date: 2026/10/11
Commit: `fa29f7d`
Browser/version: Microsoft Edge

Use disposable test records. Keep other copies of the application stopped when checking startup or API recovery.

## How to record results

Use `Not checked`, `Passed`, `Issue found`, or `Not applicable` in the Result column. Replace `☐` with `☑` in Completed after running a check and recording its outcome; completion does not mean it passed. Explain any Not applicable result in Notes.

Check Orders, New/Edit Order, Order Detail, Clients, and their dialogs wherever applicable. Record the screen and reproduction steps for each issue. These practical checks support an accessibility review; they do not establish full accessibility compliance.

## Accessibility

| Task | Pass condition | Result | Notes / issue ID | Completed |
| --- | --- | --- | --- | :---: |
| Navigate each screen using Tab and Shift+Tab. | Every interactive control is reachable in a sensible order, with visible focus. | Passed | | ☑ |
| Activate links/buttons with Enter and buttons with Space; use keyboard controls for selects. | Actions work without a mouse. | Passed | | ☑ |
| Open duplicate and confirmation dialogs using the keyboard. | Focus enters the dialog and remains inside while it is open. | Passed | | ☑ |
| Close dialogs using their controls and Escape where dismissal is supported. | Focus returns to the triggering control; dismissal makes no unintended changes. | Passed | | ☑ |
| Inspect input labels and required-field instructions. | Fields have meaningful labels; required inputs are identifiable. | Passed | Original finding resolved: Required labels, Optional notes, and field-specific validation added. Codex retested order/client forms on 11 October 2026; details below. | ☑ |
| Submit an incomplete form and an invalid email. | Errors identify the affected fields and explain how to correct them. | Passed | | ☑ |
| Save a record, trigger an error, and change a status. | Feedback is clear and does not rely only on colour. | Passed | | ☑ |
| Read the screens with a screen reader, if available. | Headings, control names, table headers, dialogs, and feedback are understandable. |  Not checked | Screen-reader testing skipped | ☐ |
| Inspect text, controls, and focus contrast; use a contrast checker if available. | Text and controls remain distinguishable, including muted text and status badges. | Passed | | ☑ |
| Run an automated accessibility scan, if available. | Findings are investigated and documented alongside the manual checks. | Passed | (Performance) 54; (Accessibility) 100; (Best Practices) 100; (SEO) 90. Made use of Lighthouse (Inspection Window) | ☑ |

## Responsive layout

Use browser developer tools to set viewport widths. Check every screen and dialog at each width, including forms with validation errors and long content.

| Task | Pass condition | Result | Notes / issue ID | Completed |
| --- | --- | --- | --- | :---: |
| Review at approximately 375px wide. | Navigation, inputs, and buttons are usable without overlap or clipping. | Passed | | ☑ |
| Review at approximately 768px wide. | Layout remains readable and controls fit. | Passed | | ☑ |
| Review at a normal desktop width. | Content and actions are clearly arranged. | Passed | | ☑ |
| Review at 200% browser zoom. | Text stays readable and controls remain accessible. | Passed | | ☑ |
| Inspect order/client tables at narrow widths. | All columns/actions remain accessible through a usable layout or table scrolling. | Passed | Checked at 375px Width | ☑ |
| Open dialogs at narrow widths and zoomed views. | Content and dismissal/confirmation buttons remain reachable. | Passed | Checked at 375px Width | ☑ |
| Enter long references, names, notes, and item descriptions. | Content wraps or truncates understandably without breaking the layout. | Passed | Checked at 375px Width | ☑ |

## Edge cases and recovery

Performed by Codex on 11 October 2026 against main commit `fa29f7d` using the Codex in-app browser, isolated HTTPS ports, and a separate LocalDB instance/database. See [edge-case review report](EDGE_CASE_REVIEW.md) for scope and environment notes.

| Task | Pass condition | Result | Notes / issue ID | Completed |
| --- | --- | --- | --- | :---: |
| View an empty order/client list in an isolated database. | A helpful empty state appears with an appropriate next action. | Passed | Empty order/client tables showed useful next actions in the isolated database. | ☑ |
| Apply filters that match no orders, then reset them. | The empty result is clear; Reset restores the unfiltered list. | Passed | NO-MATCH returned the empty state; Reset restored EDGE-001. | ☑ |
| Submit whitespace-only required fields and an invalid email. | Invalid values are rejected without saving a partial record. | Passed | UI rejected invalid email; API rejected whitespace reference/client name with HTTP 400. No partial records. | ☑ |
| Try zero/negative quantities and negative prices. | Values follow the documented validation rules; errors are useful. | Passed | Quantity 0/-1 and price -1 were rejected in the UI. | ☑ |
| Try a zero unit price and decimal quantity. | Behaviour follows the documented rules and is explained by validation where invalid. | Passed | Zero price saved EDGE-ZERO at 0.00; quantity 1.5 was rejected in UI and API. | ☑ |
| Create an order with quantity 3 and unit price 19.99. | Saved line total, subtotal, and total are 59.97, calculated by the server. | Passed | EDGE-001 saved line total/subtotal/total 59.97. | ☑ |
| Remove all line items and attempt to save. | An order cannot be saved without the required line items. | Passed | Last-line Remove was disabled; API rejected an empty item array with HTTP 400. | ☑ |
| Submit an identical duplicate order. | The existing order is identified; no additional order is created. | Passed | Identical submission showed the existing order overview; no extra order. | ☑ |
| Submit an existing reference with changed order details. | The duplicate overview identifies differences and provides a clear modification path. | Passed | Changed Notes produced a duplicate dialog listing Notes as different. | ☑ |
| Follow the duplicate dialog's modification action. | The correct existing order opens with its saved details. | Passed | Modify existing order opened EDGE-001 with saved values, without applying submitted differences. | ☑ |
| Double-click Save or press it repeatedly during submission. | One intended record results; submission feedback is clear. | Passed | Double-click Submit produced one EDGE-001; controls disabled while saving. | ☑ |
| Create an order with new client information. | The client is created and the order links to it. | Passed | EDGE-001 auto-created Edge Client and linked its ID. | ☑ |
| Use an existing client's email with different casing/outer whitespace. | Client matching follows the documented normalization rules without unintended duplicates. | Passed | Outer whitespace/uppercase EDGE@EXAMPLE.COM reused Edge Client without overwriting its profile. | ☑ |
| Add/edit a client using another client's email. | A useful duplicate-email error appears and the client list remains usable. | Passed | Add and edit duplicate-email attempts showed a clear error; table stayed usable. | ☑ |
| Deactivate a client with historical orders, then reactivate it. | Historical orders remain readable; client availability follows the documented rules. | Passed | Remove deactivated the referenced client and preserved order history; Edit/reactivation restored selection. | ☑ |
| Inspect available actions on a Pending order. | Confirm/Cancel are offered; direct Fulfil is unavailable. | Passed | Pending offered Mark Confirmed and Cancel order; Fulfilled was unavailable. | ☑ |
| Confirm an order, then inspect its available actions. | Fulfil/Cancel are offered; invalid backward transitions are unavailable. | Passed | Confirmed offered Mark Fulfilled and Cancel order; no backward action. | ☑ |
| Inspect Fulfilled and Cancelled orders. | Terminal orders cannot transition further or have their details modified. | Passed | Fulfilled/Cancelled had no further actions; direct edit routes showed the lock message. API rejected a terminal transition with 409. | ☑ |
| Edit the same Pending order in two tabs; save one, then the other. | The stale save receives clear conflict feedback and does not overwrite the newer changes. | Passed | Second tab received This order changed. Reload it before saving.; Winning update persisted. | ☑ |
| Refresh an order detail URL and open it directly in a new tab. | The correct order loads through normal routing. | Passed | Reload and direct detail navigation loaded EDGE-001 correctly. | ☑ |
| Open an order detail URL containing an unknown ID. | A useful not-found state appears with a route back to the list. | Passed | Unknown GUID showed Order not found. and a link back to Orders. | ☑ |
| Cancel an edit or confirmation dialog. | Unsaved changes are not persisted. | Passed | Cancelling edits kept saved Notes; Keep unchanged dismissed cancellation without changing status. | ☑ |
| Stop the API and attempt a read; restart it and retry. | Loading/error feedback is useful and the screen recovers. | Passed | API outage ended with useful feedback; after restart Apply filters restored orders and clients. | ☑ |
| Stop the API before submitting a write, then restart it. | Failure feedback is clear; the write is not automatically replayed. | Passed | Offline submission showed connection feedback and re-enabled Submit; restart created no EDGE-OFFLINE order. | ☑ |
| Launch normally with both projects stopped initially. | The UI loads as the API becomes ready, without requiring a browser refresh. | Passed | Both test services were restarted; the initial loading page populated automatically when API became ready. Isolated ports; Visual Studio launch was not repeated. | ☑ |

## Issue log

### Required-field finding: resolved and retested

Codex retested the working-tree update on 11 October 2026 using the in-app browser and the isolated OITEdgeReview database/HTTPS services. The original reviewer and commit above refer to the initial review.

| Task | Recorded result | Completed |
| --- | --- | :---: |
| Clear field requirements | Order/client/line-item inputs show Required; Notes show Optional; forms explain the labels. | ☑ |
| Empty submission | Specific errors appear for missing fields. Untouched forms initially show no errors. | ☑ |
| Invalid values | Whitespace names, invalid email, fractional quantity, and excessive price precision show field-level explanations. | ☑ |
| Accessible feedback | Invalid inputs expose aria-invalid and aria-describedby links to their error messages. Errors remain outside labels so control names remain stable. Screen-reader announcement behaviour was not tested. | ☑ |
| Correction and saving | Errors cleared after correction; REQUIRED-001 saved at 59.97, and a new client saved and reopened for editing without stale errors. | ☑ |
| Responsive retest | Order/client validation layout fitted a 375px viewport without horizontal overflow. | ☑ |
| Automated regression checks | All 25 frontend tests passed across five files; production Angular build passed. | ☑ |

The required-field issue is resolved. This retest does not constitute a new Lighthouse scan or a screen-reader test. All writes remained in the isolated database.

Copy this block for each finding:

```text
Issue ID:
Screen / route:
Browser / viewport / zoom:
Steps to reproduce:
Expected behaviour:
Actual behaviour:
Screenshot or recording (optional):
Severity: blocks use / affects use / cosmetic
Resolution:
Retest result:
```

## Review completion

- [x] Every performed check has a recorded outcome, and intentionally skipped checks are identified.
- [x] Issues have reproduction details and a recorded resolution or remaining limitation.
- [x] Fixes have been retested on the affected screens and viewport sizes.
- [x] Remaining limitations are reflected in the submission documentation.
- [x] The scoped accessibility/responsive/edge-case review task is marked complete based on the recorded results.

Overall outcome: Passed for the documented scope after the required-field fix and retest.

Remaining issues / limitations: Screen-reader testing intentionally skipped; Lighthouse was not rerun after the fix. The LocalDB shell startup issue is documented in the edge-case report. Performance diagnostics remain separate from this review.
