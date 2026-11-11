# Takwene catalogue frontend

Angular 22, standalone components, strict TypeScript and templates, Router, and HttpClient.
This is a browser application with no server rendering, editing, authentication screens, audio playback,
polling, or analytics. The supplied Takwene logo is preserved unchanged.

## Setup

Use Node **>=24.15.0 <25** and npm 11. Implementation was verified with Node 24.22.2 and npm 11.19.1.
Angular runtime/compiler 22.2.5, CLI/build 22.2.6, TypeScript 6.0.3, and RxJS 7.8.2 are pinned in `package.json` and `package-lock.json`. Angular's [compatibility table](https://angular.dev/reference/versions)
documents the supported combinations.

```bash
npm ci
npm start
```

These commands run from `frontend/`. If using nvm on macOS, Linux, or WSL, run `nvm install` followed
by `nvm use` first. If Node 24 and npm 11 are already installed, nvm is unnecessary.

Open `http://localhost:4200/tracks`. Run the existing API separately on `http://localhost:5000`;
see [the setup guide](../docs/README.md). `proxy.conf.json` owns the local API target. Components
only use relative `/api` URLs. No JWT is needed for these two GET endpoints. Frontend unit tests were omitted at the author’s request; verification uses compilation and live browser checks.

```bash
npm run build                 # dist/takwene/browser
npm run typecheck             # Angular compiler, including templates, with no output
npm run format:check          # authored files only
npm run format               # format authored files
```

## Routes and ownership

| Path                 | Behaviour                                                                                                                             |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| `/tracks`            | Paginated catalogue. `status=draft\|submitted\|distributed` and positive `page` preserve selection. Defaults are all tracks/page one. |
| `/tracks/:id`        | Independent detail GET; the same query context is carried into its return link. Invalid GUIDs and HTTP 404 show a missing-track view. |
| `/` or unknown paths | Redirect to `/tracks`.                                                                                                                |

```text
src/app/
  app.*                         shell, configuration, routes
  page-focus.directive.ts       focus newly rendered page headings
  shared/                       decorative signal artwork and loading indicator
  tracks/
    data/                       contracts, validated query rules, two GET requests
    pages/track-list/            catalogue state, filters, rows, pagination
    pages/track-detail/          selected track state, metadata, return context
    components/
      status-label/             separate track and distribution status domains
      distribution-map/         semantic HTML for the returned deliveries
      release-date.pipe.ts      timezone-safe calendar dates
public/brand/                   supplied Takwene logo
public/assets/                  unchanged skill CSS and licensed local icons
```

Each routed page owns one request stream. `switchMap` cancels obsolete requests; request-local
error handling keeps retry and subsequent route changes working. Equivalent normalized queries
do not refetch. Filtering resets the page to one. There is no global store or persistent list cache.
The API boundary checks required response fields and collections; malformed responses enter a recoverable
failure state. Unknown status strings have a neutral `Unknown status` label.

Loading, empty catalogue, no matching status, out-of-range page, request failure with retry, and
missing track are separate states. A failed request never substitutes fixtures. Counts and page
controls use returned API metadata. A distributed track can still show a rejected DSP delivery.
Only returned services are drawn; absent services receive no invented status.

Release dates are calendar dates, formatted explicitly in UTC without shifting the day. Submission
timestamps are instants, formatted using Angular DatePipe in **UTC**, with visible timezone labels
and machine-readable `<time datetime>` values. Invalid calendar dates display `Date unavailable`.

Keyboard focus moves to a newly activated page heading and stays on a selected filter during filtering.
Native controls expose pressed/disabled states. Shared entrance classes live in `src/motion.css`;
component-specific motion stays in the component stylesheet. Animations run automatically and stop
under `prefers-reduced-motion`; the distribution map remains complete.

## Production hosting

Serve `dist/takwene/browser` over HTTPS. Reverse proxy `/api/*` to the .NET API **before** applying
the SPA fallback. Serve existing frontend assets normally; other frontend routes, including direct
`/tracks/:id` requests, must fall back to `index.html`. Do not rewrite API failures to HTML.
The development proxy is not part of the production build. No deployment domain is assigned;
set canonical and absolute social image URLs when one is supplied.

Funnel Display, Funnel Sans, and DM Mono load from Google Fonts with local system fallbacks.
DSP and navigation icons are local; source, licence, and trademark records are in
`public/assets/icons/icons-credits.json`. No stock photographs, cover art, or fabricated endorsements
are used.
