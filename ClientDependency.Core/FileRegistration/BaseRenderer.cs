using System.Collections.Generic;
using System.Web;

namespace ClientDependency.FileRegistration
{
    
    public abstract class BaseRenderer : BaseFileRegistrationProvider
    {
        public virtual void RegisterDependencies(List<IClientDependencyFile> allDependencies,
            HashSet<IClientDependencyPath> paths,
            out string jsOutput,
            out string cssOutput,
            HttpContext http)
        {
            WriteDependencies(allDependencies, paths, out jsOutput, out cssOutput, http);
        }
    }
}

