# Getting Started with Transit to Fully Open-Source TUEL

This guide walks you through cloning the repository, configuring environments, and running the test suites that power the TUEL automation framework.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or VS Code with C# extensions
- Chrome or Edge browser (for UI automation)
- Git
- Docker (optional, for containerized execution)

## Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/omerakben/transit-to-open-source-tuel.git
   cd transit-to-open-source-tuel
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore tuel.test/TUEL.TestFramework.csproj
   ```

3. **Build the solution**
   ```bash
   dotnet build tuel.test/TUEL.TestFramework.sln
   ```

## Configuration

1. **Create a runsettings file**
   ```bash
   cd tuel.test
   cp TUEL.TestFramework.runsettings.example TUEL.TestFramework.runsettings
   ```

2. **Update the copied file** with your environment values:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <RunSettings>
     <TestRunParameters>
       <!-- General Configuration -->
       <Parameter name="ENV" value="Development" />
       <Parameter name="BaseURL" value="https://your-app.example.com" />
       <Parameter name="BaseurlAPI" value="https://your-api.example.com/api" />
       <Parameter name="DefaultTimeoutSeconds" value="30" />

       <!-- Azure AD / Entra ID -->
       <Parameter name="EntraIdTenantId" value="your-tenant-id" />
       <Parameter name="EntraIdClientId" value="your-client-id" />
       <Parameter name="EntraIdClientSecret" value="your-client-secret" />
       <Parameter name="EntraIdApiScope" value="api://your-resource/.default" />

       <!-- Optional: ROPC credentials -->
       <Parameter name="UserName" value="user@example.com" />
       <Parameter name="UserPassword" value="your-password" />
     </TestRunParameters>
   </RunSettings>
   ```

3. **Smart wait strategies** are enabled by default. Adjust timeouts or retry counts in the same file to fit your application’s responsiveness.

## Running Tests

From the repository root:

```bash
dotnet test tuel.test/TUEL.TestFramework.sln --settings tuel.test/TUEL.TestFramework.runsettings
```

### Filter by category

```bash
dotnet test tuel.test/TUEL.TestFramework.sln --settings tuel.test/TUEL.TestFramework.runsettings --filter "TestCategory=API"
dotnet test tuel.test/TUEL.TestFramework.sln --settings tuel.test/TUEL.TestFramework.runsettings --filter "TestCategory=UI"
```

### Select a browser at runtime

Specify the `Browser` parameter if your runsettings file includes browser overrides:

```bash
dotnet test tuel.test/TUEL.TestFramework.sln --settings tuel.test/TUEL.TestFramework.runsettings -- TestRunParameters.Browser=local-chrome
```

## Docker Setup (Optional)

1. **Build the image**
   ```bash
    docker build -t tuel-test-framework .
   ```

2. **Run tests inside the container**
   ```bash
   docker run --rm -v "$(pwd)/test-results:/app/test-results" tuel-test-framework
   ```

3. **Use Docker Compose**
   ```bash
   docker-compose up tuel-test-framework
   ```

## Project Structure

```
transit-to-open-source-tuel/
├── docs/                     # Guides and references
├── tuel.test/                 # Main test project
│   ├── API/                  # API automation
│   ├── Web/                  # UI automation
│   ├── Configuration/        # Shared configuration helpers
│   ├── TUEL.TestFramework.sln
│   ├── TUEL.TestFramework.csproj
│   ├── TUEL.TestFramework.runsettings.example
│   └── README.md
├── .github/                  # Community health files & workflows
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Writing Your First Test

### API Example

```csharp
[TestClass]
public class ProductsApiTests : ApiTestBase
{
    [TestMethod]
    [TestCategory("API")]
    [Description("Validates that the products endpoint responds with HTTP 200.")]
    public async Task GetProducts_Should_Return_Ok()
    {
        var response = await ExecuteGetAsync("/products");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(response.Content);
    }
}
```

### UI Example

```csharp
[TestClass]
public class DashboardSmokeTests : Base
{
    [TestMethod]
    [TestCategory("UI")]
    [Description("Confirms the dashboard renders core widgets for authenticated users.")]
    public void Dashboard_Should_Render_Core_Widgets()
    {
        var dashboard = new DashboardPOM(Driver);

        Assert.IsTrue(dashboard.VerifyPageTitle());
        Assert.IsTrue(dashboard.VerifyMainHeader());
    }
}
```

## Troubleshooting

1. **Authentication failures**
   - Confirm Entra ID credentials and scopes.
   - Use `EntraIdUseLocalJwt=true` for local smoke testing when appropriate.

2. **UI element not found**
   - Ensure the application under test is reachable at `BaseURL`.
   - Validate browser drivers are installed or allow WebDriverManager to download them.

3. **Timeouts**
   - Increase `DefaultTimeoutSeconds` or the explicit wait values in runsettings.
   - Inspect network stability; leverage structured logs in `logs/`.

## Getting Help

- Review [`SUPPORT.md`](../SUPPORT.md) for preferred channels.
- Create issues in the GitHub repository with the provided templates.
- Check the discussions board (once enabled) for community Q&A.

## Maintainer Contact

- **Maintainer**: Omer “Ozzy” Akben — Full-Stack Developer • AI Engineer • SDET
- **Email**: [me@omerakben.com](mailto:me@omerakben.com)
- **Phone**: [(267) 512-4566](tel:+12675124566)
- **Portfolio**: [omerakben.com](https://omerakben.com)

Thank you for building with TUEL! Contributions that align with the project [vision](../VISION.md) are always welcome.
