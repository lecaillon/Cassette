using System;
using Cassette;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
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
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configureCassette is null)
            {
                throw new ArgumentNullException(nameof(configureCassette));
            }

            services.AddLogging();
            services.AddOptions();

            services.ConfigureAll(configureCassette);
            services.TryAddTransient<ReplayingHandler>();

            return services;
        }
    }
}
