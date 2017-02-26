using DataWebApp.Data;
using DataWebApp.Models;
using DataWebApp.Multitenancy;
using DataWebApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using Yeast.Multitenancy;

namespace DataWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add Multitenancy
            services.AddMultitenancy<SampleTenant, SampleTenantResolver>();

            services.AddOptions();
            services.Configure<SampleMultitenancyOptions>(Configuration.GetSection("Multitenancy"));

            // Add Identity
            services.AddTenantIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureTenantServices<SampleTenant>(
                (tenant, tenantServices) =>
                {
                    // Configure EF
                    tenantServices.AddEntityFrameworkSqlServer();
                    tenantServices.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(tenant.ConnectionString)
                    );
                }
            );

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();

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

            // Use Multitenancy
            app.UseMultitenancy<SampleTenant>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.ConfigureTenant<SampleTenant>((tenantApp, tenantCtx) => {
                tenantApp.UseTenantIdentity(tenantCtx);

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
