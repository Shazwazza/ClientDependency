using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection.NestedProviders;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.ModelBinding;


namespace ClientDependency.vNext.Test
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseErrorPage();

            app.UseServices(services =>
            {
                services.AddMvc();
                services.AddCdf();
            });

            app.UseMvc();

            app.UseWelcomePage();
        }       
    }
    
}
