using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TUEL.TestFramework.Security.Jwt
{
    /// <summary>
    /// Creates signed JWT tokens using RS256 or ES256 algorithms from PEM encoded keys.
    /// </summary>
    public static class LocalJwtTokenFactory
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string CreateToken(LocalJwtOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.PrivateKey))
            {
                throw new InvalidOperationException("Local JWT configuration requires a private key (PEM format) to sign the token.");
            }

            var algorithm = options.Algorithm?.ToUpperInvariant() ?? "RS256";
            var now = DateTimeOffset.UtcNow;
            var expiration = now.Add(options.Lifetime <= TimeSpan.Zero ? TimeSpan.FromMinutes(60) : options.Lifetime);

            var header = new Dictionary<string, object>
            {
                ["alg"] = algorithm,
                ["typ"] = "JWT"
            };

            if (!string.IsNullOrWhiteSpace(options.KeyId))
            {
                header["kid"] = options.KeyId;
            }

            var payload = BuildPayload(options, now, expiration);

            var headerEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header, SerializerOptions)));
            var payloadEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, SerializerOptions)));
            var signingInput = string.Concat(headerEncoded, '.', payloadEncoded);

            var signature = algorithm switch
            {
                "RS256" => SignWithRsa(options.PrivateKey!, signingInput),
                "ES256" => SignWithEcdsa(options.PrivateKey!, signingInput, 32),
                _ => throw new NotSupportedException($"Unsupported JWT algorithm '{algorithm}'. Only RS256 and ES256 are supported for local JWTs.")
            };

            return string.Concat(signingInput, '.', signature);
        }

        private static IDictionary<string, object> BuildPayload(LocalJwtOptions options, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
        {
            var payload = new Dictionary<string, object>
            {
                ["iat"] = issuedAt.ToUnixTimeSeconds(),
                ["nbf"] = issuedAt.AddMinutes(-1).ToUnixTimeSeconds(),
                ["exp"] = expiresAt.ToUnixTimeSeconds(),
                ["iss"] = options.Issuer ?? "urn:tuel:local",
                ["aud"] = options.Audience ?? options.ClientId ?? "tuel-local",
                ["sub"] = options.Subject ?? options.ClientId ?? "local-user",
                ["name"] = options.Name ?? "Local Automation Account"
            };

            if (!string.IsNullOrWhiteSpace(options.Role))
            {
                payload["role"] = options.Role;
            }

            if (!string.IsNullOrWhiteSpace(options.ClientId))
            {
                payload["azp"] = options.ClientId;
            }

            if (options.AdditionalClaims != null)
            {
                foreach (var claim in options.AdditionalClaims)
                {
                    payload[claim.Key] = claim.Value;
                }
            }

            return payload;
        }

        private static string SignWithRsa(string pem, string input)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            var data = Encoding.UTF8.GetBytes(input);
            var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Base64UrlEncode(signature);
        }

        private static string SignWithEcdsa(string pem, string input, int coordinateSize)
        {
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(pem);
            var data = Encoding.UTF8.GetBytes(input);
            var derSignature = ecdsa.SignData(data, HashAlgorithmName.SHA256);
            var joseSignature = ConvertDerToJose(derSignature, coordinateSize);
            return Base64UrlEncode(joseSignature);
        }

        private static byte[] ConvertDerToJose(byte[] derSignature, int coordinateSize)
        {
            var reader = new AsnReader(derSignature, AsnEncodingRules.DER);
            var sequence = reader.ReadSequence();
            var r = sequence.ReadIntegerBytes().ToArray();
            var s = sequence.ReadIntegerBytes().ToArray();
            sequence.ThrowIfNotEmpty();

            var rFixed = NormalizeCoordinate(r, coordinateSize);
            var sFixed = NormalizeCoordinate(s, coordinateSize);

            var result = new byte[coordinateSize * 2];
            Buffer.BlockCopy(rFixed, 0, result, 0, coordinateSize);
            Buffer.BlockCopy(sFixed, 0, result, coordinateSize, coordinateSize);
            return result;
        }

        private static byte[] NormalizeCoordinate(byte[] value, int size)
        {
            if (value.Length == size)
            {
                return value;
            }

            if (value.Length > size)
            {
                // Remove leading zero when ASN.1 integer uses sign bit padding.
                var offset = value.Length - size;
                var slice = new byte[size];
                Buffer.BlockCopy(value, offset, slice, 0, size);
                return slice;
            }

            var padded = new byte[size];
            Buffer.BlockCopy(value, 0, padded, size - value.Length, value.Length);
            return padded;
        }

        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data)
                           .TrimEnd('=')
                           .Replace('+', '-')
                           .Replace('/', '_');
        }
    }
}
