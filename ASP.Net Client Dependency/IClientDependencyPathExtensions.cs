using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ClientDependency.Core
{
    public static class IClientDependencyPathExtensions
    {

        public static string ResolvePath(this IClientDependencyPath path)
        {
            if (string.IsNullOrEmpty(path.Path))
            {
                throw new ArgumentException("The Path specified is null", "Path");
            }
            if (path.Path[0] == '~')
            {
                return HttpContext.Current.ResolveUrl(path.Path);
            }
            return path.Path;            
        }

    }
}
