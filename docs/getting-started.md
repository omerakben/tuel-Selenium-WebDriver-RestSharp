# Getting Started with EnterpriseTestFramework

This guide will help you get up and running with EnterpriseTestFramework quickly and efficiently.

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** or **VS Code** - [Download VS Code](https://code.visualstudio.com/)
- **Chrome** or **Edge** browser
- **Git** - [Download here](https://git-scm.com/downloads)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/EnterpriseTestFramework.git
cd EnterpriseTestFramework
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

## Configuration

### 1. Copy Configuration File

Copy the template configuration file:

```bash
cp loc.test/EnterpriseTestFramework.runsettings loc.test/EnterpriseTestFramework.local.runsettings
```

### 2. Configure Your Environment

Edit `EnterpriseTestFramework.local.runsettings` with your specific values:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
    <TestRunParameters>
        <!-- General Configuration -->
        <Parameter name="ENV" value="Development" />
        <Parameter name="BaseURL" value="https://your-app.com" />
        <Parameter name="BaseurlAPI" value="https://your-api.com/api" />
        <Parameter name="DefaultTimeoutSeconds" value="30" />

        <!-- Authentication Configuration -->
        <Parameter name="EntraIdTenantId" value="your-tenant-id" />
        <Parameter name="EntraIdClientId" value="your-client-id" />
        <Parameter name="EntraIdClientSecret" value="your-client-secret" />
        <Parameter name="EntraIdApiScope" value="your-api-scope" />

        <!-- Optional: User Credentials for ROPC Flow -->
        <Parameter name="UserName" value="your-email@domain.com" />
        <Parameter name="UserPassword" value="your-password" />
    </TestRunParameters>
</RunSettings>
```

## Running Tests

### Run All Tests

```bash
dotnet test loc.test/EnterpriseTestFramework.Tests.csproj --settings EnterpriseTestFramework.local.runsettings
```

### Run Specific Test Categories

```bash
# Run only API tests
dotnet test --filter "TestCategory=API"

# Run only UI tests
dotnet test --filter "TestCategory=UI"

# Run only authentication tests
dotnet test --filter "TestCategory=Authentication"
```

### Run Tests with Specific Browser

```bash
# Run with Chrome
dotnet test --settings EnterpriseTestFramework.local.runsettings -- TestRunParameters.Browser=local-chrome

# Run with Edge
dotnet test --settings EnterpriseTestFramework.local.runsettings -- TestRunParameters.Browser=local-edge
```

## Docker Setup (Optional)

### 1. Build Docker Image

```bash
docker build -t enterprise-test-framework .
```

### 2. Run Tests in Docker

```bash
docker run --rm -v $(pwd)/test-results:/app/test-results enterprise-test-framework
```

### 3. Use Docker Compose

```bash
# Run tests only
docker-compose up enterprise-test-framework

# Run with sample API and web app
docker-compose --profile with-api --profile with-web up
```

## Project Structure

```
EnterpriseTestFramework/
├── loc.test/                          # Main test project
│   ├── API/                          # API test classes
│   │   ├── Auth/                     # Authentication tests
│   │   └── Products/                 # Product API tests
│   ├── Web/                          # UI test classes
│   │   ├── PageObjects/              # Page Object Models
│   │   ├── TestClasses/              # UI test classes
│   │   └── Support/                  # Helper classes
│   ├── EnterpriseTestFramework.runsettings  # Configuration template
│   └── EnterpriseTestFramework.Tests.csproj # Project file
├── docs/                             # Documentation
├── .github/workflows/                # CI/CD pipelines
├── Dockerfile                        # Docker configuration
├── docker-compose.yml               # Docker Compose setup
├── LICENSE                          # MIT License
├── CONTRIBUTING.md                  # Contribution guidelines
└── README.md                        # Project overview
```

## Writing Your First Test

### API Test Example

```csharp
[TestClass]
public class MyApiTests : APIBase
{
    [TestMethod]
    [Description("Verifies that the API returns a successful response")]
    public async Task GetProducts_Returns_Status_OK()
    {
        var response = await ExecuteGetAsync("/products");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(response.Content);
    }
}
```

### UI Test Example

```csharp
[TestClass]
public class MyUiTests : Base
{
    [TestMethod]
    [Description("Verifies that the dashboard loads correctly")]
    public void Dashboard_Loads_Successfully()
    {
        var dashboardPage = new DashboardPOM(Driver);

        Assert.IsTrue(dashboardPage.VerifyPageTitle());
        Assert.IsTrue(dashboardPage.VerifyMainHeader());
    }
}
```

## Authentication Setup

### Option 1: Client Credentials Flow (Recommended for CI/CD)

```xml
<Parameter name="EntraIdTenantId" value="your-tenant-id" />
<Parameter name="EntraIdClientId" value="your-client-id" />
<Parameter name="EntraIdClientSecret" value="your-client-secret" />
<Parameter name="EntraIdApiScope" value="your-api-scope" />
```

### Option 2: ROPC Flow (For interactive testing)

```xml
<Parameter name="UserName" value="your-email@domain.com" />
<Parameter name="UserPassword" value="your-password" />
<Parameter name="EntraIdTenantId" value="your-tenant-id" />
<Parameter name="EntraIdClientId" value="your-client-id" />
<Parameter name="EntraIdApiScope" value="your-api-scope" />
```

### Option 3: Local JWT (For development/testing)

```xml
<Parameter name="EntraIdUseLocalJwt" value="true" />
<Parameter name="EntraIdLocalJwtRole" value="TestUser" />
```

## Troubleshooting

### Common Issues

1. **Tests fail with authentication errors**
   - Verify your Azure AD configuration
   - Check that the API scope is correct
   - Ensure the client secret is valid

2. **UI tests fail with element not found**
   - Check that the application is running
   - Verify the BaseURL configuration
   - Ensure the browser is properly installed

3. **Tests timeout**
   - Increase the DefaultTimeoutSeconds value
   - Check network connectivity
   - Verify the application is responsive

### Getting Help

- Check the [Troubleshooting Guide](troubleshooting.md)
- Review the [Examples](examples/)
- Open an issue on [GitHub](https://github.com/yourusername/EnterpriseTestFramework/issues)

## Next Steps

- Read the [Architecture Overview](architecture.md)
- Learn about [Page Object Model](page-object-model.md)
- Explore [Best Practices](best-practices.md)
- Check out [Examples](examples/)

## Support

- 📖 [Documentation](docs/)
- 🐛 [Report Issues](https://github.com/yourusername/EnterpriseTestFramework/issues)
- 💬 [Discussions](https://github.com/yourusername/EnterpriseTestFramework/discussions)
- 📧 [Contact](mailto:your-email@example.com)

---

**Happy Testing! 🚀**
