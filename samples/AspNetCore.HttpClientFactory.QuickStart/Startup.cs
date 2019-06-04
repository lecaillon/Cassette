using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace AspNetCore.HttpClientFactory.QuickStart
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddRefitClient<IGeoApi>()
                    .ConfigureHttpClient(options => options.BaseAddress = new Uri("https://geo.api.gouv.fr"))
                    .AddReplayingHttpMessageHandler(); // Add the replaying message handler for the the IGeoApi,
                                                       // only if Cassette has been previously registered by calling AddCassette().
                                                       // The idea is to activate Cassette only during the integration tests.


        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
