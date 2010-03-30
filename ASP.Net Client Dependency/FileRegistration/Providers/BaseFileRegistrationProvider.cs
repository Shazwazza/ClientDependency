using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Configuration.Provider;
using System.Web;
using System.Linq;
using ClientDependency.Core.Controls;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.FileRegistration.Providers
{
	public abstract class BaseFileRegistrationProvider : ProviderBase
	{
        /// <summary>
        /// Constructor sets defaults
        /// </summary>
        public BaseFileRegistrationProvider()
        {
            IsDebugMode = false;
        }

		
		protected HashSet<IClientDependencyPath> FolderPaths { get; set; }
        protected List<IClientDependencyFile> AllDependencies { get; set; }

        /// <summary>
        /// Set to true to disable composite scripts so all scripts/css comes through as individual files.
        /// </summary>
        public bool IsDebugMode { get; set; }

        #region Abstract methods/properties

        protected abstract void RegisterJsFiles(List<IClientDependencyFile> jsDependencies);
        protected abstract void RegisterCssFiles(List<IClientDependencyFile> cssDependencies);
        protected abstract void ProcessSingleJsFile(string js);
        protected abstract void ProcessSingleCssFile(string css); 
        
        #endregion
        
        #region Provider Initialization

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
          
            if (config != null && config["isDebug"] != null)
            {
                bool isDebug;
                if (bool.TryParse(config["isDebug"], out isDebug))
                {
                    IsDebugMode = isDebug;
                }
                else
                {
                    throw new ArgumentException("The isDebug parameter value specified for the provider named " + this.Name + " is invalid");
                }                    
            }

            base.Initialize(name, config);
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Returns the url for the composite file handler for the filePath specified.
        /// </summary>
        /// <param name="filePath">The file path is a semi-colon delimited string of file paths</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCompositeFileUrl(string filePaths, ClientDependencyType type)
        {
            //build the combined composite list url
            string handler = "{0}?s={1}&t={2}";
            string combinedurl = string.Format(handler, ClientDependencySettings.Instance.CompositeFileHandlerPath, HttpContext.Current.Server.UrlEncode(EncodeTo64(filePaths)), type.ToString());
            return combinedurl;
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        } 
        #endregion

        #region Public Methods        

        /// <summary>
        /// Returns a URL used to return a compbined/compressed/optimized version of all dependencies.
        /// <remarks>
        /// The full url with the encoded query strings for the handler which will process the composite list
        /// of dependencies. The handler will compbine, compress, minify, and output cache the results
        /// based on a hash key of the base64 encoded string.
        /// </remarks>        
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public string ProcessCompositeList(List<IClientDependencyFile> dependencies, ClientDependencyType type)
        {
            string rVal;
            if (dependencies.Count == 0)
                return "";

            //build the combined composite list url            
            StringBuilder files = new StringBuilder();
            foreach (IClientDependencyFile a in dependencies)
            {
                files.Append(a.FilePath + ";");
            }
            string combinedurl = GetCompositeFileUrl(files.ToString(), type);
            rVal = AppendVersionQueryString(combinedurl); //append our version to the combined url            		

            //if (url.Length > CompositeDependencyHandler.MaxHandlerUrlLength)
            //    throw new ArgumentOutOfRangeException("The number of files in the composite group " + groupName + " creates a url handler address that exceeds the CompositeDependencyHandler MaxHandlerUrlLength. Reducing the amount of files in this composite group should fix the issue");
            return rVal;
        } 
        #endregion

        #region Private Methods        

        /// <summary>
        /// Ensures the correctly resolved file path is set for each dependency (i.e. so that ~ are taken care of) and also
        /// prefixes the file path with the correct base path specified for the PathNameAlias if specified.
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="paths"></param>
        /// <param name="control"></param>
        protected void UpdateFilePaths()
        {
            foreach (IClientDependencyFile dependency in AllDependencies)
            {
                if (!string.IsNullOrEmpty(dependency.PathNameAlias))
                {
                    List<IClientDependencyPath> paths = FolderPaths.ToList();
                    IClientDependencyPath path = paths.Find(
                        delegate(IClientDependencyPath p)
                        {
                            return p.Name == dependency.PathNameAlias;
                        }
                    );
                    if (path == null)
                    {
                        throw new NullReferenceException("The PathNameAlias specified for dependency " + dependency.FilePath + " does not exist in the ClientDependencyPathCollection");
                    }
                    string basePath = path.ResolvedPath.EndsWith("/") ? path.ResolvedPath : path.ResolvedPath + "/";
                    dependency.FilePath = basePath + dependency.FilePath;
                }
                else
                {
                    //dependency.FilePath = DependantControl.ResolveUrl(dependency.FilePath);
                    dependency.FilePath = VirtualPathUtility.ToAbsolute(dependency.FilePath);
                }

                //append query strings to each file if we are in debug mode
                if (this.IsDebugMode)
                {
                    dependency.FilePath = AppendVersionQueryString(dependency.FilePath);
                }
            }
        }

        private string AppendVersionQueryString(string url)
        {
            if (ClientDependencySettings.Instance.Version == 0)
                return url;

            //ensure there's not duplicated query string syntax
            url += url.Contains('?') ? "&" : "?";
            //append a version
            url += "cdv=" + ClientDependencySettings.Instance.Version.ToString();
            return url;
        } 
        #endregion
		

	}
}
