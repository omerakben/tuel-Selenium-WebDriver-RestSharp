using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUEL.TestFramework.Logging;
using TUEL.TestFramework.Security.Encryption;
using TUEL.TestFramework.Security.Secrets.Providers;

namespace TUEL.TestFramework.Security.Secrets
{
    /// <summary>
    /// Central entry point for resolving secrets referenced in configuration.
    /// </summary>
    public static class SecretManager
    {
        private static readonly object SyncRoot = new();
        private static readonly ConcurrentDictionary<string, string?> Cache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, ISecretProvider> Providers = new(StringComparer.OrdinalIgnoreCase);

        private static bool _initialized;
        private static SecretManagerOptions _options = default!;
        private static IDisposable? _disposableProvider;

        public static void Initialize(TestContext context)
        {
            if (_initialized)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_initialized)
                {
                    return;
                }

                _options = SecretManagerOptions.From(context);

                Providers["env"] = new EnvironmentSecretProvider();

                if (_options.KeyVaultUri is not null)
                {
                    var keyVaultProvider = new AzureKeyVaultSecretProvider(_options);
                    Providers["kv"] = keyVaultProvider;
                    Providers["keyvault"] = keyVaultProvider;
                    _disposableProvider = keyVaultProvider;
                    TestLogger.LogInformation("SecretManager: Azure Key Vault provider configured for vault {0}", _options.KeyVaultUri);
                }

                if (!string.IsNullOrWhiteSpace(_options.EncryptionKey))
                {
                    var encryptionService = new AesEncryptionService(_options.EncryptionKey);
                    Providers["enc"] = new EncryptedSecretProvider(encryptionService);
                    TestLogger.LogInformation("SecretManager: Encrypted configuration provider enabled (AES-256).");
                }
                else
                {
                    TestLogger.LogWarning("SecretManager: Configuration encryption key not provided. enc:// secrets will be unavailable.");
                }

                if (_options.AllowPlaintextFallback)
                {
                    TestLogger.LogWarning("SecretManager: Plaintext fallback is enabled. Consider disabling it for production environments.");
                }

                _initialized = true;
            }
        }

        /// <summary>
        /// Resolve a secret for the provided value. Plain values are returned as-is unless plaintext fallback is disabled.
        /// </summary>
        public static string? ResolveSecret(string? value, string? logicalName = null, bool warnOnPlaintext = true, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (!SecretReference.TryParse(value, out var reference) || reference is null)
            {
                if (warnOnPlaintext && !_options.AllowPlaintextFallback && !string.IsNullOrWhiteSpace(logicalName))
                {
                    TestLogger.LogWarning("SecretManager: Plaintext value detected for '{0}'. Replace with env:// or kv:// reference.", logicalName);
                }

                return value;
            }

            if (Cache.TryGetValue(reference.Original, out var cachedValue))
            {
                return cachedValue;
            }

            if (!Providers.TryGetValue(reference.Scheme, out var provider))
            {
                throw new InvalidOperationException($"No secret provider registered for scheme '{reference.Scheme}'.");
            }

            var context = new SecretResolutionContext(_options, logicalName, cancellationToken);
            try
            {
                var resolvedSecret = provider.ResolveSecretAsync(reference, context, cancellationToken)
                                              .ConfigureAwait(false)
                                              .GetAwaiter()
                                              .GetResult();

                if (string.IsNullOrWhiteSpace(resolvedSecret) && !_options.AllowPlaintextFallback)
                {
                    throw new InvalidOperationException($"Secret '{logicalName ?? reference.Original}' resolved to an empty value.");
                }

                Cache[reference.Original] = resolvedSecret;
                return resolvedSecret;
            }
            catch (Exception ex)
            {
                if (!_options.AllowPlaintextFallback)
                {
                    throw;
                }

                TestLogger.LogWarning("SecretManager: Failed to resolve secret '{0}' via scheme '{1}'. Falling back to plaintext. Error: {2}",
                    logicalName ?? reference.Original,
                    reference.Scheme,
                    ex.Message);
                return value;
            }
        }

        /// <summary>
        /// Attempt to resolve a secret asynchronously (for APIs that already operate asynchronously).
        /// </summary>
        public static Task<string?> ResolveSecretAsync(string? value, string? logicalName = null, bool warnOnPlaintext = true, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResolveSecret(value, logicalName, warnOnPlaintext, cancellationToken));
        }

        /// <summary>
        /// Dispose any disposable providers. Intended to be called from AssemblyCleanup.
        /// </summary>
        public static void Shutdown()
        {
            lock (SyncRoot)
            {
                _disposableProvider?.Dispose();
                _disposableProvider = null;
                _initialized = false;
                Cache.Clear();
                Providers.Clear();
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            throw new InvalidOperationException("SecretManager must be initialized before resolving secrets. Call SecretManager.Initialize(TestContext) during assembly initialization.");
        }
    }
}
