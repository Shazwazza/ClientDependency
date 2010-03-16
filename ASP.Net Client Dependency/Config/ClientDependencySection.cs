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
       
		[ConfigurationProperty("compositeFiles", IsRequired = true)]
		public CompositeFileSection CompositeFileElement
		{
		    get
		    {
				return (CompositeFileSection)this["compositeFiles"];
		    }
		}

		[ConfigurationProperty("fileRegistration", IsRequired = true)]
		public FileRegistrationSection FileRegistrationElement
		{
			get
			{
				return (FileRegistrationSection)this["fileRegistration"];
			}
		}

        [ConfigurationProperty("loggerType", IsRequired = false)]
        public string LoggerType
        {
            get
            {
                return (string)this["loggerType"];
            }
        }
	}

}
