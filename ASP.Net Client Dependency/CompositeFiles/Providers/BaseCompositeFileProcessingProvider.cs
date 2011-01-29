using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.Config;
using System.IO;
using System.Web;
using System.Net;
using System.IO.Compression;
using System.Configuration.Provider;
using System.Web.Hosting;

namespace ClientDependency.Core.CompositeFiles.Providers
{
	public abstract class BaseCompositeFileProcessingProvider : ProviderBase
	{
       
        /// <summary>
        /// constructor sets defaults
        /// </summary>
        protected BaseCompositeFileProcessingProvider()
        {
            PersistCompositeFiles = true;
            EnableCssMinify = true;
            EnableJsMinify = true;   
         
            _mapPath = HostingEnvironment.MapPath;         
        }

        /// <summary>
        /// Internal constructor used for testing
        /// </summary>
        /// <param name="mapPath"></param>
        internal BaseCompositeFileProcessingProvider(Func<string, string> mapPath)
        {
            PersistCompositeFiles = true;
            EnableCssMinify = true;
            EnableJsMinify = true;   
            _mapPath = mapPath;
        }

	    private readonly Func<string, string> _mapPath;

        private readonly string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

		#region Provider Members

		public abstract FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type);
        public abstract byte[] CombineFiles(string[] strFiles, HttpContextBase context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs);
		public abstract byte[] CompressBytes(CompressionType type, byte[] fileBytes);

        /// <summary>
        /// Flags whether or not to enable composite file script creation/persistence.
        /// Composite file persistence will increase performance in the case of cache turnover or application
        /// startup since the files are already combined and compressed.
        /// This also allows for the ability to easily clear the cache so the files are refreshed.
        /// </summary>
        public bool PersistCompositeFiles { get; set; }
        public bool EnableCssMinify { get; set; }
        public bool EnableJsMinify { get; set; }
        public DirectoryInfo CompositeFilePath { get; set; }

		#endregion

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config != null)
            {
                if (config["enableCssMinify"] != null)
                {
                    bool enableCssMinify;
                    if (bool.TryParse(config["enableCssMinify"], out enableCssMinify))
                        EnableCssMinify = enableCssMinify;
                }
                if (config["enableJsMinify"] != null)
                {
                    bool enableJsMinify;
                    if (bool.TryParse(config["enableJsMinify"], out enableJsMinify))
                        EnableJsMinify = enableJsMinify;
                }

                if (config["persistFiles"] != null)
                {
                    bool persistFiles;
                    if (bool.TryParse(config["persistFiles"], out persistFiles))
                        PersistCompositeFiles = persistFiles;
                }
                if (config["compositeFilePath"] != null)
                {
                    CompositeFilePath = new DirectoryInfo(_mapPath(config["compositeFilePath"]));    
                }                
                else
                {
                    //set the default
                    CompositeFilePath = new DirectoryInfo(_mapPath("~/App_Data/ClientDependency"));
                }
            }
            
        }

        protected string MinifyFile(string fileContents, ClientDependencyType type)
        {
            switch (type)
            {
                case ClientDependencyType.Css:
                    return EnableCssMinify ? CssMin.CompressCSS(fileContents) : fileContents;
                case ClientDependencyType.Javascript:
                    return EnableJsMinify ? JSMin.CompressJS(fileContents) : fileContents;
                default:
                    return fileContents;
            }
        }

	    /// <summary>
	    /// This ensures that all paths (i.e. images) in a CSS file have their paths change to absolute paths.
	    /// </summary>
	    /// <param name="fileContents"></param>
	    /// <param name="type"></param>
	    /// <param name="url"></param>
	    /// <param name="http"></param>
	    /// <returns></returns>
	    protected string ParseCssFilePaths(string fileContents, ClientDependencyType type, string url, HttpContextBase http)
		{
			//if it is a CSS file we need to parse the URLs
			if (type == ClientDependencyType.Css)
			{
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                fileContents = CssFileUrlFormatter.TransformCssFile(fileContents, uri.MakeAbsoluteUri(http));
			}
			return fileContents;
		}

	    /// <summary>
	    /// Tries to convert the url to a uri, then read the request into a string and return it.
	    /// This takes into account relative vs absolute URI's
	    /// </summary>
	    /// <param name="url"></param>
	    /// <param name="requestContents"></param>
	    /// <param name="http"></param>
	    /// <returns>true if successful, false if not successful</returns>
	    /// <remarks>
	    /// if the path is a relative local path, the we use Server.Execute to get the request output, otherwise
	    /// if it is an absolute path, a WebClient request is made to fetch the contents.
	    /// </remarks>
	    protected bool TryReadUri(string url, out string requestContents, HttpContextBase http)
		{
			Uri uri;
			if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
			{
                
                //if its a relative path, and it's an aspx page, then execute it, 
                //otherwise change it to an absolute path and try to request it.
                if (!uri.IsAbsoluteUri)
                {
                    if (uri.ToString().ToUpper().EndsWith(".ASPX"))
                    {
                        //its a relative path so use the execute method
                        var sw = new StringWriter();
                        try
                        {
                            http.Server.Execute(url, sw);
                            requestContents = sw.ToString();
                            sw.Close();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", url, ex.Message), ex);
                        }
                    }
                    else
                    {
                        uri = uri.MakeAbsoluteUri(http);
                    }                    
                }

				try
				{
                    requestContents = GetXmlResponse(uri);
					return true;
				}
				catch (Exception ex)
				{
                    ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", url, ex.Message), ex);
				}
				

			}
			requestContents = "";
			return false;
		}

        /// <summary>
        /// Gets the web response and ensures that the BOM is not present not matter what encoding is specified.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private string GetXmlResponse(Uri resource)
        {
            string xml;

            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                xml = client.DownloadString(resource);
            }

            if (xml.StartsWith(_byteOrderMarkUtf8))
            {
                xml = xml.Remove(0, _byteOrderMarkUtf8.Length - 1);
            }

            return xml;
        }

	}
}
