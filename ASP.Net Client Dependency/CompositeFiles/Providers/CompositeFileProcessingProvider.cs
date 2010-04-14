using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	/// A utility class for combining, minifying, compressing and saving composite scripts/css files
	/// </summary>
	public class CompositeFileProcessingProvider : BaseCompositeFileProcessingProvider
	{

        public const string DefaultName = "CompositeFileProcessor";

		/// <summary>
		/// Saves the file's bytes to disk with a hash of the byte array
		/// </summary>
		/// <param name="fileContents"></param>
		/// <param name="type"></param>
		/// <returns>The new file path</returns>
		/// <remarks>
		/// the extension will be: .cdj for JavaScript and .cdc for CSS
		/// </remarks>
		public override FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type)
		{
            //don't save the file if composite files are disabled.
            if (!this.PersistCompositeFiles)
                return null;

			if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CompositeFilePath.Exists)
				ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CompositeFilePath.Create();
			FileInfo fi = new FileInfo(
                Path.Combine(ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CompositeFilePath.FullName,
					ClientDependencySettings.Instance.Version.ToString() + "_" 
                        + fileContents.GetHashCode().ToString() + ".cd" + type.ToString().Substring(0, 1).ToUpper()));
			if (fi.Exists)
				fi.Delete();
			FileStream fs = fi.Create();
			fs.Write(fileContents, 0, fileContents.Length);
			fs.Close();
			return fi;
		}

		/// <summary>
		/// combines all files to a byte array
		/// </summary>
		/// <param name="fileList"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public override byte[] CombineFiles(string[] strFiles, HttpContext context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs)
		{

			List<CompositeFileDefinition> fDefs = new List<CompositeFileDefinition>();

			MemoryStream ms = new MemoryStream(5000);
            StreamWriter sw = new StreamWriter(ms, Encoding.Default);
			foreach (string s in strFiles)
			{
				if (!string.IsNullOrEmpty(s))
				{
					try
					{
						FileInfo fi = new FileInfo(context.Server.MapPath(s));
						if (ClientDependencySettings.Instance.FileBasedDependencyExtensionList.Contains(fi.Extension.ToUpper()))
						{
							//if the file doesn't exist, then we'll assume it is a URI external request
							if (!fi.Exists)
							{
								WriteFileToStream(ref sw, s, type, ref fDefs);
							}
							else
							{
								WriteFileToStream(ref sw, fi, type, s, ref fDefs);
							}
						}
						else
						{
							//if it's not a file based dependency, try to get the request output.
							WriteFileToStream(ref sw, s, type, ref fDefs);
						}
					}
					catch (Exception ex)
					{
						Type exType = ex.GetType();
						if (exType.Equals(typeof(NotSupportedException)) 
							|| exType.Equals(typeof(ArgumentException))
							|| exType.Equals(typeof(HttpException)))
						{
							//could not parse the string into a fileinfo or couldn't mappath, so we assume it is a URI
							WriteFileToStream(ref sw, s, type, ref fDefs);
						}
						else
						{
							//if this fails, log the exception in trace, but continue
                            ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", s, ex.Message), ex);
						}
					}
				}

				if (type == ClientDependencyType.Javascript)
				{
					sw.Write(";;;"); //write semicolons in case the js isn't formatted correctly. This also helps for debugging.
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
		/// Writes the output of an external request to the stream. Returns true/false if succesful or not.
		/// </summary>
		/// <param name="sw"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		private bool WriteFileToStream(ref StreamWriter sw, string url, ClientDependencyType type, ref List<CompositeFileDefinition> fileDefs)
		{
			string requestOutput;
			bool rVal = false;
			rVal = TryReadUri(url, out requestOutput);
			if (rVal)
			{
				//write the contents of the external request.
                sw.WriteLine(MinifyFile(ParseCssFilePaths(requestOutput, type, url), type));
				fileDefs.Add(new CompositeFileDefinition(url, false));
			}
			return rVal;
		}

		private bool WriteFileToStream(ref StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, ref List<CompositeFileDefinition> fileDefs)
		{
			try
			{
				//if it is a file based dependency then read it
				string fileContents = File.ReadAllText(fi.FullName);
                sw.WriteLine(MinifyFile(ParseCssFilePaths(fileContents, type, origUrl), type));
				fileDefs.Add(new CompositeFileDefinition(origUrl, true));
				return true;
			}
			catch (Exception ex)
			{
                ClientDependencySettings.Instance.Logger.Error(string.Format("Could not write file {0} contents to stream. EXCEPTION: {1}", fi.FullName, ex.Message), ex);
				return false;
			}			
		}		

		
	}
}
