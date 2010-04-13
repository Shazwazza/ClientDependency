using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Configuration.Provider;
using System.Web;
using System.Linq;
using ClientDependency.Core.Controls;
using ClientDependency.Core.Config;
using ClientDependency.Core;

namespace ClientDependency.Core.FileRegistration.Providers
{
	public abstract class BaseFileRegistrationProvider : ProviderBase
	{
        /// <summary>
        /// Constructor sets defaults
        /// </summary>
        public BaseFileRegistrationProvider()
        {
        }
		
        //protected HashSet<IClientDependencyPath> FolderPaths { get; set; }
        //protected List<IClientDependencyFile> AllDependencies { get; set; }

        #region Abstract methods/properties

        protected abstract string RenderJsDependencies(List<IClientDependencyFile> jsDependencies);
        protected abstract string RenderCssDependencies(List<IClientDependencyFile> cssDependencies);
        protected abstract string RenderSingleJsFile(string js);
        protected abstract string RenderSingleCssFile(string css); 
        
        #endregion
        
        #region Provider Initialization

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {          

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

        #region Protected Methods

        /// <summary>
        /// Ensures the correctly resolved file path is set for each dependency (i.e. so that ~ are taken care of) and also
        /// prefixes the file path with the correct base path specified for the PathNameAlias if specified.
        /// </summary>
        /// <param name="dependencies">The dependencies list for which file paths will be updated</param>
        /// <param name="folderPaths"></param>
        protected void UpdateFilePaths(IEnumerable<IClientDependencyFile> dependencies
            , HashSet<IClientDependencyPath> folderPaths)
        {
            foreach (IClientDependencyFile dependency in dependencies)
            {
                if (!string.IsNullOrEmpty(dependency.PathNameAlias))
                {
                    List<IClientDependencyPath> paths = folderPaths.ToList();
                    IClientDependencyPath path = paths.Find(
                        (p) =>
                        {
                            return p.Name == dependency.PathNameAlias;
                        }
                    );
                    if (path == null)
                    {
                        throw new NullReferenceException("The PathNameAlias specified for dependency " + dependency.FilePath + " does not exist in the ClientDependencyPathCollection");
                    }
                    var resolvedPath = path.ResolvePath();
                    string basePath = resolvedPath.EndsWith("/") ? resolvedPath : resolvedPath + "/";
                    dependency.FilePath = basePath + dependency.FilePath;
                }
                else
                {
                    dependency.FilePath = dependency.ResolveFilePath();
                }

                //append query strings to each file if we are in debug mode
                if (ConfigurationHelper.IsCompilationDebug)
                {
                    dependency.FilePath = AppendVersionQueryString(dependency.FilePath);
                }
            }
        }

        /// <summary>
        /// This will ensure that no duplicates have made it into the collection.
        /// Duplicates WILL occur if the same dependency is registered in 2 different ways: 
        /// one with a global path and one with a full path. This is because paths may not be defined
        /// until we render so we cannot de-duplicate at the time of registration.
        /// De-duplication will remove the dependency with a lower priority or later in the list.
        /// This also must be called after UpdatePaths are called since we need to full path filled in.
        /// </summary>
        /// <param name="dependencies">The dependencies list for which duplicates will be removed</param>
        /// <param name="folderPaths"></param>
        protected void EnsureNoDuplicates(List<IClientDependencyFile> dependencies
            , HashSet<IClientDependencyPath> folderPaths)
        {
            var dupPaths = dependencies
                .Select(x => x.FilePath) //Project each element to its uniqueID property
                .GroupBy(x => x) //Project each element to its uniqueID property
                .Where(x => x.Skip(1).Any()) //Filter the groups by groups that have more than 1 element
                .Select(x => x.Key) //Project each group to the group's key (back to uniqueID)
                .ToList();

            var toKeep = new List<IClientDependencyFile>();

            foreach (var d in dupPaths)
            {
                //find the dups and return an object with the associated index
                var dups = dependencies
                    .Where(x => x.FilePath == d)
                    .Select(x => new { Index = dependencies.IndexOf(x), File = x })
                    .ToList();

                var priorities = dups.Select(x => x.File.Priority).Distinct().ToList();
                //if there's more than 1 priority defined, we know we need to remove by priority
                //instead of by index
                if (priorities.Count() > 1)
                {
                    toKeep.Add(dups
                        .Where(x => x.File.Priority == priorities
                            .Min())
                        .First().File);
                }
                else
                {
                    //if not by priority, we just need to keep the first on in the list
                    toKeep.Add(dups
                        .Where(x => x.Index == dups
                            .Select(p => p.Index)
                            .Min())
                        .First().File);
                }
            }

            //now we need to remove the dups that don't exist in our to keep list
            var toRemove = dependencies
                .Where(x => dupPaths.Contains(x.FilePath)) //find files that match our dup file paths
                .Where(x => !toKeep.Contains(x)) //exlude the to keeps
                .ToList();

            foreach (var r in toRemove)
            {
                dependencies.Remove(r);
            }

        }

        #endregion

        #region Private Methods

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
