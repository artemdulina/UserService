using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserServiceTester.CustomSections
{
    public class Master : ConfigurationElement
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
        public string Ip
        {
            get
            {
                return this["ip"] as string;
            }
        }
    }
}
