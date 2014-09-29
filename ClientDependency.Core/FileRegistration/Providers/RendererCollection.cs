using System;
using System.Configuration.Provider;

namespace ClientDependency.FileRegistration.Providers
{
    public class RendererCollection : ProviderCollection
    {
        public new BaseRenderer this[string name]
        {
            get { return (BaseRenderer)base[name]; }
        }

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is BaseRenderer))
                throw new ArgumentException("Invalid provider type", "provider");

            base.Add(provider);
        }
    }
}
