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
        public void RegisterDependencies(Control dependantControl, ClientDependencyCollection dependencies, HashSet<IClientDependencyPath> paths)
        {
            DependantControl = dependantControl;
            AllDependencies = new List<IClientDependencyFile>(dependencies);
            FolderPaths = paths;

            UpdateFilePaths();

            List<IClientDependencyFile> jsDependencies = AllDependencies
                .Where(x => x.DependencyType == ClientDependencyType.Javascript)
                .ToList();

            List<IClientDependencyFile> cssDependencies = AllDependencies
                .Where(x => x.DependencyType == ClientDependencyType.Css)
                .ToList();

            // sort by priority
            jsDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            cssDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            RegisterCssFiles(cssDependencies.ConvertAll<IClientDependencyFile>(a => { return (IClientDependencyFile)a; }));
            RegisterJsFiles(jsDependencies.ConvertAll<IClientDependencyFile>(a => { return (IClientDependencyFile)a; }));

        }

        protected Control DependantControl { get; set; }
    }
}
