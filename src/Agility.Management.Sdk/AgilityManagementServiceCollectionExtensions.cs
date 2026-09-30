using Agility.Management.Sdk;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="AgilityManagementClient"/> with dependency injection.</summary>
public static class AgilityManagementServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AgilityManagementClient"/> as a typed HTTP client, so its <see cref="HttpClient"/>
    /// comes from <c>IHttpClientFactory</c> and its handlers are pooled and rotated. If an
    /// <see cref="IAccessTokenProvider"/> is registered and <see cref="AgilityManagementOptions.AccessTokenProvider"/>
    /// isn't set, the registered provider is used.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the options, e.g. <c>o => o.AccessToken = config["Agility:Token"]</c>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>, to add handlers or configure the primary handler.</returns>
    public static IHttpClientBuilder AddAgilityManagement(this IServiceCollection services, Action<AgilityManagementOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<AgilityManagementOptions>().Configure<IServiceProvider>((options, sp) =>
        {
            configure(options);
            options.AccessTokenProvider ??= sp.GetService<IAccessTokenProvider>();
        });
        return services.AddHttpClient(nameof(AgilityManagementClient))
            .AddTypedClient((http, sp) => new AgilityManagementClient(sp.GetRequiredService<IOptions<AgilityManagementOptions>>().Value, http));
    }
}
