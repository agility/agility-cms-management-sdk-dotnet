using Agility.Management.Sdk;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="AgilityManagementClient"/> with dependency injection.</summary>
public static class AgilityManagementServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AgilityManagementClient"/> as a typed HTTP client, so its <see cref="HttpClient"/>
    /// comes from <c>IHttpClientFactory</c> and its handlers are pooled and rotated. The client is transient;
    /// its credentials are shared. If an <see cref="IAccessTokenProvider"/> is registered and
    /// <see cref="AgilityManagementOptions.AccessTokenProvider"/> isn't set, the registered provider is used.
    /// With <see cref="AgilityManagementOptions.RefreshToken"/>, one provider is shared by every resolved client,
    /// so the access token is cached once and a rotated refresh token reaches all of them.
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
            // Options are built once, so this provider is shared by every client the container creates.
            if (options.AccessTokenProvider is null && string.IsNullOrWhiteSpace(options.AccessToken) && !string.IsNullOrWhiteSpace(options.RefreshToken))
                options.AccessTokenProvider = CreateSharedRefreshProvider(options, sp.GetRequiredService<IHttpClientFactory>());
        });
        return services.AddHttpClient(nameof(AgilityManagementClient))
            .AddTypedClient((http, sp) => new AgilityManagementClient(sp.GetRequiredService<IOptions<AgilityManagementOptions>>().Value, http));
    }

    private static RefreshTokenAccessTokenProvider CreateSharedRefreshProvider(AgilityManagementOptions options, IHttpClientFactory httpClients)
    {
        // The refresh call goes through a credential-less client: the OAuth endpoints are anonymous.
        var oauthOptions = new AgilityManagementOptions { BaseUrl = options.BaseUrl, ApplicationName = options.ApplicationName };
        var provider = new RefreshTokenAccessTokenProvider(async (refreshToken, ct) =>
        {
            using var oauthClient = new AgilityManagementClient(oauthOptions, httpClients.CreateClient(nameof(AgilityManagementClient)));
            return await oauthClient.OAuth.RefreshAsync(refreshToken, ct).ConfigureAwait(false);
        }, options.RefreshToken!, null);
        if (options.RefreshTokenChanged is { } changed) provider.RefreshTokenChanged += (_, token) => changed(token);
        return provider;
    }
}
