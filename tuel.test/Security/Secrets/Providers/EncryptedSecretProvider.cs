using System;
using System.Threading;
using System.Threading.Tasks;
using TUEL.TestFramework.Security.Encryption;

namespace TUEL.TestFramework.Security.Secrets.Providers
{
    /// <summary>
    /// Resolves secrets encrypted with AES-256 (format: enc://aes256/{cipher}?iv={iv}).
    /// </summary>
    internal sealed class EncryptedSecretProvider : ISecretProvider
    {
        private readonly AesEncryptionService _encryptionService;

        public EncryptedSecretProvider(AesEncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        public Task<string?> ResolveSecretAsync(SecretReference reference, SecretResolutionContext context, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(reference.Scheme, "enc", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("EncryptedSecretProvider only handles enc:// references.");
            }

            var segments = reference.Identifier.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length == 0)
            {
                throw new InvalidOperationException("Encrypted secret reference must specify algorithm and cipher text.");
            }

            var algorithm = segments[0];
            if (!string.Equals(algorithm, "aes256", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException($"Unsupported encryption algorithm '{algorithm}'. Only aes256 is supported.");
            }

            string? cipherBase64;
            string? ivBase64 = null;

            if (segments.Length == 2)
            {
                cipherBase64 = segments[1];
            }
            else if (segments.Length == 3)
            {
                ivBase64 = segments[1];
                cipherBase64 = segments[2];
            }
            else
            {
                throw new InvalidOperationException("Encrypted secret reference must follow enc://aes256/{cipherBase64}?iv={ivBase64} or enc://aes256/{ivBase64}/{cipherBase64}.");
            }

            if (reference.Parameters.TryGetValue("iv", out var queryIv) && !string.IsNullOrWhiteSpace(queryIv))
            {
                ivBase64 = queryIv;
            }

            if (string.IsNullOrWhiteSpace(ivBase64))
            {
                throw new InvalidOperationException("Encrypted secret reference must provide an initialization vector (IV).");
            }

            try
            {
                var cipherBytes = Convert.FromBase64String(cipherBase64);
                var ivBytes = Convert.FromBase64String(ivBase64);
                var decrypted = _encryptionService.Decrypt(cipherBytes, ivBytes);
                return Task.FromResult<string?>(decrypted);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Encrypted secret contains an invalid base64 value.", ex);
            }
        }
    }
}
