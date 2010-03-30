using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.Mvc.Providers
{
    public class LazyLoadRenderer : BaseRenderer
    {
        protected override void RegisterJsFiles(List<IClientDependencyFile> jsDependencies)
        {
            throw new NotImplementedException();
        }

        protected override void RegisterCssFiles(List<IClientDependencyFile> cssDependencies)
        {
            throw new NotImplementedException();
        }

        protected override void ProcessSingleJsFile(string js)
        {
            throw new NotImplementedException();
        }

        protected override void ProcessSingleCssFile(string css)
        {
            throw new NotImplementedException();
        }
    }
}
