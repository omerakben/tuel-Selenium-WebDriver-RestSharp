using System.Threading;
using System.Threading.Tasks;

namespace TUEL.TestFramework.Security.Secrets.Providers
{
    /// <summary>
    /// Contract for secret providers (environment variables, Azure Key Vault, encryption, etc.).
    /// </summary>
    public interface ISecretProvider
    {
        /// <summary>
        /// Resolves the secret value for the specified reference.
        /// </summary>
        Task<string?> ResolveSecretAsync(SecretReference reference, SecretResolutionContext context, CancellationToken cancellationToken = default);
    }
}
