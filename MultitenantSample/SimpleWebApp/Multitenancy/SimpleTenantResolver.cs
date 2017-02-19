using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yeast.Multitenancy;
using Yeast.Multitenancy.Implementations;

namespace SimpleWebApp.Multitenancy
{
    public class SimpleTenantResolver : MemCachedTenantResolver<SimpleTenant>
    {
        private readonly SimpleMultitenancyOptions _multitenancyOptions;
        private readonly IContainer _container;

        public SimpleTenantResolver(IEnumerable<TenantServicesConfiguration<SimpleTenant>> tenantServicesFactories, IMemoryCache cache, IContainer container, ILogger<SimpleTenantResolver> logger, IOptions<SimpleMultitenancyOptions> multitenancyOptions) : base(tenantServicesFactories, cache) {
            _container = container;
            _logger = logger;
            _multitenancyOptions = multitenancyOptions.Value;
        }

        protected override TenantContext<SimpleTenant> BuildTenantContext(SimpleTenant tenant, IServiceCollection tenantServices)
        {
            var tenantContainer = _container.CreateChildContainer();

            tenantContainer.Populate(tenantServices);

            var tenantCtx = new TenantContext<SimpleTenant>(tenant, tenantContainer.GetInstance<IServiceProvider>());
            CacheContext(tenantCtx, new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(5) });
            return tenantCtx;
        }

        protected override Task<SimpleTenant> IdentifyTenantAsync(HttpContext context)
        {
            return Task.Run(() => {
                return _multitenancyOptions.Tenants.SingleOrDefault(t => t.Port == context.Request.Host.Port);
            });
        }
    }
}