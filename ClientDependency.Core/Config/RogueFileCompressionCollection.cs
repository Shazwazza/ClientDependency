using System.Configuration;

namespace ClientDependency.Config
{
    public class RogueFileCompressionCollection : ConfigurationElementCollection
    {
        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            var e = new RogueFileCompressionElement();
            return e;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RogueFileCompressionElement)element);
        }
    }
}
