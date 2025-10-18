using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace TUEL.TestFramework.Security.Secrets.Providers
{
    /// <summary>
    /// Retrieves secrets from Azure Key Vault using DefaultAzureCredential.
    /// </summary>
    internal sealed class AzureKeyVaultSecretProvider : ISecretProvider, IDisposable
    {
        private static readonly Uri Scope = new("https://vault.azure.net/.default");

        private readonly Uri _defaultVaultUri;
        private readonly TokenCredential _credential;
        private readonly HttpClient _httpClient;

        public AzureKeyVaultSecretProvider(SecretManagerOptions options)
        {
            if (options.KeyVaultUri is null)
            {
                throw new ArgumentNullException(nameof(options.KeyVaultUri), "KeyVaultUri must be configured before using AzureKeyVaultSecretProvider.");
            }

            _defaultVaultUri = options.KeyVaultUri;

            var credentialOptions = new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = true,
                ExcludeSharedTokenCacheCredential = false
            };

            if (!string.IsNullOrWhiteSpace(options.ManagedIdentityClientId))
            {
                credentialOptions.ManagedIdentityClientId = options.ManagedIdentityClientId;
            }

            if (!string.IsNullOrWhiteSpace(options.TenantId))
            {
                credentialOptions.SharedTokenCacheTenantId = options.TenantId;
                credentialOptions.VisualStudioTenantId = options.TenantId;
            }

            _credential = new DefaultAzureCredential(credentialOptions);
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(options.SecretFetchTimeoutSeconds)
            };
        }

        public async Task<string?> ResolveSecretAsync(SecretReference reference, SecretResolutionContext context, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(reference.Scheme, "kv", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(reference.Scheme, "keyvault", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("AzureKeyVaultSecretProvider only handles kv:// or keyvault:// references.");
            }

            var secretName = reference.Identifier;
            if (string.IsNullOrWhiteSpace(secretName))
            {
                throw new InvalidOperationException("Secret name is missing from Key Vault reference.");
            }

            var vaultUri = ResolveVaultUri(reference, context.Options);
            var version = reference.Parameters.TryGetValue("version", out var v) ? v : null;
            var requestUri = BuildSecretUri(vaultUri, secretName, version);

            var accessToken = await _credential.GetTokenAsync(new TokenRequestContext(new[] { Scope.ToString() }), cancellationToken).ConfigureAwait(false);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"Failed to retrieve secret '{secretName}' from Key Vault ({response.StatusCode}). Response: {body}");
            }

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!document.RootElement.TryGetProperty("value", out var valueElement))
            {
                throw new InvalidOperationException($"Key Vault response for secret '{secretName}' does not contain a 'value' property.");
            }

            return valueElement.GetString();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private static Uri BuildSecretUri(Uri vaultUri, string secretName, string? version)
        {
            var builder = new UriBuilder(vaultUri)
            {
                Path = string.IsNullOrEmpty(version)
                    ? $"secrets/{secretName}"
                    : $"secrets/{secretName}/{version}",
                Query = "api-version=7.3"
            };

            return builder.Uri;
        }

        private static Uri ResolveVaultUri(SecretReference reference, SecretManagerOptions options)
        {
            if (reference.Parameters.TryGetValue("vault", out var vaultOverride) && Uri.TryCreate(vaultOverride, UriKind.Absolute, out var customVaultUri))
            {
                return customVaultUri;
            }

            if (options.KeyVaultUri is null)
            {
                throw new InvalidOperationException("Key Vault URI must be configured globally or provided via secret reference query parameter 'vault'.");
            }

            return options.KeyVaultUri;
        }
    }
}
