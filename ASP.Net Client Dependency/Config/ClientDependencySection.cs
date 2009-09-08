using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Linq;

namespace ClientDependency.Core.Config
{
	public class ClientDependencySection : ConfigurationSection
	{

		

        
        
        /// <summary>
        /// Set the default isDebugMode property for all loaders
        /// </summary>
        [ConfigurationProperty("isDebugMode", DefaultValue = "false")]
        public bool IsDebugMode
        {
            get { return (bool)base["isDebugMode"]; }
            set { base["isDebugMode"] = value; }
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

		
	}

}
