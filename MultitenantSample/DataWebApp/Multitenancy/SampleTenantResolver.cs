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

namespace DataWebApp.Multitenancy
{
    public class SampleTenantResolver : MemCachedTenantResolver<SampleTenant>
    {
        private readonly SampleMultitenancyOptions _multitenancyOptions;
        private readonly IContainer _container;

        public SampleTenantResolver(IEnumerable<TenantServicesConfiguration<SampleTenant>> tenantServicesFactories, IMemoryCache cache, IContainer container, ILogger<SampleTenantResolver> logger, IOptions<SampleMultitenancyOptions> multitenancyOptions) : base(tenantServicesFactories, cache)
        {
            _container = container;
            _logger = logger;
            _multitenancyOptions = multitenancyOptions.Value;
        }

        protected override TenantContext<SampleTenant> BuildTenantContext(SampleTenant tenant, IServiceCollection tenantServices)
        {
            var tenantContainer = _container.CreateChildContainer();

            tenantContainer.Populate(tenantServices);

            var tenantCtx = new TenantContext<SampleTenant>(tenant, tenantContainer.GetInstance<IServiceProvider>());
            CacheContext(tenantCtx, new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(5) });
            return tenantCtx;
        }

        protected override Task<SampleTenant> IdentifyTenantAsync(HttpContext context)
        {
            return Task.Run(() => {
                return _multitenancyOptions.Tenants.SingleOrDefault(t => t.Port == context.Request.Host.Port);
            });
        }
    }
}
