using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Text.RegularExpressions;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using System.Net;
using System.Globalization;
using System.IO.Compression;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Compilation;

namespace ClientDependency.Core.Module
{

    /// <summary>
    /// This module currently replaces rogue scripts with composite scripts.
    /// Eventually it will handle css files and MVC implementation
    /// </summary>
    public class ClientDependencyModule : IHttpModule
    {
        #region IHttpModule Members

        void IHttpModule.Dispose() { }

        /// <summary>
        /// Binds the events
        /// </summary>
        /// <param name="context"></param>
        void IHttpModule.Init(HttpApplication app)
        {
            app.PreRequestHandlerExecute += new EventHandler(app_PreRequestHandlerExecute);
            LoadFilterTypes();
        }
  
        #endregion

        private List<Type> m_FilterTypes = new List<Type>();

        #region Private Methods

        private void LoadFilterTypes()
        {
            foreach (var f in ClientDependencySettings.Instance.ConfigSection.Filters.Cast<ProviderSettings>())
            {
                var t = BuildManager.GetType(f.Type, false, true);
                if (t != null)
                {
                    m_FilterTypes.Add(t);
                }                
            }
        }

        /// <summary>
        /// loads instances of all registered filters.
        /// </summary>
        /// <param name="app"></param>
        private IEnumerable<IFilter> LoadFilters(HttpApplication app)
        {
            HttpContextWrapper ctx = new HttpContextWrapper(app.Context);
            var loadedFilters = new List<IFilter>();

            foreach (var t in m_FilterTypes)
            {
                var filter = (IFilter)Activator.CreateInstance(t);
                filter.SetHttpContext(ctx);
                loadedFilters.Add(filter);

            }

            return loadedFilters;
        }

        /// <summary>
        /// Ensure the current running handler is valid in order to proceed with the module filter.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private bool ValidateCurrentHandler(HttpApplication app, IEnumerable<IFilter> filters)
        {
            foreach (var f in filters)
            {
                //if any filter validates the handler then we need to add the response filter
                if (f.ValidateCurrentHandler()) return true;
            }
            return false;
        }

        private void ExecuteFilter(HttpApplication app, IEnumerable<IFilter> filters)
        {
            ResponseFilterStream filter = new ResponseFilterStream(app.Response.Filter);
            foreach (var f in filters)
            {
                if (f.CanExecute())
                {
                    filter.TransformString += f.UpdateOutputHtml;
                }
            }
            app.Response.Filter = filter;
        } 
        #endregion

        /// <summary>
        /// Checks if any assigned filters validate the current handler, if so then assigns any filter
        /// that CanExecute to the response filter chain.
        /// 
        /// Checks if the request MIME type matches the list of mime types specified in the config,
        /// if it does, then it compresses it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void app_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;

            var filters = LoadFilters(app);

            if (ValidateCurrentHandler(app, filters))
            {
                ExecuteFilter(app, filters);
            }
            
            //if debug is on, then don't compress
            if (!ConfigurationHelper.IsCompilationDebug)
            {
                MimeTypeCompressor c = new MimeTypeCompressor(app.Context);
                c.AddCompression();
            }
        }
        
    }
}
