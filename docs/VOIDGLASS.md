# VoidGlass integration

This personal redesign lives on `main-ui-overhaul`. Review the `main` branch for the technical assignment.

## Source and ownership

VoidGlass is Donovan Minnie's theme, brought across from his [RecipeShare repository](https://github.com/Evendyce/RecipeShare-Complete-/tree/main/RecipeShare.Web/wwwroot/voidglass). The original SCSS token maps, theme mixin, palette application, animated vector background, static vector background, and theme license are retained. The application footer displays the original theme and author credit.

RecipeShare's Blazor components are adapted as Angular components and shared styles. The order/client workflows continue to use their existing domain folders and external HTML templates.

## Files and customization

| File or folder | Purpose |
| --- | --- |
| `OIT/oit.client/src/theme/voidglass/themes/` | Original dark/light palettes and their CSS variable output. |
| `OIT/oit.client/src/theme/voidglass/voidglass.scss` | Form-friendly surface, foreground, border, feedback, and background adaptations. |
| `OIT/oit.client/src/styles.scss` | Shared layout, controls, tables, and dialogs; imports VoidGlass through Angular's Sass build. |
| `OIT/oit.client/src/app/shared/theme/` | Angular preference service and tests. |
| `OIT/oit.client/src/app/shared/components/theme-toggle/` | Accessible theme button with external template and styles. |
| `OIT/oit.client/public/voidglass/` | Original background assets and early preference script. |
| `OIT/oit.client/src/theme/voidglass/LICENSE.txt` | Original VoidGlass attribution terms. |

Dark is the default. The header button switches between dark and light, retaining the selection in `localStorage` under `vg-theme`, the same key used by RecipeShare. A small external script applies the preference before Angular renders, avoiding a palette flash. If browser storage is blocked, the app still switches theme for the current session.

Semantic colors keep operational text readable: cyan buttons use a dark foreground, light-mode primary controls use a darker blue, and each palette has its own muted text, error text, input border, and modal surface. Original palette tokens remain available as `--vg-*`. Panels use backdrop blur with sufficiently opaque fallback surfaces; the geometric background stays decorative and ignores pointer input.

The reduced-motion media query replaces the animated SVG with its original static counterpart and removes button transitions. Forced-colors mode hides decorative backgrounds and disables blur. The skip link focuses the main content without changing the active route.

## Verification — 11 October 2026

Browser checks used the isolated `OITEdgeReview` LocalDB instance and `OIT_EdgeReview_20261011` database, with API port 7246 and Angular port 62010. They did not use the application's normal database.

| Task | Completed |
| --- | :---: |
| Integrate original palettes, vector assets, attribution, and glass surfaces. | ☑ |
| Add Angular theme switcher and persist the preference across reloads. | ☑ |
| Build production assets within the existing bundle budgets. | ☑ |
| Run all frontend tests: 27 passed, including two theme preference tests. | ☑ |
| Inspect dark/light order lists, client management, and field validation. | ☑ |
| Inspect narrow-screen forms and local table scrolling at a 375px viewport. | ☑ |
| Inspect duplicate overview in dark mode and client confirmation in light mode; cancel removal. | ☑ |
| Inspect status progression with readable current-status foreground. | ☑ |
| Add reduced-motion and forced-colors CSS fallbacks. | ☑ |
| Re-run a full accessibility audit and screen-reader checks for the redesign. | ☐ |

This is targeted theme regression verification. The earlier main-branch UI checklist remains the assignment baseline; its accessibility results are not a fresh audit of this redesign. Backend code was unchanged by the theme integration.
