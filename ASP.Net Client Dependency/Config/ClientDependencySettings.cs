using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Configuration.Provider;
using System.Web;
using System.Configuration;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Logging;
using System.IO;

namespace ClientDependency.Core.Config
{
    public class ClientDependencySettings
    {
        /// <summary>
        /// used for singleton
        /// </summary>
        private static ClientDependencySettings _settings;
        private static readonly object Lock = new object();

        /// <summary>
        /// Default constructor, for use with a web context app
        /// </summary>
        internal ClientDependencySettings()
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(
                    "HttpContext.Current must exist when using the empty constructor for ClientDependencySettings, otherwise use the alternative constructor");
            }

            LoadProviders((ClientDependencySection)ConfigurationManager.GetSection("clientDependency"), new HttpContextWrapper(HttpContext.Current));
            
        }

        internal ClientDependencySettings(FileInfo configFile, HttpContextBase ctx)
        {
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            LoadProviders((ClientDependencySection)configuration.GetSection("clientDependency"), ctx);
        }

        /// <summary>
        /// Singleton, used for web apps
        /// </summary>
        public static ClientDependencySettings Instance
        {
            get
            {
                if (_settings == null)
                {
                    lock(Lock)
                    {
                        //double check
                        if (_settings == null)
                        {
                            _settings = new ClientDependencySettings();
                        }
                    }
                }
                return _settings;
            }
        }

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

        
        //public bool EnableCompositeFiles { get; set; }
        
        public int Version { get; set; }

        public ILogger Logger { get; private set; }

        public BaseRenderer DefaultMvcRenderer { get; private set; }

        public RendererCollection MvcRendererCollection { get; private set; }

        public WebFormsFileRegistrationProvider DefaultFileRegistrationProvider { get; private set; }

        public FileRegistrationProviderCollection FileRegistrationProviderCollection { get; private set; }

        public BaseCompositeFileProcessingProvider DefaultCompositeFileProcessingProvider { get; private set; }

        public CompositeFileProcessingProviderCollection CompositeFileProcessingProviderCollection { get; private set; }

        public ClientDependencySection ConfigSection { get; private set; }
       
        public string CompositeFileHandlerPath { get; set; }
       
        internal void LoadProviders(ClientDependencySection section, HttpContextBase http)
        {
         
            ConfigSection = section;

            FileRegistrationProviderCollection = new FileRegistrationProviderCollection();
            CompositeFileProcessingProviderCollection = new CompositeFileProcessingProviderCollection();
            MvcRendererCollection = new RendererCollection();

            // if there is no section found, then create one
            if (ConfigSection == null)
            {
                //create a new section with the default settings
                ConfigSection = new ClientDependencySection();                            
            }

            //load the providers from the config, if there isn't config sections then add default providers
            LoadDefaultCompositeFileConfig(ConfigSection, http);
            LoadDefaultMvcFileConfig(ConfigSection);
            LoadDefaultFileRegConfig(ConfigSection);

            //set the defaults

            DefaultFileRegistrationProvider = FileRegistrationProviderCollection[ConfigSection.FileRegistrationElement.DefaultProvider];
            if (DefaultFileRegistrationProvider == null)
                throw new ProviderException("Unable to load default file registration provider");

            DefaultCompositeFileProcessingProvider = CompositeFileProcessingProviderCollection[ConfigSection.CompositeFileElement.DefaultProvider];
            if (DefaultCompositeFileProcessingProvider == null)
                throw new ProviderException("Unable to load default composite file provider");

            DefaultMvcRenderer = MvcRendererCollection[ConfigSection.MvcElement.DefaultRenderer];
            if (DefaultMvcRenderer == null)
                throw new ProviderException("Unable to load default mvc renderer");

            //need to check if it's an http path or a lambda path
            var path = ConfigSection.CompositeFileElement.CompositeFileHandlerPath;
            CompositeFileHandlerPath = path.StartsWith("~/")
                ? VirtualPathUtility.ToAbsolute(ConfigSection.CompositeFileElement.CompositeFileHandlerPath, http.Request.ApplicationPath) 
                : ConfigSection.CompositeFileElement.CompositeFileHandlerPath;

            Version = ConfigSection.Version;

            FileBasedDependencyExtensionList = ConfigSection.FileBasedDependencyExtensionList.ToList();


            if (string.IsNullOrEmpty(ConfigSection.LoggerType))
            {
                Logger = new NullLogger();
            }
            else
            {
                var t = Type.GetType(ConfigSection.LoggerType);
                if (!typeof(ILogger).IsAssignableFrom(t))
                {
                    throw new ArgumentException("The loggerType '" + ConfigSection.LoggerType + "' does not inherit from ClientDependency.Core.Logging.ILogger");
                }

                Logger = (ILogger)Activator.CreateInstance(t);
            }
                    
        }

        private void LoadDefaultFileRegConfig(ClientDependencySection section)
        {
            if (section.CompositeFileElement.Providers.Count == 0)
            {
                //create new providers
                var php = new PageHeaderProvider();
                php.Initialize(PageHeaderProvider.DefaultName, null);
                FileRegistrationProviderCollection.Add(php);

                var csrp = new LazyLoadProvider();
                csrp.Initialize(LazyLoadProvider.DefaultName, null);
                FileRegistrationProviderCollection.Add(csrp);

                var lcp = new LoaderControlProvider();
                lcp.Initialize(LoaderControlProvider.DefaultName, null);
                FileRegistrationProviderCollection.Add(lcp);
            }
            else
            {
                ProvidersHelper.InstantiateProviders(section.FileRegistrationElement.Providers, FileRegistrationProviderCollection, typeof(BaseFileRegistrationProvider));
            }

        }

        private void LoadDefaultCompositeFileConfig(ClientDependencySection section, HttpContextBase http)
        {
            if (section.CompositeFileElement.Providers.Count == 0)
            {
                var cfpp = new CompositeFileProcessingProvider();
                cfpp.Initialize(CompositeFileProcessingProvider.DefaultName, null);
                cfpp.Initialize(http);
                CompositeFileProcessingProviderCollection.Add(cfpp);
            }
            else
            {
                ProvidersHelper.InstantiateProviders(section.CompositeFileElement.Providers, CompositeFileProcessingProviderCollection, typeof(BaseCompositeFileProcessingProvider));
                //since the BaseCompositeFileProcessingProvider is an IHttpProvider, we need to do the http init
                foreach(var p in CompositeFileProcessingProviderCollection.Cast<BaseCompositeFileProcessingProvider>())
                {
                    p.Initialize(http);
                }
            }
            
        }

        private void LoadDefaultMvcFileConfig(ClientDependencySection section)
        {
            if (section.MvcElement.Renderers.Count == 0)
            {
                var mvc = new StandardRenderer();
                mvc.Initialize(StandardRenderer.DefaultName, null);
                MvcRendererCollection.Add(mvc);
            }
            else
            {
                ProvidersHelper.InstantiateProviders(section.MvcElement.Renderers, MvcRendererCollection, typeof(BaseRenderer));
            }

        }
    }
}

