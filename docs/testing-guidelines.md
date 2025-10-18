# Testing Guidelines

This guide summarizes expectations for writing and maintaining tests in the Transit to Fully Open-Source TUEL framework.

## Core Principles

- **Deterministic**: Tests must produce consistent results across machines and runs. Avoid randomization without deterministic seeds.
- **Atomic**: Each test validates a single behavior. Use helper methods or data builders to keep tests focused.
- **Observable**: Use `TestLogger` and structured logging rather than `Console.WriteLine`.
- **Fast Feedback**: Prefer lightweight setup, parallel execution, and smart waits instead of sleep calls.

## Test Layers

| Layer | Purpose | Recommendations |
|-------|---------|-----------------|
| Unit | Verify small pieces of logic | Use xUnit/MSTest attributes, mock external dependencies |
| API | Validate service endpoints | Use RestSharp clients, assert on status codes + payload shape |
| UI | Validate end-to-end flows | Use Page Object Models, explicit waits, stable selectors |
| Integration | Exercise component boundaries | Set up environment-specific data, clean up after tests |

## Naming

- Follow `MethodUnderTest_ExpectedBehavior_State` for test method names.
- Organize tests by feature area (e.g., `Web/TestClasses/Templates.cs`).
- Annotate with `[Description]` to provide context in reports.

## Data Management

- Use data builders or fixtures instead of hard-coded literals.
- Prefer environment-agnostic data; when unavoidable, gate environment-specific assertions with configuration flags.
- Ensure data created during tests is cleaned up to avoid cross-test pollution.

## Wait Strategies

- Use `WaitVisible`, `WaitClickable`, and other helpers in `Web/Support`.
- Enable smart waits in runsettings (`UseSmartWaitStrategies=true`).
- Monitor for regressions using the performance suite in `docs/performance-optimization.md`.

## Parallel Execution

- Keep tests thread-safe. Avoid static mutable state.
- Use `ClassInitialize` only when necessary; prefer per-test setup.
- Configure the `RunSettings` file to control parallelism when scaling suites.

## Assertions

- One logical assertion per test where possible.
- Provide custom failure messages to aid debugging.
- For UI tests, assert both the presence and state of elements (e.g., visible and enabled).

## Logging & Reporting

- Use `TestLogger.LogInformation` within tests for context.
- Capture screenshots on failure via the existing hooks in `TestBase`.
- Ensure new helpers integrate with the reporting pipeline.

## Pull Request Expectations

- New features require positive and negative test coverage.
- Bug fixes must include a regression test that fails without the fix.
- All tests must pass in CI; provide instructions for reproducing environment-specific issues.

## Linting & Formatting

- Run `dotnet format` before opening a pull request.
- For UI locators, prefer data attributes over brittle selectors.
- Keep helper methods within the relevant Page Object or Support class.

## Additional Resources

- [`CONTRIBUTING.md`](../CONTRIBUTING.md)
- [`performance-optimization.md`](performance-optimization.md)
- [`VISION.md`](../VISION.md)

Questions? Open an issue with the `question` label or start a discussion once the forum is enabled.
