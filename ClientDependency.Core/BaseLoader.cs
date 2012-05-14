using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using System.Configuration.Provider;
using System.Runtime.CompilerServices;

//Make a 'friend' to mvc app
[assembly: InternalsVisibleTo("ClientDependency.Core.Mvc")]

namespace ClientDependency.Core
{
    
    public class BaseLoader
    {

        public BaseLoader(HttpContextBase http)
        {
            CurrentContext = http;
        }

        protected HttpContextBase CurrentContext { get; private set; }

        public BaseFileRegistrationProvider Provider { get; set; }

        /// <summary>
        /// Tracks all dependencies and maintains a deduplicated list
        /// </summary>
        internal List<ProviderDependencyList> Dependencies = new List<ProviderDependencyList>();
        /// <summary>
        /// Tracks all paths and maintains a deduplicated list
        /// </summary>
        internal HashSet<IClientDependencyPath> Paths = new HashSet<IClientDependencyPath>();

        /// <summary>
        /// Adds a path to the current loader
        /// </summary>
        /// <param name="pathNameAlias"></param>
        /// <param name="path"></param>
        /// <returns>Returns the current loader instance so you can chain calls together</returns>
        public BaseLoader AddPath(string pathNameAlias, string path)
        {
            AddPath(new BasicPath() { Name = pathNameAlias, Path = path });
            return this;
        }

        /// <summary>
        /// Adds a path to the current loader
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns the current loader instance so you can chain calls together</returns>
        public BaseLoader AddPath(IClientDependencyPath path)
        {
            Paths.Add(path);
            return this;
        }		

        /// <summary>
        /// Registers dependencies with the specified provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="dependencies"></param>
        /// <param name="paths"></param>
        /// <param name="currProviders"></param>
        /// <remarks>
        /// This is the top most overloaded method
        /// </remarks>
        public void RegisterClientDependencies(BaseFileRegistrationProvider provider, IEnumerable<IClientDependencyFile> dependencies, IEnumerable<IClientDependencyPath> paths, ProviderCollection currProviders)
        {
            //find or create the ProviderDependencyList for the provider
            ProviderDependencyList currList = Dependencies
                .Where(x => x.ProviderIs(provider))
                .DefaultIfEmpty(new ProviderDependencyList(provider))
                .SingleOrDefault();
            
            //add the dependencies that don't have a provider specified
            currList.AddDependencies(dependencies
                .Where(x => string.IsNullOrEmpty(x.ForceProvider)));
            
            //add the list if it is new
            if (!Dependencies.Contains(currList) && currList.Dependencies.Count > 0)
                Dependencies.Add(currList); 

            //we need to look up all of the dependencies that have forced providers, 
            //check if we've got a provider list for it, create one if not and add the dependencies
            //to it.
            var allProviderNamesInList = dependencies
                .Select(x => x.ForceProvider)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct();
            var forceProviders = (from provName in allProviderNamesInList
                                  where currProviders[provName] != null
                                  select (BaseFileRegistrationProvider) currProviders[provName]).ToList();
            foreach (var prov in forceProviders)
            {
                //find or create the ProviderDependencyList for the prov
                var p = prov;
                var forceList = Dependencies
                    .Where(x => x.ProviderIs(prov))
                    .DefaultIfEmpty(new ProviderDependencyList(prov))
                    .SingleOrDefault();
                //add the dependencies that don't have a force provider specified
                forceList.AddDependencies(dependencies
                    .Where(x => x.ForceProvider == p.Name));
                //add the list if it is new
                if (!Dependencies.Contains(forceList))
                    Dependencies.Add(forceList);
            }

            //add the paths, ensure no dups
            Paths.UnionWith(paths);
        }

        public void RegisterClientDependencies(List<IClientDependencyFile> dependencies, params IClientDependencyPath[] paths)
        {
            //We will combine both the MVC and web forms providers here to pass in to the method since this method could be executing
            //under the webforms context or the mvc context. This list as a parameter is used only for forced providers and wont
            //matter if it includes the webforms and mvc ones together since they will only ever render when they are in their own
            //context. This is better than checking if it is a System.Web.UI.Page handler currently IMO, plus the provider names
            //between mvc and webforms are generally different.
            var combinedCollection = new ProviderCollection();
            foreach (ProviderBase p in ClientDependencySettings.Instance.MvcRendererCollection)
                combinedCollection.Add(p);
            foreach (ProviderBase p in ClientDependencySettings.Instance.FileRegistrationProviderCollection)
                combinedCollection.Add(p);

            RegisterClientDependencies(Provider, dependencies, paths, combinedCollection);
        }

        public void RegisterClientDependencies(List<IClientDependencyFile> dependencies, IEnumerable<IClientDependencyPath> paths)
        {
            //We will combine both the MVC and web forms providers here to pass in to the method since this method could be executing
            //under the webforms context or the mvc context. This list as a parameter is used only for forced providers and wont
            //matter if it includes the webforms and mvc ones together since they will only ever render when they are in their own
            //context. This is better than checking if it is a System.Web.UI.Page handler currently IMO, plus the provider names
            //between mvc and webforms are generally different.
            var combinedCollection = new ProviderCollection();
            foreach (ProviderBase p in ClientDependencySettings.Instance.MvcRendererCollection)
                combinedCollection.Add(p);
            foreach (ProviderBase p in ClientDependencySettings.Instance.FileRegistrationProviderCollection)
                combinedCollection.Add(p);

            RegisterClientDependencies(Provider, dependencies, paths, combinedCollection);
        }



        #region RegisterDependency overloads

        public void RegisterDependency(string filePath, ClientDependencyType type)
        {
            RegisterDependency(filePath, "", type);
        }

        public void RegisterDependency(string filePath, ClientDependencyType type, object htmlAttributes)
        {
            RegisterDependency(filePath, "", type, htmlAttributes);
        }

        public void RegisterDependency(int priority, string filePath, ClientDependencyType type)
        {
            RegisterDependency(priority, filePath, "", type);
        }

		public void RegisterDependency(int group, int priority, string filePath, ClientDependencyType type)
		{
			RegisterDependency(group, priority, filePath, "", type);
		}
		
		public void RegisterDependency(int priority, string filePath, ClientDependencyType type, object htmlAttributes)
        {
            RegisterDependency(priority, filePath, "", type, htmlAttributes);
        }

		public void RegisterDependency(int group, int priority, string filePath, ClientDependencyType type, object htmlAttributes)
		{
			RegisterDependency(group, priority, filePath, "", type, htmlAttributes);
		}
		
		public void RegisterDependency(string filePath, string pathNameAlias, ClientDependencyType type)
        {
            RegisterDependency(Constants.DefaultPriority, filePath, pathNameAlias, type);
        }

        public void RegisterDependency(string filePath, string pathNameAlias, ClientDependencyType type, object htmlAttributes)
        {
            RegisterDependency(Constants.DefaultPriority, filePath, pathNameAlias, type, htmlAttributes);
        }

        public void RegisterDependency(int priority, string filePath, string pathNameAlias, ClientDependencyType type)
        {
            RegisterDependency(Constants.DefaultGroup, priority, filePath, pathNameAlias, type);
        }

        public void RegisterDependency(int priority, string filePath, string pathNameAlias, ClientDependencyType type, object htmlAttributes)
        {
            RegisterDependency(Constants.DefaultGroup, priority, filePath, pathNameAlias, type, htmlAttributes);
        }

		// those two methods below actually do the work
		// everything else above is just overloads

		/// <summary>
		/// Dynamically registers a dependency into the loader at runtime.
		/// This is similar to ScriptManager.RegisterClientScriptInclude.
		/// Registers a file dependency with the default provider.
		/// </summary>
		/// <param name="group">The dependencies group identifier.</param>
		/// <param name="priority">The dependency priority.</param>
		/// <param name="filePath">The dependency file.</param>
		/// <param name="pathNameAlias">The dependency files path alias.</param>
		/// <param name="type">The type of the dependency.</param>
		public void RegisterDependency(int group, int priority, string filePath, string pathNameAlias, ClientDependencyType type)
        {
            var file = new BasicFile(type) { Group = group, Priority = priority, FilePath = filePath, PathNameAlias = pathNameAlias };
            RegisterClientDependencies(new List<IClientDependencyFile> { file }, new List<IClientDependencyPath>()); //send an empty paths collection
        }

		/// <summary>
		/// Dynamically registers a dependency into the loader at runtime.
		/// This is similar to ScriptManager.RegisterClientScriptInclude.
		/// Registers a file dependency with the default provider.
		/// </summary>
		/// <param name="group">The dependencies group identifier.</param>
		/// <param name="priority">The dependency priority.</param>
		/// <param name="filePath">The dependency file.</param>
		/// <param name="pathNameAlias">The dependency files path alias.</param>
		/// <param name="type">The type of the dependency.</param>
		/// <param name="htmlAttributes"></param>
		public void RegisterDependency(int group, int priority, string filePath, string pathNameAlias, ClientDependencyType type, object htmlAttributes)
        {
            var file = new BasicFile(type) { Group = group, Priority = priority, FilePath = filePath, PathNameAlias = pathNameAlias };

            //now add the attributes to the list
            foreach(var d in htmlAttributes.ToDictionary())
            {
                file.HtmlAttributes.Add(d.Key, d.Value.ToString());
            }

            RegisterClientDependencies(new List<IClientDependencyFile> { file }, new List<IClientDependencyPath>()); //send an empty paths collection
        } 

        #endregion


    }
}
