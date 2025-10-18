using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TUEL.TestFramework.Security.Secrets
{
    /// <summary>
    /// Strongly-typed configuration options controlling secret resolution.
    /// </summary>
    public sealed class SecretManagerOptions
    {
        /// <summary>
        /// Optional Azure Key Vault URI (e.g. https://myvault.vault.azure.net/).
        /// </summary>
        public Uri? KeyVaultUri { get; init; }

        /// <summary>
        /// Optional managed identity client id when authenticating against Key Vault.
        /// </summary>
        public string? ManagedIdentityClientId { get; init; }

        /// <summary>
        /// Optional static tenant id when building DefaultAzureCredential.
        /// </summary>
        public string? TenantId { get; init; }

        /// <summary>
        /// Indicates whether plaintext fallback is permitted when a value is not expressed as a secret reference.
        /// </summary>
        public bool AllowPlaintextFallback { get; init; }

        /// <summary>
        /// Optional base64 encoded encryption key used for encrypted configuration sections.
        /// </summary>
        public string? EncryptionKey { get; init; }

        /// <summary>
        /// Timeout for network secret retrieval in seconds.
        /// </summary>
        public int SecretFetchTimeoutSeconds { get; init; } = 30;

        /// <summary>
        /// Build options from MSTest run settings.
        /// </summary>
        public static SecretManagerOptions From(TestContext context)
        {
            if (context?.Properties == null)
            {
                throw new InvalidOperationException("TestContext with run settings is required before initializing SecretManager");
            }

            var keyVaultUriText = context.Properties["SecretManagement__KeyVaultUri"]?.ToString();
            Uri? keyVaultUri = null;
            if (!string.IsNullOrWhiteSpace(keyVaultUriText) && Uri.TryCreate(keyVaultUriText, UriKind.Absolute, out var parsedUri))
            {
                keyVaultUri = parsedUri;
            }

            var plaintextFallbackValue = context.Properties["SecretManagement__AllowPlaintextFallback"]?.ToString();
            var allowPlaintextFallback = string.IsNullOrWhiteSpace(plaintextFallbackValue)
                ? false
                : bool.TryParse(plaintextFallbackValue, out var parsedBool) && parsedBool;

            var encryptionKeySetting = context.Properties["SecretManagement__ConfigurationEncryptionKey"]?.ToString();
            var encryptionKey = ResolveEnvironmentValue(encryptionKeySetting);

            var managedIdentityClientId = context.Properties["SecretManagement__ManagedIdentityClientId"]?.ToString();
            var tenantId = context.Properties["SecretManagement__TenantId"]?.ToString();
            var timeoutSetting = context.Properties["SecretManagement__SecretFetchTimeoutSeconds"]?.ToString();
            var timeoutSeconds = 30;
            if (!string.IsNullOrWhiteSpace(timeoutSetting) && int.TryParse(timeoutSetting, out var parsedTimeout) && parsedTimeout > 0)
            {
                timeoutSeconds = parsedTimeout;
            }

            return new SecretManagerOptions
            {
                KeyVaultUri = keyVaultUri,
                ManagedIdentityClientId = string.IsNullOrWhiteSpace(managedIdentityClientId) ? null : managedIdentityClientId,
                TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId,
                AllowPlaintextFallback = allowPlaintextFallback,
                EncryptionKey = encryptionKey,
                SecretFetchTimeoutSeconds = timeoutSeconds
            };
        }

        private static string? ResolveEnvironmentValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            const string envPrefix = "env://";
            if (trimmed.StartsWith(envPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var variableName = trimmed[envPrefix.Length..];
                return Environment.GetEnvironmentVariable(variableName);
            }

            const string envShortPrefix = "env:";
            if (trimmed.StartsWith(envShortPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var variableName = trimmed[envShortPrefix.Length..];
                return Environment.GetEnvironmentVariable(variableName);
            }

            return trimmed;
        }
    }
}
