using System;

namespace TUEL.TestFramework.Security.Encryption
{
    /// <summary>
    /// Helper methods for creating encrypted configuration payloads compatible with enc:// references.
    /// </summary>
    public static class ConfigurationEncryptionUtility
    {
        /// <summary>
        /// Encrypts the provided plaintext using the provided base64-encoded AES-256 key and returns an enc:// reference string.
        /// </summary>
        public static string EncryptToReference(string plainText, string base64Key)
        {
            var service = new AesEncryptionService(base64Key);
            var (cipher, iv) = service.Encrypt(plainText);
            return BuildReference(cipher, iv);
        }

        /// <summary>
        /// Build the enc:// reference string from cipher and IV components.
        /// </summary>
        public static string BuildReference(string cipherBase64, string ivBase64)
        {
            if (string.IsNullOrWhiteSpace(cipherBase64))
            {
                throw new ArgumentException("Cipher text must be provided", nameof(cipherBase64));
            }

            if (string.IsNullOrWhiteSpace(ivBase64))
            {
                throw new ArgumentException("Initialization vector must be provided", nameof(ivBase64));
            }

            var escapedCipher = Uri.EscapeDataString(cipherBase64);
            var escapedIv = Uri.EscapeDataString(ivBase64);
            return $"enc://aes256/{escapedCipher}?iv={escapedIv}";
        }
    }
}
