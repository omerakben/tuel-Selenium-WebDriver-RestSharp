# TUEL Test Framework - Complete API Documentation

## A+++ Quality Enterprise Test Automation Framework

> **Complete API Reference** for the TUEL Test Framework - A+++ quality enterprise-grade test automation solution.

---

## 📋 Table of Contents

- [Core Framework](#core-framework)
- [Architecture Patterns](#architecture-patterns)
- [Security Features](#security-features)
- [Performance Engine](#performance-engine)
- [Error Handling](#error-handling)
- [Monitoring & Analytics](#monitoring--analytics)
- [Testing Utilities](#testing-utilities)
- [Configuration Management](#configuration-management)
- [Examples & Usage](#examples--usage)

---

## Core Framework

### `TUEL.TestFramework.TestBase`

**Abstract base class for all test classes providing common test functionality.**

```csharp
public abstract class TestBase
{
    /// <summary>
    /// Gets or sets the test context which provides information about
    /// and functionality for the current test run.
    /// </summary>
    public TestContext TestContext { get; set; }

    /// <summary>
    /// Test initialization method called before each test method
    /// </summary>
    [TestInitialize]
    public virtual void TestInitialize();

    /// <summary>
    /// Test cleanup method called after each test method
    /// </summary>
    [TestCleanup]
    public virtual void TestCleanup();
}
```

**Usage Example:**
```csharp
[TestClass]
public class MyTestClass : TestBase
{
    [TestMethod]
    public void MyTest()
    {
        // Test implementation
    }
}
```

### `TUEL.TestFramework.InitializeTestAssembly`

**Static class for assembly-level initialization and configuration management.**

```csharp
[TestClass]
public static class InitializeTestAssembly
{
    // Configuration Properties
    public static string ENV { get; private set; }
    public static string Browser { get; private set; }
    public static string UiUrl { get; private set; }
    public static string BaseApiUrl { get; private set; }

    // Authentication Properties
    public static string EntraIdTenantId { get; private set; }
    public static string EntraIdClientId { get; private set; }
    public static string EntraIdApiScope { get; private set; }

    // WebDriver Properties
    public static bool WebDriverEnablePooling { get; private set; }
    public static int WebDriverMaxPoolSize { get; private set; }

    /// <summary>
    /// Creates a WebDriver instance with proper configuration
    /// </summary>
    public static IWebDriver CreateWebDriver();
}
```

---

## Architecture Patterns

### Command Pattern

**Implements the Command pattern for test operations with undo/redo capabilities.**

```csharp
public interface ICommand
{
    Task<bool> ExecuteAsync();
    string CommandName { get; }
    Dictionary<string, object> Parameters { get; }
}

public class NavigateCommand : TestCommand
{
    public NavigateCommand(string url, Func<Task<bool>> navigationAction);
}

public class ElementInteractionCommand : TestCommand
{
    public ElementInteractionCommand(string elementId, string action, Func<Task<bool>> interactionAction);
}

public class CommandInvoker
{
    public async Task<bool> ExecuteCommandAsync(ICommand command);
    public async Task<bool> UndoLastCommandAsync();
    public async Task<bool> RedoLastCommandAsync();
    public bool CanUndo { get; }
    public bool CanRedo { get; }
}
```

**Usage Example:**
```csharp
var command = new NavigateCommand("https://example.com", async () => {
    Driver.Navigate().GoToUrl("https://example.com");
    return true;
});

var invoker = new CommandInvoker();
await invoker.ExecuteCommandAsync(command);
```

### Observer Pattern

**Implements the Observer pattern for test event notifications.**

```csharp
public interface ITestObserver
{
    Task OnTestStartedAsync(string testName);
    Task OnTestCompletedAsync(string testName, bool success);
    Task OnTestFailedAsync(string testName, Exception exception);
    Task OnTestStepExecutedAsync(string testName, string stepName, bool success);
}

public class TestEventPublisher
{
    public void Subscribe(ITestObserver observer);
    public void Unsubscribe(ITestObserver observer);
    public async Task NotifyTestStartedAsync(string testName);
    public async Task NotifyTestCompletedAsync(string testName, bool success);
    public async Task NotifyTestFailedAsync(string testName, Exception exception);
}
```

### Builder Pattern

**Implements the Builder pattern for complex test configurations.**

```csharp
public class TestConfigurationBuilder
{
    public TestConfigurationBuilder SetEnvironment(string environment);
    public TestConfigurationBuilder SetBrowser(string browser);
    public TestConfigurationBuilder SetHeadless(bool headless);
    public TestConfigurationBuilder SetTimeouts(int defaultTimeout, int pageLoadTimeout, int scriptTimeout);
    public TestConfigurationBuilder SetRetrySettings(int maxRetries, int retryDelay);
    public TestConfigurationBuilder SetSecuritySettings(bool forceHttps, bool enableAuditLogging);
    public TestConfigurationBuilder SetPerformanceSettings(bool enablePooling, int poolSize);
    public TestConfiguration Build();
}
```

**Usage Example:**
```csharp
var config = new TestConfigurationBuilder()
    .SetEnvironment("production")
    .SetBrowser("chrome")
    .SetHeadless(true)
    .SetTimeouts(30, 60, 30)
    .SetRetrySettings(3, 1000)
    .SetSecuritySettings(true, true)
    .SetPerformanceSettings(true, 4)
    .Build();
```

### Decorator Pattern

**Implements the Decorator pattern for test method enhancements.**

```csharp
public abstract class TestMethodDecorator
{
    protected readonly Func<Task<bool>> _testMethod;
    public abstract Task<bool> ExecuteAsync();
}

public class PerformanceMonitoringDecorator : TestMethodDecorator
{
    public PerformanceMonitoringDecorator(Func<Task<bool>> testMethod, string testName);
}

public class RetryDecorator : TestMethodDecorator
{
    public RetryDecorator(Func<Task<bool>> testMethod, int maxRetries = 3, TimeSpan? baseDelay = null);
}

public class CompositeDecorator : TestMethodDecorator
{
    public CompositeDecorator(Func<Task<bool>> testMethod, params TestMethodDecorator[] decorators);
}
```

---

## Security Features

### Advanced Security Manager

**Comprehensive security validation and threat detection.**

```csharp
public static class AdvancedSecurityManager
{
    public static void Initialize();
    public static async Task<SecurityValidationResult> ValidateOperationAsync(string operation, Dictionary<string, object> parameters);
    public static async Task<List<SecurityThreat>> DetectThreatsAsync(string content, ThreatType type);
    public static async Task<SecurityAuditReport> PerformSecurityAuditAsync();
    public static async Task StartSecurityMonitoringAsync();
}
```

### Advanced Encryption

**Enterprise-grade encryption utilities.**

```csharp
public static class AdvancedEncryption
{
    public static async Task<EncryptionResult> EncryptAesGcmAsync(string plaintext, byte[] key);
    public static async Task<string> DecryptAesGcmAsync(EncryptionResult encryptedData, byte[] key);
    public static byte[] GenerateSecureKey(int keySize = 32);
    public static string CreateSecureHash(string input, byte[] salt);
}
```

### Authentication Security

**Advanced authentication validation and security.**

```csharp
public static class AuthenticationSecurity
{
    public static async Task<JwtValidationResult> ValidateJwtTokenAsync(string token);
    public static async Task<bool> ValidateMfaAsync(string userId, string mfaCode);
    public static async Task<SessionSecurityResult> ValidateSessionSecurityAsync(string sessionId);
}
```

---

## Performance Engine

### Advanced Performance Engine

**Intelligent performance optimization and monitoring.**

```csharp
public static class AdvancedPerformanceEngine
{
    public static void Initialize();

    public static class IntelligentCache
    {
        public static async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        public static void InvalidatePattern(string pattern);
        public static void Clear();
        public static CacheStatistics GetStatistics();
    }

    public static class ParallelExecutionEngine
    {
        public static async Task<List<T>> ExecuteParallelAsync<T>(IEnumerable<Func<Task<T>>> tasks, int? maxConcurrency = null);
        public static async Task ExecuteParallelWithProgressAsync<T>(IEnumerable<T> items, Func<T, Task> action, int? maxConcurrency = null);
        public static async Task<List<T>> ExecuteWithAdaptiveConcurrencyAsync<T>(IEnumerable<Func<Task<T>>> tasks);
    }

    public static class AdvancedMonitoring
    {
        public static void RecordDetailedMetric(string operation, long durationMs, Dictionary<string, object>? context = null);
        public static PerformanceAnalysis GetPerformanceAnalysis();
        public static SystemResourceMetrics GetSystemResourceMetrics();
    }

    public static class MemoryOptimizer
    {
        public static void OptimizeMemoryUsage();
        public static MemoryUsageStatistics GetMemoryStatistics();
    }
}
```

### Performance Monitor

**Real-time performance metrics and reporting.**

```csharp
public static class PerformanceMonitor
{
    public static bool Enabled { get; set; }
    public static string StartTimer(string operationName);
    public static void StopTimer(string timerId, string operationName, Dictionary<string, object>? additionalData = null);
    public static void RecordMetric(string operationName, long durationMs, Dictionary<string, object>? additionalData = null);
    public static void TimeOperation(string operationName, Action action, Dictionary<string, object>? additionalData = null);
    public static async Task TimeOperationAsync(string operationName, Func<Task> action, Dictionary<string, object>? additionalData = null);
    public static T TimeOperation<T>(string operationName, Func<T> function, Dictionary<string, object>? additionalData = null);
    public static PerformanceStatistics? GetStatistics(string operationName);
    public static Dictionary<string, PerformanceStatistics> GetAllStatistics();
    public static string GenerateReport();
    public static void ClearMetrics();
}
```

---

## Error Handling

### Error Handling Utility

**Comprehensive error handling with retry mechanisms.**

```csharp
public static class ErrorHandlingUtility
{
    public static bool ExecuteWithRetry(Action action, string operationName, int maxRetries = 3, TimeSpan? retryDelay = null, bool throwOnFailure = true);
    public static async Task<bool> ExecuteWithRetryAsync(Func<Task> action, string operationName, int maxRetries = 3, TimeSpan? retryDelay = null, bool throwOnFailure = true);
    public static T ExecuteWithRetry<T>(Func<T> function, string operationName, int maxRetries = 3, TimeSpan? retryDelay = null, T defaultValue = default(T));
    public static bool SafeExecute(Action action, string operationName);
    public static T SafeExecute<T>(Func<T> function, string operationName, T defaultValue = default(T));
    public static string CreateErrorMessage(string operation, string context = "", Exception? innerException = null);
    public static bool IsRetryableException(Exception exception);
    public static string GetUserFriendlyErrorMessage(Exception exception);
}
```

---

## Monitoring & Analytics

### Test Logger

**Structured logging with sensitive data masking.**

```csharp
public static class TestLogger
{
    public static void SetLogger(ITestLogger logger);
    public static ITestLogger Instance { get; }
    public static void LogTrace(string message, params object[] args);
    public static void LogDebug(string message, params object[] args);
    public static void LogInformation(string message, params object[] args);
    public static void LogWarning(string message, params object[] args);
    public static void LogError(string message, params object[] args);
    public static void LogCritical(string message, params object[] args);
    public static void LogException(Exception exception, string message = null, params object[] args);
}

public interface ITestLogger
{
    void Log(LogLevel level, string message, params object[] args);
    void LogTrace(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogCritical(string message, params object[] args);
    void LogException(Exception exception, string message = null, params object[] args);
}
```

### Quality Validator

**Comprehensive quality validation and reporting.**

```csharp
public static class QualityValidator
{
    public static string RunQualityValidation();
}
```

---

## Testing Utilities

### Test Coverage Validator

**Automated test coverage analysis and validation.**

```csharp
public static class TestCoverageValidator
{
    public static TestCoverageAnalysis AnalyzeCoverage(Assembly assembly);
    public static TestValidationResult ValidateCriticalCoverage(Assembly assembly);
    public static string GenerateCoverageReport(Assembly assembly);
    public static List<string> ValidateTestNamingConventions(Assembly assembly);
    public static TestExecutionAnalysis AnalyzeTestExecution(Assembly assembly);
}
```

---

## Configuration Management

### Test Configuration

**Centralized configuration service for test framework settings.**

```csharp
public static class TestConfiguration
{
    // Timeout Configuration
    public static int DefaultTimeoutSeconds { get; private set; }
    public static int ApiTimeoutSeconds { get; private set; }
    public static int PageTransitionTimeoutSeconds { get; private set; }
    public static int ElementVisibilityTimeoutSeconds { get; private set; }
    public static int ElementClickabilityTimeoutSeconds { get; private set; }
    public static int RetryDelayMilliseconds { get; private set; }
    public static int MaxRetryAttempts { get; private set; }

    // Wait Strategy Configuration
    public static bool UseSmartWaitStrategies { get; private set; }
    public static bool WaitForAjaxCompletion { get; private set; }
    public static bool WaitForPageLoadState { get; private set; }

    // Performance Configuration
    public static bool EnableParallelExecution { get; private set; }
    public static int MaxParallelThreads { get; private set; }
    public static bool EnablePerformanceMetrics { get; private set; }

    // Security Configuration
    public static bool ForceHttps { get; private set; }
    public static bool EnableSecureTokenStorage { get; private set; }
    public static bool EnableAuditLogging { get; private set; }

    // Logging Configuration
    public static bool EnableStructuredLogging { get; private set; }
    public static string LogLevel { get; private set; }
    public static bool MaskSensitiveDataInLogs { get; private set; }

    public static void Initialize(TestContext context);
    public static TimeSpan GetDefaultTimeout();
    public static TimeSpan GetApiTimeout();
    public static TimeSpan GetPageTransitionTimeout();
    public static TimeSpan GetElementVisibilityTimeout();
    public static TimeSpan GetElementClickabilityTimeout();
    public static TimeSpan GetRetryDelay();
}
```

### Secret Manager

**Comprehensive secret management with multiple providers.**

```csharp
public static class SecretManager
{
    public static void Initialize(TestContext context);
    public static string? ResolveSecret(string? value, string? logicalName = null, bool warnOnPlaintext = true, CancellationToken cancellationToken = default);
    public static Task<string?> ResolveSecretAsync(string? value, string? logicalName = null, bool warnOnPlaintext = true, CancellationToken cancellationToken = default);
    public static void Shutdown();
}
```

---

## 🌐 Web Testing

### WebDriver Factory

**Advanced WebDriver creation and management.**

```csharp
public static class WebDriverFactory
{
    public static IWebDriver CreateDriver(WebDriverRequestOptions request);
}

public class WebDriverRequestOptions
{
    public string BrowserName { get; set; } = string.Empty;
    public WebDriverProviderType Provider { get; set; }
    public bool Headless { get; set; }
    public Uri? RemoteServerUri { get; set; }
    public TimeSpan CommandTimeout { get; set; }
    public IDictionary<string, object>? AdditionalCapabilities { get; set; }
}

public enum WebDriverProviderType
{
    Local,
    SeleniumGrid
}
```

### WebDriver Lifecycle Manager

**Intelligent WebDriver lifecycle management with pooling.**

```csharp
public static class WebDriverLifecycleManager
{
    public static void Configure(WebDriverLifecycleOptions options);
    public static IWebDriver AcquireDriver();
    public static void ReleaseDriver(IWebDriver? driver, bool reusable);
    public static void Shutdown();
}

public class WebDriverLifecycleOptions
{
    public bool EnablePooling { get; set; } = true;
    public int MaxPoolSize { get; set; } = 2;
    public TimeSpan MaxIdleTime { get; set; } = TimeSpan.FromMinutes(10);
    public string BrowserName { get; set; } = string.Empty;
    public bool Headless { get; set; }
    public WebDriverProviderType Provider { get; set; }
    public Uri? RemoteServerUri { get; set; }
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(180);
    public IDictionary<string, object>? AdditionalCapabilities { get; set; }
}
```

### UI Helper

**Enhanced UI interaction utilities with smart waits.**

```csharp
public static class UIHelper
{
    public static readonly TimeSpan DefaultTimeout;
    public static readonly TimeSpan ElementVisibilityTimeout;
    public static readonly TimeSpan ElementClickabilityTimeout;
    public static readonly TimeSpan PageTransitionTimeout;
    public static readonly TimeSpan RetryDelay;
    public static readonly int DefaultRetries;

    public static bool IsPipelineEnvironment();
    public static IWebElement WaitVisible(this IWebDriver driver, By by, TimeSpan? timeout = null);
    public static IWebElement WaitClickable(this IWebDriver driver, By by, TimeSpan? timeout = null);
    public static void ClickElement(this IWebDriver driver, By by, TimeSpan? timeout = null);
    public static void EnterText(this IWebDriver driver, By by, string? text, bool clearFirst = true, TimeSpan? timeout = null);
    public static string GetElementText(this IWebDriver driver, By by, TimeSpan? timeout = null);
    public static bool IsDisplayedSafe(this IWebDriver driver, By by);
    public static IWebElement WaitVisibleWithRetry(this IWebDriver driver, By by, TimeSpan? timeout = null, int maxRetries = -1);
    public static bool WaitForPageTransition(this IWebDriver driver, TimeSpan? timeout = null);
    public static bool WaitForUrlChange(this IWebDriver driver, string previousUrl, TimeSpan? timeout = null);
    public static bool WaitForUrlContains(this IWebDriver driver, string urlPart, TimeSpan? timeout = null);
    public static void Retry(Action act, int times = -1);
    public static async Task RetryAsync(Func<Task> act, int times = -1);
}
```

### Base Page

**Abstract base class for Page Object Model implementations.**

```csharp
public abstract class BasePage
{
    protected IWebDriver Driver { get; }
    protected string BaseUrl { get; }
    protected abstract By UniqueLocator { get; }

    // Common Business Application Locators
    protected readonly By mainHeader;
    protected readonly By navigationTabs;
    protected readonly By dashboardTab;
    protected readonly By completedTab;
    protected readonly By customersTab;
    protected readonly By templatesTab;
    protected readonly By pricingTab;
    protected readonly By dataTable;
    protected readonly By tableHeaders;
    protected readonly By tableRows;
    protected readonly By noRecordsMessage;
    protected readonly By paginationContainer;
    protected readonly By pageInfo;
    protected readonly By itemsPerPageSelector;
    protected readonly By searchInput;

    // Common Business Application Methods
    public virtual bool VerifyPageTitle();
    public virtual bool VerifyMainHeader();
    public virtual bool VerifyNavigationTabsPresent();
    public virtual bool VerifyDataTablePresent();
    public virtual bool VerifyTableHasData();
    public virtual bool VerifyNoRecordsMessage();
    public virtual int GetDataRowCount();
    public virtual bool VerifyPaginationControls();
    public virtual bool VerifyPageStatusDisplay();
    public virtual bool VerifyItemsPerPageSelector();
    public virtual bool VerifySearchInput();
    public virtual void ClickNavigationTab(string tabName);
    public virtual List<string> GetColumnHeaders(bool includeEmpty = false, string? emptyPlaceholder = null);
    public virtual string GetPageStatus();
    public virtual void WaitUntilPageIsLoaded(TimeSpan? timeout = null);
    public string GetPageTitle();

    // Utility Methods
    protected void Click(By by);
    protected void EnterText(By by, string? text, bool clearFirst = true);
    protected string? GetText(By by);
    protected bool IsElementVisible(By by);

    // Generic Search Helpers
    protected static string? ExtractFirstToken(string? rowText);
    protected static string? ExtractFirstStatus(string? rowText, params string[] keywords);
    protected (bool hasMatch, bool emptyState, int rowCount) RunStrictSearch(string term, Action<string> searchAction, Func<List<string>> rowsProvider, Func<bool> emptyStateProvider);
}
```

---

## 🔐 Authentication

### Entra Auth Helper

**Comprehensive Azure AD authentication with multiple flows.**

```csharp
public static class EntraAuthHelper
{
    public static async Task<string> GetAccessTokenAsync();
    public static async Task<string> GetTokenUsingROPCAsync();
    public static async Task<string> GetTokenUsingMsalROPCAsync();
    public static void ClearCache();
    public static async Task<HttpClient> CreateAuthenticatedHttpClientAsync();
    public static async Task AddAuthenticationHeaderAsync(HttpRequestMessage request);
    public static string GetAuthenticationInfoForTestContext();
}
```

---

## Examples & Usage

### Basic Test Implementation

```csharp
[TestClass]
public class ExampleTest : TestBase
{
    private IWebDriver _driver;
    private BasePage _page;

    [TestInitialize]
    public void Setup()
    {
        _driver = InitializeTestAssembly.CreateWebDriver();
        _page = new ExamplePage(_driver);
    }

    [TestMethod]
    public async Task ExampleTestMethod()
    {
        // Use performance monitoring
        await PerformanceMonitor.TimeOperationAsync("ExampleTest", async () =>
        {
            // Use error handling with retry
            var success = await ErrorHandlingUtility.ExecuteWithRetryAsync(async () =>
            {
                _page.NavigateToPage();
                return _page.VerifyPageLoaded();
            }, "PageNavigation", maxRetries: 3);

            Assert.IsTrue(success, "Page should load successfully");
        });
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver?.Quit();
    }
}
```

### Advanced Test with Decorators

```csharp
[TestMethod]
public async Task AdvancedTestWithDecorators()
{
    var testMethod = new Func<Task<bool>>(async () =>
    {
        // Test implementation
        return true;
    });

    // Apply decorators
    var decoratedTest = TestComponentFactory.CreateCompositeDecorator(
        testMethod,
        TestComponentFactory.CreatePerformanceDecorator(testMethod, "AdvancedTest"),
        TestComponentFactory.CreateRetryDecorator(testMethod, 3)
    );

    var result = await decoratedTest.ExecuteAsync();
    Assert.IsTrue(result);
}
```

### Security Validation Example

```csharp
[TestMethod]
public async Task SecurityValidationExample()
{
    var parameters = new Dictionary<string, object>
    {
        ["Url"] = "https://secure.example.com",
        ["Input"] = "user@example.com",
        ["FilePath"] = "/safe/path/file.txt"
    };

    var validation = await AdvancedSecurityManager.ValidateOperationAsync("SecureOperation", parameters);

    Assert.IsTrue(validation.IsSecure, "Operation should be secure");
    Assert.IsTrue(validation.SecurityScore >= 90, "Security score should be high");
}
```

### Performance Monitoring Example

```csharp
[TestMethod]
public async Task PerformanceMonitoringExample()
{
    // Start performance monitoring
    AdvancedPerformanceEngine.Initialize();

    // Use intelligent cache
    var cachedData = await AdvancedPerformanceEngine.IntelligentCache.GetOrCreateAsync(
        "test-data",
        async () => await LoadTestDataAsync(),
        TimeSpan.FromMinutes(5)
    );

    // Execute with parallel processing
    var tasks = Enumerable.Range(1, 10).Select(i =>
        new Func<Task<string>>(async () => await ProcessItemAsync(i))
    );

    var results = await AdvancedPerformanceEngine.ParallelExecutionEngine
        .ExecuteWithAdaptiveConcurrencyAsync(tasks);

    // Get performance analysis
    var analysis = AdvancedPerformanceEngine.AdvancedMonitoring.GetPerformanceAnalysis();

    Assert.IsTrue(analysis.OverallPerformanceScore >= 90, "Performance should be excellent");
}
```

---

## Best Practices

### 1. **Always Use Structured Logging**
```csharp
// Good
TestLogger.LogInformation("User {0} logged in successfully", userId);

// Avoid
Console.WriteLine($"User {userId} logged in");
```

### 2. **Implement Proper Error Handling**
```csharp
// Good
var result = await ErrorHandlingUtility.ExecuteWithRetryAsync(
    async () => await RiskyOperationAsync(),
    "RiskyOperation",
    maxRetries: 3
);

// Avoid
try
{
    await RiskyOperationAsync();
}
catch (Exception ex)
{
    // Handle error
}
```

### 3. **Use Performance Monitoring**
```csharp
// Good
await PerformanceMonitor.TimeOperationAsync("DatabaseQuery", async () =>
{
    return await database.QueryAsync(query);
});

// Avoid
var stopwatch = Stopwatch.StartNew();
await database.QueryAsync(query);
stopwatch.Stop();
```

### 4. **Implement Security Validation**
```csharp
// Good
var validation = await AdvancedSecurityManager.ValidateOperationAsync(
    "UserInput",
    new Dictionary<string, object> { ["Input"] = userInput }
);

// Avoid
// Direct use without validation
```

### 5. **Use Configuration Management**
```csharp
// Good
var timeout = TestConfiguration.GetElementVisibilityTimeout();
Driver.WaitVisible(element, timeout);

// Avoid
Driver.WaitVisible(element, TimeSpan.FromSeconds(15));
```

---

## Getting Started

1. **Install Dependencies**
   ```bash
   dotnet add package Selenium.WebDriver
   dotnet add package MSTest.TestFramework
   dotnet add package Microsoft.Extensions.Configuration
   ```

2. **Configure Framework**
   ```csharp
   [AssemblyInitialize]
   public static void AssemblyInit(TestContext context)
   {
       TestConfiguration.Initialize(context);
       SecretManager.Initialize(context);
       AdvancedPerformanceEngine.Initialize();
       AdvancedSecurityManager.Initialize();
   }
   ```

3. **Create Your First Test**
   ```csharp
   [TestClass]
   public class MyFirstTest : TestBase
   {
       [TestMethod]
       public async Task MyTestMethod()
       {
           await PerformanceMonitor.TimeOperationAsync("MyTest", async () =>
           {
               // Your test implementation
           });
       }
   }
   ```

---

## Support & Community

- **Documentation**: [Complete API Reference](docs/api-reference.md)
- **Examples**: [Code Samples](examples/)
- **Issues**: [GitHub Issues](https://github.com/omerakben/tuel-Selenium-WebDriver-RestSharp/issues)
- **Discussions**: [GitHub Discussions](https://github.com/omerakben/tuel-Selenium-WebDriver-RestSharp/discussions)

---

**Built by [Omer "Ozzy" Akben](https://omerakben.com)**
*Full-Stack Developer • AI Engineer • SDET*
me@omerakben.com • (267) 512-4566

---

*This documentation represents the complete API reference for the TUEL Test Framework - A+++ quality enterprise-grade test automation solution.*
