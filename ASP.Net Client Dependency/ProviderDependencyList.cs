using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.FileRegistration.Providers;


namespace ClientDependency.Core
{
	internal class ProviderDependencyList
	{
		internal ProviderDependencyList(BaseFileRegistrationProvider provider)
		{
			Provider = provider;
			Dependencies = new ClientDependencyCollection();
		}

		internal bool Contains(BaseFileRegistrationProvider provider)
		{
			return Provider.Name == provider.Name;
		}

		internal void AddDependencies(IEnumerable<IClientDependencyFile> list)
		{
			Dependencies.UnionWith(list);
		}

		internal void AddDependency(IClientDependencyFile file)
		{
			Dependencies.Add(file);
		}

		internal ClientDependencyCollection Dependencies { get; private set; }
		internal BaseFileRegistrationProvider Provider { get; private set; }
	}
}
