using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using ClientDependency.Core.Config;

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
            //This event is late enough that the ContentType of the request is set
            //but not too late that we've lost the ability to change the response
            //app.BeginRequest += new EventHandler(HandleRequest);
            app.PostRequestHandlerExecute +=new EventHandler(HandleRequest);
            LoadFilterTypes();
        }

        /// <summary>
        /// Checks if any assigned filters validate the current handler, if so then assigns any filter
        /// that CanExecute to the response filter chain.
        /// 
        /// Checks if the request MIME type matches the list of mime types specified in the config,
        /// if it does, then it compresses it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleRequest(object sender, EventArgs e)
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
            if (!IsCompressibleContentType(app.Response))
                return;

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

        /// <summary>
        /// Determines whether the content type can be compressed
        /// </summary>
        /// <param name="response">The HttpRequest to check.</param>
        /// <returns>
        /// 	<c>true</c> if the content type can be compressed; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsCompressibleContentType(HttpResponse response)
        {
            //TODO: Is there a better way to check the ContentType is something we want to compress?
            switch (response.ContentType.ToLower())
            {
                case "text/html":
                case "text/css":
                case "text/plain":
                case "application/x-javascript":
                case "text/javascript":
                case "text/xml":
                case "application/xml":
                case "":
                    return true;

                default:
                    return false;
            }
        }
        #endregion

    }
}
