using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ClientDependency.Core.Controls;

namespace ClientDependency.Core
{
    /// <summary>
    /// Allows creating pre-defined bundles for scrips and styles
    /// </summary>
    public static class BundleManager
    {
        static BundleManager()
        {
            GetHttpContextDelegate = () => new HttpContextWrapper(HttpContext.Current);
        }

        internal static Func<HttpContextBase> GetHttpContextDelegate { get; set; }

        private static readonly Dictionary<BundleDefinition, IEnumerable<IClientDependencyFile>> Bundles = new Dictionary<BundleDefinition, IEnumerable<IClientDependencyFile>>();

        internal static void ClearBundles()
        {
            lock (Bundles)
            {
                Bundles.Clear();
            }
        }

        /// <summary>
        /// Returns all Css bundles registered
        /// </summary>
        /// <returns></returns>
        internal static IDictionary<BundleDefinition, IEnumerable<IClientDependencyFile>> GetCssBundles()
        {
            IEnumerable<KeyValuePair<BundleDefinition, IEnumerable<IClientDependencyFile>>> found;
            lock (Bundles)
            {
                found = Bundles.Where(x => x.Key.Type == ClientDependencyType.Css).ToArray();
            }
            return found.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Returns a css bundle by name
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        internal static BundleResult GetCssBundle(string bundleName)
        {
            IEnumerable<KeyValuePair<BundleDefinition, IEnumerable<IClientDependencyFile>>> found;
            lock (Bundles)
            {
                found = Bundles.Where(x => x.Key.Type == ClientDependencyType.Css && x.Key.Name.Equals(bundleName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            }
            if (!found.Any()) return null;
            var b = found.First();
            return new BundleResult {Definition = b.Key, Files = b.Value};
        }

        /// <summary>
        /// Returns all js bundles registered
        /// </summary>
        /// <returns></returns>
        internal static IDictionary<BundleDefinition, IEnumerable<IClientDependencyFile>> GetJsBundles()
        {
            IEnumerable<KeyValuePair<BundleDefinition, IEnumerable<IClientDependencyFile>>> found; 
            lock (Bundles)
            {
                found = Bundles.Where(x => x.Key.Type == ClientDependencyType.Javascript).ToArray();
            }
            return found.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Returns a js bundle by name
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        internal static BundleResult GetJsBundle(string bundleName)
        {
            IEnumerable<KeyValuePair<BundleDefinition, IEnumerable<IClientDependencyFile>>> found;
            lock (Bundles)
            {
                found = Bundles.Where(x => x.Key.Type == ClientDependencyType.Javascript && x.Key.Name.Equals(bundleName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            }
            if (!found.Any()) return null;
            var b = found.First();
            return new BundleResult { Definition = b.Key, Files = b.Value };
        }

        #region CreateCssBundle
        public static void CreateCssBundle(string name, params CssFile[] files)
        {
            lock (Bundles)
            {
                //Bundles.AddOrUpdate(new BundleDefinition(ClientDependencyType.Css, name), s => OrderFiles(files), (s, enumerable) => OrderFiles(files));
                Bundles[new BundleDefinition(ClientDependencyType.Css, name)] = OrderFiles(files);
            }
        }

        /// <summary>
        /// Creates a pre-defined bundle and sets the priority for each file supplied, unless any of the files already have a priority set
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <param name="files"></param>
        public static void CreateCssBundle(string name, int priority, params CssFile[] files)
        {
            //set priorities on files that don't have one set already
            var filesToChange = files.Where(x => x.Priority == Constants.DefaultPriority);
            foreach (var f in filesToChange)
            {
                f.Priority = priority;
            }

            lock (Bundles)
            {
                //Bundles.AddOrUpdate(new BundleDefinition(ClientDependencyType.Css, name), s => OrderFiles(files), (s, enumerable) => OrderFiles(files));
                Bundles[new BundleDefinition(ClientDependencyType.Css, name)] = OrderFiles(files);
            }
        }

        /// <summary>
        /// Creates a pre-defined bundle and sets the priority and group for each file supplied, unless any of the files already have a priority/group set
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <param name="group"></param>
        /// <param name="files"></param>
        public static void CreateCssBundle(string name, int priority, int group, params CssFile[] files)
        {
            //set priorities on files that don't have one set already
            var filesToChange = files.Where(x => x.Priority == Constants.DefaultPriority);
            foreach (var f in filesToChange)
            {
                f.Priority = priority;
            }
            //set groups on files that don't have one set already
            filesToChange = files.Where(x => x.Group == Constants.DefaultGroup);
            foreach (var f in filesToChange)
            {
                f.Group = group;
            }

            lock (Bundles)
            {
                //Bundles.AddOrUpdate(new BundleDefinition(ClientDependencyType.Css, name), s => OrderFiles(files), (s, enumerable) => OrderFiles(files));
                Bundles[new BundleDefinition(ClientDependencyType.Css, name)] = OrderFiles(files);
            }
        }  
        #endregion

        #region CreateJsBundle
        public static void CreateJsBundle(string name, params JavascriptFile[] files)
        {
            lock (Bundles)
            {
                //Bundles.AddOrUpdate(new BundleDefinition(ClientDependencyType.Javascript, name), s => OrderFiles(files), (s, enumerable) => OrderFiles(files));
                Bundles[new BundleDefinition(ClientDependencyType.Javascript, name)] = OrderFiles(files);
            }
        }

        public static void CreateJsBundle(string name, int priority, params JavascriptFile[] files)
        {
            lock (Bundles)
            {
                //Bundles.AddOrUpdate(new BundleDefinition(ClientDependencyType.Javascript, name), s => OrderFiles(files), (s, enumerable) => OrderFiles(files));
                Bundles[new BundleDefinition(ClientDependencyType.Javascript, name)] = OrderFiles(files);
            }
        }

        public static void CreateJsBundle(string name, int priority, int group, params JavascriptFile[] files)
        {
            lock (Bundles)
            {
                //Bundles.AddOrUpdate(new BundleDefinition(ClientDependencyType.Javascript, name), s => OrderFiles(files), (s, enumerable) => OrderFiles(files));
                Bundles[new BundleDefinition(ClientDependencyType.Javascript, name)] = OrderFiles(files);
            }
        } 
        #endregion
        
        /// <summary>
        /// This will order the files
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private static IEnumerable<IClientDependencyFile> OrderFiles(IList<IClientDependencyFile> files)
        {
            return !files.Any() ? files : DependencySorter.SortItems(files);
        }
    }
}
