using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ClientDependency.Core.Mvc
{
    public static class ViewContextExtensions
    {
        public static DependencyRenderer GetLoader(this ViewContext vc)
        {
            bool isNew;
            var instance = DependencyRenderer.TryCreate(vc.HttpContext, out isNew);
            return instance;
        }
    }
}
