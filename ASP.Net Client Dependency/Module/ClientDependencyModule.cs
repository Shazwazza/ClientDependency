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
        void IHttpModule.Init(HttpApplication context)
        {
            context.PostMapRequestHandler += new EventHandler(context_PostMapRequestHandler);
        }

        /// <summary>
        /// This event handler adds a response filter to the current response object
        /// if the response is being handled by a Page or MvcHandler object.
        /// If it is a different type of handler, then everything is ignored.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void context_PostMapRequestHandler(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;

            //ensure it's a page handler or mvchandler
            IHttpHandler handler = app.Context.CurrentHandler as Page;
            if (handler == null)
            {
                handler = app.Context.CurrentHandler as MvcHandler;
            }

            if (handler != null)
            {
                app.Response.Filter = new RogueFileFilter(app.Response.Filter);
            }
        }

        #endregion

        
    }
}
