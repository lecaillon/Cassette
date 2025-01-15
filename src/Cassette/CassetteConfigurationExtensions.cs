using System;
using System.Linq;
using Cassette;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extensions methods for configuring Cassette.
/// </summary>
public static class CassetteConfigurationExtensions
{
    /// <summary>
    ///     Adds Cassette and related services to the <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddCassette(this IServiceCollection services, Action<CassetteOptions> configureCassette)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureCassette);

        services.AddLogging();
        services.AddOptions();

        services.ConfigureAll(configureCassette);
        services.TryAddTransient<ReplayingHandler>();

        return services;
    }

    /// <summary> 
    ///     Adds the Cassette message handler to a dedicated <see cref="System.Net.Http.HttpClient"/>, 
    ///     only if <see cref="AddCassette(IServiceCollection, Action{CassetteOptions})"/> has been previously called. 
    ///     Otherwise does not do anything.
    /// </summary>
    public static IHttpClientBuilder AddReplayingHttpMessageHandler(this IHttpClientBuilder builder)
    {
        if (builder.Services.IsCassetteRegistered())
        {
            builder.AddHttpMessageHandler<ReplayingHandler>();
        }
        return builder;
    }

    private static bool IsCassetteRegistered(this IServiceCollection services) =>
        services.Any(x => x.ServiceType == typeof(ReplayingHandler));
}
