using System.Configuration;

namespace Service.CustomSections
{
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
}
