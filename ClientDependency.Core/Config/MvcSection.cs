using System.Configuration;

namespace ClientDependency.Config
{
    public class MvcSection : ConfigurationElement
    {

        [ConfigurationProperty("renderers")]
        public ProviderSettingsCollection Renderers
        {
            get { return (ProviderSettingsCollection)base["renderers"]; }
        }

        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("defaultRenderer", DefaultValue = "StandardRenderer")]
        public string DefaultRenderer
        {
            get { return (string)base["defaultRenderer"]; }
            set { base["defaultRenderer"] = value; }
        }

    }
}
