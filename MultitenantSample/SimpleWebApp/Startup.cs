using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleWebApp.Multitenancy;
using StructureMap;
using System;
using Yeast.Multitenancy;

namespace SimpleWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add multitenancy
            services.AddMultitenancy<SimpleTenant, SimpleTenantResolver>();

            services.AddOptions();
            services.Configure<SimpleMultitenancyOptions>(Configuration.GetSection("Multitenancy"));

            // Add framework services.
            services.AddMvc();

            // Use StructureMap
            var container = new Container();
            container.Populate(services);

            return container.GetInstance<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Enable tenant identification
            app.UseMultitenancy<SimpleTenant>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // Fork pipeline per tenant
            app.ConfigureTenant<SimpleTenant>((tenantApp, tenant) => {
                tenantApp.UseStaticFiles();

                tenantApp.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
            });

            // Fallback
            app.Run(async context => {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tenant not found");
            });
        }
    }
}
