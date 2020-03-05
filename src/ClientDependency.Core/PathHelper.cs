using System;
using System.IO;
using System.Web;

namespace ClientDependency.Core
{
    internal class PathHelper
    {
        public static bool TryGetFileExtension(string filePath, out string extension)
        {
            try
            {
                extension = Path.GetExtension(filePath);
                return true;
            }
            catch (ArgumentException)
            {
                extension = null;
                return false;
            }
        }

        public static bool TryGetFileInfo(string path, HttpContextBase http, out FileInfo fileInfo)
        {
            if (!TryMapPath(path, http, out var mapped))
            {
                fileInfo = null;
                return false;
            }

            try
            {
                fileInfo = new FileInfo(mapped);
                return fileInfo.Exists;
            }
            catch (Exception)
            {
                fileInfo = null;
                return false;
            }
        }

        public static bool TryMapPath(string path, HttpContextBase http, out string mappedPath)
        {
            try
            {
                mappedPath = http.Server.MapPath(path);
                return true;
            }
            catch (Exception)
            {
                mappedPath = null;
                return false;
            }

        }
    }
}
