using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ClientDependency.Core.Config
{
	public class CompositeFileSection : ConfigurationElement
	{

		[ConfigurationProperty("providers")]
		public ProviderSettingsCollection Providers
		{
			get { return (ProviderSettingsCollection)base["providers"]; }
		}

		[StringValidator(MinLength = 1)]
		[ConfigurationProperty("defaultProvider", DefaultValue = "PageHeaderProvider")]
		public string DefaultProvider
		{
			get { return (string)base["defaultProvider"]; }
			set { base["defaultProvider"] = value; }
		}		

		[ConfigurationProperty("compositeFileHandlerPath", DefaultValue = "DependencyHandler.axd")]
		public string CompositeFileHandlerPath
		{
			get { return (string)base["compositeFileHandlerPath"]; }
			set { base["compositeFileHandlerPath"] = value; }
		}
	}
}
