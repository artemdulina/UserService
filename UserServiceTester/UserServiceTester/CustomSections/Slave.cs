using System;
using System.Configuration;

namespace UserServiceTester.CustomSections
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
            set
            {
                base["ip"] = value;
            }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public string Port
        {
            get
            {
                return this["port"] as string;
            }
            set
            {
                base["port"] = value;
            }
        }
    }

    public class Master : Slave
    {

    }

    [ConfigurationCollection(typeof(Slaves), AddItemName = "slave")]
    public class Slaves : ConfigurationElementCollection
    {
        public Slave this[int index]
        {
            get
            {
                return BaseGet(index) as Slave;
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        public new Slave this[string responseString]
        {
            get
            {
                return (Slave)BaseGet(responseString);
            }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Slave();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (Slave)element;
        }
    }

    public class MasterSlavesConfig : ConfigurationSection
    {
        public static MasterSlavesConfig GetConfig()
        {
            return (MasterSlavesConfig)ConfigurationManager.GetSection("masterslaves") ?? new MasterSlavesConfig();
        }

        [ConfigurationProperty("slaves")]
        public Slaves Slaves
        {
            get
            {
                return this["slaves"] as Slaves;
            }
        }

        [ConfigurationProperty("master")]
        public Master Master
        {
            get
            {
                return this["master"] as Master;
            }
        }
    }
}
