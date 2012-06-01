using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ClientDependency.Core.Config
{
	public class FileRegistrationSection : ConfigurationElement
	{

		[ConfigurationProperty("providers")]
		public ProviderSettingsCollection Providers
		{
			get { return (ProviderSettingsCollection)base["providers"]; }
		}

		[StringValidator(MinLength = 1)]
        [ConfigurationProperty("defaultProvider", DefaultValue = "LoaderControlProvider")]
		public string DefaultProvider
		{
			get { return (string)base["defaultProvider"]; }
			set { base["defaultProvider"] = value; }
		}

        [Obsolete("Use the ClientDependencySection.FileBasedDepdendenyExtensions instead")]
        [ConfigurationProperty("fileDependencyExtensions", DefaultValue = ".js,.css")]
        public string FileBasedDepdendenyExtensions
        {
            get
            {
                return (string)base["fileDependencyExtensions"];
            }
            set
            {
                base["fileDependencyExtensions"] = value;
            }
        }

        [Obsolete("Use the ClientDependencySection.FileBasedDependencyExtensionList instead")]
        public IEnumerable<string> FileBasedDependencyExtensionList
        {
            get
            {
                return FileBasedDepdendenyExtensions.Split(',')
                    .Select(x => x.Trim().ToUpper());
            }
        }

	}
}
