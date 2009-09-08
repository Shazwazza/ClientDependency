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

namespace ClientDependency.Core.CompositeFiles.Providers
{
	public interface ICompositeFileProcessingProvider
	{
		FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type);
		byte[] CombineFiles(string[] strFiles, HttpContext context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs);
		byte[] CompressBytes(CompressionType type, byte[] fileBytes);
	}
}
