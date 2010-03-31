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
        /// <param name="dependantControl"></param>
        /// <param name="js"></param>
        /// <param name="css"></param>
        protected abstract void RegisterDependencies(Control dependantControl, string js, string css);

        /// <summary>
        /// Called to register the dependencies into the page/control/output
        /// </summary>
        /// <param name="dependantControl"></param>
        /// <param name="dependencies"></param>
        /// <param name="paths"></param>
        public void RegisterDependencies(Control dependantControl, ClientDependencyCollection dependencies, HashSet<IClientDependencyPath> paths)
        {
            var ctl = dependantControl;
            var allDependencies = new List<IClientDependencyFile>(dependencies);
            var folderPaths = paths;

            UpdateFilePaths(allDependencies, folderPaths);
            EnsureNoDuplicates(allDependencies, folderPaths);

            List<IClientDependencyFile> jsDependencies = allDependencies
                .Where(x => x.DependencyType == ClientDependencyType.Javascript)
                .ToList();

            List<IClientDependencyFile> cssDependencies = allDependencies
                .Where(x => x.DependencyType == ClientDependencyType.Css)
                .ToList();

            // sort by priority
            jsDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            cssDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            string cssOutput = RenderCssDependencies(cssDependencies.ConvertAll<IClientDependencyFile>(a => { return (IClientDependencyFile)a; }));
            string jsOutput = RenderJsDependencies(jsDependencies.ConvertAll<IClientDependencyFile>(a => { return (IClientDependencyFile)a; }));

            RegisterDependencies(dependantControl, jsOutput, cssOutput);
        }        
    }
}
