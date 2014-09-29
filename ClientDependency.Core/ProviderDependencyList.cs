using System.Collections.Generic;
using ClientDependency.FileRegistration;

namespace ClientDependency
{
	internal class ProviderDependencyList
	{
		internal ProviderDependencyList(BaseFileRegistrationProvider provider)
		{
			Provider = provider;
            Dependencies = new List<IClientDependencyFile>();
		}

		internal bool ProviderIs(BaseFileRegistrationProvider provider)
		{
			return Provider.Name == provider.Name;
		}

		internal void AddDependencies(IEnumerable<IClientDependencyFile> list)
		{
			Dependencies.AddRange(list);
		}

		internal void AddDependency(IClientDependencyFile file)
		{
			Dependencies.Add(file);
		}

        internal List<IClientDependencyFile> Dependencies { get; private set; }
		internal BaseFileRegistrationProvider Provider { get; private set; }
	}
}
