using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using ClientDependency.Core.FileRegistration.Providers;
using System.IO;
using System.Web;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public abstract class BaseRenderer : BaseFileRegistrationProvider
    {


        public void RegisterDependencies(List<IClientDependencyFile> allDependencies, 
            HashSet<IClientDependencyPath> paths,
            out string jsOutput,
            out string cssOutput,
            HttpContextBase http)
        {            
            var folderPaths = paths;

            UpdateFilePaths(allDependencies, folderPaths, http);
            EnsureNoDuplicates(allDependencies, folderPaths);

            var jsDependencies = allDependencies
                .Where(x => x.DependencyType == ClientDependencyType.Javascript)
                .ToList();

            var cssDependencies = allDependencies
                .Where(x => x.DependencyType == ClientDependencyType.Css)
                .ToList();

            // sort by priority
            jsDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            cssDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            cssOutput = RenderCssDependencies(cssDependencies.ConvertAll(a => a), http);
            jsOutput = RenderJsDependencies(jsDependencies.ConvertAll(a => a), http);         
        }
    }
}

