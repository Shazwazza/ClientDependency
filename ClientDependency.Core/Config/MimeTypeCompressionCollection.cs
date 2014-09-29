using System.Configuration;

namespace ClientDependency.Config
{
    

    public class MimeTypeCompressionCollection : ConfigurationElementCollection
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
            var e = new MimeTypeCompressionElement();
            return e;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MimeTypeCompressionElement)element);
        }
    }
}
