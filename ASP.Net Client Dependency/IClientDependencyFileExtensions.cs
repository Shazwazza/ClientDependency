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
        /// <param name="http"></param>
        /// <returns></returns>
        public static string ResolveFilePath(this IClientDependencyFile file, HttpContextBase http)
        {
            if (string.IsNullOrEmpty(file.FilePath))
            {
                throw new ArgumentException("The Path specified is null", "Path");
            }
            if (file.FilePath[0] == '~')
            {
                return http.ResolveUrl(file.FilePath);
            }
            if (!http.IsAbsolute(file.FilePath))
            {
                //get the relative path
                var path = http.Request.AppRelativeCurrentExecutionFilePath.Substring(0, http.Request.AppRelativeCurrentExecutionFilePath.LastIndexOf('/') + 1);
                return http.ResolveUrl(path + file.FilePath);
            }
            return file.FilePath;
        }

        

    }
}
