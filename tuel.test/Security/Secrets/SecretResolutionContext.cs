using System.Threading;

namespace TUEL.TestFramework.Security.Secrets
{
    /// <summary>
    /// Context passed to secret providers during resolution.
    /// </summary>
    public sealed class SecretResolutionContext
    {
        public SecretResolutionContext(SecretManagerOptions options, string? logicalName, CancellationToken cancellationToken)
        {
            Options = options;
            LogicalName = logicalName;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Current secret manager configuration.
        /// </summary>
        public SecretManagerOptions Options { get; }

        /// <summary>
        /// Optional name describing the secret (used for diagnostics only).
        /// </summary>
        public string? LogicalName { get; }

        /// <summary>
        /// Cancellation token for long running operations.
        /// </summary>
        public CancellationToken CancellationToken { get; }
    }
}
