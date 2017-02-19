using Yeast.Multitenancy;

namespace SimpleWebApp.Multitenancy
{
    public class SimpleTenant : ITenant
    {
        public string Name { get; set; }

        public int Port { get; set; }

        public string Identifier {
            get {
                return Name;
            }
        }
    }
}
