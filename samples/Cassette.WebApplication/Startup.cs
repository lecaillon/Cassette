using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Cassette.WebApplication
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
                    .AddReplayingHttpMessageHandler(); // Will add the Cassette replaying handler for the IGeoApi 
                                                       // only if AddCassette() is previously called.
                                                       // The idea obviously is to activate Cassette only a test env.
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
