using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.IO;
using System.Linq;
using ClientDependency.Core.Config;

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
        /// The handler file name, by default this is DependencyHandler.axd which is the default config setting.
        /// this can be overridden by specifying the property.
        /// </summary>
        private static readonly string HandlerFileName = ClientDependencySettings.Instance.CompositeFileHandlerPath;

        /// <summary>
        /// When building composite includes, it creates a Base64 encoded string of all of the combined dependency file paths
        /// for a given composite group. If this group contains too many files, then the file path with the query string will be very long.
        /// This is the maximum allowed number of characters that there is allowed, otherwise an exception is thrown.
        /// </summary>
        /// <remarks>
        /// If this handler path needs to change, it can be change by setting it in the global.asax on application start
        /// </remarks>
        public static int MaxHandlerUrlLength { get; set; }

        bool IHttpHandler.IsReusable
        {
            get
            {
                return true;
            }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            HttpResponse response = context.Response;
            string fileset = context.Server.UrlDecode(context.Request["s"]);
            ClientDependencyType type;
            int version = 0;
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

            for (int i = 0; i < 5; i++)
            {
                outputBytes = ProcessRequestInternal(context, fileset, type, version, outputBytes);
                if (outputBytes != null && outputBytes.Length > 0)
                    break;

                ClientDependencySettings.Instance.Logger.Error(() => string.Format("No bytes were returned, this is attempt {0}. Fileset: {1}, Type: {2}, Version: {3}", i, fileset, type, version));
            }

            if (outputBytes != null && outputBytes.Length > 0)
            {
                ClientDependencySettings.Instance.Logger.Fatal(() => string.Format("No bytes were returned after 5 attempts. Fileset: {0}, Type: {1}, Version: {2}", fileset, type, version));
                List<CompositeFileDefinition> fDefs;
                outputBytes = GetCombinedFiles(context, fileset, type, out fDefs);
            }

            context.Response.ContentType = type == ClientDependencyType.Javascript ? "text/javascript" : "text/css";
            context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);
        }

        internal byte[] ProcessRequestInternal(HttpContext context, string fileset, ClientDependencyType type, int version, byte[] outputBytes)
        {
            //get the map to the composite file for this file set, if it exists.
            CompositeFileMap map = CompositeFileXmlMapper.Instance.GetCompositeFile(fileset, version);

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
                        CompressionType cType = GetCompression(context);
                        outputBytes = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CompressBytes(cType, fileBytes);
                        SetContentEncodingHeaders(context, cType);
                        //save combined file
                        FileInfo compositeFile = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.SaveCompositeFile(outputBytes, type);
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

            SetCaching(context, compositeFileName);
            return outputBytes;
        }

        private byte[] GetCombinedFiles(HttpContext context, string fileset, ClientDependencyType type, out List<CompositeFileDefinition> fDefs)
        {
            //get the file list
            string[] strFiles = DecodeFrom64(fileset).Split(';');
            //combine files and get the definition types of them (internal vs external resources)
            return ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CombineFiles(strFiles, context, type, out fDefs);
        }

        private void ProcessFromFile(HttpContext context, CompositeFileMap map, out string compositeFileName, out byte[] outputBytes)
        {
            //the saved file's bytes are already compressed.
            outputBytes = map.GetCompositeFileBytes();
            compositeFileName = map.CompositeFileName;
            CompressionType cType = (CompressionType)Enum.Parse(typeof(CompressionType), map.CompressionType);
            SetContentEncodingHeaders(context, cType);
        }

        /// <summary>
        /// Sets the output cache parameters and also the client side caching parameters
        /// </summary>
        /// <param name="context"></param>
        private void SetCaching(HttpContext context, string fileName)
        {
            //This ensures OutputCaching is set for this handler and also controls
            //client side caching on the browser side. Default is 10 days.
            TimeSpan duration = TimeSpan.FromDays(10);
            HttpCachePolicy cache = context.Response.Cache;
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetExpires(DateTime.Now.Add(duration));
            cache.SetMaxAge(duration);
            cache.SetValidUntilExpires(true);
            cache.SetLastModified(DateTime.Now);
            cache.SetETag(Guid.NewGuid().ToString());
            //set server OutputCache to vary by our params
            cache.VaryByParams["t"] = true;
            cache.VaryByParams["s"] = true;
            //don't allow varying by wildcard
            cache.SetOmitVaryStar(true);
            //ensure client browser maintains strict caching rules
            cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
            //This is the only way to set the max-age cachability header in ASP.Net!
            FieldInfo maxAgeField = cache.GetType().GetField("_maxAge", BindingFlags.Instance | BindingFlags.NonPublic);
            maxAgeField.SetValue(cache, duration);

            //make this output cache dependent on the file if there is one.
            if (!string.IsNullOrEmpty(fileName))
                context.Response.AddFileDependency(fileName);
        }

        /// <summary>
        /// Sets the content encoding headers based on compressions
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        private void SetContentEncodingHeaders(HttpContext context, CompressionType type)
        {
            if (type == CompressionType.deflate)
            {
                context.Response.AddHeader("Content-encoding", "deflate");
            }
            else if (type == CompressionType.gzip)
            {
                context.Response.AddHeader("Content-encoding", "gzip");
            }
        }

        /// <summary>
        /// Check what kind of compression to use
        /// </summary>
        private CompressionType GetCompression(HttpContext context)
        {
            CompressionType type = CompressionType.none;
            string acceptEncoding = context.Request.Headers["Accept-Encoding"];

            if (!string.IsNullOrEmpty(acceptEncoding))
            {
                //deflate is faster in .Net according to Mads Kristensen (blogengine.net)
                if (acceptEncoding.Contains("deflate"))
                {
                    type = CompressionType.deflate;
                }
                else if (acceptEncoding.Contains("gzip"))
                {
                    type = CompressionType.gzip;
                }
            }

            return type;
        }

        private string DecodeFrom64(string toDecode)
        {
            byte[] toDecodeAsBytes = System.Convert.FromBase64String(toDecode);
            return System.Text.ASCIIEncoding.ASCII.GetString(toDecodeAsBytes);
        }
    }
}

