using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using System.Net;
using System.Globalization;
using System.IO.Compression;
using System.Configuration;

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
            app.PostMapRequestHandler += new EventHandler(app_PostMapRequestHandler);
            app.PreRequestHandlerExecute += new EventHandler(app_PreRequestHandlerExecute);
        }

  
        #endregion

        /// <summary>
        /// This event handler adds a response filter to the current response object
        /// if the response is being handled by a Page or MvcHandler object.
        /// If it is a different type of handler, then everything is ignored.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void app_PostMapRequestHandler(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;

            //if debug is on, then don't compress
            if (!ConfigurationHelper.IsCompilationDebug)
            {
                //ensure it's a page handler or mvchandler
                IHttpHandler handler = app.Context.CurrentHandler as Page;
                if (handler == null)
                {
                    handler = app.Context.CurrentHandler as MvcHandler;
                }

                if (handler != null)
                {
                    app.Response.Filter = new ResponseFilter(app.Response.Filter, new HttpContextWrapper(app.Context));
                }
            }

            
        }

        /// <summary>
        /// Checks if the request MIME type matches the list of mime types specified in the config,
        /// if it does, then it compresses it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void app_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            
            //if debug is on, then don't compress
            if (!ConfigurationHelper.IsCompilationDebug)
            {
                MimeTypeCompressor c = new MimeTypeCompressor(app.Context);
                c.AddCompression();
            }
        }
        
    }
}
