using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUEL.TestFramework;
using TUEL.TestFramework.Architecture;
using TUEL.TestFramework.Security.Advanced;
using TUEL.TestFramework.Performance.Advanced;
using TUEL.TestFramework.Support;
using TUEL.TestFramework.Monitoring;
using TUEL.TestFramework.Testing;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Examples
{
    /// <summary>
    /// Comprehensive test examples demonstrating A+++ quality testing practices.
    /// These examples showcase all framework features and best practices.
    /// </summary>
    [TestClass]
    public class ComprehensiveTestExamples : TestBase
    {
        private TestEventPublisher _eventPublisher;
        private CommandInvoker _commandInvoker;

        [TestInitialize]
        public async Task TestSetup()
        {
            // Initialize advanced features
            AdvancedPerformanceEngine.Initialize();
            AdvancedSecurityManager.Initialize();

            // Setup event publisher and observers
            _eventPublisher = new TestEventPublisher();
            _commandInvoker = new CommandInvoker();

            // Subscribe to test events
            _eventPublisher.Subscribe(new TestExecutionObserver());

            await _eventPublisher.NotifyTestStartedAsync(TestContext.TestName);

            TestLogger.LogInformation("Test setup completed for: {0}", TestContext.TestName);
        }

        [TestCleanup]
        public new async Task TestCleanup()
        {
            await _eventPublisher.NotifyTestCompletedAsync(TestContext.TestName, true);

            // Generate performance report
            var performanceReport = PerformanceMonitor.GenerateReport();
            TestLogger.LogInformation("Performance Report:\n{0}", performanceReport);

            TestLogger.LogInformation("Test cleanup completed for: {0}", TestContext.TestName);
        }

        #region Architecture Pattern Examples

        [TestMethod]
        [TestCategory("Architecture")]
        [Description("Demonstrates Command pattern implementation with undo/redo capabilities")]
        public async Task CommandPatternExample_ShouldExecuteAndUndoCommands()
        {
            // Arrange
            var navigationCommand = TestComponentFactory.CreateNavigateCommand(
                "https://example.com",
                async () =>
                {
                    TestLogger.LogInformation("Navigating to example.com");
                    await Task.Delay(100); // Simulate navigation
                    return true;
                }
            );

            var elementCommand = TestComponentFactory.CreateElementInteractionCommand(
                "submit-button",
                "click",
                async () =>
                {
                    TestLogger.LogInformation("Clicking submit button");
                    await Task.Delay(50); // Simulate click
                    return true;
                }
            );

            // Act & Assert
            var navigationResult = await _commandInvoker.ExecuteCommandAsync(navigationCommand);
            Assert.IsTrue(navigationResult, "Navigation command should execute successfully");

            var elementResult = await _commandInvoker.ExecuteCommandAsync(elementCommand);
            Assert.IsTrue(elementResult, "Element interaction command should execute successfully");

            // Test undo functionality
            Assert.IsTrue(_commandInvoker.CanUndo, "Should be able to undo commands");
            var undoResult = await _commandInvoker.UndoLastCommandAsync();
            Assert.IsTrue(undoResult, "Should be able to undo last command");

            // Test redo functionality
            Assert.IsTrue(_commandInvoker.CanRedo, "Should be able to redo commands");
            var redoResult = await _commandInvoker.RedoLastCommandAsync();
            Assert.IsTrue(redoResult, "Should be able to redo last command");
        }

        [TestMethod]
        [TestCategory("Architecture")]
        [Description("Demonstrates Builder pattern for complex test configuration")]
        public void BuilderPatternExample_ShouldCreateOptimalConfiguration()
        {
            // Act
            var config = TestComponentFactory.CreateConfigurationBuilder()
                .SetEnvironment("production")
                .SetBrowser("chrome")
                .SetHeadless(true)
                .SetTimeouts(30, 60, 30)
                .SetRetrySettings(3, 1000)
                .SetSecuritySettings(true, true)
                .SetPerformanceSettings(true, 4)
                .Build();

            // Assert
            Assert.AreEqual("production", config.Environment);
            Assert.AreEqual("chrome", config.Browser);
            Assert.IsTrue(config.Headless);
            Assert.AreEqual(TimeSpan.FromSeconds(30), config.DefaultTimeout);
            Assert.AreEqual(TimeSpan.FromSeconds(60), config.PageLoadTimeout);
            Assert.AreEqual(TimeSpan.FromSeconds(30), config.ScriptTimeout);
            Assert.AreEqual(3, config.MaxRetries);
            Assert.AreEqual(TimeSpan.FromMilliseconds(1000), config.RetryDelay);
            Assert.IsTrue(config.ForceHttps);
            Assert.IsTrue(config.EnableAuditLogging);
            Assert.IsTrue(config.EnableWebDriverPooling);
            Assert.AreEqual(4, config.WebDriverPoolSize);
        }

        [TestMethod]
        [TestCategory("Architecture")]
        [Description("Demonstrates Decorator pattern for test method enhancements")]
        public async Task DecoratorPatternExample_ShouldApplyMultipleDecorators()
        {
            // Arrange
            var testMethod = new Func<Task<bool>>(async () =>
            {
                TestLogger.LogInformation("Executing core test logic");
                await Task.Delay(100); // Simulate test execution
                return true;
            });

            // Act - Apply multiple decorators
            var decoratedTest = TestComponentFactory.CreateCompositeDecorator(
                testMethod,
                TestComponentFactory.CreatePerformanceDecorator(testMethod, "DecoratorTest"),
                TestComponentFactory.CreateRetryDecorator(testMethod, 2)
            );

            var result = await decoratedTest.ExecuteAsync();

            // Assert
            Assert.IsTrue(result, "Decorated test should execute successfully");
        }

        #endregion

        #region Security Examples

        [TestMethod]
        [TestCategory("Security")]
        [Description("Demonstrates comprehensive security validation")]
        public async Task SecurityValidationExample_ShouldValidateSecureOperations()
        {
            // Arrange
            var secureParameters = new Dictionary<string, object>
            {
                ["Url"] = "https://secure.example.com",
                ["Input"] = "user@example.com",
                ["FilePath"] = "/safe/path/file.txt",
                ["Credentials"] = "env://SECURE_CREDENTIALS"
            };

            var insecureParameters = new Dictionary<string, object>
            {
                ["Url"] = "http://insecure.example.com",
                ["Input"] = "'; DROP TABLE users; --",
                ["FilePath"] = "../../../sensitive-file.txt",
                ["Credentials"] = "password=plaintext123"
            };

            // Act
            var secureValidation = await AdvancedSecurityManager.ValidateOperationAsync("SecureOperation", secureParameters);
            var insecureValidation = await AdvancedSecurityManager.ValidateOperationAsync("InsecureOperation", insecureParameters);

            // Assert
            Assert.IsTrue(secureValidation.IsSecure, "Secure operation should pass validation");
            Assert.IsTrue(secureValidation.SecurityScore >= 90, "Secure operation should have high security score");
            Assert.AreEqual(0, secureValidation.Threats.Count, "Secure operation should have no threats");

            Assert.IsFalse(insecureValidation.IsSecure, "Insecure operation should fail validation");
            Assert.IsTrue(insecureValidation.SecurityScore < 50, "Insecure operation should have low security score");
            Assert.IsTrue(insecureValidation.Threats.Count > 0, "Insecure operation should have threats detected");
        }

        [TestMethod]
        [TestCategory("Security")]
        [Description("Demonstrates advanced encryption and decryption")]
        public async Task AdvancedEncryptionExample_ShouldEncryptAndDecryptData()
        {
            // Arrange
            var plaintext = "Sensitive test data that needs encryption";
            var key = AdvancedSecurityManager.AdvancedEncryption.GenerateSecureKey();

            // Act
            var encryptionResult = await AdvancedSecurityManager.AdvancedEncryption.EncryptAesGcmAsync(plaintext, key);
            var decryptedText = await AdvancedSecurityManager.AdvancedEncryption.DecryptAesGcmAsync(encryptionResult, key);

            // Assert
            Assert.IsNotNull(encryptionResult.Ciphertext, "Ciphertext should not be null");
            Assert.IsNotNull(encryptionResult.Tag, "Tag should not be null");
            Assert.IsNotNull(encryptionResult.Nonce, "Nonce should not be null");
            Assert.AreEqual("AES-256-GCM", encryptionResult.Algorithm);
            Assert.AreEqual(plaintext, decryptedText, "Decrypted text should match original");
        }

        [TestMethod]
        [TestCategory("Security")]
        [Description("Demonstrates JWT token validation")]
        public async Task JwtValidationExample_ShouldValidateTokenStructure()
        {
            // Arrange
            var validToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            var invalidToken = "invalid.token.structure";

            // Act
            var validResult = await AdvancedSecurityManager.AuthenticationSecurity.ValidateJwtTokenAsync(validToken);
            var invalidResult = await AdvancedSecurityManager.AuthenticationSecurity.ValidateJwtTokenAsync(invalidToken);

            // Assert
            Assert.IsTrue(validResult.IsValid, "Valid token should pass validation");
            Assert.IsTrue(validResult.SignatureValid, "Valid token should have valid signature");
            Assert.IsTrue(validResult.OverallValid, "Valid token should be overall valid");

            Assert.IsFalse(invalidResult.IsValid, "Invalid token should fail validation");
            Assert.IsTrue(invalidResult.Errors.Count > 0, "Invalid token should have validation errors");
        }

        #endregion

        #region Performance Examples

        [TestMethod]
        [TestCategory("Performance")]
        [Description("Demonstrates intelligent caching with automatic expiration")]
        public async Task IntelligentCacheExample_ShouldCacheAndInvalidateData()
        {
            // Arrange
            var cacheKey = "test-data";
            var callCount = 0;

            // Act - First call (cache miss)
            var data1 = await AdvancedPerformanceEngine.IntelligentCache.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    callCount++;
                    TestLogger.LogInformation("Cache miss - generating data");
                    await Task.Delay(100); // Simulate data generation
                    return $"Generated data {callCount}";
                },
                TimeSpan.FromSeconds(2)
            );

            // Second call (cache hit)
            var data2 = await AdvancedPerformanceEngine.IntelligentCache.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    callCount++;
                    TestLogger.LogInformation("Cache miss - generating data");
                    await Task.Delay(100);
                    return $"Generated data {callCount}";
                },
                TimeSpan.FromSeconds(2)
            );

            // Wait for expiration
            await Task.Delay(3000);

            // Third call (cache miss after expiration)
            var data3 = await AdvancedPerformanceEngine.IntelligentCache.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    callCount++;
                    TestLogger.LogInformation("Cache miss after expiration - generating data");
                    await Task.Delay(100);
                    return $"Generated data {callCount}";
                },
                TimeSpan.FromSeconds(2)
            );

            // Assert
            Assert.AreEqual("Generated data 1", data1);
            Assert.AreEqual("Generated data 1", data2, "Second call should use cached data");
            Assert.AreEqual("Generated data 2", data3, "Third call should generate new data after expiration");
            Assert.AreEqual(2, callCount, "Data generation should be called twice");
        }

        [TestMethod]
        [TestCategory("Performance")]
        [Description("Demonstrates parallel execution with adaptive concurrency")]
        public async Task ParallelExecutionExample_ShouldExecuteTasksInParallel()
        {
            // Arrange
            var tasks = new List<Func<Task<string>>>();
            for (int i = 1; i <= 10; i++)
            {
                var taskId = i; // Capture loop variable
                tasks.Add(async () =>
                {
                    TestLogger.LogInformation("Executing task {0}", taskId);
                    await Task.Delay(100); // Simulate work
                    return $"Task {taskId} completed";
                });
            }

            // Act
            var results = await AdvancedPerformanceEngine.ParallelExecutionEngine
                .ExecuteWithAdaptiveConcurrencyAsync(tasks);

            // Assert
            Assert.AreEqual(10, results.Count, "All tasks should complete");
            for (int i = 0; i < results.Count; i++)
            {
                Assert.AreEqual($"Task {i + 1} completed", results[i], $"Task {i + 1} should complete correctly");
            }
        }

        [TestMethod]
        [TestCategory("Performance")]
        [Description("Demonstrates comprehensive performance monitoring")]
        public async Task PerformanceMonitoringExample_ShouldTrackDetailedMetrics()
        {
            // Arrange
            var operationName = "PerformanceTestOperation";
            var context = new Dictionary<string, object>
            {
                ["TestType"] = "Performance",
                ["Environment"] = "Test",
                ["Browser"] = "Chrome"
            };

            // Act
            for (int i = 0; i < 5; i++)
            {
                var duration = 100 + (i * 50); // Varying durations
                AdvancedPerformanceEngine.AdvancedMonitoring.RecordDetailedMetric(
                    operationName,
                    duration,
                    context
                );
                await Task.Delay(10); // Small delay between recordings
            }

            // Get performance analysis
            var analysis = AdvancedPerformanceEngine.AdvancedMonitoring.GetPerformanceAnalysis();
            var operationStats = analysis.OperationStatistics[operationName];

            // Assert
            Assert.IsNotNull(operationStats, "Operation statistics should be available");
            Assert.AreEqual(5, operationStats.ExecutionCount, "Should have 5 executions");
            Assert.AreEqual(5, analysis.TotalOperations, "Should track 1 operation");
            Assert.IsTrue(analysis.OverallPerformanceScore >= 0, "Performance score should be calculated");
        }

        #endregion

        #region Error Handling Examples

        [TestMethod]
        [TestCategory("ErrorHandling")]
        [Description("Demonstrates retry mechanism with exponential backoff")]
        public async Task RetryMechanismExample_ShouldRetryOnFailure()
        {
            // Arrange
            var attemptCount = 0;
            var maxRetries = 3;

            // Act
            var result = await ErrorHandlingUtility.ExecuteWithRetryAsync(
                async () =>
                {
                    attemptCount++;
                    TestLogger.LogInformation("Attempt {0}/{1}", attemptCount, maxRetries);

                    if (attemptCount < 3)
                    {
                        throw new InvalidOperationException($"Simulated failure on attempt {attemptCount}");
                    }

                    // Success on third attempt
                },
                "RetryTest",
                maxRetries: maxRetries,
                retryDelay: TimeSpan.FromMilliseconds(100)
            );

            // Assert
            Assert.IsTrue(result, "Operation should succeed after retries");
            Assert.AreEqual(3, attemptCount, "Should attempt 3 times");
        }

        [TestMethod]
        [TestCategory("ErrorHandling")]
        [Description("Demonstrates safe execution without exceptions")]
        public void SafeExecutionExample_ShouldHandleExceptionsGracefully()
        {
            // Arrange
            var riskyOperation = new Action(() =>
            {
                throw new InvalidOperationException("Simulated risky operation failure");
            });

            var safeOperation = new Func<string>(() =>
            {
                return "Safe operation result";
            });

            // Act
            var riskyResult = ErrorHandlingUtility.SafeExecute(riskyOperation, "RiskyOperation");
            var safeResult = ErrorHandlingUtility.SafeExecute(safeOperation, "SafeOperation", "default");

            // Assert
            Assert.IsFalse(riskyResult, "Risky operation should fail gracefully");
            Assert.AreEqual("Safe operation result", safeResult, "Safe operation should return correct result");
        }

        [TestMethod]
        [TestCategory("ErrorHandling")]
        [Description("Demonstrates user-friendly error messages")]
        public void UserFriendlyErrorExample_ShouldProvideHelpfulMessages()
        {
            // Arrange
            var timeoutException = new TimeoutException("Operation timed out");
            var httpException = new System.Net.Http.HttpRequestException("Network error");
            var genericException = new InvalidOperationException("Something went wrong");

            // Act
            var timeoutMessage = ErrorHandlingUtility.GetUserFriendlyErrorMessage(timeoutException);
            var httpMessage = ErrorHandlingUtility.GetUserFriendlyErrorMessage(httpException);
            var genericMessage = ErrorHandlingUtility.GetUserFriendlyErrorMessage(genericException);

            // Assert
            Assert.IsTrue(timeoutMessage.Contains("timed out"), "Timeout message should be user-friendly");
            Assert.IsTrue(httpMessage.Contains("Network communication failed"), "HTTP message should be user-friendly");
            Assert.IsTrue(genericMessage.Contains("unexpected error"), "Generic message should be user-friendly");
        }

        #endregion

        #region Test Coverage Examples

        [TestMethod]
        [TestCategory("TestCoverage")]
        [Description("Demonstrates test coverage analysis")]
        public void TestCoverageExample_ShouldAnalyzeCoverage()
        {
            // Act
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var coverageAnalysis = TestCoverageValidator.AnalyzeCoverage(assembly);
            var criticalValidation = TestCoverageValidator.ValidateCriticalCoverage(assembly);
            var namingValidation = TestCoverageValidator.ValidateTestNamingConventions(assembly);

            // Assert
            Assert.IsNotNull(coverageAnalysis, "Coverage analysis should be available");
            Assert.IsTrue(coverageAnalysis.TestClasses > 0, "Should have test classes");
            Assert.IsTrue(coverageAnalysis.TotalTestMethods > 0, "Should have test methods");
            Assert.IsTrue(coverageAnalysis.CoveragePercentage >= 0, "Coverage percentage should be calculated");

            Assert.IsNotNull(criticalValidation, "Critical validation should be available");
            Assert.IsTrue(criticalValidation.CoveragePercentage >= 0, "Critical coverage percentage should be calculated");

            Assert.IsNotNull(namingValidation, "Naming validation should be available");
        }

        #endregion

        #region Integration Examples

        [TestMethod]
        [TestCategory("Integration")]
        [Description("Demonstrates complete framework integration")]
        public async Task CompleteIntegrationExample_ShouldUseAllFrameworkFeatures()
        {
            // This test demonstrates the complete integration of all framework features

            // 1. Performance Monitoring
            await PerformanceMonitor.TimeOperationAsync("CompleteIntegrationTest", async () =>
            {
                // 2. Security Validation
                var securityParams = new Dictionary<string, object>
                {
                    ["Url"] = "https://secure.example.com",
                    ["Input"] = "test@example.com"
                };

                var securityValidation = await AdvancedSecurityManager.ValidateOperationAsync("IntegrationTest", securityParams);
                Assert.IsTrue(securityValidation.IsSecure, "Security validation should pass");

                // 3. Error Handling with Retry
                var operationResult = await ErrorHandlingUtility.ExecuteWithRetryAsync(
                    async () =>
                    {
                        // 4. Intelligent Caching
                        var cachedData = await AdvancedPerformanceEngine.IntelligentCache.GetOrCreateAsync(
                            "integration-data",
                            async () =>
                            {
                                await Task.Delay(50);
                                return "Integration test data";
                            },
                            TimeSpan.FromMinutes(1)
                        );

                        Assert.AreEqual("Integration test data", cachedData);
                    },
                    "IntegrationOperation",
                    maxRetries: 3
                );

                Assert.IsTrue(operationResult, "Operation should succeed");

                // 5. Command Pattern
                var command = TestComponentFactory.CreateNavigateCommand(
                    "https://example.com",
                    async () =>
                    {
                        await Task.Delay(10);
                        return true;
                    }
                );

                var commandResult = await _commandInvoker.ExecuteCommandAsync(command);
                Assert.IsTrue(commandResult, "Command should execute successfully");

                // 6. Structured Logging
                TestLogger.LogInformation("Complete integration test executed successfully");
            });

            // 7. Performance Analysis
            var performanceAnalysis = AdvancedPerformanceEngine.AdvancedMonitoring.GetPerformanceAnalysis();
            Assert.IsNotNull(performanceAnalysis, "Performance analysis should be available");

            // 8. Memory Optimization
            AdvancedPerformanceEngine.MemoryOptimizer.OptimizeMemoryUsage();
            var memoryStats = AdvancedPerformanceEngine.MemoryOptimizer.GetMemoryStatistics();
            Assert.IsNotNull(memoryStats, "Memory statistics should be available");
        }

        #endregion
    }

    /// <summary>
    /// Test execution observer for demonstrating Observer pattern.
    /// </summary>
    public class TestExecutionObserver : ITestObserver
    {
        public async Task OnTestStartedAsync(string testName)
        {
            TestLogger.LogInformation("Observer: Test started - {0}", testName);
            await Task.CompletedTask;
        }

        public async Task OnTestCompletedAsync(string testName, bool success)
        {
            TestLogger.LogInformation("Observer: Test completed - {0}, Success: {1}", testName, success);
            await Task.CompletedTask;
        }

        public async Task OnTestFailedAsync(string testName, Exception exception)
        {
            TestLogger.LogError("Observer: Test failed - {0}, Error: {1}", testName, exception.Message);
            await Task.CompletedTask;
        }

        public async Task OnTestStepExecutedAsync(string testName, string stepName, bool success)
        {
            TestLogger.LogDebug("Observer: Test step executed - {0}, Step: {1}, Success: {2}", testName, stepName, success);
            await Task.CompletedTask;
        }
    }
}
