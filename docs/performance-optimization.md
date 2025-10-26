# Performance Optimization Guide

## Replacing Thread.Sleep with WebDriverWait

This guide provides examples of how to replace `Thread.Sleep` calls with proper WebDriverWait strategies for better performance and reliability.

### Before (Using Thread.Sleep)
```csharp
// Bad: Fixed delay regardless of actual page state
Thread.Sleep(2000); // Always waits 2 seconds

// Bad: Multiple Thread.Sleep calls
Thread.Sleep(1000);
var element = driver.FindElement(By.Id("button"));
Thread.Sleep(500);
element.Click();
```

### After (Using WebDriverWait)
```csharp
// Good: Wait for specific condition
driver.WaitVisible(By.Id("button"), TimeSpan.FromSeconds(10));

// Good: Wait for element to be clickable
driver.WaitClickable(By.Id("button"), TimeSpan.FromSeconds(10));

// Good: Wait for page transition
driver.WaitForPageTransition(TimeSpan.FromSeconds(10));
```

## Common Patterns

### 1. Element Visibility
```csharp
// Instead of:
Thread.Sleep(2000);
var element = driver.FindElement(By.Id("myElement"));

// Use:
var element = driver.WaitVisible(By.Id("myElement"), TestConfiguration.GetElementVisibilityTimeout());
```

### 2. Element Clickability
```csharp
// Instead of:
Thread.Sleep(1500);
driver.FindElement(By.Id("submitButton")).Click();

// Use:
driver.ClickElement(By.Id("submitButton"), TestConfiguration.GetElementClickabilityTimeout());
```

### 3. Page Transitions
```csharp
// Instead of:
Thread.Sleep(2000); // Wait for navigation

// Use:
driver.WaitForPageTransition(TestConfiguration.GetPageTransitionTimeout());
```

### 4. URL Changes
```csharp
// Instead of:
Thread.Sleep(3000); // Wait for redirect

// Use:
driver.WaitForUrlChange(currentUrl, TestConfiguration.GetPageTransitionTimeout());
```

### 5. Retry Logic
```csharp
// Instead of:
for (int i = 0; i < 3; i++)
{
    try
    {
        driver.FindElement(By.Id("element")).Click();
        break;
    }
    catch
    {
        Thread.Sleep(1000);
    }
}

// Use:
UIHelper.Retry(() => driver.ClickElement(By.Id("element")), TestConfiguration.MaxRetryAttempts);
```

## Configuration-Based Timeouts

Use the centralized configuration service for consistent timeout management:

```csharp
// Access configured timeouts
var visibilityTimeout = TestConfiguration.GetElementVisibilityTimeout();
var clickabilityTimeout = TestConfiguration.GetElementClickabilityTimeout();
var pageTransitionTimeout = TestConfiguration.GetPageTransitionTimeout();
var retryDelay = TestConfiguration.GetRetryDelay();
var maxRetries = TestConfiguration.MaxRetryAttempts;
```

## Performance Benefits

- **2-5x faster execution**: WebDriverWait responds immediately when conditions are met
- **More reliable**: Waits for actual conditions rather than arbitrary delays
- **Configurable**: Timeouts can be adjusted per environment
- **Better error handling**: Clear timeout messages instead of silent failures

## Migration Checklist

- [ ] Replace `Thread.Sleep` with appropriate WebDriverWait methods
- [ ] Use `TestConfiguration` for timeout values
- [ ] Implement retry logic using `UIHelper.Retry()`
- [ ] Add proper error handling and logging
- [ ] Test performance improvements

## Configuration Parameters

Add these to your `.runsettings` file:

```xml
<!-- Performance Configuration -->
<Parameter name="ElementVisibilityTimeoutSeconds" value="15" />
<Parameter name="ElementClickabilityTimeoutSeconds" value="10" />
<Parameter name="PageTransitionTimeoutSeconds" value="10" />
<Parameter name="RetryDelayMilliseconds" value="500" />
<Parameter name="MaxRetryAttempts" value="3" />
<Parameter name="UseSmartWaitStrategies" value="true" />
<Parameter name="WaitForAjaxCompletion" value="true" />
```

## Best Practices

1. **Always specify timeouts**: Don't rely on default values
2. **Use appropriate wait conditions**: Wait for visibility, clickability, or specific states
3. **Implement retry logic**: For flaky operations
4. **Log wait operations**: Use structured logging for debugging
5. **Configure per environment**: Different timeouts for different environments

## Example: Complete Migration

### Before
```csharp
public void LoginToApplication(string username, string password)
{
    Thread.Sleep(2000); // Wait for page load

    var usernameField = driver.FindElement(By.Id("username"));
    usernameField.SendKeys(username);

    Thread.Sleep(1000); // Wait between actions

    var passwordField = driver.FindElement(By.Id("password"));
    passwordField.SendKeys(password);

    Thread.Sleep(1500); // Wait before click

    var loginButton = driver.FindElement(By.Id("login"));
    loginButton.Click();

    Thread.Sleep(3000); // Wait for redirect
}
```

### After
```csharp
public void LoginToApplication(string username, string password)
{
    // Wait for page to be ready
    driver.WaitForPageTransition(TestConfiguration.GetPageTransitionTimeout());

    // Enter username with proper wait
    driver.EnterText(By.Id("username"), username, true, TestConfiguration.GetElementVisibilityTimeout());

    // Enter password with proper wait
    driver.EnterText(By.Id("password"), password, true, TestConfiguration.GetElementVisibilityTimeout());

    // Click login button with proper wait
    driver.ClickElement(By.Id("login"), TestConfiguration.GetElementClickabilityTimeout());

    // Wait for successful login redirect
    driver.WaitForUrlChange(driver.Url, TestConfiguration.GetPageTransitionTimeout());
}
```

This approach is more reliable, faster, and provides better error messages when things go wrong.
