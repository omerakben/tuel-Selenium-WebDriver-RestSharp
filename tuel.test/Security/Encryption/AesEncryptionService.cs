using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TUEL.TestFramework.Security.Encryption
{
    /// <summary>
    /// Minimal AES-256 encryption helper for configuration secrets.
    /// </summary>
    internal sealed class AesEncryptionService
    {
        private readonly byte[] _key;

        public AesEncryptionService(string base64Key)
        {
            if (string.IsNullOrWhiteSpace(base64Key))
            {
                throw new ArgumentException("Encryption key must be provided", nameof(base64Key));
            }

            try
            {
                _key = Convert.FromBase64String(base64Key);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Configuration encryption key must be a base64 encoded string", ex);
            }

            if (_key.Length != 32)
            {
                throw new InvalidOperationException($"Configuration encryption key must be 32 bytes (256 bits). Provided key length: {_key.Length} bytes");
            }
        }

        public (string CipherTextBase64, string IvBase64) Encrypt(string plainText)
        {
            if (plainText is null)
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return (Convert.ToBase64String(cipherBytes), Convert.ToBase64String(aes.IV));
        }

        public string Decrypt(ReadOnlySpan<byte> cipherText, ReadOnlySpan<byte> iv)
        {
            if (iv.Length != 16)
            {
                throw new InvalidOperationException("AES IV must be 16 bytes (128 bits).");
            }

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _key;
            aes.IV = iv.ToArray();

            using var decryptor = aes.CreateDecryptor();
            using var cipherStream = new MemoryStream(cipherText.ToArray());
            using var cryptoStream = new CryptoStream(cipherStream, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
