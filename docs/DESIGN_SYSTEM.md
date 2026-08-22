# KeyForge Design System

## Visual direction

KeyForge uses a dark-first, minimal interface with quiet neutral surfaces and a violet accent. Green communicates completion, amber communicates caution, and red is reserved for errors. Interfaces should feel focused and professional, with decoration kept secondary to practice content.

## Tokens

Tokens live in `src/KeyForge/wwwroot/app.css` and use the `--kf-*` prefix.

- Colors: `background`, `surface`, `surface-elevated`, three text levels, `primary`, `success`, `warning`, `error`, `border`, and overlay. Soft and hover variants support accessible states without repeating color values; the elevated shadow is tokenized as well.
- Spacing: `--kf-space-1` through `--kf-space-6` map to 4, 8, 12, 16, 24, and 32px.
- Radius: `--kf-radius-control`, `--kf-radius-card`, and `--kf-radius-container`.
- Typography: the system sans-serif stack plus body, small/meta, section-title, and responsive page-title sizes.

## Reusable classes

- Layout: `.kf-page-container`, `.kf-page-header`, `.kf-page-header__description`, `.kf-card-grid`
- Typography: `.kf-page-title`, `.kf-section-title`, `.kf-body-text`, `.kf-text-secondary`, `.kf-text-muted`, `.kf-meta`
- Card: `.kf-card`, `.kf-card__body`, `.kf-card__header`, `.kf-card__title`, `.kf-card__description`, `.kf-card__meta`
- Buttons: `.kf-button` with `.kf-button--primary` or `.kf-button--secondary`
- Badge: `.kf-status-badge` plus a status modifier
- Progress: `.kf-progress` and `.kf-progress__value`; set `--kf-progress-value` to a percentage and provide the appropriate ARIA progressbar attributes

Prefer these CSS classes over new Razor components until shared behavior, not just shared appearance, justifies a component.

## Lesson status

Use matching card and badge modifiers:

| Status | Card | Badge |
| --- | --- | --- |
| Locked | `.kf-card--locked` | `.kf-status-badge--locked` |
| Available | `.kf-card--available` | `.kf-status-badge--available` |
| Completed | `.kf-card--completed` | `.kf-status-badge--completed` |

Status selection belongs to presentation mapping; lesson unlocking and completion decisions remain in the existing application services.

## UI rules

- Build pages inside the shared layout container and start them with `.kf-page-header`.
- Use spacing and color tokens instead of one-off values.
- Keep one clear primary action per section; use secondary buttons for alternatives.
- Never rely on color alone: pair statuses with visible text and preserve focus outlines.
- The card grid uses auto-fit columns for desktop and tablet, then naturally collapses on narrow screens. The application shell switches between mobile navigation and a fixed sidebar at one 48rem breakpoint.
