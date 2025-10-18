using System;
using System.Collections.Generic;

namespace TUEL.TestFramework.Web.Support
{
    public enum WebDriverProviderType
    {
        Local,
        SeleniumGrid
    }

    /// <summary>
    /// Options required to create a WebDriver instance.
    /// </summary>
    public sealed class WebDriverRequestOptions
    {
        public string BrowserName { get; init; } = "edge";
        public WebDriverProviderType Provider { get; init; } = WebDriverProviderType.Local;
        public Uri? RemoteServerUri { get; init; }
        public bool Headless { get; init; }
        public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromMinutes(3);
        public IDictionary<string, object>? AdditionalCapabilities { get; init; }
    }
}
