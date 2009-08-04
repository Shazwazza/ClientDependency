using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.Providers;

namespace ClientDependency.Core
{
	internal class ProviderDependencyList
	{
		internal ProviderDependencyList(ClientDependencyProvider provider)
		{
			Provider = provider;
			Dependencies = new ClientDependencyCollection();
		}

		internal bool Contains(ClientDependencyProvider provider)
		{
			return Provider.Name == provider.Name;
		}

		internal void AddDependencies(ClientDependencyCollection list)
		{
			Dependencies.UnionWith(list);
		}

		internal void AddDependency(IClientDependencyFile file)
		{
			Dependencies.Add(file);
		}

		internal ClientDependencyCollection Dependencies { get; private set; }
		internal ClientDependencyProvider Provider { get; private set; }
	}
}
