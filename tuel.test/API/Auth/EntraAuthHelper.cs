using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using TUEL.TestFramework.Security.Jwt;

namespace TUEL.TestFramework.API.Auth
{
    public static class EntraAuthHelper
    {
        private static IPublicClientApplication? _publicApp;
        private static IConfidentialClientApplication? _confidentialApp;
        private static string? _cachedToken;
        private static DateTimeOffset? _tokenExpiry;
        private static readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(InitializeTestAssembly.ApiTimeoutSeconds) };
        private static readonly object _tokenLock = new object();

        private static string GetTenantSpecificAuthority()
        {
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdTenantId)) throw new InvalidOperationException("EntraIdTenantId is not configured for GetTenantSpecificAuthority.");
            return $"https://login.microsoftonline.com/{InitializeTestAssembly.EntraIdTenantId}";
        }

        private static string GetTenantSpecificTokenEndpoint()
        {
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdTenantId)) throw new InvalidOperationException("EntraIdTenantId is not configured for GetTenantSpecificTokenEndpoint.");
            return $"{GetTenantSpecificAuthority()}/oauth2/v2.0/token";
        }

        // Retrieves an access token using the configured authentication flow (ROPC or Client Credentials).
        public static async Task<string> GetAccessTokenAsync()
        {
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_cachedToken) && _tokenExpiry.HasValue && _tokenExpiry.Value > DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    Console.WriteLine("Using cached token");
                    return _cachedToken!;
                }
            }

            return await AcquireNewTokenAsync();
        }

        // Determines the correct authentication flow and acquires a new token.
        private static async Task<string> AcquireNewTokenAsync()
        {
            if (InitializeTestAssembly.EntraIdUseLocalJwt)
            {
                Console.WriteLine($"Using local JWT for role: {InitializeTestAssembly.EntraIdLocalJwtRole ?? "DefaultRole"}");
                _cachedToken = GetLocalJwtToken(InitializeTestAssembly.EntraIdLocalJwtRole);
                _tokenExpiry = DateTimeOffset.UtcNow.AddHours(1);
                return _cachedToken;
            }
            if (!string.IsNullOrEmpty(InitializeTestAssembly.Email) && !string.IsNullOrEmpty(InitializeTestAssembly.Password))
            {
                Console.WriteLine("Using Resource Owner Password Credentials (ROPC) flow via direct HTTP");
                return await GetTokenUsingROPCAsync();
            }
            else
            {
                Console.WriteLine("Using Client Credentials flow");
                return await GetTokenUsingClientCredentialsAsync();
            }
        }

        private static async Task<string> GetTokenUsingROPCAsync()
        {
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdTenantId))
                throw new InvalidOperationException("EntraIdTenantId is not configured. Cannot form ROPC token endpoint.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdClientId))
                throw new InvalidOperationException("EntraIdClientId is not configured for ROPC.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.Email))
                throw new InvalidOperationException("Email is not configured for ROPC.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.Password))
                throw new InvalidOperationException("Password is not configured for ROPC.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdApiScope))
                throw new InvalidOperationException("EntraIdApiScope is not configured for ROPC.");
            var tokenUrl = GetTenantSpecificTokenEndpoint();
            Console.WriteLine($"ROPC Token Endpoint: {tokenUrl}");

            using var client = new HttpClient();
            var requestBody = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", InitializeTestAssembly.EntraIdClientId),
                new KeyValuePair<string, string>("username", InitializeTestAssembly.Email),
                new KeyValuePair<string, string>("password", InitializeTestAssembly.Password),
                new KeyValuePair<string, string>("scope", InitializeTestAssembly.EntraIdApiScope)
            };

            if (!string.IsNullOrEmpty(InitializeTestAssembly.EntraIdClientSecret))
            {
                requestBody.Add(new KeyValuePair<string, string>("client_secret", InitializeTestAssembly.EntraIdClientSecret));
            }

            var body = new FormUrlEncodedContent(requestBody);
            var response = await client.PostAsync(tokenUrl, body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ROPC Request Body: {await body.ReadAsStringAsync()}");
                throw new Exception($"ROPC token acquisition failed: {response.StatusCode} - {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            var accessToken = json["access_token"]?.ToString();

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Access token not found in ROPC response.");
            }

            _cachedToken = accessToken;

            if (json["expires_in"] != null && int.TryParse(json["expires_in"].ToString(), out int expiresIn))
            {
                _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
            }

            Console.WriteLine($"Token acquired using ROPC, expires in: {json["expires_in"]?.ToString() ?? "N/A"} seconds");
            return _cachedToken;
        }

        private static async Task<string> GetTokenUsingClientCredentialsAsync()
        {
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdTenantId))
                throw new InvalidOperationException("EntraIdTenantId is not configured. Cannot form MSAL authority or HTTP token endpoint.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdClientId))
                throw new InvalidOperationException("EntraIdClientId is not configured for Client Credentials.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdClientSecret)) // Client secret is essential for confidential client flow
                throw new InvalidOperationException("EntraIdClientSecret is not configured for Client Credentials.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdApiScope))
                throw new InvalidOperationException("EntraIdApiScope is not configured for Client Credentials.");

            var msalAuthority = GetTenantSpecificAuthority();
            try
            {
                if (_confidentialApp == null)
                {
                    _confidentialApp = ConfidentialClientApplicationBuilder
                        .Create(InitializeTestAssembly.EntraIdClientId)
                        .WithClientSecret(InitializeTestAssembly.EntraIdClientSecret)
                        .WithAuthority(new Uri(msalAuthority))
                        .Build();
                }

                var scopes = new[] { InitializeTestAssembly.EntraIdApiScope };
                var result = await _confidentialApp.AcquireTokenForClient(scopes).ExecuteAsync();

                if (string.IsNullOrEmpty(result.AccessToken))
                {
                    throw new Exception("MSAL Client Credentials returned a null or empty access token.");
                }

                _cachedToken = result.AccessToken;
                _tokenExpiry = result.ExpiresOn;

                Console.WriteLine($"Token acquired using MSAL Client Credentials, expires at: {result.ExpiresOn}");
                return _cachedToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MSAL Client Credentials failed: {ex.Message}. Falling back to direct HTTP.");
            }
            return await GetTokenUsingHttpClientCredentialsAsync();
        }

        private static async Task<string> GetTokenUsingHttpClientCredentialsAsync()
        {
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdTenantId))
                throw new InvalidOperationException("EntraIdTenantId is not configured. Cannot form HTTP client credentials token endpoint.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdClientId))
                throw new InvalidOperationException("EntraIdClientId is not configured for HTTP Client Credentials.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdClientSecret))
                throw new InvalidOperationException("EntraIdClientSecret is not configured for HTTP Client Credentials.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdApiScope))
                throw new InvalidOperationException("EntraIdApiScope is not configured for HTTP Client Credentials.");

            var tokenEndpoint = GetTenantSpecificTokenEndpoint();
            Console.WriteLine($"HTTP Client Credentials Token Endpoint: {tokenEndpoint}");

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", InitializeTestAssembly.EntraIdClientId),
                new KeyValuePair<string, string>("client_secret", InitializeTestAssembly.EntraIdClientSecret),
                new KeyValuePair<string, string>("scope", InitializeTestAssembly.EntraIdApiScope)
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, formData);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"HTTP Client Credentials Request Body: {await formData.ReadAsStringAsync()}");
                throw new Exception($"Client Credentials token acquisition via HTTP failed: {response.StatusCode} - {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            var accessToken = json["access_token"]?.ToString();

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Access token not found in HTTP Client Credentials response.");
            }
            _cachedToken = accessToken;

            if (json["expires_in"] != null && int.TryParse(json["expires_in"].ToString(), out int expiresIn))
            {
                _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
            }

            Console.WriteLine($"Token acquired using HTTP Client Credentials, expires in: {json["expires_in"]?.ToString() ?? "N/A"} seconds");
            return _cachedToken;
        }

        public static async Task<string> GetTokenUsingMsalROPCAsync()
        {
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdTenantId))
                throw new InvalidOperationException("EntraIdTenantId is not configured. Cannot form MSAL ROPC authority.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdClientId))
                throw new InvalidOperationException("EntraIdClientId is not configured for MSAL ROPC.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.Email))
                throw new InvalidOperationException("Email is not configured for MSAL ROPC.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.Password))
                throw new InvalidOperationException("Password is not configured for MSAL ROPC.");
            if (string.IsNullOrEmpty(InitializeTestAssembly.EntraIdApiScope))
                throw new InvalidOperationException("EntraIdApiScope is not configured for MSAL ROPC.");

            var msalAuthority = GetTenantSpecificAuthority();

            if (_publicApp == null)
            {
                _publicApp = PublicClientApplicationBuilder
                    .Create(InitializeTestAssembly.EntraIdClientId)
                    .WithAuthority(new Uri(msalAuthority))
                    .Build();
            }

            var scopes = new[] { InitializeTestAssembly.EntraIdApiScope };
            try
            {
                var result = await _publicApp.AcquireTokenByUsernamePassword(
                    scopes,
                    InitializeTestAssembly.Email,
                    InitializeTestAssembly.Password!)
                    .ExecuteAsync();

                if (string.IsNullOrEmpty(result.AccessToken))
                {
                    throw new Exception("MSAL ROPC returned a null or empty access token.");
                }
                _cachedToken = result.AccessToken;
                _tokenExpiry = result.ExpiresOn;
                return _cachedToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"MSAL ROPC authentication failed: {ex.Message}", ex);
            }
        }

        private static string GetLocalJwtToken(string? role)
        {
            var privateKey = InitializeTestAssembly.EntraIdLocalJwtPrivateKey;
            if (string.IsNullOrWhiteSpace(privateKey))
            {
                throw new InvalidOperationException("Local JWT requested but EntraIdLocalJwtPrivateKey is not configured. Provide a PEM encoded key via SecretManager (e.g. env://).");
            }

            var additionalClaims = new Dictionary<string, object>();
            if (InitializeTestAssembly.EntraIdLocalJwtIncludeScopeClaim && !string.IsNullOrWhiteSpace(InitializeTestAssembly.EntraIdApiScope))
            {
                additionalClaims["scp"] = InitializeTestAssembly.EntraIdApiScope;
            }

            var lifetimeMinutes = Math.Clamp(InitializeTestAssembly.EntraIdLocalJwtLifetimeMinutes, 5, 240);

            return LocalJwtTokenFactory.CreateToken(new LocalJwtOptions
            {
                Algorithm = InitializeTestAssembly.EntraIdLocalJwtSigningAlgorithm,
                PrivateKey = privateKey,
                KeyId = InitializeTestAssembly.EntraIdLocalJwtKeyId,
                Issuer = InitializeTestAssembly.EntraIdLocalJwtIssuer ?? InitializeTestAssembly.EntraIdAuthority,
                Audience = InitializeTestAssembly.EntraIdLocalJwtAudience ?? InitializeTestAssembly.EntraIdApiScope,
                Subject = InitializeTestAssembly.Email ?? InitializeTestAssembly.EntraIdClientId,
                Name = InitializeTestAssembly.Email ?? "Local Automation Account",
                Role = role ?? InitializeTestAssembly.EntraIdLocalJwtRole ?? "DefaultRole",
                ClientId = InitializeTestAssembly.EntraIdClientId,
                Lifetime = TimeSpan.FromMinutes(lifetimeMinutes),
                AdditionalClaims = additionalClaims.Count > 0 ? additionalClaims : null
            });
        }

        public static void ClearCache()
        {
            lock (_tokenLock)
            {
                _cachedToken = null;
                _tokenExpiry = null;
            }
        }

        public static async Task<HttpClient> CreateAuthenticatedHttpClientAsync()
        {
            var token = await GetAccessTokenAsync();
            var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(InitializeTestAssembly.ApiTimeoutSeconds) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public static async Task AddAuthenticationHeaderAsync(HttpRequestMessage request)
        {
            var token = await GetAccessTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static string GetAuthenticationInfoForTestContext()
        {
            var info = new StringBuilder();
            info.AppendLine("=== Authentication Configuration (from InitializeTestAssembly) ===");
            info.AppendLine($"Environment: {InitializeTestAssembly.ENV}");
            info.AppendLine($"Tenant ID: {InitializeTestAssembly.EntraIdTenantId}");
            info.AppendLine($"Client ID: {InitializeTestAssembly.EntraIdClientId}");
            info.AppendLine($"API Scope: {InitializeTestAssembly.EntraIdApiScope}");
            info.AppendLine($"Has Client Secret: {!string.IsNullOrEmpty(InitializeTestAssembly.EntraIdClientSecret)}");

            string authority = string.IsNullOrEmpty(InitializeTestAssembly.EntraIdTenantId) ? "N/A (TenantId not configured)" : GetTenantSpecificAuthority();
            string tokenEndpoint = string.IsNullOrEmpty(InitializeTestAssembly.EntraIdTenantId) ? "N/A (TenantId not configured)" : GetTenantSpecificTokenEndpoint();

            info.AppendLine($"Calculated Authority: {authority}");
            info.AppendLine($"Calculated Token Endpoint: {tokenEndpoint}");
            info.AppendLine($"Use Local JWT: {InitializeTestAssembly.EntraIdUseLocalJwt}");
            info.AppendLine($"Local JWT Role: {InitializeTestAssembly.EntraIdLocalJwtRole ?? "N/A"}");
            info.AppendLine($"Local JWT Algorithm: {InitializeTestAssembly.EntraIdLocalJwtSigningAlgorithm}");
            info.AppendLine($"Local JWT Lifetime Minutes: {InitializeTestAssembly.EntraIdLocalJwtLifetimeMinutes}");
            info.AppendLine($"Local JWT KeyId Configured: {!string.IsNullOrWhiteSpace(InitializeTestAssembly.EntraIdLocalJwtKeyId)}");
            if (!string.IsNullOrEmpty(InitializeTestAssembly.Email))
            {
                info.AppendLine($"Username: {InitializeTestAssembly.Email}");
            }
            info.AppendLine("=========================================================");
            return info.ToString();
        }
    }
}
