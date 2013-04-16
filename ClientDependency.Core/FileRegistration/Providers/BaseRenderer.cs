using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using ClientDependency.Core.Config;
using ClientDependency.Core.FileRegistration.Providers;
using System.IO;
using System.Web;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public abstract class BaseRenderer : BaseFileRegistrationProvider
    {

        public virtual void RegisterDependencies(List<IClientDependencyFile> allDependencies,
            HashSet<IClientDependencyPath> paths,
            out string jsOutput,
            out string cssOutput,
            HttpContextBase http)
        {
            //we may have already processed this so don't do it again
            if (http.Items["BaseRenderer.RegisterDependencies"] == null)
            {
                http.Items["BaseRenderer.RegisterDependencies"] = true;

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
            
            cssOutput = cssBuilder.ToString();
            jsOutput = jsBuilder.ToString();
        }
    }
}

