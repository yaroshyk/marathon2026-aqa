# Restaurant AQA framework — group pre-order homework

**Direction:** 1 — group pre-order. I implemented and deployed both the backend (homework 5) and the frontend (homework 6),
so this suite tests my own deployment: UI scenarios for the main flows and API scenarios for authorization and validation.

**Approach:** BDD + Screenplay, on top of the course framework (Reqnroll + TUnit + Playwright for .NET). Gherkin keeps every
rule readable for a non-developer; Screenplay lets one actor play every persona of a scenario (the owner, guests, another
customer, a waiter, a visitor) through tasks and questions, while page objects keep the locators in one place.

| Feature file | Level | What it covers | UI | API |
|---|---|---|---|---|
| `Features/GroupPreOrderParticipants.feature` | L1 | the owner sees guests, dishes, quantities, subtotals and the total; the total follows cart changes; unsubmitted and empty carts; empty group; only the owner sees the list; unknown group | 6 | 10 |
| `Features/GroupPreOrderSubmission.feature` | L2 | only submitted carts are sent and equal dishes are merged; the order gets `SubmittedByCustomer`, the group `Submitted`; every reason a submit is refused; repeated submit; only the owner submits | 4 | 9 |
| `Features/GroupPreOrderRemoval.feature` | L3 | removing one of several guests, a guest who submitted, the last guest; a guest of another group; an unknown guest; only the owner removes | 1 | 8 |
| `Features/GroupPreOrderWaiter.feature` | L2, L3 | the waiter sees the submitted pre-order; end to end: guests add dishes → the owner reviews → removes a guest → submits → the waiter sees the order without the removed guest | 2 | — |

40 tests in total (Scenario Outline examples counted separately); tags: `@group-preorder`, `@L1`/`@L2`/`@L3`, `@ui`/`@api`,
`@negative`, `@smoke`, `@e2e`.

Test data is prepared through the API, as the test pyramid allows: the owner books a table, guests join by the invite link,
fill and submit their carts, and the owner places their own pre-order. What the owner and the waiter see is then checked on
the screen (`@ui`) or in the service's answer (`@api`). Expected values are computed from what the guests did and the menu
prices, never copied from the application's answer.

### Behaviour chosen where the homework does not define it

| Case | Behaviour the tests expect |
|---|---|
| Someone signed in who is not the owner (another customer, a waiter) | refused: `403`, `"Only the reservation owner can perform this action."` |
| Not signed in (a visitor, or a guest with only a guest token) | asked to sign in: `401` |
| A group nobody joined | `participants: []`, `total: 0` |
| Submitting a group that was already submitted | `400`, `"Group reservation was submitted."` (error code `GroupReservationSubmitted`) |
| Removing the last guest | allowed; the answer is `participants: []`, `total: 0` |
| Removing a guest of another group | `400`, `"Guest session does not belong to this group reservation."` |
| Removing a guest who does not exist / an unknown group | `404` |
| The owner has no pre-order of their own | `400`, `"Order for reservation '<id>' not found."`; on the screen the same text comes as an error toast and the group stays open |

One rule cannot be set up through the public API: *"the submitted carts contain no dishes"*. The service refuses to submit
an empty cart and refuses to change a submitted one, so that scenario is reported as **skipped** with this reason (the rule is
covered by the backend unit tests).

### Test data and clean-up

- Every booking gets its own random slot 2–30 months ahead on the configured table (start = now + N days), so scenarios run in
  parallel and repeated runs never meet on the same table and time.
- After each scenario the owner removes the guests, cancels their own pre-order and cancels the reservation. The backend has
  no route that deletes a reservation, so a cancelled reservation record stays; every run therefore uses fresh, unique slots.
- Most scenarios book `GROUP_TABLE_ID`; only the two waiter scenarios book `TESTTABLE_ID`, so the waiter's reservation list,
  which the waiter screen loads in full, grows by two records per run at most.

### Run the homework suite

```bash
cp .env.example .env            # fill in UI_URL, API_URL, the accounts and the tables (see "Configuration")
dotnet build
dotnet run --no-build -- --report-trx --treenode-filter "/*/*/*/*[Category=group-preorder]"     # whole suite
dotnet run --no-build -- --treenode-filter "/*/*/*/*[Category=L2]"                              # one level: L1, L2 or L3
dotnet run --no-build -- --treenode-filter "/*/*/*/*[(Category=group-preorder)&(Category=api)]" # only API (or ui)
dotnet run --no-build -- --treenode-filter "/*/*/*/*[Category=e2e]"                             # the end-to-end journey
```

Reports land in `TestResults/`: the TUnit HTML report and TRX, the Reqnroll HTML report in `TestResults/reqnroll/`, and a
Playwright trace for every failed `@ui` scenario in `TestResults/traces/` (`npx playwright show-trace <file>.zip` replays it
with DOM snapshots, network and screenshots). In CI the same files are attached to the run as artifacts and the job summary
lists the counts and the failed scenarios. A failed test never stops the pipeline: the report, artifact and summary steps
always run.

Settings the group pre-order scenarios need (in `.env` locally, as repository variables/secrets in CI):

| Setting | CI name | Meaning |
|---|---|---|
| `UI_URL`, `API_URL` | variables `AQA_UI_URL`, `AQA_API_URL` | my frontend and my API (with the stage) |
| `CUSTOMER_USERNAME`, `CUSTOMER_PASSWORD` | secrets `AQA_CUSTOMER_*` | the customer who owns the group reservations |
| `OTHER_CUSTOMER_USERNAME`, `OTHER_CUSTOMER_PASSWORD` | secrets `AQA_OTHER_CUSTOMER_*` | a second customer, never the owner |
| `TESTTABLE_ID`, `TESTTABLE_WAITER_USERNAME`, `TESTTABLE_WAITER_PASSWORD` | variable `AQA_TESTTABLE_ID`, secrets `AQA_TESTTABLE_WAITER_*` | a table that seats 5+ and the waiter of its location |
| `GROUP_TABLE_ID` | variable `AQA_GROUP_TABLE_ID` | optional: a table that seats 5+ at another location (defaults to `TESTTABLE_ID`) |
| `WAITER_USERNAME`, `WAITER_PASSWORD` | secrets `AQA_WAITER_*` | any waiter account (used as "a waiter" who is not the owner) |
| `PLAYWRIGHT_PROXY` | — | optional: an outbound proxy for machines that reach the internet only through one |

`aqa-tests.yml` runs the homework suite on every push to `main` that touches the tests, nightly, and on demand
(Actions → AQA Tests → Run workflow, with a scope: `group-preorder`, `L1`, `L2`, `L3`, `ui`, `api` or `all`).
`all` also runs the sample features that came with the course framework (`Auth`, `MenuCatalog`, `Registration`, …); they
are not part of this homework.

---

End-to-end tests for the Green & Tasty restaurant application: Reqnroll (Gherkin) + TUnit + Playwright for .NET, with the Screenplay pattern.

Guidance for extending the suite (writing Gherkin, steps, Screenplay, test data, working with an AI assistant) is in [CLAUDE.md](CLAUDE.md).

## Run the tests

```bash
cp .env.example .env        # then fill in your own values (see "Configuration")
dotnet build
dotnet run --no-build -- --report-trx                          # everything
dotnet run --no-build -- --treenode-filter "/*/*/*/*[Category=smoke]"   # by tag
```

Everything a run produces lands in `TestResults/`: the TUnit report and TRX, the Reqnroll report in `reqnroll/`, and Playwright traces in `traces/` (open one with `npx playwright show-trace <file>.zip`).

Some scenarios are expected to fail (`@known-defect`, bugs in the application) or are skipped (`@wip`, no admin account configured). That is intentional.

## Quality gates

| Where | What runs |
|---|---|
| Git hooks (local) | `pre-commit`: `dotnet format` check and Gherkin lint on staged files · `commit-msg`: Conventional Commits · `pre-push`: build with analyzers |
| `aqa-quality.yml` | on every push and pull request: no generated files tracked, format, build (warnings are errors), Gherkin lint, test discovery |
| `aqa-tests.yml` | the scenarios themselves, nightly and on demand (Actions → AQA Tests → Run workflow) |

Enable the hooks once per clone, from the repository root:

```bash
dotnet tool restore --tool-manifest aqa/restaurant-aqa-framework/.config/dotnet-tools.json
git config core.hooksPath aqa/restaurant-aqa-framework/.husky
```

The hooks only act on commits and pushes that touch `aqa/restaurant-aqa-framework`. Gherkin lint needs Node.js.

## Configuration

There is no shared environment and no committed `.env`: copy `.env.example` to `.env` and fill it in with the data of **your own** deployment (see "Testing your own deployment"). `.env` is git-ignored; never commit it.

Settings come from `.env` or real environment variables (variables win):

| Setting | Meaning |
|---|---|
| `UI_URL` | where the frontend is served |
| `API_URL` | base URL of the backend API, including the stage (for example `…/develop`) |
| `WAITER_USERNAME`, `WAITER_PASSWORD` | a waiter account that already exists in the environment under test |
| `TESTTABLE_ID`, `TESTTABLE_WAITER_*` | a table and the waiter assigned to it (only for scenarios that need them) |
| `ADMIN_USERNAME`, `ADMIN_PASSWORD` | an admin account; without it the admin scenarios are skipped |

The full list, with defaults, is in `.env.example`.

## Testing your own deployment

### If you deployed the backend / frontend / DevOps homework

1. `cp .env.example .env` and set `UI_URL` and `API_URL` to your deployment (API URL includes the stage, for example `…/develop`).
2. Waiters cannot be created by signing up, so create a waiter (and, for the admin scenarios, an admin) in *your* environment and put its credentials in `WAITER_*` / `ADMIN_*`. Optionally add a table and its assigned waiter (`TESTTABLE_*`).
3. Run `dotnet build` and a small slice first (`--treenode-filter "/*/*/*/*[Category=smoke]"`).

### If you have not deployed your own homework

If you deployed the original application, test that: set `UI_URL` / `API_URL` as above. Otherwise point the framework at a comparable public site (Booking, Airbnb, OpenTable, …) and write all the layers for it yourself: Gherkin features, step definitions, Screenplay tasks and questions, page objects with locators, and API clients where the site has a usable API. Use only flows you are allowed to automate (no purchases, no real bookings, respect the site's terms), and set `UI_URL` to that site.

1. **Generate a skeleton with Playwright codegen** (see "Presetup instructions"): record one flow, for example search → open a result → check details. Save the generated code to a scratch file, it is only raw material.
2. **Split it across the layers**: locators and small actions go to `Ui/` page objects, the flow becomes a task in `Screenplay/Tasks/`, the read-backs become questions in `Screenplay/Questions/`, the business wording goes to `Features/` and thin glue to `StepDefinitions/`.
3. **Replace weak locators.** Codegen often emits positional or text-fragile selectors. Prefer role + accessible name, label, test id, then CSS (see [CLAUDE.md](CLAUDE.md)); check them in the Playwright inspector.
4. Tag scenarios `@ui` / `@api`, keep them independent, and run `npx gherkin-lint -c .gherkin-lintrc "Features/*.feature"`.

### CI on your own public repository

Publish your work in your own public Git repository (the link is the deliverable). Workflows are disabled in a fresh fork until you enable them. Add the same values under Settings → Secrets and variables → Actions:

- **Variables:** `AQA_UI_URL`, `AQA_API_URL`, `AQA_TESTTABLE_ID`
- **Secrets:** `AQA_WAITER_USERNAME`, `AQA_WAITER_PASSWORD`, `AQA_ADMIN_USERNAME`, `AQA_ADMIN_PASSWORD`, `AQA_TESTTABLE_WAITER_USERNAME`, `AQA_TESTTABLE_WAITER_PASSWORD`

`aqa-quality.yml` runs on every push and pull request; `aqa-tests.yml` (nightly and on demand) tests whatever those variables point at. Never put credentials in the repository, in `.env.example` or in test code.

## Presetup instructions

This framework contains PlaywriteInstaller script so you don't need to setup it manually.
For full BDD highlighting in your IDE you may need to install Reqnroll extension https://docs.reqnroll.net/latest/ide-integrations/index.html
To start the Playwright code generator open Powershell in the framework directory and enter the line:
```bash
bin/Debug/net10.0/playwright.ps1 codegen "<your UI_URL or the public site you test>"
```
Build first so `playwright.ps1` exists. The URL is optional.
