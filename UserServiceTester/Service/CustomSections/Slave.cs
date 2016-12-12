using System.Configuration;

namespace Service.CustomSections
{
    public class Slave : ConfigurationElement
    {
        [ConfigurationProperty("ip", IsRequired = true)]
        public string Ip
        {
            get
            {
                return this["ip"] as string;
            }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public string Port
        {
            get
            {
                return this["port"] as string;
            }
        }
    }
}
