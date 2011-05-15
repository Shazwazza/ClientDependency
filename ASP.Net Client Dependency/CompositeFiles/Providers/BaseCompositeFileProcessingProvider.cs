using System;
using System.Collections.Generic;
using System.Text;
using ClientDependency.Core.Config;
using System.IO;
using System.Web;
using System.Net;
using System.Configuration.Provider;

namespace ClientDependency.Core.CompositeFiles.Providers
{
    public abstract class BaseCompositeFileProcessingProvider : ProviderBase, IHttpProvider
    {

        #region Static Methods

        static string EncodeTo64Url(string toEncode)
        {
            string returnValue = EncodeTo64(toEncode);

            // returnValue is base64 = may contain a-z, A-Z, 0-9, +, /, and =.
            // the = at the end is just a filler, can remove
            // then convert the + and / to "base64url" equivalent
            //
            returnValue = returnValue.TrimEnd(new char[] { '=' });
            returnValue = returnValue.Replace("+", "-");
            returnValue = returnValue.Replace("/", "_");

            return returnValue;
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.Default.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        #endregion

        private const string DefaultDependencyPath = "~/App_Data/ClientDependency";
        private readonly string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        /// <summary>
        /// The path specified in the config
        /// </summary>
        private string _compositeFilePath;

        /// <summary>
        /// constructor sets defaults
        /// </summary>
        protected BaseCompositeFileProcessingProvider()
        {
            PersistCompositeFiles = true;
            EnableCssMinify = true;
            EnableJsMinify = true;

            _compositeFilePath = DefaultDependencyPath;
        }

        #region Public Properties

        /// <summary>
        /// Flags whether or not to enable composite file script creation/persistence.
        /// Composite file persistence will increase performance in the case of cache turnover or application
        /// startup since the files are already combined and compressed.
        /// This also allows for the ability to easily clear the cache so the files are refreshed.
        /// </summary>
        public bool PersistCompositeFiles { get; set; }
        public bool EnableCssMinify { get; set; }
        public bool EnableJsMinify { get; set; }

        /// <summary>
        /// The Url type to use for the dependency handler 
        /// </summary>
        public CompositeUrlType UrlType { get; protected set; }

        /// <summary>
        /// Returns the CompositeFilePath
        /// </summary>
        /// <returns></returns>
        public DirectoryInfo CompositeFilePath { get; protected set; }

        /// <summary>
        /// Returns the set of white listed domains
        /// </summary>
        public IList<string> BundleDomains { get; protected set; }

        #endregion

        #region IHttpProvider Members

        public void Initialize(HttpContextBase http)
        {
            CompositeFilePath = new DirectoryInfo(http.Server.MapPath(_compositeFilePath));
        }

        #endregion

        public abstract FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type, HttpServerUtilityBase server);
        public abstract byte[] CombineFiles(string[] strFiles, HttpContextBase context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs);
        public abstract byte[] CompressBytes(CompressionType type, byte[] fileBytes);

        /// <summary>
        /// Returns the url for the composite file handler for the filePath specified.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="type"></param>
        /// <param name="http"></param>
        /// <param name="appendVersion"></param>
        /// <returns></returns>
        public virtual string GetCompositeFileUrl(string filePaths, ClientDependencyType type, HttpContextBase http, bool appendVersion)
        {
            var url = new StringBuilder();

            switch (UrlType)
            {
                case CompositeUrlType.Base64QueryStrings:
                    const string handler = "{0}?s={1}&t={2}";
                    url.Append(string.Format(handler,
                                             ClientDependencySettings.Instance.CompositeFileHandlerPath,
                                             http.Server.UrlEncode(EncodeTo64(filePaths)), type));
                    break;
                case CompositeUrlType.Base64Paths:
                    filePaths = EncodeTo64Url(filePaths);

                    url.Append(ClientDependencySettings.Instance.CompositeFileHandlerPath);
                    int pos = 0;
                    while (filePaths.Length > pos)
                    {
                        url.Append("/");
                        int len = Math.Min(filePaths.Length - pos, 240);
                        url.Append(filePaths.Substring(pos, len));
                        pos += 240;
                    }
                    int version = ClientDependencySettings.Instance.Version;
                    if (appendVersion && version > 0)
                    {
                        url.Append(".");
                        url.Append(version.ToString());
                    }
                    switch (type)
                    {
                        case ClientDependencyType.Css:
                            url.Append(".css");
                            break;
                        case ClientDependencyType.Javascript:
                            url.Append(".js");
                            break;
                    }
                    break;
                case CompositeUrlType.MappedId:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return url.ToString();
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config == null)
                return;

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

            //set the default
            UrlType = CompositeUrlType.Base64QueryStrings;
            if (config["urlType"] != null)
            {
                try
                {
                    UrlType = (CompositeUrlType)Enum.Parse(typeof(CompositeUrlType), config["urlType"]);
                }
                catch (ArgumentException)
                {
                    //swallow exception, we've set the default
                }
            }

            _compositeFilePath = config["compositeFilePath"] ?? DefaultDependencyPath;

            string bundleDomains = config["bundleDomains"];
            if (bundleDomains != null)
                bundleDomains = bundleDomains.Trim();
            if (string.IsNullOrEmpty(bundleDomains))
            {
                BundleDomains = new List<string>();
            }
            else
            {
                string[] domains = bundleDomains.Split(new char[] { ',' });
                for (int i = 0; i < domains.Length; i++)
                {
                    // make sure we have a starting dot and a trailing port
                    // ie 'maps.google.com' will be stored as '.maps.google.com:80'
                    if (domains[i].IndexOf(':') < 0)
                        domains[i] = domains[i] + ":80";
                    if (!domains[i].StartsWith("."))
                        domains[i] = "." + domains[i];
                }
                BundleDomains = new List<string>(domains);
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
                    // get the domain to test, with starting dot and trailing port, then compare with
                    // declared (authorized) domains. the starting dot is here to allow for subdomain
                    // approval, eg '.maps.google.com:80' will be approved by rule '.google.com:80', yet
                    // '.roguegoogle.com:80' will not.
                    var domain = string.Format(".{0}:{1}", uri.Host, uri.Port);
                    bool bundle = false;
                    foreach (string bundleDomain in BundleDomains)
                    {
                        if (domain.EndsWith(bundleDomain))
                        {
                            bundle = true;
                            break;
                        }
                    }
                    if (bundle)
                    {
                        requestContents = GetXmlResponse(uri);
                        return true;
                    }
                    else
                    {
                        ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. Domain is not white-listed.", url), null);
                    }
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
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
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
