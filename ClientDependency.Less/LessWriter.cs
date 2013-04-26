using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using ClientDependency.Less;
using dotless.Core;

namespace ClientDependency.Less
{
    /// <summary>
    /// A file writer for dotLess
    /// </summary>
    public sealed class LessWriter : IFileWriter
    {        
        public bool WriteToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, HttpContextBase http)
        {
            try
            {
                
                //if it is a file based dependency then read it				
                var fileContents = File.ReadAllText(fi.FullName, Encoding.UTF8); //read as utf 8
                
                //NOTE: passing in null will automatically for the web configuration section to be loaded in!
                var output = LessWeb.Parse(fileContents, null);
                
                DefaultFileWriter.WriteContentToStream(provider, sw, output, type, http, origUrl);
                
                return true;
            }
            catch (Exception ex)
            {
                ClientDependencySettings.Instance.Logger.Error(string.Format("Could not write file {0} contents to stream. EXCEPTION: {1}", fi.FullName, ex.Message), ex);
                return false;
            }

            

            
        }
    }
}
