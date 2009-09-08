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

		/// <summary>
		/// This is the folder that composite script/css files will be stored once they are combined.
		/// </summary>
		[StringValidator(MinLength = 1)]
		[ConfigurationProperty("compositeFilePath", DefaultValue = "~/App_Data/ClientDependency")]
		public string CompositeFilePath
		{
			get { return (string)base["compositeFilePath"]; }
			set { base["compositeFilePath"] = value; }
		}

		[ConfigurationProperty("compositeFileHandlerPath", DefaultValue = "DependencyHandler.axd")]
		public string CompositeFileHandlerPath
		{
			get { return (string)base["compositeFileHandlerPath"]; }
			set { base["compositeFileHandlerPath"] = value; }
		}
	}
}
