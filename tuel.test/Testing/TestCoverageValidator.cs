using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Testing
{
    /// <summary>
    /// Comprehensive test coverage validation utility.
    /// Provides test coverage analysis, validation, and reporting capabilities.
    /// </summary>
    public static class TestCoverageValidator
    {
        /// <summary>
        /// Analyzes test coverage for a given assembly.
        /// </summary>
        /// <param name="assembly">Assembly to analyze</param>
        /// <returns>Test coverage analysis results</returns>
        public static TestCoverageAnalysis AnalyzeCoverage(Assembly assembly)
        {
            var analysis = new TestCoverageAnalysis();

            var allTypes = assembly.GetTypes();
            var testClasses = allTypes.Where(t => IsTestClass(t)).ToList();
            var nonTestClasses = allTypes.Where(t => !IsTestClass(t) && !IsFrameworkClass(t)).ToList();

            analysis.TotalClasses = nonTestClasses.Count;
            analysis.TestClasses = testClasses.Count;
            analysis.CoveredClasses = 0;

            foreach (var testClass in testClasses)
            {
                var testMethods = GetTestMethods(testClass);
                analysis.TotalTestMethods += testMethods.Count;

                // Analyze test categories
                foreach (var method in testMethods)
                {
                    var categories = GetTestCategories(method);
                    foreach (var category in categories)
                    {
                        if (!analysis.TestCategories.ContainsKey(category))
                        {
                            analysis.TestCategories[category] = 0;
                        }
                        analysis.TestCategories[category]++;
                    }
                }

                // Check if this test class covers a corresponding business class
                var coveredClass = FindCoveredClass(testClass, nonTestClasses);
                if (coveredClass != null)
                {
                    analysis.CoveredClasses++;
                    analysis.CoverageMap[testClass.Name] = coveredClass.Name;
                }
            }

            analysis.CoveragePercentage = analysis.TotalClasses > 0
                ? (double)analysis.CoveredClasses / analysis.TotalClasses * 100
                : 0;

            return analysis;
        }

        /// <summary>
        /// Validates that all critical business classes have corresponding tests.
        /// </summary>
        /// <param name="assembly">Assembly to validate</param>
        /// <returns>Validation results</returns>
        public static TestValidationResult ValidateCriticalCoverage(Assembly assembly)
        {
            var result = new TestValidationResult();
            var analysis = AnalyzeCoverage(assembly);

            // Define critical classes that must have tests
            var criticalPatterns = new[]
            {
                "Auth", "Authentication", "Login", "Security",
                "API", "Controller", "Service", "Manager",
                "Configuration", "Settings", "Options"
            };

            var allTypes = assembly.GetTypes();
            var businessClasses = allTypes.Where(t => !IsTestClass(t) && !IsFrameworkClass(t)).ToList();

            foreach (var businessClass in businessClasses)
            {
                var isCritical = criticalPatterns.Any(pattern =>
                    businessClass.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));

                if (isCritical)
                {
                    var hasTest = HasCorrespondingTest(businessClass, allTypes);
                    if (!hasTest)
                    {
                        result.MissingCriticalTests.Add(businessClass.Name);
                        TestLogger.LogWarning("Critical class '{0}' lacks corresponding test coverage", businessClass.Name);
                    }
                }
            }

            result.IsValid = result.MissingCriticalTests.Count == 0;
            result.CoveragePercentage = analysis.CoveragePercentage;

            return result;
        }

        /// <summary>
        /// Generates a comprehensive test coverage report.
        /// </summary>
        /// <param name="assembly">Assembly to analyze</param>
        /// <returns>Formatted test coverage report</returns>
        public static string GenerateCoverageReport(Assembly assembly)
        {
            var analysis = AnalyzeCoverage(assembly);
            var validation = ValidateCriticalCoverage(assembly);

            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Test Coverage Analysis Report ===");
            report.AppendLine($"Assembly: {assembly.GetName().Name}");
            report.AppendLine($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine();

            // Overall statistics
            report.AppendLine("## Overall Statistics");
            report.AppendLine($"Total Business Classes: {analysis.TotalClasses}");
            report.AppendLine($"Test Classes: {analysis.TestClasses}");
            report.AppendLine($"Covered Classes: {analysis.CoveredClasses}");
            report.AppendLine($"Coverage Percentage: {analysis.CoveragePercentage:F1}%");
            report.AppendLine($"Total Test Methods: {analysis.TotalTestMethods}");
            report.AppendLine();

            // Test categories
            if (analysis.TestCategories.Any())
            {
                report.AppendLine("## Test Categories");
                foreach (var (category, count) in analysis.TestCategories.OrderByDescending(kvp => kvp.Value))
                {
                    report.AppendLine($"- {category}: {count} tests");
                }
                report.AppendLine();
            }

            // Coverage mapping
            if (analysis.CoverageMap.Any())
            {
                report.AppendLine("## Test Coverage Mapping");
                foreach (var (testClass, businessClass) in analysis.CoverageMap)
                {
                    report.AppendLine($"- {testClass} → {businessClass}");
                }
                report.AppendLine();
            }

            // Critical coverage validation
            report.AppendLine("## Critical Coverage Validation");
            if (validation.IsValid)
            {
                report.AppendLine("PASS: All critical classes have test coverage");
            }
            else
            {
                report.AppendLine("WARNING: Missing test coverage for critical classes:");
                foreach (var missingClass in validation.MissingCriticalTests)
                {
                    report.AppendLine($"- {missingClass}");
                }
            }
            report.AppendLine();

            // Recommendations
            report.AppendLine("## Recommendations");
            if (analysis.CoveragePercentage < 80)
            {
                report.AppendLine("WARNING: Test coverage is below 80%. Consider adding more tests.");
            }
            else if (analysis.CoveragePercentage < 90)
            {
                report.AppendLine("INFO: Test coverage is good but could be improved to reach 90%.");
            }
            else
            {
                report.AppendLine("PASS: Excellent test coverage!");
            }

            if (validation.MissingCriticalTests.Any())
            {
                report.AppendLine("SECURITY: Add tests for critical security and business logic classes.");
            }

            return report.ToString();
        }

        /// <summary>
        /// Validates that test methods follow naming conventions.
        /// </summary>
        /// <param name="assembly">Assembly to validate</param>
        /// <returns>List of test methods that don't follow conventions</returns>
        public static List<string> ValidateTestNamingConventions(Assembly assembly)
        {
            var violations = new List<string>();
            var testClasses = assembly.GetTypes().Where(t => IsTestClass(t)).ToList();

            foreach (var testClass in testClasses)
            {
                var testMethods = GetTestMethods(testClass);

                foreach (var method in testMethods)
                {
                    var methodName = method.Name;

                    // Check if method name follows convention: MethodName_Scenario_ExpectedResult
                    if (!IsValidTestMethodName(methodName))
                    {
                        violations.Add($"{testClass.Name}.{methodName}");
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// Analyzes test execution patterns and identifies potential issues.
        /// </summary>
        /// <param name="assembly">Assembly to analyze</param>
        /// <returns>Test execution analysis</returns>
        public static TestExecutionAnalysis AnalyzeTestExecution(Assembly assembly)
        {
            var analysis = new TestExecutionAnalysis();
            var testClasses = assembly.GetTypes().Where(t => IsTestClass(t)).ToList();

            foreach (var testClass in testClasses)
            {
                var testMethods = GetTestMethods(testClass);

                foreach (var method in testMethods)
                {
                    // Check for proper test attributes
                    var hasTestMethod = method.GetCustomAttribute<TestMethodAttribute>() != null;
                    var hasDescription = method.GetCustomAttribute<DescriptionAttribute>() != null;
                    var hasTestCategory = method.GetCustomAttribute<TestCategoryAttribute>() != null;

                    if (!hasTestMethod)
                    {
                        analysis.MissingTestMethodAttribute.Add($"{testClass.Name}.{method.Name}");
                    }

                    if (!hasDescription)
                    {
                        analysis.MissingDescription.Add($"{testClass.Name}.{method.Name}");
                    }

                    if (!hasTestCategory)
                    {
                        analysis.MissingTestCategory.Add($"{testClass.Name}.{method.Name}");
                    }

                    // Check for proper setup/cleanup methods
                    var hasSetup = method.Name.Contains("Setup") || method.Name.Contains("Initialize");
                    var hasCleanup = method.Name.Contains("Cleanup") || method.Name.Contains("TearDown");

                    if (hasSetup && !method.GetCustomAttribute<TestInitializeAttribute>() != null)
                    {
                        analysis.IncorrectSetupMethod.Add($"{testClass.Name}.{method.Name}");
                    }

                    if (hasCleanup && !method.GetCustomAttribute<TestCleanupAttribute>() != null)
                    {
                        analysis.IncorrectCleanupMethod.Add($"{testClass.Name}.{method.Name}");
                    }
                }
            }

            return analysis;
        }

        #region Private Helper Methods

        private static bool IsTestClass(Type type)
        {
            return type.GetCustomAttribute<TestClassAttribute>() != null ||
                   type.Name.EndsWith("Test", StringComparison.OrdinalIgnoreCase) ||
                   type.Name.EndsWith("Tests", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFrameworkClass(Type type)
        {
            var frameworkNamespaces = new[]
            {
                "Microsoft.VisualStudio.TestTools.UnitTesting",
                "OpenQA.Selenium",
                "RestSharp",
                "System.",
                "Microsoft."
            };

            return frameworkNamespaces.Any(ns => type.Namespace?.StartsWith(ns) == true);
        }

        private static List<MethodInfo> GetTestMethods(Type testClass)
        {
            return testClass.GetMethods()
                .Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null)
                .ToList();
        }

        private static List<string> GetTestCategories(MethodInfo method)
        {
            var categoryAttributes = method.GetCustomAttributes<TestCategoryAttribute>();
            return categoryAttributes.SelectMany(attr => attr.TestCategories).ToList();
        }

        private static Type? FindCoveredClass(Type testClass, List<Type> businessClasses)
        {
            var testClassName = testClass.Name;

            // Remove common test suffixes
            var baseName = testClassName
                .Replace("Test", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Tests", "", StringComparison.OrdinalIgnoreCase);

            return businessClasses.FirstOrDefault(bc =>
                bc.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase) ||
                bc.Name.Contains(baseName, StringComparison.OrdinalIgnoreCase));
        }

        private static bool HasCorrespondingTest(Type businessClass, List<Type> allTypes)
        {
            var testClasses = allTypes.Where(t => IsTestClass(t)).ToList();
            return FindCoveredClass(businessClass, testClasses) != null;
        }

        private static bool IsValidTestMethodName(string methodName)
        {
            // Valid patterns: MethodName_Scenario_ExpectedResult, MethodName_WhenCondition_ThenResult
            var validPatterns = new[]
            {
                @"^[A-Z][a-zA-Z0-9]*_[A-Z][a-zA-Z0-9]*_[A-Z][a-zA-Z0-9]*$",
                @"^[A-Z][a-zA-Z0-9]*_When[A-Z][a-zA-Z0-9]*_Then[A-Z][a-zA-Z0-9]*$",
                @"^[A-Z][a-zA-Z0-9]*_Should[A-Z][a-zA-Z0-9]*$"
            };

            return validPatterns.Any(pattern => System.Text.RegularExpressions.Regex.IsMatch(methodName, pattern));
        }

        #endregion
    }

    /// <summary>
    /// Represents test coverage analysis results.
    /// </summary>
    public class TestCoverageAnalysis
    {
        public int TotalClasses { get; set; }
        public int TestClasses { get; set; }
        public int CoveredClasses { get; set; }
        public double CoveragePercentage { get; set; }
        public int TotalTestMethods { get; set; }
        public Dictionary<string, int> TestCategories { get; set; } = new();
        public Dictionary<string, string> CoverageMap { get; set; } = new();
    }

    /// <summary>
    /// Represents test validation results.
    /// </summary>
    public class TestValidationResult
    {
        public bool IsValid { get; set; }
        public double CoveragePercentage { get; set; }
        public List<string> MissingCriticalTests { get; set; } = new();
    }

    /// <summary>
    /// Represents test execution analysis results.
    /// </summary>
    public class TestExecutionAnalysis
    {
        public List<string> MissingTestMethodAttribute { get; set; } = new();
        public List<string> MissingDescription { get; set; } = new();
        public List<string> MissingTestCategory { get; set; } = new();
        public List<string> IncorrectSetupMethod { get; set; } = new();
        public List<string> IncorrectCleanupMethod { get; set; } = new();
    }
}
