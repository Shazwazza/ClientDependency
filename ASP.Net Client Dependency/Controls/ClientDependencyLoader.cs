using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Linq;
using ClientDependency.Core.Config;
using ClientDependency.Core.FileRegistration.Providers;

namespace ClientDependency.Core.Controls
{
	[ParseChildren(typeof(ClientDependencyPath), ChildrenAsProperties = true)]
	public class ClientDependencyLoader : Control
	{
        
		/// <summary>
		/// Constructor sets the defaults.
		/// </summary>
		public ClientDependencyLoader()
		{
			Paths = new ClientDependencyPathCollection();
            
            //by default the provider is the default provider 
            m_Base.Provider = ClientDependencySettings.Instance.DefaultFileRegistrationProvider;           			
		}

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //add this object to the context and validate the context type
            if (this.Context != null)
            {
                if (!this.Context.Items.Contains(ContextKey))
                {
                    lock (m_Locker)
                    {
                        if (!this.Context.Items.Contains(ContextKey))
                        {
                            //The context needs to be a page
                            Page page = this.Context.Handler as Page;
                            if (page == null)
                                throw new InvalidOperationException("ClientDependencyLoader only works with Page based handlers.");
                            this.Context.Items[ContextKey] = this;
                        }                        
                    }                    
                }
                else
                {
                    throw new InvalidOperationException("Only one ClientDependencyLoader may exist on a page");
                }                               
            }
            else
                throw new InvalidOperationException("ClientDependencyLoader requires an HttpContext");
        }

		public const string ContextKey = "ClientDependencyLoader";

        private static object m_Locker = new object();

        public string ProviderName
        {
            get
            {
                return m_Base.Provider.Name;
            }
            set
            {
                m_Base.Provider = ClientDependencySettings.Instance.FileRegistrationProviderCollection[value];
            }
        }

		/// <summary>
		/// Singleton per request instance.
		/// </summary>
		/// <exception cref="NullReferenceException">
		/// If no ClientDependencyLoader control exists on the current page, an exception is thrown.
		/// </exception>
		public static ClientDependencyLoader Instance
		{
			get
			{
				if (!HttpContext.Current.Items.Contains(ContextKey))
					return null;
				return HttpContext.Current.Items[ContextKey] as ClientDependencyLoader;
			}
		}

        private BaseLoader m_Base = new BaseLoader(ContextKey);

		/// <summary>
		/// Need to set the container for each of the paths to support databinding.
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			foreach (ClientDependencyPath path in Paths)
			{
				path.Parent = this;
			}	
		}

		/// <summary>
		/// Need to bind all children paths.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			foreach (ClientDependencyPath path in Paths)
			{
				path.DataBind();
			}				
		}
		
        /// <summary>
        /// Finds all dependencies on the page and renders them
        /// </summary>
        /// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

            m_Base.m_Paths.UnionWith(Paths.Cast<IClientDependencyPath>());
            RegisterClientDependencies((WebFormsFileRegistrationProvider)m_Base.Provider, this.Page, m_Base.m_Paths);
			RenderDependencies();
		}

		private void RenderDependencies()
		{
            m_Base.m_Dependencies.ForEach(x =>
				{
                    ((WebFormsFileRegistrationProvider)x.Provider)
                        .RegisterDependencies(this.Page, x.Dependencies, m_Base.m_Paths);
				});
		}

		[PersistenceMode(PersistenceMode.InnerProperty)]
		public ClientDependencyPathCollection Paths { get; private set; }       
      
		#region Static Helper methods

		/// <summary>
		/// Checks if a loader already exists, if it does, it returns it, otherwise it will
		/// create a new one in the control specified.
		/// isNew will be true if a loader was created, otherwise false if it already existed.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="isNew"></param>
		/// <returns></returns>
		public static ClientDependencyLoader TryCreate(Control parent, out bool isNew)
		{
			if (ClientDependencyLoader.Instance == null)
			{
				ClientDependencyLoader loader = new ClientDependencyLoader();
				parent.Controls.Add(loader);
				isNew = true;
				return loader;
			}
			else
			{
				isNew = false;
				return ClientDependencyLoader.Instance;
			}

		}        

		#endregion

		/// <summary>
		/// Registers a file dependency
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="type"></param>
		public ClientDependencyLoader RegisterDependency(string filePath, ClientDependencyType type)
		{
            m_Base.RegisterDependency(filePath, type);
			return this;
		}

        /// <summary>
        /// Registers a file dependency
        /// </summary>
        public ClientDependencyLoader RegisterDependency(int priority, string filePath, ClientDependencyType type)
        {
            m_Base.RegisterDependency(priority, filePath, type);
            return this;
        }

        /// <summary>
        /// Registers a file dependency
        /// </summary>
        public ClientDependencyLoader RegisterDependency(int priority, string filePath, string pathNameAlias, ClientDependencyType type)
        {
            m_Base.RegisterDependency(priority, filePath, pathNameAlias, type);
            return this;
        }

		/// <summary>
		/// Registers a file dependency 
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="pathNameAlias"></param>
		/// <param name="type"></param>
		public ClientDependencyLoader RegisterDependency(string filePath, string pathNameAlias, ClientDependencyType type)
		{
			m_Base.RegisterDependency(ClientDependencyInclude.DefaultPriority, filePath, pathNameAlias, type);
			return this;
		}
		
		/// <summary>
		/// Adds a path to the current loader
		/// </summary>
		/// <param name="pathNameAlias"></param>
		/// <param name="path"></param>
		public ClientDependencyLoader AddPath(string pathNameAlias, string path)
		{
			AddPath(new BasicPath() { Name = pathNameAlias, Path = path });
			return this;
		}

		/// <summary>
		/// Adds a path to the current loader
		/// </summary>
		/// <param name="pathNameAlias"></param>
		/// <param name="path"></param>
		public void AddPath(IClientDependencyPath path)
		{
            m_Base.m_Paths.Add(path);
		}		

		/// <summary>
		/// Registers dependencies
		/// </summary>
		/// <param name="control"></param>
		/// <param name="paths"></param>
		public void RegisterClientDependencies(Control control, ClientDependencyPathCollection paths)
		{
            RegisterClientDependencies((WebFormsFileRegistrationProvider)m_Base.Provider, control, paths.Cast<IClientDependencyPath>());
		}

		/// <summary>
		/// Registers dependencies with the provider name specified
		/// </summary>
		/// <param name="providerName"></param>
		/// <param name="control"></param>
		/// <param name="paths"></param>
		public void RegisterClientDependencies(string providerName, Control control, IEnumerable<IClientDependencyPath> paths)
		{
			RegisterClientDependencies(ClientDependencySettings.Instance.FileRegistrationProviderCollection[providerName], control, paths);
		}

		/// <summary>
		/// Registers dependencies with the provider specified by T
		/// </summary>
		public void RegisterClientDependencies<T>(Control control, List<IClientDependencyPath> paths)
			where T : WebFormsFileRegistrationProvider
		{
			//need to find the provider with the type
			WebFormsFileRegistrationProvider found = null;
			foreach (WebFormsFileRegistrationProvider p in ClientDependencySettings.Instance.FileRegistrationProviderCollection)
			{
				if (p.GetType().Equals(typeof(T)))
				{
					found = p;
					break;
				}
			}
			if (found == null)
				throw new ArgumentException("Could not find the ClientDependencyProvider specified by T");

			RegisterClientDependencies(found, control, paths);
		}

		public void RegisterClientDependencies(WebFormsFileRegistrationProvider provider, Control control, IEnumerable<IClientDependencyPath> paths)
		{
            IEnumerable<IClientDependencyFile> dependencies = FindDependencies(control);
            m_Base.RegisterClientDependencies(provider, dependencies, paths, ClientDependencySettings.Instance.FileRegistrationProviderCollection);
		}

		/// <summary>
		/// Find all dependencies of this control and it's entire child control heirarchy.
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
        private IEnumerable<IClientDependencyFile> FindDependencies(Control control)
		{
            var ctls = new List<Control>(control.FlattenChildren());
            ctls.Add(control);
            
            List<IClientDependencyFile> dependencies = new List<IClientDependencyFile>();
			
            // add child dependencies
			Type iClientDependency = typeof(IClientDependencyFile);
            foreach (Control ctl in ctls)
			{
                // find dependencies
                Type controlType = ctl.GetType();
                
                foreach (Attribute attribute in Attribute.GetCustomAttributes(controlType))
                {
                    if (attribute is ClientDependencyAttribute)
                    {
                        dependencies.Add((ClientDependencyAttribute)attribute);
                    }
                }

                if (iClientDependency.IsAssignableFrom(ctl.GetType()))
                {
                    IClientDependencyFile include = (IClientDependencyFile)ctl;
                    dependencies.Add(include);
                }
                
			}

			return dependencies;
		}		

	}

	

	
}
