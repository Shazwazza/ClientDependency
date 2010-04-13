using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ClientDependency.Core
{
    public static class IClientDependencyFileExtensions
    {

        public static string ResolveFilePath(this IClientDependencyFile file)
        {
            if (string.IsNullOrEmpty(file.FilePath))
            {
                throw new ArgumentException("The Path specified is null", "Path");
            }
            if (file.FilePath[0] == '~')
            {
                return VirtualPathUtility.ToAbsolute(file.FilePath, HttpContext.Current.Request.ApplicationPath);
            }
            return file.FilePath;
        }

    }
}
