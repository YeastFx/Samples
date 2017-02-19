using System.Collections.ObjectModel;

namespace SimpleWebApp.Multitenancy
{
    public class SimpleMultitenancyOptions
    {
        public Collection<SimpleTenant> Tenants { get; set; }
    }
}
