using System;
using System.Threading;
using System.Threading.Tasks;

namespace TUEL.TestFramework.Security.Secrets.Providers
{
    /// <summary>
    /// Resolves secrets from environment variables.
    /// </summary>
    internal sealed class EnvironmentSecretProvider : ISecretProvider
    {
        public Task<string?> ResolveSecretAsync(SecretReference reference, SecretResolutionContext context, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(reference.Scheme, "env", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"EnvironmentSecretProvider can only handle env:// references (received '{reference.Original}')");
            }

            var variableName = reference.Identifier;
            if (string.IsNullOrWhiteSpace(variableName))
            {
                throw new InvalidOperationException("Environment variable name is missing from env:// secret reference");
            }

            var value = Environment.GetEnvironmentVariable(variableName);
            return Task.FromResult<string?>(value);
        }
    }
}
