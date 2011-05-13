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

			ClientDependencyType type;
			string fileset;
			int version = 0;

			if (string.IsNullOrEmpty(context.Request.PathInfo))
			{
				// querystring format
				fileset = context.Request["s"];
				if (!string.IsNullOrEmpty(context.Request["cdv"]) && !Int32.TryParse(context.Request["cdv"], out version))
					throw new ArgumentException("Could not parse the version in the request");
				try
				{
					type = (ClientDependencyType)Enum.Parse(typeof(ClientDependencyType), context.Request["t"], true);
				}
				catch
				{
					throw new ArgumentException("Could not parse the type set in the request");
				}
			}
			else
			{
				// path format
				var segs = context.Request.PathInfo.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				fileset = "";
				int i = 0;
				while (i < segs.Length - 1)
					fileset += segs[i++];
				int pos;
				pos = segs[i].IndexOf('.');
				if (pos < 0)
					throw new ArgumentException("Could not parse the type set in the request");
				fileset += segs[i].Substring(0, pos);
				string ext = segs[i].Substring(pos + 1);
				pos = ext.IndexOf('.');
				if (pos > 0)
				{
					if (!Int32.TryParse(ext.Substring(0, pos), out version))
						throw new ArgumentException("Could not parse the version in the request");
					ext = ext.Substring(pos + 1);
				}
				ext = ext.ToLower();
				if (ext == "js")
					type = ClientDependencyType.Javascript;
				else if (ext == "css")
					type = ClientDependencyType.Css;
				else
					throw new ArgumentException("Could not parse the type set in the request");
			}

			fileset = context.Server.UrlDecode(fileset);
			
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
            string[] strFiles = DecodeFrom64Url(fileset).Split(';');
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
		/// <param name="type">The type (css or js).</param>
		/// <param name="version">The version.</param>
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

			/* // proper way to do it is to have
			 * cache.SetVaryByCustom("cdparms");
			 * 
			 * // then have this in global.asax
			 * public override string GetVaryByCustomString(HttpContext context, string arg)
			 * {
			 *   if (arg == "cdparms")
			 *   {
			 *     if (string.IsNullOrEmpty(context.Request.PathInfo))
			 *     {
			 *       // querystring format
			 *       return context.Request["s"] + "+" + context.Request["t"] + "+" + (context.Request["v"] ?? "0");
			 *     }
			 *     else
			 *     {
			 *	     // path format
			 *	     return context.Request.PathInfo.Replace('/', '');
			 *     }
			 *   }
			 * }
			 * 
			 * // that way, there would be one cache entry for both querystring and path formats.
			 * // but, it requires a global.asax and I can't find a way to do without it.
			 */

			// in any case, cache already varies by pathInfo (build-in) so for path formats, we do not need anything
			// just add params for querystring format, just in case...
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

		string DecodeFrom64Url(string toDecode)
		{
			// see BaseFileRegistrationProvider.EncodeTo64Url
			//
			toDecode = toDecode.Replace("-", "+");
			toDecode = toDecode.Replace("_", "/");
			int rem = toDecode.Length % 4; // 0 (aligned), 1, 2 or 3 (not aligned)
			if (rem > 0)
				toDecode = toDecode.PadRight(toDecode.Length + 4 - rem, '='); // align

			return DecodeFrom64(toDecode);
		}

        private string DecodeFrom64(string toDecode)
        {
            byte[] toDecodeAsBytes = System.Convert.FromBase64String(toDecode);
            return Encoding.Default.GetString(toDecodeAsBytes);
        }
    }
}

