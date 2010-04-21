using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Linq;
using ClientDependency.Core.Logging;

namespace ClientDependency.Core.Config
{
    public class ClientDependencySection : ConfigurationSection
	{
        private CompositeFileSection m_CompositeFiles = new CompositeFileSection();
        private FileRegistrationSection m_FileRego = new FileRegistrationSection();
        private MvcSection m_Mvc = new MvcSection();

        /// <summary>
        /// Set the version for the files, this will reset all composite file caching, and if
        /// composite files are disabled will add a query string to each request so that 
        /// any client side cached files will be re-downloaded.
        /// </summary>
        [ConfigurationProperty("version", DefaultValue = 0)]
        public int Version
        {
            get { return (int)base["version"]; }
            set { base["version"] = value; }
        }
       
		[ConfigurationProperty("compositeFiles")]
		public CompositeFileSection CompositeFileElement
		{
		    get
		    {
                if (this["compositeFiles"] == null)
                {
                    return m_CompositeFiles;
                }
				return (CompositeFileSection)this["compositeFiles"];
		    }
		}

		[ConfigurationProperty("fileRegistration")]
		public FileRegistrationSection FileRegistrationElement
		{
			get
			{
				return (FileRegistrationSection)this["fileRegistration"];
			}
		}

        [ConfigurationProperty("mvc")]
        public MvcSection MvcElement
        {
            get
            {
                return (MvcSection)this["mvc"];
            }
        }

        [ConfigurationProperty("loggerType")]
        public string LoggerType
        {
            get
            {
                return (string)this["loggerType"];
            }
        }

        /// <summary>
        /// Not really supposed to be used by public, but can implement at your own risk!
        /// This by default assigns the MvcFilter and RogueFileFilter.
        /// </summary>
        [ConfigurationProperty("filters", IsRequired = false)]
        public ProviderSettingsCollection Filters
        {
            get
            {
                var obj = base["filters"];

                if (obj == null || ((obj is ConfigurationElementCollection) && ((ConfigurationElementCollection)obj).Count == 0))
                {
                    var col = new ProviderSettingsCollection();
                    col.Add(new ProviderSettings("RogueFileFilter", "ClientDependency.Core.Module.RogueFileFilter, ClientDependency.Core"));
                    col.Add(new ProviderSettings("MvcFilter", "ClientDependency.Core.Mvc.MvcFilter, ClientDependency.Core.Mvc"));
                    return col;
                }
                else
                {
                    return (ProviderSettingsCollection)obj;
                }
            }
        }
	}

}
