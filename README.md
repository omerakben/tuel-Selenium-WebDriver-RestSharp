# LOC.API.TEST - Letters of Credit Testing Framework

The repository contains the automation testing framework for the Letters of Credit (LOC) application's API and UI components.

## Getting Started

1. **Prerequisites**
   - .NET 8.0 SDK
   - Visual Studio 2022 or VS Code
   - Chrome or Edge browser (for UI tests)

2. **Configuration**
   - Edit the `LOC.RestSharp.runsettings` with your environment-specific values for pipeline setup
   - Configure authentication parameters for your test environment

3. **Running Tests in Pipeline**
   ```bash
   cd loc.test
   dotnet test LOCRestSharp.sln --settings LOC.RestSharp.runsettings
   ```

## Features

- **API Testing**: RestSharp-based API testing with authentication
- **UI Testing**: Selenium WebDriver-based UI automation with Page Object Model
- **Authentication**: Microsoft Entra ID integration with multiple auth flows
- **Multi-Environment**: Configurable for different test environments

## Test Categories

- **API Tests**: API endpoint testing
- **UI Tests**: End-to-end UI testing
- **Authentication Tests**: Authentication flow validation

# Getting Started
TODO:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test on Local
- Copy the `LOC.RestSharp.runsettings` and create `LOC.local.runsettings` with your actual values
- On Visiual Studio Test > Configure Run Settings > 
- Check the 'Auto detect runsettings Files' and --> click 'Select Solution Wide runsettings Files' choose `LOC.local.runsettings`
- Open 'Test Explorer' and start to run tests.