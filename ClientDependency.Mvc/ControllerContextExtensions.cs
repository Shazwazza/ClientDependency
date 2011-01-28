using System.Web.Mvc;

namespace ClientDependency.Core.Mvc
{
    public static class ControllerContextExtensions
    {
        public static DependencyRenderer GetLoader(this ControllerContext cc)
        {
            bool isNew;
            var instance = DependencyRenderer.TryCreate(cc.HttpContext, out isNew);
            return instance;
        }
    }
}