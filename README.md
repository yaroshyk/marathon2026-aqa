# IT Marathon 2026 — Test Automation homework

**Direction:** 1 — group pre-order (I implemented and deployed my own backend and frontend, and this suite tests them).
**Approach:** BDD + Screenplay on the course framework (Reqnroll + TUnit + Playwright for .NET).

The framework, the scenarios and the full description (what is covered, the behaviour chosen where the homework does not
define it, how to configure and run the tests, how to run a level or a tag, where the reports are) are in
[aqa/restaurant-aqa-framework/README.md](aqa/restaurant-aqa-framework/README.md).

CI: [.github/workflows/aqa-tests.yml](.github/workflows/aqa-tests.yml) runs the suite on every push to `main` that changes
the tests, nightly and on demand, and attaches the TUnit, TRX and Reqnroll reports and the Playwright traces of failed
scenarios to each run. [.github/workflows/aqa-quality.yml](.github/workflows/aqa-quality.yml) checks format, build, Gherkin
lint and test discovery on every push and pull request.
