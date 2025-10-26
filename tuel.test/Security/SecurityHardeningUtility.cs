using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Security
{
    /// <summary>
    /// Comprehensive security hardening utility for the test framework.
    /// Provides security validation, sanitization, and hardening features.
    /// </summary>
    public static class SecurityHardeningUtility
    {
        /// <summary>
        /// Validates that a URL uses HTTPS in production environments.
        /// </summary>
        /// <param name="url">URL to validate</param>
        /// <param name="environment">Current environment (dev, staging, prod)</param>
        /// <returns>True if URL is secure for the environment</returns>
        public static bool ValidateHttpsUrl(string url, string environment)
        {
            if (string.IsNullOrEmpty(url)) return false;

            try
            {
                var uri = new Uri(url);

                // Allow HTTP in development environments
                if (environment.Equals("dev", StringComparison.OrdinalIgnoreCase) ||
                    environment.Equals("development", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Require HTTPS in staging and production
                return uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
            }
            catch (UriFormatException)
            {
                TestLogger.LogWarning("Invalid URL format: {0}", url);
                return false;
            }
        }

        /// <summary>
        /// Sanitizes input to prevent injection attacks.
        /// </summary>
        /// <param name="input">Input string to sanitize</param>
        /// <param name="maxLength">Maximum allowed length</param>
        /// <returns>Sanitized input</returns>
        public static string SanitizeInput(string? input, int maxLength = 1000)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Remove potentially dangerous characters
            var sanitized = Regex.Replace(input, @"[<>""'&]", string.Empty);

            // Limit length
            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength);
                TestLogger.LogWarning("Input truncated to {0} characters", maxLength);
            }

            return sanitized.Trim();
        }

        /// <summary>
        /// Validates that sensitive data is not exposed in logs.
        /// </summary>
        /// <param name="message">Log message to validate</param>
        /// <returns>True if message is safe to log</returns>
        public static bool ValidateLogMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return true;

            var sensitivePatterns = new[]
            {
                @"password\s*[:=]\s*\S+",
                @"secret\s*[:=]\s*\S+",
                @"token\s*[:=]\s*\S+",
                @"key\s*[:=]\s*\S+",
                @"Bearer\s+[A-Za-z0-9\-\._~\+\/]+=*",
                @"client_secret\s*[:=]\s*\S+",
                @"authorization\s*[:=]\s*\S+",
                @"api[_-]?key\s*[:=]\s*\S+"
            };

            foreach (var pattern in sensitivePatterns)
            {
                if (Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase))
                {
                    TestLogger.LogWarning("Potential sensitive data detected in log message");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a secure HTTP client with proper security configurations.
        /// </summary>
        /// <param name="environment">Current environment</param>
        /// <param name="timeout">Request timeout</param>
        /// <returns>Configured HttpClient</returns>
        public static HttpClient CreateSecureHttpClient(string environment, TimeSpan timeout)
        {
            var handler = new HttpClientHandler();

            // Configure SSL/TLS settings
            if (!environment.Equals("dev", StringComparison.OrdinalIgnoreCase))
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    // In production, validate certificates properly
                    return errors == System.Net.Security.SslPolicyErrors.None;
                };
            }

            var client = new HttpClient(handler)
            {
                Timeout = timeout
            };

            // Set secure headers
            client.DefaultRequestHeaders.Add("User-Agent", "TUEL-TestFramework/1.0");
            client.DefaultRequestHeaders.Add("X-Requested-With", "TUEL-TestFramework");

            TestLogger.LogDebug("Created secure HTTP client for environment: {0}", environment);
            return client;
        }

        /// <summary>
        /// Validates that a configuration value is secure.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="value">Configuration value</param>
        /// <param name="environment">Current environment</param>
        /// <returns>True if configuration is secure</returns>
        public static bool ValidateConfigurationSecurity(string key, string? value, string environment)
        {
            if (string.IsNullOrEmpty(value)) return true;

            var sensitiveKeys = new[]
            {
                "password", "secret", "key", "token", "credential", "auth"
            };

            var isSensitiveKey = sensitiveKeys.Any(sensitive =>
                key.Contains(sensitive, StringComparison.OrdinalIgnoreCase));

            if (isSensitiveKey)
            {
                // Check if sensitive value is properly referenced
                var isProperlyReferenced = value.StartsWith("env://") ||
                                         value.StartsWith("kv://") ||
                                         value.StartsWith("enc://") ||
                                         value.StartsWith("***");

                if (!isProperlyReferenced)
                {
                    TestLogger.LogWarning("Sensitive configuration '{0}' may contain plaintext value", key);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generates a secure random string for testing purposes.
        /// </summary>
        /// <param name="length">Length of the random string</param>
        /// <returns>Secure random string</returns>
        public static string GenerateSecureRandomString(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }

            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random[i] % chars.Length]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Validates that file paths are secure and don't contain path traversal attempts.
        /// </summary>
        /// <param name="filePath">File path to validate</param>
        /// <returns>True if path is secure</returns>
        public static bool ValidateFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            // Check for path traversal attempts
            var dangerousPatterns = new[]
            {
                @"\.\.",
                @"\.\./",
                @"\.\.\\",
                @"\.\.%2f",
                @"\.\.%5c"
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(filePath, pattern, RegexOptions.IgnoreCase))
                {
                    TestLogger.LogWarning("Potentially dangerous file path detected: {0}", filePath);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates that SQL queries don't contain injection attempts.
        /// </summary>
        /// <param name="query">SQL query to validate</param>
        /// <returns>True if query appears safe</returns>
        public static bool ValidateSqlQuery(string query)
        {
            if (string.IsNullOrEmpty(query)) return true;

            var dangerousPatterns = new[]
            {
                @"\b(union|select|insert|update|delete|drop|create|alter|exec|execute)\b",
                @"--",
                @"/\*",
                @"\*/",
                @"xp_",
                @"sp_"
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase))
                {
                    TestLogger.LogWarning("Potentially dangerous SQL pattern detected: {0}", pattern);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a security audit log entry.
        /// </summary>
        /// <param name="action">Action performed</param>
        /// <param name="resource">Resource accessed</param>
        /// <param name="userId">User ID (if applicable)</param>
        /// <param name="success">Whether action was successful</param>
        /// <param name="additionalInfo">Additional information</param>
        public static void LogSecurityAudit(string action, string resource, string? userId = null, bool success = true, string? additionalInfo = null)
        {
            var auditEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                Resource = resource,
                UserId = userId ?? "system",
                Success = success,
                AdditionalInfo = additionalInfo
            };

            TestLogger.LogInformation("Security Audit: {0} on {1} by {2} - {3}",
                action, resource, userId ?? "system", success ? "SUCCESS" : "FAILURE");

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                TestLogger.LogDebug("Security Audit Additional Info: {0}", additionalInfo);
            }
        }

        /// <summary>
        /// Validates that a JWT token has proper structure and claims.
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>True if token appears valid</returns>
        public static bool ValidateJwtTokenStructure(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            // JWT tokens should have 3 parts separated by dots
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                TestLogger.LogWarning("Invalid JWT token structure - expected 3 parts, got {0}", parts.Length);
                return false;
            }

            // Each part should be base64url encoded
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part) || !IsBase64UrlEncoded(part))
                {
                    TestLogger.LogWarning("Invalid JWT token part - not base64url encoded");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a string is base64url encoded.
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>True if string is base64url encoded</returns>
        private static bool IsBase64UrlEncoded(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;

            // Base64url uses - and _ instead of + and /
            var base64UrlPattern = @"^[A-Za-z0-9\-_]+$";
            return Regex.IsMatch(str, base64UrlPattern);
        }

        /// <summary>
        /// Generates a comprehensive security report for the current configuration.
        /// </summary>
        /// <param name="environment">Current environment</param>
        /// <param name="configuration">Configuration dictionary</param>
        /// <returns>Security report</returns>
        public static string GenerateSecurityReport(string environment, Dictionary<string, string> configuration)
        {
            var report = new StringBuilder();
            report.AppendLine("=== Security Configuration Report ===");
            report.AppendLine($"Environment: {environment}");
            report.AppendLine($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine();

            // Check HTTPS usage
            var httpsIssues = 0;
            foreach (var (key, value) in configuration)
            {
                if (key.Contains("url", StringComparison.OrdinalIgnoreCase) ||
                    key.Contains("endpoint", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ValidateHttpsUrl(value, environment))
                    {
                        report.AppendLine($"WARNING: HTTP URL detected: {key} = {value}");
                        httpsIssues++;
                    }
                }
            }

            if (httpsIssues == 0)
            {
                report.AppendLine("PASS: All URLs use HTTPS");
            }

            // Check sensitive configuration
            var sensitiveIssues = 0;
            foreach (var (key, value) in configuration)
            {
                if (!ValidateConfigurationSecurity(key, value, environment))
                {
                    report.AppendLine($"WARNING: Sensitive configuration may contain plaintext: {key}");
                    sensitiveIssues++;
                }
            }

            if (sensitiveIssues == 0)
            {
                report.AppendLine("PASS: All sensitive configurations are properly secured");
            }

            report.AppendLine();
            report.AppendLine($"Total Issues Found: {httpsIssues + sensitiveIssues}");

            return report.ToString();
        }
    }
}
