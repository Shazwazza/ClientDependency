using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ClientDependency.Core.CompositeFiles;

namespace ClientDependency.Core
{
    internal static class CssHelper
    {
        private static readonly Regex ImportCssRegex = new Regex(@"@import url\((.+?)\);?", RegexOptions.Compiled);
        private static readonly Regex CssUrlRegex = new Regex(@"url\(((?![""']?data:|[""']?#).+?)\)", RegexOptions.Compiled);

        /// <summary>
        /// Returns the paths for the import statements and the resultant original css without the import statements
        /// </summary>
        /// <param name="content">The original css contents</param>
        /// <param name="importedPaths"></param>
        /// <returns></returns>
        public static string ParseImportStatements(string content, out IEnumerable<string> importedPaths)
        {
            IEnumerable<string> externalImports;
            return ParseImportStatements(content, out importedPaths, out externalImports);
        }

        /// <summary>
        /// Returns the paths for the import statements and the resultant original css without the import statements
        /// </summary>
        /// <param name="content">The original css contents</param>
        /// <param name="importedPaths"></param>
        /// <param name="externalImports">Will contain every external CSS import call that was extracted from the content</param>
        /// <returns></returns>
        public static string ParseImportStatements(string content, out IEnumerable<string> importedPaths, out IEnumerable<string> externalImports)
        {
            var pathsFound = new List<string>();
            var externalImportsFound = new List<string>();
            var matches = ImportCssRegex.Matches(content);
            foreach (Match match in matches)
            {
                var urlMatch = CssUrlRegex.Match(match.Value);
                bool isExternal = false;
                if (urlMatch.Success && urlMatch.Groups.Count >= 2)
                {
                    var path = urlMatch.Groups[1].Value.Trim('\'', '"');
                    if ((path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                         || path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                         || path.StartsWith("//", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        externalImportsFound.Add(path);
                        isExternal = true;
                    }
                }

                //Strip the import statement                
                content = content.ReplaceFirst(match.Value, "");

                if (isExternal)
                    continue;

                //write import css content
                var filePath = match.Groups[1].Value.Trim('\'', '"');
                pathsFound.Add(filePath);
            }

            importedPaths = pathsFound;
            externalImports = externalImportsFound;
            return content.Trim();
        }

        /// <summary>
        /// Returns the CSS file with all of the url's formatted to be absolute locations
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="url"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        public static string ReplaceUrlsWithAbsolutePaths(string fileContents, string url, HttpContextBase http)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            fileContents = CssHelper.ReplaceUrlsWithAbsolutePaths(fileContents, uri.MakeAbsoluteUri(http));
            return fileContents;
        }

        /// <summary>
        /// Returns the CSS file with all of the url's formatted to be absolute locations
        /// </summary>
        /// <param name="fileContent">content of the css file</param>
        /// <param name="cssLocation">the uri location of the css file</param>
        /// <returns></returns>
        public static string ReplaceUrlsWithAbsolutePaths(string fileContent, Uri cssLocation)
        {
            var str = CssUrlRegex.Replace(fileContent, m =>
                {
                    if (m.Groups.Count == 2)
                    {
                        var match = m.Groups[1].Value.Trim('\'', '"');
                        var hashSplit = match.Split(new[] {'#'}, StringSplitOptions.RemoveEmptyEntries);

                        return string.Format(@"url(""{0}{1}"")",
                                             (match.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                                             || match.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                                             || match.StartsWith("//", StringComparison.InvariantCultureIgnoreCase)) ? match : new Uri(cssLocation, match).PathAndQuery,
                                             hashSplit.Length > 1 ? ("#" + hashSplit[1]) : "");
                    }
                    return m.Value;
                });            

            return str;
        }

        /// <summary>
        /// Minifies Css from a string input
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string MinifyCss(string body)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(body);
                writer.Flush();
                return MinifyCss(ms);
            }
        }

        /// <summary>
        /// Minifies CSS from a stream input
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string MinifyCss(Stream stream)
        {
            var cssMinify = new CssMinifier();
            if (!stream.CanRead) throw new InvalidOperationException("Cannot read input stream");
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            return cssMinify.Minify(new StreamReader(stream));
        }
    }
}