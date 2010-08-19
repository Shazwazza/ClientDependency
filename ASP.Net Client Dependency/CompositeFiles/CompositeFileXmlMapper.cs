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
                return m_Mapper;
            }
        }

        private CompositeFileXmlMapper()
        {
            Initialize();
        }

        private static readonly CompositeFileXmlMapper m_Mapper = new CompositeFileXmlMapper();

        private const string MapFileName = "map.xml";

        private XDocument m_Doc;
        private FileInfo m_XmlFile;
        private object m_Lock = new object();

        /// <summary>
        /// Loads in the existing file contents. If the file doesn't exist, it creates one.
        /// </summary>
        private void Initialize()
        {
            //return if composite files are disabled.
            if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.PersistCompositeFiles)
                return;

            //Name the map file according to the machine name
            m_XmlFile = new FileInfo(GetXmlMapPath());

            EnsureXmlFile();

            lock (m_Lock)
            {
                try
                {
                    m_Doc = XDocument.Load(m_XmlFile.FullName);
                }
                catch (XmlException ex)
                {
                    //if it's an xml exception, create a new one and try one more time... should always work.
                    CreateNewXmlFile();
                    m_Doc = XDocument.Load(m_XmlFile.FullName);
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
        private string GetXmlMapPath()
        {
            var folder = ClientDependencySettings.Instance.
                            DefaultCompositeFileProcessingProvider.
                            CompositeFilePath.FullName;
            var folderMd5 = GenerateMD5(folder);
            return Path.Combine(folder,Environment.MachineName + "-" + folderMd5 + "-" + MapFileName);
        }


        /// <summary>rate a MD5 hash of a string
        /// method to gene
        /// </summary>
        /// <param name="strToHash">string to hash</param>
        /// <returns>hashed string</returns>
        private string GenerateMD5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] byteArray = Encoding.ASCII.GetBytes(str);
            byteArray = md5.ComputeHash(byteArray);
            string hashedValue = "";
            foreach (byte b in byteArray)
            {
                hashedValue += b.ToString("x2");
            }
            return hashedValue;
        }

        private void CreateNewXmlFile()
        {
            //return if composite files are disabled.
            if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.PersistCompositeFiles)
                return;

            if (File.Exists(m_XmlFile.FullName))
            {
                File.Delete(m_XmlFile.FullName);
            }

            m_Doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                                            new XElement("map"));
            m_Doc.Save(m_XmlFile.FullName);
        }

        private void EnsureXmlFile()
        {
            if (!File.Exists(m_XmlFile.FullName))
            {
                lock (m_Lock)
                {
                    //double check
                    if (!File.Exists(m_XmlFile.FullName))
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
        /// <returns></returns>
        public CompositeFileMap GetCompositeFile(string base64Key, int version, string compression)
        {
            //return null if composite files are disabled.
            if (!ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.PersistCompositeFiles)
                return null;

            XElement x = FindItem(base64Key, version, compression);
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
        /// <param name="dependentFiles"></param>
        /// <param name="compositeFile"></param>
        /// <example>
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

            lock (m_Lock)
            {
                //see if we can find an item with the key already
                XElement x = FindItem(base64Key, version, compressionType);

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
                    m_Doc.Root.Add(new XElement("item",
                        new XAttribute("key", base64Key),
                        new XAttribute("file", compositeFile),
                        new XAttribute("compression", compressionType),
                        new XAttribute("version", version),
                        CreateFileNode(dependentFiles)));
                }

                m_Doc.Save(m_XmlFile.FullName);
            }
        }

        private XElement FindItem(string key, int version, string compression)
        {
            return m_Doc.Root.Elements("item")
                    .Where(e => (string)e.Attribute("key") == key
                        && (string)e.Attribute("version") == version.ToString()
                        && (string)e.Attribute("compression") == compression)
                    .SingleOrDefault();
        }

        private XElement CreateFileNode(List<FileInfo> files)
        {
            XElement x = new XElement("files");

            //add all of the files
            files.ForEach(d =>
            {
                x.Add(new XElement("file",
                    new XAttribute("name", d.FullName)));
            });

            return x;
        }
    }
}
