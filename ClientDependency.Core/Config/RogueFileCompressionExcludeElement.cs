using System.Configuration;

namespace ClientDependency.Config
{
    
    public class RogueFileCompressionExcludeElement : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public string FilePath
        {
            get
            {
                return (string)this["path"];
            }
        }

        public override int GetHashCode()
        {
            return this.FilePath.GetHashCode();
        }

    }
}
