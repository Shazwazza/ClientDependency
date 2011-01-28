using System.Web;

namespace ClientDependency.Core.Mvc
{
    public static class HttpContextBaseExtensions
    {
        public static DependencyRenderer GetLoader(this HttpContextBase http)
        {
            bool isNew;
            var instance = DependencyRenderer.TryCreate(http, out isNew);
            return instance;
        }
    }
}