using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using ClientDependency.Core.Config;
using System.IO;
using System.Web;
using System.Net;
using System.IO.Compression;
using System.Configuration.Provider;
using ClientDependency.Core.CompositeFiles;

namespace ClientDependency.Core.CompositeFiles.Providers
{

	/// <summary>
	/// A provider for combining, minifying, compressing and saving composite scripts/css files
	/// </summary>
	public class CompositeFileProcessingProvider : BaseCompositeFileProcessingProvider
	{

	    public const string DefaultName = "CompositeFileProcessor";
		

	    /// <summary>
	    /// Saves the file's bytes to disk with a hash of the byte array
	    /// </summary>
	    /// <param name="fileContents"></param>
	    /// <param name="type"></param>
	    /// <param name="server"></param>
	    /// <returns>The new file path</returns>
	    /// <remarks>
	    /// the extension will be: .cdj for JavaScript and .cdc for CSS
	    /// </remarks>
	    public override FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type, HttpServerUtilityBase server)
		{
            //don't save the file if composite files are disabled.
            if (!PersistCompositeFiles)
                return null;

            if (!CompositeFilePath.Exists)
                CompositeFilePath.Create();
			
            var fi = new FileInfo(
                Path.Combine(CompositeFilePath.FullName,
					ClientDependencySettings.Instance.Version + "_"
                        + Guid.NewGuid().ToString("N") + ".cd" + type.ToString().Substring(0, 1).ToUpper()));
			
            if (fi.Exists)
				fi.Delete();
			
            var fs = fi.Create();
			fs.Write(fileContents, 0, fileContents.Length);
			fs.Close();
			return fi;
		}

	    /// <summary>
	    /// combines all files to a byte array
	    /// </summary>
	    /// <param name="filePaths"></param>
	    /// <param name="context"></param>
	    /// <param name="type"></param>
	    /// <param name="fileDefs"></param>
	    /// <returns></returns>
	    public override byte[] CombineFiles(string[] filePaths, HttpContextBase context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs)
		{
			var fDefs = new List<CompositeFileDefinition>();

			var ms = new MemoryStream(5000);            
            var sw = new StreamWriter(ms, Encoding.UTF8);

			foreach (var s in filePaths)
			{
			    var def = WritePathToStream(type, s, context, sw);
                if (def != null)
                {
                    fDefs.Add(def);
                }
			}
			sw.Flush();
			byte[] outputBytes = ms.ToArray();
			sw.Close();
			ms.Close();
			fileDefs = fDefs;
			return outputBytes;
		}

		/// <summary>
		/// Compresses the bytes if the browser supports it
		/// </summary>
		public override byte[] CompressBytes(CompressionType type, byte[] fileBytes)
		{
            return SimpleCompressor.CompressBytes(type, fileBytes);
		}

        /// <summary>
        /// Writes a given path to the stream
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path">The path could be a local url or an absolute url</param>
        /// <param name="context"></param>
        /// <param name="sw"></param>
        /// <returns>If successful returns a CompositeFileDefinition, otherwise returns null</returns>
        private CompositeFileDefinition WritePathToStream(ClientDependencyType type, string path, HttpContextBase context, StreamWriter sw)
        {
            CompositeFileDefinition def = null;
            if (!string.IsNullOrEmpty(path))
            {                
                try
                {
                    var fi = new FileInfo(context.Server.MapPath(path));
                    if (ClientDependencySettings.Instance.FileBasedDependencyExtensionList.Contains(fi.Extension.ToUpper()))
                    {
                        //if the file doesn't exist, then we'll assume it is a URI external request
                        def = !fi.Exists 
                            ? WriteFileToStream(sw, path, type, context) //external request
                            : WriteFileToStream(sw, fi, type, path, context); //internal request
                    }
                    else
                    {
                        //if it's not a file based dependency, try to get the request output.
                        def = WriteFileToStream(sw, path, type, context);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is NotSupportedException
                        || ex is ArgumentException
                        || ex is HttpException)
                    {
                        //could not parse the string into a fileinfo or couldn't mappath, so we assume it is a URI
                        def = WriteFileToStream(sw, path, type, context);
                    }
                    else
                    {
                        //if this fails, log the exception, but continue
                        ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", path, ex.Message), ex);
                    }
                }                
            }

            if (type == ClientDependencyType.Javascript)
            {
                sw.Write(";;;"); //write semicolons in case the js isn't formatted correctly. This also helps for debugging.
            }

            return def;
        }

	    /// <summary>
	    /// Writes the output of an external request to the stream
	    /// </summary>
	    /// <param name="sw"></param>
	    /// <param name="url"></param>
	    /// <param name="type"></param>
	    /// <param name="fileDefs"></param>
	    /// <param name="http"></param>
	    /// <returns></returns>
	    [Obsolete("Use the equivalent method without the 'ref' parameters")]
        [EditorBrowsable(EditorBrowsableState.Never)]
	    protected virtual void WriteFileToStream(ref StreamWriter sw, string url, ClientDependencyType type, ref List<CompositeFileDefinition> fileDefs, HttpContextBase http)
	    {
	        var def = WriteFileToStream(sw, url, type, http);
            if (def != null)
            {
                fileDefs.Add(def);
            }
	    }

        /// <summary>
        /// Writes the output of an external request to the stream
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="url"></param>
        /// <param name="type"></param>
        /// <param name="http"></param>
        protected virtual CompositeFileDefinition WriteFileToStream(StreamWriter sw, string url, ClientDependencyType type, HttpContextBase http)
        {
            string requestOutput;
            var rVal = TryReadUri(url, out requestOutput, http);
            if (!rVal) return null;

            //write the contents of the external request.
            WriteContentToStream(sw, requestOutput, type, http, url);
            return new CompositeFileDefinition(url, false);
        }

        /// <summary>
        ///  Writes the output of a local file to the stream
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="fi"></param>
        /// <param name="type"></param>
        /// <param name="origUrl"></param>
        /// <param name="fileDefs"></param>
        /// <param name="http"></param>
        [Obsolete("Use the equivalent method without the 'ref' parameters")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void WriteFileToStream(ref StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, ref List<CompositeFileDefinition> fileDefs, HttpContextBase http)
        {
            var def = WriteFileToStream(sw, fi, type, origUrl, http);
            if (def != null)
            {
                fileDefs.Add(def);
            }
        }

        /// <summary>
        /// Writes the output of a local file to the stream
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="fi"></param>
        /// <param name="type"></param>
        /// <param name="origUrl"></param>
        /// <param name="http"></param>
        protected virtual CompositeFileDefinition WriteFileToStream(StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, HttpContextBase http)
        {
            try
            {
                //if it is a file based dependency then read it				
                var fileContents = File.ReadAllText(fi.FullName, Encoding.UTF8); //read as utf 8
                WriteContentToStream(sw, fileContents, type, http, origUrl);
                return new CompositeFileDefinition(origUrl, true);
            }
            catch (Exception ex)
            {
                ClientDependencySettings.Instance.Logger.Error(string.Format("Could not write file {0} contents to stream. EXCEPTION: {1}", fi.FullName, ex.Message), ex);
                return null;
            }
        }

	    /// <summary>
	    /// Writes the actual contents of a file or request result to the stream and ensures the contents are minified if necessary
	    /// </summary>
	    /// <param name="sw"></param>
	    /// <param name="content"></param>
	    /// <param name="type"></param>
	    /// <param name="context"></param>
	    /// <param name="originalUrl">The original Url that the content is related to</param>
	    private void WriteContentToStream(StreamWriter sw, string content, ClientDependencyType type, HttpContextBase context, string originalUrl)
		{
			if (type == ClientDependencyType.Css)
			{
                IEnumerable<string> importedPaths;
                var removedImports = CssHelper.ParseImportStatements(content, out importedPaths);

                //need to write the imported sheets first since these theoretically should *always* be at the top for browser to support them
                foreach (var importPath in importedPaths)
                {
                    var uri = new Uri(originalUrl, UriKind.RelativeOrAbsolute)
                        .MakeAbsoluteUri(context);
                    var absolute = uri.ToAbsolutePath(importPath);                    
                    WritePathToStream(ClientDependencyType.Css, absolute, context, sw);
                }

                //ensure the Urls in the css are changed to absolute
                var parsedUrls = ParseCssFilePaths(removedImports, ClientDependencyType.Css, originalUrl, context);

                //then we write the css with the removed import statements
                sw.WriteLine(MinifyFile(parsedUrls, ClientDependencyType.Css));
			}
			else
			{
				sw.WriteLine(MinifyFile(content, type));
			}
		}
	}
}
