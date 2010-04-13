using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ClientDependency.Core.Config
{
    class RogueFileCompressionElement : ConfigurationElement
    {
        [ConfigurationProperty("compressJs", DefaultValue = true)]
        public bool CompressJs
        {
            get
            {
                return (bool)this["compressJs"];
            }
        }

        [ConfigurationProperty("compressCss", DefaultValue = true)]
        public bool CompressCss
        {
            get
            {
                return (bool)this["compressCss"];
            }
        }

        [ConfigurationProperty("path", DefaultValue = "*")]
        public string FilePath
        {
            get
            {
                return (string)this["path"];
            }
        }

        public override int GetHashCode()
        {
            return this.FilePath.GetHashCode();
        }

    }
}
