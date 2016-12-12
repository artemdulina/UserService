using System.Configuration;

namespace Service.CustomSections
{
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
