using DataWebApp.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using Yeast.Multitenancy;

namespace DataWebApp.Multitenancy
{
    /// <summary>
    /// Identity extensions for <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class IdentityAppBuilderExtensions
    {
        /// <summary>
        /// Enables ASP.NET identity for the current application.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <param name="tenantCtx">The <see cref="TenantContext{SampleTenant}"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance this method extends.</returns>
        public static IApplicationBuilder UseTenantIdentity(this IApplicationBuilder app, TenantContext<SampleTenant> tenantCtx)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseCookieAuthentication(new CookieAuthenticationOptions() {
                AuthenticationScheme = AccountController.AppAuthenticationScheme,
                CookieName = $"{tenantCtx.Tenant.Identifier.Replace(" ", "_")}.Identity",
                LoginPath = new PathString("/Account/Login/"),
                LogoutPath = new PathString("/Account/Logout/"),
                AccessDeniedPath = new PathString("/Account/Forbidden/"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });
            return app;
        }
    }
}
