using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using ClientDependency.Core.Config;
using System.Security.Cryptography;

namespace ClientDependency.Core.CompositeFiles
{

    /// <summary>
    /// Creates an XML file to map a saved composite file to the URL requested for the 
    /// dependency handler. 
    /// This is used in order to determine which individual files are dependant on what composite file so 
    /// a user can remove it to clear the cache, and also if the cache expires but the file still exists
    /// this allows the system to simply read the one file again instead of compiling all of the other files
    /// into one again.
    /// </summary>
    public class CompositeFileXmlMapper
    {

        /// <summary>
        /// Singleton
        /// </summary>
        public static CompositeFileXmlMapper Instance
        {
            get
            {
                return Mapper;
            }
        }

        private CompositeFileXmlMapper()
        {
            Initialize();
        }

        private static readonly CompositeFileXmlMapper Mapper = new CompositeFileXmlMapper();

        private const string MapFileName = "map.xml";

        private XDocument _doc;
        private FileInfo _xmlFile;
        private readonly object _locker = new object();

        /// <summary>
        /// Loads in the existing file contents. If the file doesn't exist, it creates one.
        /// </summary>
        private void Initialize()
        {
            //return if composite files are disabled.
            if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.PersistCompositeFiles)
                return;

            //Name the map file according to the machine name
            _xmlFile = new FileInfo(GetXmlMapPath());

            EnsureXmlFile();

            lock (_locker)
            {
                try
                {
                    _doc = XDocument.Load(_xmlFile.FullName);
                }
                catch (XmlException)
                {
                    //if it's an xml exception, create a new one and try one more time... should always work.
                    CreateNewXmlFile();
                    _doc = XDocument.Load(_xmlFile.FullName);
                }
            }
        }

        /// <summary>
        /// Returns the full path the map xml file for the current machine and install folder.
        /// </summary>
        /// <remarks>
        /// We need to create the map based on the combination of both machine name and install folder because
        /// this deals with issues for load balanced environments and file locking and also 
        /// deals with issues when the ClientDependency folder is deployed between environments
        /// since you would want your staging ClientDependencies in your live and vice versa.
        /// This is however based on the theory that each website you have will have a unique combination
        /// of folder path and machine name.
        /// </remarks>
        /// <returns></returns>
        private static string GetXmlMapPath()
        {
            var folder = ClientDependencySettings.Instance.
                            DefaultCompositeFileProcessingProvider.
                            CompositeFilePath.FullName;
            var folderMd5 = GenerateMd5(folder);
            return Path.Combine(folder,Environment.MachineName + "-" + folderMd5 + "-" + MapFileName);
        }


        /// <summary>rate a MD5 hash of a string
        /// method to gene
        /// </summary>
        /// <returns>hashed string</returns>
        private static string GenerateMd5(string str)
        {
            var md5 = new MD5CryptoServiceProvider();
            var byteArray = Encoding.ASCII.GetBytes(str);
            byteArray = md5.ComputeHash(byteArray);
            return byteArray.Aggregate("", (current, b) => current + b.ToString("x2"));
        }

        private void CreateNewXmlFile()
        {
            //return if composite files are disabled.
            if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.PersistCompositeFiles)
                return;

            if (File.Exists(_xmlFile.FullName))
            {
                File.Delete(_xmlFile.FullName);
            }

            _doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                                            new XElement("map"));
            _doc.Save(_xmlFile.FullName);
        }

        private void EnsureXmlFile()
        {
            if (!File.Exists(_xmlFile.FullName))
            {
                lock (_locker)
                {
                    //double check
                    if (!File.Exists(_xmlFile.FullName))
                    {
                        if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CompositeFilePath.Exists)
                            ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CompositeFilePath.Create();
                        CreateNewXmlFile();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the composite file map associated with the base 64 key of the URL, the version and the compression type
        /// </summary>
        /// <param name="base64Key"></param>
        /// <param name="version"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        public CompositeFileMap GetCompositeFile(string base64Key, int version, string compression)
        {
            //return null if composite files are disabled.
            if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.PersistCompositeFiles)
                return null;

            var x = FindItem(base64Key, version, compression);
            try
            {
                return (x == null ? null : new CompositeFileMap(base64Key,
                    x.Attribute("compression").Value,
                    x.Attribute("file").Value,
                    x.Descendants("file")
                        .Select(f => new FileInfo(f.Attribute("name").Value))
                        .ToList(), int.Parse(x.Attribute("version").Value)));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64Key"></param>
        ///<param name="compressionType"></param>
        ///<param name="dependentFiles"></param>
        /// <param name="compositeFile"></param>
        ///<param name="version"></param>
        ///<example>
        /// <![CDATA[
        /// <map>
        ///		<item key="XSDFSDKJHLKSDIOUEYWCDCDSDOIUPOIUEROIJDSFHG" 
        ///			file="C:\asdf\App_Data\ClientDependency\123456.cdj"
        ///			compresion="deflate">
        ///			<files>
        ///				<file name="C:\asdf\JS\jquery.js" />
        ///				<file name="C:\asdf\JS\jquery.ui.js" />		
        ///			</files>
        ///		</item>
        /// </map>
        /// ]]>
        /// </example>
        public void CreateMap(string base64Key, string compressionType, List<FileInfo> dependentFiles, string compositeFile, int version)
        {
            //return if composite files are disabled.
            if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.PersistCompositeFiles)
                return;

            lock (_locker)
            {
                //see if we can find an item with the key already
                var x = FindItem(base64Key, version, compressionType);

                if (x != null)
                {
                    x.Attribute("file").Value = compositeFile;
                    //remove all of the files so we can re-add them.
                    x.Element("files").Remove();

                    x.Add(CreateFileNode(dependentFiles));
                }
                else
                {
                    //if it doesn't exist, create it
                    _doc.Root.Add(new XElement("item",
                        new XAttribute("key", base64Key),
                        new XAttribute("file", compositeFile),
                        new XAttribute("compression", compressionType),
                        new XAttribute("version", version),
                        CreateFileNode(dependentFiles)));
                }

                _doc.Save(_xmlFile.FullName);
            }
        }

        private XElement FindItem(string key, int version, string compression)
        {
            return _doc.Root.Elements("item")
                    .Where(e => (string)e.Attribute("key") == key
                        && (string)e.Attribute("version") == version.ToString()
                        && (string)e.Attribute("compression") == compression)
                    .SingleOrDefault();
        }

        private static XElement CreateFileNode(List<FileInfo> files)
        {
            var x = new XElement("files");

            //add all of the files
            files.ForEach(d => x.Add(new XElement("file",
                                                  new XAttribute("name", d.FullName))));

            return x;
        }
    }
}
