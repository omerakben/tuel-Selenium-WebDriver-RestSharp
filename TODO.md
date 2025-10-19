# TUEL Test Framework TODO

Last reviewed: 2025-10-19

## Current Snapshot
- Build fails: `dotnet test` stops because `tuel.test/TUEL.TestFramework.csproj:55-63` re-declares compile items that the SDK already includes.
- Quality tooling is broken: `tuel.test/QualityValidation/QualityValidator.cs:144` does not compile, and `GetProjectPath()` points at the build output instead of the repo root.
- Runtime hygiene gaps: there are 56 `Thread.Sleep` calls across `tuel.test/Web/TestClasses`, `tuel.test/Web/PageObjectFiles`, and `tuel.test/Web/Support`.
- Logging is inconsistent: 34 `Console.WriteLine` calls and ~798 `TestContext.WriteLine` statements drown out structured logging.

## Immediate Blockers
- [ ] Remove or refactor the manual `<Compile Include>` list in `tuel.test/TUEL.TestFramework.csproj:55-63` so the SDK implicit includes work and the project builds.
- [ ] Fix `QualityValidator` to compile (`analysis.HttpsCompliant` should compare `CountPatternInFiles(...) == 0`) and make `GetProjectPath()` resolve to the repository root so the quality scan sees source files.
- [ ] Add a shared import for `TUEL.TestFramework.Logging` (e.g., `global using` in `tuel.test/GlobalUsings.cs` or explicit usings) so files such as `tuel.test/API/Auth/EntraAuthHelper.cs` compile when they call `TestLogger`.

## High Priority (after the build passes)
- [ ] Replace the 56 remaining `Thread.Sleep` calls with waits or retry helpers (focus on `tuel.test/Web/TestClasses`, `tuel.test/Web/PageObjectFiles`, `tuel.test/Web/Support/UIHelper.cs`).
- [ ] Swap the 34 `Console.WriteLine` usages (see `tuel.test/TestBase.cs`, `tuel.test/API/APIBase.cs`, `tuel.test/API/Auth/EntraAuthHelper.cs`, `tuel.test/Web/PageObjectFiles/LoginPOM.cs`) for `TestLogger` or MSTest logging.
- [ ] Tame the `TestContext.WriteLine` flood by centralising repeated logging patterns into helper methods and honouring log levels.
- [ ] Add fast-running smoke tests that do not depend on live Azure AD or the production TUEL UI (supply fakes/mocks so CI can run `dotnet test` headlessly).

## Quality & Tooling
- [ ] Re-enable the quality gate once it compiles: run `QualityValidator.RunQualityValidation()` within a test and fail the build when regressions appear.
- [ ] Add a CI job (GitHub Actions or Azure Pipelines) that runs `dotnet test --configuration Release` with a local/headless WebDriver to prove the harness works end-to-end.
- [ ] Capture metrics from `WebDriverLifecycleManager.ReleaseDriver` so pooling effectiveness and flaky disposals are observable.

## Documentation & Developer Experience
- [ ] Update `docs/getting-started.md` to match the real build steps (`dotnet build`, prerequisites, local UI smoke instructions) once the build issues are resolved.
- [ ] Refresh `tuel.test/TUEL.TestFramework.runsettings.example` to clarify which parameters must reference secrets (`env://`, `kv://`, `enc://`) and provide safe local defaults.
- [ ] Move the aspirational marketing documents (e.g., `README_A_PLUS_PLUS.md`, `A_PLUS_PLUS_PLUS_QUALITY_ACHIEVED.md`) under `docs/archive` or clearly mark them as vision so the main README reflects the current state.

## Stretch / Nice to Have
- [ ] Add integration tests for `SecretManager` that validate Azure Key Vault and encrypted configuration flows using test doubles.
- [ ] Emit HTML artifacts from `TestCoverageValidator.GenerateCoverageReport` and publish them from CI runs.
- [ ] Package a Docker-based Selenium setup (standalone Chrome/Edge) to let contributors run UI tests without external infrastructure.

## Metrics to Track
- Thread.Sleep occurrences: 56 (`rg "Thread\.Sleep" tuel.test | wc -l`)
- Console.WriteLine occurrences: 34 (`rg "Console\.WriteLine" tuel.test | wc -l`)
- TestContext.WriteLine occurrences: 798 (`rg "TestContext\.WriteLine" tuel.test | wc -l`)
- Build command: `dotnet test tuel.test/TUEL.TestFramework.sln`
