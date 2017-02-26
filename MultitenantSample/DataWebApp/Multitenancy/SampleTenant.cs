using Yeast.Multitenancy;

namespace DataWebApp.Multitenancy
{
    public class SampleTenant : ITenant
    {
        public string Name { get; set; }

        public int Port { get; set; }

        public string ConnectionString { get; set; }

        public string Identifier {
            get {
                return Name;
            }
        }
    }
}
