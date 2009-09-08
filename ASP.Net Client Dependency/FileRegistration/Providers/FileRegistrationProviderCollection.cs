using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;

namespace ClientDependency.Core.FileRegistration.Providers
{
	public class FileRegistrationProviderCollection : ProviderCollection
	{
		public new BaseFileRegistrationProvider this[string name]
		{
			get { return (BaseFileRegistrationProvider)base[name]; }
		}

		public override void Add(ProviderBase provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			if (!(provider is BaseFileRegistrationProvider))
				throw new ArgumentException("Invalid provider type", "provider");

			base.Add(provider);
		}
	}

}
