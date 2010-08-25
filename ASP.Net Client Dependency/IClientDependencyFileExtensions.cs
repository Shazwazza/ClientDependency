using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ClientDependency.Core
{
    public static class IClientDependencyFileExtensions
    {

        /// <summary>
        /// Resolves an absolute web path for the file path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ResolveFilePath(this IClientDependencyFile file)
        {
            if (string.IsNullOrEmpty(file.FilePath))
            {
                throw new ArgumentException("The Path specified is null", "Path");
            }
            if (file.FilePath[0] == '~')
            {
                return HttpContext.Current.ResolveUrl(file.FilePath);
            }
            else if (!HttpContext.Current.IsAbsolute(file.FilePath))
            {
                //get the relative path
                var path = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.Substring(0, HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.LastIndexOf('/') + 1);
                return HttpContext.Current.ResolveUrl(path + file.FilePath);
            }
            return file.FilePath;
        }

        

    }
}
