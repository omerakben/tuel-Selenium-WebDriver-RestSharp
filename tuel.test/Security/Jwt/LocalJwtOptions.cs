using System;
using System.Collections.Generic;

namespace TUEL.TestFramework.Security.Jwt
{
    /// <summary>
    /// Configuration settings controlling locally generated JWT tokens for development/test scenarios.
    /// </summary>
    public sealed class LocalJwtOptions
    {
        public string Algorithm { get; init; } = "RS256";
        public string? PrivateKey { get; init; }
        public string? KeyId { get; init; }
        public string? Issuer { get; init; }
        public string? Audience { get; init; }
        public string? Subject { get; init; }
        public string? Name { get; init; }
        public string? Role { get; init; }
        public string? ClientId { get; init; }
        public TimeSpan Lifetime { get; init; } = TimeSpan.FromMinutes(60);
        public IDictionary<string, object>? AdditionalClaims { get; init; }
    }
}
