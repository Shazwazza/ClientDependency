using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using ClientDependency.Core.Config;
using System.Security.Cryptography;

namespace ClientDependency.Core.CompositeFiles.Providers
{

    /// <summary>
    /// Creates an XML file to map a saved composite file to the URL requested for the 
    /// dependency handler. 
    /// This is used in order to determine which individual files are dependant on what composite file so 
    /// a user can remove it to clear the cache, and also if the cache expires but the file still exists
    /// this allows the system to simply read the one file again instead of compiling all of the other files
    /// into one again.
    /// </summary>
    public class XmlFileMapper : BaseFileMapProvider
    {

        private const string MapFileName = "map.xml";

        private XDocument _doc;
        private FileInfo _xmlFile;
        private DirectoryInfo _xmlMapFolder;
        private string _fileMapVirtualFolder = "~/App_Data/ClientDependency";
        private readonly object _locker = new object();

        public override void Initialize(System.Web.HttpContextBase http)
        {
            _xmlMapFolder = new DirectoryInfo(http.Server.MapPath(_fileMapVirtualFolder));

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
        /// Initializes the provider, loads in the existing file contents. If the file doesn't exist, it creates one.
        /// </summary>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config["mapPath"] != null)
            {
                _fileMapVirtualFolder = config["mapPath"];
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
        private string GetXmlMapPath()
        {
            var folder = _xmlMapFolder.FullName;
            var folderMd5 = GenerateMd5(folder);
            return Path.Combine(folder, Environment.MachineName + "-" + folderMd5 + "-" + MapFileName);
        }


        /// <summary>rate a MD5 hash of a string
        /// method to gene
        /// </summary>
        /// <returns>hashed string</returns>
        private string GenerateMd5(string str)
        {
            var md5 = new MD5CryptoServiceProvider();
            var byteArray = Encoding.ASCII.GetBytes(str);
            byteArray = md5.ComputeHash(byteArray);
            return byteArray.Aggregate("", (current, b) => current + b.ToString("x2"));
        }

        private void CreateNewXmlFile()
        {
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
                        if (!_xmlMapFolder.Exists)
                            _xmlMapFolder.Create();
                        CreateNewXmlFile();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the composite file map associated with the base 64 key of the URL, the version and the compression type
        /// </summary>
        /// <param name="fileKey"></param>
        /// <param name="version"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        public override CompositeFileMap GetCompositeFile(string fileKey, int version, string compression)
        {

            var x = FindItem(fileKey, version, compression);
            try
            {
                return (x == null ? null : new CompositeFileMap(fileKey,
                    (string)x.Attribute("compression"),
                    (string)x.Attribute("file"),
                    x.Descendants("file")
                        .Select(f => new FileInfo((string)f.Attribute("name")))
                        .ToList(), int.Parse((string)x.Attribute("version"))));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileKey"></param>
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
        public override void CreateMap(string fileKey, string compressionType, IEnumerable<FileInfo> dependentFiles, string compositeFile, int version)
        {
            lock (_locker)
            {
                //see if we can find an item with the key already
                var x = FindItem(fileKey, version, compressionType);

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
                        new XAttribute("key", fileKey),
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

        private XElement CreateFileNode(IEnumerable<FileInfo> files)
        {
            var x = new XElement("files");

            //add all of the files
            foreach (var d in files)
            {
                x.Add(new XElement("file", new XAttribute("name", d.FullName)));
            }

            return x;
        }
    }
}
