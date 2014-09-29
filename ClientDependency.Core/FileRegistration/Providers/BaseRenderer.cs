using System.Collections.Generic;
using System.Web;

namespace ClientDependency.FileRegistration.Providers
{
    
    public abstract class BaseRenderer : BaseFileRegistrationProvider
    {
        public virtual void RegisterDependencies(List<IClientDependencyFile> allDependencies,
            HashSet<IClientDependencyPath> paths,
            out string jsOutput,
            out string cssOutput,
            HttpContextBase http)
        {
            WriteDependencies(allDependencies, paths, out jsOutput, out cssOutput, http);
        }
    }
}

