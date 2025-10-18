using System;
using System.Collections.Generic;
using System.Linq;

namespace TUEL.TestFramework.Security.Secrets
{
    /// <summary>
    /// Represents a parsed secret reference, e.g. env://MY_VARIABLE or kv://MySecret?version=123.
    /// </summary>
    public sealed class SecretReference
    {
        private SecretReference(string original, string scheme, string identifier, IReadOnlyDictionary<string, string> parameters)
        {
            Original = original;
            Scheme = scheme;
            Identifier = identifier;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the original secret reference string.
        /// </summary>
        public string Original { get; }

        /// <summary>
        /// Gets the provider scheme (e.g. env, kv, enc).
        /// </summary>
        public string Scheme { get; }

        /// <summary>
        /// Gets the identifier portion of the reference (host + path without leading slash).
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets querystring-style parameters provided with the secret reference.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>
        /// Try to parse a secret reference. Returns <c>true</c> when the value uses the provider syntax (<c>scheme://value</c>).
        /// </summary>
        public static bool TryParse(string? value, out SecretReference? reference)
        {
            reference = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmed = value.Trim();
            var delimiterIndex = trimmed.IndexOf("://", StringComparison.Ordinal);
            if (delimiterIndex <= 0)
            {
                return false;
            }

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                return false;
            }

            var scheme = uri.Scheme;
            if (string.IsNullOrWhiteSpace(scheme))
            {
                return false;
            }

            var identifier = BuildIdentifier(uri);
            var parameters = ParseParameters(uri.Query);

            reference = new SecretReference(trimmed, scheme.ToLowerInvariant(), identifier, parameters);
            return true;
        }

        private static string BuildIdentifier(Uri uri)
        {
            var host = uri.Host ?? string.Empty;
            var path = uri.AbsolutePath?.Trim('/') ?? string.Empty;

            if (string.IsNullOrEmpty(host))
            {
                return path;
            }

            if (string.IsNullOrEmpty(path))
            {
                return host;
            }

            return string.Create(host.Length + 1 + path.Length, (host, path), (span, state) =>
            {
                state.host.AsSpan().CopyTo(span);
                span[state.host.Length] = '/';
                state.path.AsSpan().CopyTo(span[(state.host.Length + 1)..]);
            });
        }

        private static IReadOnlyDictionary<string, string> ParseParameters(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new Dictionary<string, string>();
            }

            return query.TrimStart('?')
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(parameter => parameter.Split('=', 2))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(parts => Uri.UnescapeDataString(parts[0]),
                                      parts => Uri.UnescapeDataString(parts[1]),
                                      StringComparer.OrdinalIgnoreCase);
        }
    }
}
