using System;
using System.Collections.Generic;

namespace TUEL.TestFramework.Web.Support
{
    /// <summary>
    /// Configuration for WebDriver lifecycle management (pooling, provider, etc.).
    /// </summary>
    public sealed class WebDriverLifecycleOptions
    {
        public bool EnablePooling { get; init; } = true;
        public int MaxPoolSize { get; init; } = 2;
        public TimeSpan MaxIdleTime { get; init; } = TimeSpan.FromMinutes(10);
        public string BrowserName { get; init; } = "edge";
        public bool Headless { get; init; }
        public WebDriverProviderType Provider { get; init; } = WebDriverProviderType.Local;
        public Uri? RemoteServerUri { get; init; }
        public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromMinutes(3);
        public IDictionary<string, object>? AdditionalCapabilities { get; init; }
    }
}
