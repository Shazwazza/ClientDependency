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

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCdf(this IServiceCollection services)
        {
            //This replaces the default view type with our own
            var config = new Configuration();
            var describe = new ServiceDescriber(config);
            return services.Add(describe.Transient<IRazorView, CdfRazorView>());
        }
    }

    public class CdfRazorView : RazorView
    {
        public CdfRazorView(IRazorPageFactory pageFactory,
                         IRazorPageActivator pageActivator,
                         IViewStartProvider viewStartProvider)
            : base(pageFactory, pageActivator, viewStartProvider)
        {
        }

        public override async Task RenderAsync(ViewContext context)
        {
            using (var replacements = new PlaceholderReplacer(context.Writer))
            {
                context.Writer = replacements.Writer;

                await base.RenderAsync(context);
            }
        }
    }

    public class PlaceholderReplacer : IDisposable
    {
        private readonly TextWriter _originalWriter;
        private readonly StringWriter _writer;

        public PlaceholderReplacer(TextWriter originalWriter)
        {
            _originalWriter = originalWriter;
            _writer = new StringWriter(new StringBuilder());
        }

        public TextWriter Writer
        {
            get { return _writer; }
        }

        private void PerformReplacements()
        {
            var output = _writer.ToString();

            //do replacements
            var replaced = DependencyRenderer.GetInstance(_httpContext).ParseHtmlPlaceholders(output);

            //var replaced = "Hello world, this is now the output of your view!";

            //write to original
            _originalWriter.Write(output);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Does the replacements and disposes local resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            
            PerformReplacements();
            _writer.Dispose();
        }
    }
}
