using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.IO;
using System.Linq;
using ClientDependency.Core.Config;
using System.Text;
using System.Web.Security;

namespace ClientDependency.Core.CompositeFiles
{
    public class CompositeDependencyHandler : IHttpHandler
    {
        static CompositeDependencyHandler()
        {

        }

        private static readonly string _versionNo = string.Empty;

        private object m_Lock = new object();

        /// <summary>
        /// When building composite includes, it creates a Base64 encoded string of all of the combined dependency file paths
        /// for a given composite group. If this group contains too many files, then the file path with the query string will be very long.
        /// This is the maximum allowed number of characters that there is allowed, otherwise an exception is thrown.
        /// </summary>
        /// <remarks>
        /// If this handler path needs to change, it can be change by setting it in the global.asax on application start
        /// </remarks>
        public const int MaxHandlerUrlLength = 2048;

        bool IHttpHandler.IsReusable
        {
            get
            {
                return true;
            }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            var contextBase = new HttpContextWrapper(context);
            var response = contextBase.Response;
            var fileset = context.Server.UrlDecode(context.Request["s"]);
            ClientDependencyType type;
            var version = 0;
            int.TryParse(context.Request["cdv"], out version);
            try
            {
                type = (ClientDependencyType)Enum.Parse(typeof(ClientDependencyType), context.Request["t"], true);
            }
            catch
            {
                throw new ArgumentException("Could not parse the type set in the request");
            }

            if (string.IsNullOrEmpty(fileset))
                throw new ArgumentException("Must specify a fileset in the request");

            byte[] outputBytes = null;

            //retry up to 5 times... this is only here due to a bug found in another website that was returning a blank 
            //result. To date, it can't be replicated in VS, but we'll leave it here for error handling support... can't hurt
            for (int i = 0; i < 5; i++)
            {
                outputBytes = ProcessRequestInternal(contextBase, fileset, type, version, outputBytes);
                if (outputBytes != null && outputBytes.Length > 0)
                    break;

                ClientDependencySettings.Instance.Logger.Error(string.Format("No bytes were returned, this is attempt {0}. Fileset: {1}, Type: {2}, Version: {3}", i, fileset, type, version), null);
            }

            if (outputBytes == null || outputBytes.Length == 0)
            {
                ClientDependencySettings.Instance.Logger.Fatal(string.Format("No bytes were returned after 5 attempts. Fileset: {0}, Type: {1}, Version: {2}", fileset, type, version), null);
                List<CompositeFileDefinition> fDefs;
                outputBytes = GetCombinedFiles(contextBase, fileset, type, out fDefs);
            }

            context.Response.ContentType = type == ClientDependencyType.Javascript ? "application/x-javascript" : "text/css";
            context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);
        }

        internal byte[] ProcessRequestInternal(HttpContextBase context, string fileset, ClientDependencyType type, int version, byte[] outputBytes)
        {
            //get the compression type supported
            CompressionType cType = context.GetClientCompression(); 

            //get the map to the composite file for this file set, if it exists.
            CompositeFileMap map = CompositeFileXmlMapper.Instance.GetCompositeFile(fileset, version, cType.ToString());

            string compositeFileName = "";
            if (map != null && map.HasFileBytes)
            {
                ProcessFromFile(context, map, out compositeFileName, out outputBytes);
            }
            else
            {
                bool fromFile = false;

                lock (m_Lock)
                {
                    //check again...
                    if (map == null || !map.HasFileBytes)
                    {
                        //need to do the combining, etc... and save the file map

                        List<CompositeFileDefinition> fDefs;
                        byte[] fileBytes = GetCombinedFiles(context, fileset, type, out fDefs);
                        //compress data                        
                        outputBytes = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CompressBytes(cType, fileBytes);
                        context.AddCompressionResponseHeader(cType);
                        //save combined file
                        FileInfo compositeFile = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.SaveCompositeFile(outputBytes, type, context.Server);
                        if (compositeFile != null)
                        {
                            compositeFileName = compositeFile.FullName;
                            if (!string.IsNullOrEmpty(compositeFileName))
                            {
                                //Update the XML file map
                                CompositeFileXmlMapper.Instance.CreateMap(fileset, cType.ToString(),
                                    fDefs
                                        .Where(f => f.IsLocalFile)
                                        .Select(x => new FileInfo(context.Server.MapPath(x.Uri))).ToList(), compositeFileName,
                                        ClientDependencySettings.Instance.Version);
                            }
                        }                        
                    }
                    else
                    {
                        //files are there now, process from file.
                        fromFile = true;
                    }
                }

                if (fromFile)
                {
                    ProcessFromFile(context, map, out compositeFileName, out outputBytes);
                }
            }

            SetCaching(context, compositeFileName, fileset);
            return outputBytes;
        }

        private byte[] GetCombinedFiles(HttpContextBase context, string fileset, ClientDependencyType type, out List<CompositeFileDefinition> fDefs)
        {
            //get the file list
            string[] strFiles = DecodeFrom64(fileset).Split(';');
            //combine files and get the definition types of them (internal vs external resources)
            return ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CombineFiles(strFiles, context, type, out fDefs);
        }

        private void ProcessFromFile(HttpContextBase context, CompositeFileMap map, out string compositeFileName, out byte[] outputBytes)
        {
            //the saved file's bytes are already compressed.
            outputBytes = map.GetCompositeFileBytes();
            compositeFileName = map.CompositeFileName;
            CompressionType cType = (CompressionType)Enum.Parse(typeof(CompressionType), map.CompressionType);
            context.AddCompressionResponseHeader(cType);
        }

        /// <summary>
        /// Sets the output cache parameters and also the client side caching parameters
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileName">The name of the file that has been saved to disk</param>
        /// <param name="fileset">The Base64 encoded string supplied in the query string for the handler</param>
        private void SetCaching(HttpContextBase context, string fileName, string fileset)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                ClientDependencySettings.Instance.Logger.Error("ClientDependency handler path is null", new Exception());
                return;
            }

            //This ensures OutputCaching is set for this handler and also controls
            //client side caching on the browser side. Default is 10 days.
            var duration = TimeSpan.FromDays(10);
            var cache = context.Response.Cache;
            cache.SetCacheability(HttpCacheability.Public);

            cache.SetExpires(DateTime.Now.Add(duration));
            cache.SetMaxAge(duration);
            cache.SetValidUntilExpires(true);
            cache.SetLastModified(DateTime.Now);

            cache.SetETag(FormsAuthentication.HashPasswordForStoringInConfigFile(fileset, "MD5"));
            
            //set server OutputCache to vary by our params
            cache.VaryByParams["t"] = true;
            cache.VaryByParams["s"] = true;
            cache.VaryByParams["cdv"] = true;
            //ensure the cache is different based on the encoding specified per browser
            cache.VaryByContentEncodings["gzip"] = true;
            cache.VaryByContentEncodings["deflate"] = true;

            //don't allow varying by wildcard
            cache.SetOmitVaryStar(true);
            //ensure client browser maintains strict caching rules
            cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
            
            //This is the only way to set the max-age cachability header in ASP.Net!
            //FieldInfo maxAgeField = cache.GetType().GetField("_maxAge", BindingFlags.Instance | BindingFlags.NonPublic);
            //maxAgeField.SetValue(cache, duration);

            //make this output cache dependent on the file if there is one.
            if (!string.IsNullOrEmpty(fileName))
                context.Response.AddFileDependency(fileName);
        }       

        private string DecodeFrom64(string toDecode)
        {
            byte[] toDecodeAsBytes = System.Convert.FromBase64String(toDecode);
            return Encoding.Default.GetString(toDecodeAsBytes);
        }
    }
}

