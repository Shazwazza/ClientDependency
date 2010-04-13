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


        public void RegisterDependencies(List<IClientDependencyFile> allDependencies, 
            HashSet<IClientDependencyPath> paths,
            out string jsOutput,
            out string cssOutput)
        {            
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

            cssOutput = RenderCssDependencies(cssDependencies.ConvertAll<IClientDependencyFile>(a => { return (IClientDependencyFile)a; }));
            jsOutput = RenderJsDependencies(jsDependencies.ConvertAll<IClientDependencyFile>(a => { return (IClientDependencyFile)a; }));         
        }
    }
}

