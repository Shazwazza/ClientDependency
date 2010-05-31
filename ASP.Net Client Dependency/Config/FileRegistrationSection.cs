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

	


		/// <summary>
		/// The configuration section to set the FileBasedDependencyExtensionList. This is a comma separated list.
		/// </summary>
		/// <remarks>
		/// If this is not explicitly set, then the extensions 'js' and 'css' are the defaults.
		/// </remarks>
		[ConfigurationProperty("fileDependencyExtensions", DefaultValue = ".js,.css")]
		protected string FileBasedDepdendenyExtensions
		{
			get { return (string)base["fileDependencyExtensions"]; }
			set { base["fileDependencyExtensions"] = value; }
		}

		/// <summary>
		/// The file extensions of Client Dependencies that are file based as opposed to request based.
		/// Any file that doesn't have the extensions listed here will be request based, request based is
		/// more overhead for the server to process.
		/// </summary>
		/// <example>
		/// A request based JavaScript file may be  a .ashx that dynamically creates JavaScript server side.
        /// Or an asmx/js request based on the proxied javascript created by web services.
		/// </example>
		/// <remarks>
		/// If this is not explicitly set, then the extensions 'js' and 'css' are the defaults.
		/// </remarks>
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
