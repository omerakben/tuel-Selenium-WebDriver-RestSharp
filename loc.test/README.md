# TUEL Test Framework - Generic Testing Framework
This repository contains a generic, enterprise-grade automation testing framework for API and UI components. It is designed to be easily adaptable to various business applications.

## Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 (or other compatible IDE)
- Chrome or Edge browser for UI tests

## Configuration
1.  **Runsettings File**:
    -   Copy `TUEL.TestFramework.runsettings.example` to `TUEL.TestFramework.runsettings`.
    -   Update the `TUEL.TestFramework.runsettings` file with your environment-specific values. This file contains parameters for API and UI base URLs, user credentials, Azure AD (Entra ID) configurations, and timeouts.
    -   **Important**: For local development, you might create `TUEL.TestFramework.local.runsettings` and configure your IDE to use it.

## How to Run Tests

### From Visual Studio
1.  Open the `TUEL.TestFramework.sln` solution.
2.  Ensure the `TUEL.TestFramework.runsettings` (or your local variant) is selected under `Test` -> `Configure Run Settings` -> `Select Solution Wide .runsettings File`.
3.  Build the solution.
4.  Open Test Explorer (`Test` -> `Test Explorer`).
5.  Run all tests or select specific tests/categories.

### From Command Line
Navigate to the `TUEL.TestFramework` directory and run:
```bash
dotnet test TUEL.TestFramework.csproj --settings TUEL.TestFramework.runsettings
```

## Project Structure
-   **`TUEL.TestFramework`**: The main test project.
    -   **`API`**: Contains API test classes and related helpers.
        -   **`Auth`**: Authentication helpers (e.g., EntraAuthHelper).
        -   **`Products`**: API tests for product-related endpoints.
    -   **`Web`**: Contains UI test classes and Page Object Models.
        -   **`PageObjects`**: Page Object Model classes for UI elements.
        -   **`Support`**: Helper classes for WebDriver interactions and utilities.
        -   **`TestClasses`**: UI test classes.
    -   **`TestBase.cs`**: Base class for all API and UI tests.
    -   **`InitializeTestAssembly.cs`**: Assembly-level setup and teardown.
    -   **`TUEL.TestFramework.runsettings`**: Configuration file for test parameters.

## Contributing
Refer to `CONTRIBUTING.md` for guidelines on how to contribute to this project.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

## TODOs
- [ ] Create sample test data and demo scenarios.
- [ ] Implement remaining code quality improvements from analysis.
- [ ] Add Docker support for easy setup.
- [ ] Create CI/CD pipeline templates.
- [ ] Add comprehensive documentation and examples.
- [ ] Create portfolio showcase materials.
