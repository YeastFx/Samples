using System.Collections.ObjectModel;

namespace DataWebApp.Multitenancy
{
    public class SampleMultitenancyOptions
    {
        public Collection<SampleTenant> Tenants { get; set; }
    }
}
