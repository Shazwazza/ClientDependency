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
using ClientDependency.Core.CompositeFiles;

namespace ClientDependency.Core.FileRegistration.Providers
{
	public abstract class BaseFileRegistrationProvider : ProviderBase
	{
        /// <summary>
        /// Constructor sets defaults
        /// </summary>
        protected BaseFileRegistrationProvider()
        {
            EnableCompositeFiles = true;
        }
		
        //protected HashSet<IClientDependencyPath> FolderPaths { get; set; }
        //protected List<IClientDependencyFile> AllDependencies { get; set; }
                
        /// <summary>
        /// By default this is true but can be overriden (in either config or code). 
        /// Composite files are never enabled with compilation debug="true" however.
        /// </summary>
        protected bool EnableCompositeFiles { get; set; }

        #region Abstract methods/properties

        protected abstract string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http);
        protected abstract string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http);
        protected abstract string RenderSingleJsFile(string js);
        protected abstract string RenderSingleCssFile(string css); 
        
        #endregion
        
        #region Provider Initialization

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {          
            base.Initialize(name, config);

            if (config != null && config["enableCompositeFiles"] != null && !string.IsNullOrEmpty(config["enableCompositeFiles"]))
            {
                EnableCompositeFiles = bool.Parse(config["enableCompositeFiles"]);
            }
        }

        #endregion

        #region Static Methods

	    /// <summary>
	    /// Returns the url for the composite file handler for the filePath specified.
	    /// </summary>
	    /// <param name="filePaths"></param>
	    /// <param name="type"></param>
	    /// <param name="http"></param>
	    /// <returns></returns>
	    public static string GetCompositeFileUrl(string filePaths, ClientDependencyType type, HttpContextBase http)
        {
            //build the combined composite list url
            string handler = "{0}/{1}/{2}/0";
            string combinedurl = string.Format(handler, ClientDependencySettings.Instance.CompositeFileHandlerPath, http.Server.UrlEncode(EncodeTo64(filePaths)), type.ToString());
            return combinedurl;
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.Default.GetBytes(toEncode);
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
	    /// on the base64 encoded string.
	    /// </remarks>        
	    /// </summary>
	    /// <param name="dependencies"></param>
	    /// <param name="type"></param>
	    /// <param name="http"></param>
	    /// <returns>An array containing the list of composite file URLs. This will generally only contain 1 value unless
	    /// the number of files registered exceeds the maximum length, then it will return more than one file.</returns>
        public string[] ProcessCompositeList(IEnumerable<IClientDependencyFile> dependencies, ClientDependencyType type, HttpContextBase http)
        {
            if (!dependencies.Any())
                return new string[] { };

            //build the combined composite list urls          
            var files = new List<string>();
            var currBuilder = new StringBuilder();
            var builderCount = 1;
            var stringType = type.ToString();
            foreach (var a in dependencies)
            {
                //if the addition of this file is going to exceed 75% of the max length (since we'll be base64 encoding), we'll need to split
                if ((currBuilder.Length + 
                    a.FilePath.Length + 
                    ClientDependencySettings.Instance.CompositeFileHandlerPath.Length +
                    stringType.Length + 10) >= (CompositeDependencyHandler.MaxHandlerUrlLength * 0.75))
                {
                    //add the current output to the array
                    files.Add(currBuilder.ToString());
                    //create some new output
                    currBuilder = new StringBuilder();
                    builderCount++;
                }

                currBuilder.Append(a.FilePath + ";");
            }

            if (builderCount > files.Count)
            {
                files.Add(currBuilder.ToString());
            }

            //now, compress each url
            for (var i = 0; i < files.Count; i++)
            {
                //append our version to the combined url 
                files[i] = AppendVersionQueryString(GetCompositeFileUrl(files[i], type, http));
            }

            return files.ToArray();
        } 

        #endregion

        #region Protected Methods

	    /// <summary>
	    /// Because we can have both internal and external dependencies rendered, we need to stagger the script tag output... if they are external, we need to stop the compressing/combining
	    /// and write out the external dependency, then resume the compressing/combining handler.
	    /// </summary>
	    /// <param name="dependencies"></param>
	    /// <param name="http"></param>
	    /// <param name="builder"></param>
	    /// <param name="renderCompositeFiles"></param>
	    /// <param name="renderSingle"></param>
	    protected void WriteStaggeredDependencies(IEnumerable<IClientDependencyFile> dependencies, HttpContextBase http, StringBuilder builder, 
            Func<IEnumerable<IClientDependencyFile>, HttpContextBase, string> renderCompositeFiles, 
            Func<string, string> renderSingle)
        {
            var currNonRemoteFiles = new List<IClientDependencyFile>();
            foreach (var f in dependencies)
            {
                //if it is an external resource, then we need to break the sequence
                if (http.IsAbsolutePath(f.FilePath)
                    //remote dependencies aren't local
                    && !new Uri(f.FilePath, UriKind.RelativeOrAbsolute).IsLocalUri(http))
                {
                    //we've encountered an external dependency, so we need to break the sequence and restart it after
                    //we output the raw script tag
                    if (currNonRemoteFiles.Count > 0)
                    {
                        //render the current buffer
                        //builder.Append(RenderJsDependencies(currNonRemoteFiles, http));
                        builder.Append(renderCompositeFiles(currNonRemoteFiles, http));
                        //clear the buffer
                        currNonRemoteFiles.Clear();
                    }
                    //write out the single script tag
                    //builder.Append(RenderSingleJsFile(f.FilePath));
                    builder.Append(renderSingle(f.FilePath));
                }
                else
                {
                    //its a normal registration, add to the buffer
                    currNonRemoteFiles.Add(f);
                }
            }
            //now check if there's anything in the buffer to render
            if (currNonRemoteFiles.Count > 0)
            {
                //render the current buffer
                //builder.Append(RenderJsDependencies(currNonRemoteFiles, http));
                builder.Append(renderCompositeFiles(currNonRemoteFiles, http));
            }
        }

	    /// <summary>
	    /// Ensures the correctly resolved file path is set for each dependency (i.e. so that ~ are taken care of) and also
	    /// prefixes the file path with the correct base path specified for the PathNameAlias if specified.
	    /// </summary>
	    /// <param name="dependencies">The dependencies list for which file paths will be updated</param>
	    /// <param name="folderPaths"></param>
	    /// <param name="http"></param>
	    protected void UpdateFilePaths(IEnumerable<IClientDependencyFile> dependencies, 
            HashSet<IClientDependencyPath> folderPaths, HttpContextBase http)
        {
            foreach (var dependency in dependencies)
            {
                if (!string.IsNullOrEmpty(dependency.PathNameAlias))
                {
                    var paths = folderPaths.ToList();
                    var d = dependency;
                    var path = paths.Find(
                        (p) => p.Name == d.PathNameAlias);
                    if (path == null)
                    {
                        throw new NullReferenceException("The PathNameAlias specified for dependency " + dependency.FilePath + " does not exist in the ClientDependencyPathCollection");
                    }
                    var resolvedPath = path.ResolvePath(http);
                    var basePath = resolvedPath.EndsWith("/") ? resolvedPath : resolvedPath + "/";
                    dependency.FilePath = basePath + dependency.FilePath;
                }
                else
                {
                    dependency.FilePath = dependency.ResolveFilePath(http);
                }

                //append query strings to each file if we are in debug mode
                if (http.IsDebuggingEnabled || !EnableCompositeFiles)
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


            var toKeep = new HashSet<IClientDependencyFile>();

            foreach (var d in dependencies)
            {
                //check if it is a duplicate
                if (dependencies.Where(x => x.FilePath.ToUpper().Trim().Equals(d.FilePath.ToUpper().Trim())).Count() > 1)
                {
                    //find the dups and return an object with the associated index
                    var dups = dependencies
                        .Where(x => x.FilePath.ToUpper().Trim().Equals(d.FilePath.ToUpper().Trim()))
                        .Select((x, index) => new { Index = index, File = x })
                        .ToList();

                    var priorities = dups.Select(x => x.File.Priority).Distinct().ToList();
                    
                    //if there's more than 1 priority defined, we know we need to remove by priority
                    //instead of by index
                    if (priorities.Count() > 1)
                    {
                        toKeep.Add(dups
                            .Where(x => x.File.Priority == priorities.Min())
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
                else
                {
                    //if there's only one found, then just add it to our output
                    toKeep.Add(d);
                }

               
            }

            dependencies.Clear();
            dependencies.AddRange(toKeep);


        }

        #endregion

        #region Private Methods

        private string AppendVersionQueryString(string url)
        {
            if (ClientDependencySettings.Instance.Version == 0)
                return url;
            //the URL should end with a '0'

            url = url.TrimEnd('0') + ClientDependencySettings.Instance.Version.ToString();
            return url;
        } 
        #endregion
		

	}
}
