using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Finbuckle.ObjectOptions
{
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration information.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets a value containing the application configuration properties.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The contract for a collection of service descriptors.</param>
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMultiTenant()
                .WithStore<CustomConfigurationStore>(ServiceLifetime.Singleton)
                .WithDelegateStrategy(async context =>
                {
                    HttpContext httpContext = (HttpContext)context;
                    PathString pathString = httpContext.Request.PathBase;
                    var tenantId = await Task.FromResult(pathString.Value.Trim('/')).ConfigureAwait(false);
                    return tenantId;
                })
                .WithPerTenantOptions<MySubOptions>((options, tenantInfo) =>
                {
                    var derivedTenantInfo = tenantInfo as DerivedTenantInfo;

                    if (derivedTenantInfo != null)
                    {
                        options = derivedTenantInfo.SubOptions;
                    }

                });

            services
                .AddControllers();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Mechanisms to configure the application's request pipeline.</param>
        /// <param name="env">Information about the web hosting environment in which the application is running.</param>
#pragma warning disable CA1822 // Mark members as static
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseMultiTenant();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapDefaultControllerRoute();
                endpoints.MapControllerRoute("default", "{__tenant__=}/{controller=test}/{action=Index}");
            });
        }
    }
}
