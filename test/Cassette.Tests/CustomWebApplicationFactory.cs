using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Cassette.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((builderContext, services) =>
            {
                // Declare the cache implementation Cassette will rely on
                services.AddDistributedMemoryCache();

                // Register Cassette in the DI container
                services.AddCassette(options =>
                {
                    options.KeyPrefix = "Cassette:Tests";
                    options.CacheEntryOption.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
                });
            });
        }
    }
}
