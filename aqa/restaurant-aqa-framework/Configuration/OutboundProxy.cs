using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Configuration;

/// <summary>
/// Optional outbound proxy (PLAYWRIGHT_PROXY) for machines that reach the internet only through one, such as a
/// corporate network or a sandbox. Unset by default: CI runners and most laptops connect directly.
/// </summary>
public static class OutboundProxy
{
    private static string? Server
    {
        get
        {
            EnvConfig.EnsureLoaded();
            var value = Environment.GetEnvironmentVariable("PLAYWRIGHT_PROXY");
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    public static bool IsSet => Server is not null;

    public static Proxy? ForBrowser() => Server is { } server ? new Proxy { Server = server } : null;
}
