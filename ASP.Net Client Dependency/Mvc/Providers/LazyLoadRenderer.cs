using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.Mvc.Providers
{
    public class LazyLoadRenderer : BaseRenderer
    {
        protected override string RenderJsDependencies(List<IClientDependencyFile> jsDependencies)
        {
            throw new NotImplementedException();
        }

        protected override string RenderCssDependencies(List<IClientDependencyFile> cssDependencies)
        {
            throw new NotImplementedException();
        }

        protected override string RenderSingleJsFile(string js)
        {
            throw new NotImplementedException();
        }

        protected override string RenderSingleCssFile(string css)
        {
            throw new NotImplementedException();
        }
    }
}
