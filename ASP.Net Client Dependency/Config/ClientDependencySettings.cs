using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Configuration.Provider;
using System.IO;
using System.Web;
using System.Configuration;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Logging;

namespace ClientDependency.Core.Config
{
    public class ClientDependencySettings
    {

        private ClientDependencySettings()
        {
            LoadProviders();
        }

        /// <summary>
        /// Singleton
        /// </summary>
        public static ClientDependencySettings Instance
        {
            get
            {
                return m_Settings;
            }
        }

        private static readonly ClientDependencySettings m_Settings = new ClientDependencySettings();

        private object m_Lock = new object();
        private BaseFileRegistrationProvider m_FileRegisterProvider = null;
        private FileRegistrationProviderCollection m_FileRegisterProviders = null;

        private BaseCompositeFileProcessingProvider m_CompositeFileProvider = null;
        private CompositeFileProcessingProviderCollection m_CompositeFileProviders = null;

        public const string ConfigurationSectionName = "clientDependency";

        /// <summary>
        /// The file extensions of Client Dependencies that are file based as opposed to request based.
        /// Any file that doesn't have the extensions listed here will be request based, request based is
        /// more overhead for the server to process.
        /// </summary>
        /// <example>
        /// A request based JavaScript file may be  a .ashx that dynamically creates JavaScript server side.
        /// </example>
        /// <remarks>
        /// If this is not explicitly set, then the extensions 'js' and 'css' are the defaults.
        /// </remarks>
        public List<string> FileBasedDependencyExtensionList { get; set; }

        /// <summary>
        /// Flags whether or not to enable composite file script creation.
        /// Composite file creation will increase performance in the case of cache turnover or application
        /// startup since the files are already combined and compressed.
        /// This also allows for the ability to easily clear the cache so the files are refreshed.
        /// </summary>
        public bool EnableCompositeFiles { get; set; }
        public bool IsDebugMode { get; set; }
        public int Version { get; set; }

        private ILogger _logger;
        public ILogger Logger
        {
            get
            {
                return _logger;
            }
        }

        public BaseFileRegistrationProvider DefaultFileRegistrationProvider
        {
            get
            {
                return m_FileRegisterProvider;
            }
        }
        public FileRegistrationProviderCollection FileRegistrationProviderCollection
        {
            get
            {
                return m_FileRegisterProviders;
            }
        }
        public BaseCompositeFileProcessingProvider DefaultCompositeFileProcessingProvider
        {
            get
            {
                return m_CompositeFileProvider;
            }
        }
        public CompositeFileProcessingProviderCollection CompositeFileProcessingProviderCollection
        {
            get
            {
                return m_CompositeFileProviders;
            }
        }
        public DirectoryInfo CompositeFilePath { get; set; }
        public string CompositeFileHandlerPath { get; set; }



        private void LoadProviders()
        {
            if (m_FileRegisterProvider == null)
            {
                lock (m_Lock)
                {
                    // Do this again to make sure _provider is still null
                    if (m_FileRegisterProvider == null)
                    {
                        ClientDependencySection section = (ClientDependencySection)ConfigurationManager.GetSection("clientDependency");

                        m_FileRegisterProviders = new FileRegistrationProviderCollection();
                        m_CompositeFileProviders = new CompositeFileProcessingProviderCollection();

                        // if there is no section found, then add the standard providers to the collection with the standard 
                        // default provider
                        if (section != null)
                        {
                            // Load registered providers and point _provider to the default provider	
                            ProvidersHelper.InstantiateProviders(section.FileRegistrationElement.Providers, m_FileRegisterProviders, typeof(BaseFileRegistrationProvider));
                            ProvidersHelper.InstantiateProviders(section.CompositeFileElement.Providers, m_CompositeFileProviders, typeof(BaseCompositeFileProcessingProvider));
                        }
                        else
                        {
                            //create a new section with the default settings
                            section = new ClientDependencySection();

                            //create new providers
                            PageHeaderProvider php = new PageHeaderProvider();
                            php.Initialize(PageHeaderProvider.DefaultName, null);
                            m_FileRegisterProviders.Add(php);

                            LazyLoadProvider csrp = new LazyLoadProvider();
                            csrp.Initialize(LazyLoadProvider.DefaultName, null);
                            m_FileRegisterProviders.Add(csrp);

                            CompositeFileProcessingProvider cfpp = new CompositeFileProcessingProvider();
                            cfpp.Initialize(CompositeFileProcessingProvider.DefaultName, null);
                            m_CompositeFileProviders.Add(cfpp);

                        }


                        //set the defaults

                        m_FileRegisterProvider = m_FileRegisterProviders[section.FileRegistrationElement.DefaultProvider];
                        if (m_FileRegisterProvider == null)
                            throw new ProviderException("Unable to load default file registration provider");

                        m_CompositeFileProvider = m_CompositeFileProviders[section.CompositeFileElement.DefaultProvider];
                        if (m_CompositeFileProvider == null)
                            throw new ProviderException("Unable to load default composite file provider");

                        CompositeFileHandlerPath = section.CompositeFileElement.CompositeFileHandlerPath;
                        IsDebugMode = section.IsDebugMode;
                        this.Version = section.Version;
                        EnableCompositeFiles = section.FileRegistrationElement.EnableCompositeFiles;
                        FileBasedDependencyExtensionList = section.FileRegistrationElement.FileBasedDependencyExtensionList.ToList();
                        CompositeFilePath = new DirectoryInfo(HttpContext.Current.Server.MapPath(section.CompositeFileElement.CompositeFilePath));

                        if (string.IsNullOrEmpty(section.LoggerType))
                        {
                            _logger = new NullLogger();
                        }
                        else
                        {
                            var t = Type.GetType(section.LoggerType);
                            if (!typeof(ILogger).IsAssignableFrom(t))
                            {
                                throw new ArgumentException("The loggerType '" + section.LoggerType + "' does not inherit from ClientDependency.Core.Logging.ILogger");
                            }

                            _logger = (ILogger)Activator.CreateInstance(t);
                        }
                    }
                }
            }
        }
    }
}

