using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using ClientDependency.Core.FileRegistration.Providers;
using System.IO;

namespace ClientDependency.Core.Mvc.Providers
{
    public abstract class BaseRenderer : BaseFileRegistrationProvider
    {
        public BaseRenderer()
        {
            JsOutput = new StringBuilder();
            CssOutput = new StringBuilder();
        }

        public void RegisterDependencies(ClientDependencyCollection dependencies, HashSet<IClientDependencyPath> paths)
        {
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

        public StringBuilder JsOutput { get; protected set; }
        public StringBuilder CssOutput { get; protected set; }
        


    }
}

