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
    public abstract class WebFormsFileRegistrationProvider : BaseFileRegistrationProvider
    {
        /// <summary>
        /// Called to register the js and css into the page/control/output.
        /// </summary>
        /// <param name="http"></param>
        /// <param name="js"></param>
        /// <param name="css"></param>
        protected abstract void RegisterDependencies(HttpContextBase http, string js, string css);

        /// <summary>
        /// Called to register the dependencies into the page/control/output
        /// </summary>
        /// <param name="dependantControl"></param>
        /// <param name="allDependencies"></param>
        /// <param name="paths"></param>
        /// <param name="http"></param>
        public void RegisterDependencies(
            Control dependantControl, 
            List<IClientDependencyFile> allDependencies, 
            HashSet<IClientDependencyPath> paths, 
            HttpContextBase http)
        {

            //we may have already processed this so don't do it again
            if (http.Items["WebFormsFileRegistrationProvider.RegisterDependencies"] == null)
            {
                http.Items["WebFormsFileRegistrationProvider.RegisterDependencies"] = true;

                var folderPaths = paths;

                UpdateFilePaths(allDependencies, folderPaths, http);
                EnsureNoDuplicates(allDependencies, folderPaths);
            }

			var cssBuilder = new StringBuilder();
			var jsBuilder = new StringBuilder();

            //group by the group and order by the value
            foreach (var group in allDependencies.GroupBy(x => x.Group).OrderBy(x => x))
            {
                //sort both the js and css dependencies properly

                var jsDependencies = DependencySorter.SortItems(
                    group.Where(x => x.DependencyType == ClientDependencyType.Javascript).ToList());

                var cssDependencies = DependencySorter.SortItems(
                    allDependencies.Where(x => x.DependencyType == ClientDependencyType.Css).ToList());

                //render
                WriteStaggeredDependencies(cssDependencies, http, cssBuilder, RenderCssDependencies, RenderSingleCssFile);
                WriteStaggeredDependencies(jsDependencies, http, jsBuilder, RenderJsDependencies, RenderSingleJsFile);
            }

			var cssOutput = cssBuilder.ToString();
			var jsOutput = jsBuilder.ToString();
            RegisterDependencies(http, jsOutput, cssOutput);
        }
    }
}
