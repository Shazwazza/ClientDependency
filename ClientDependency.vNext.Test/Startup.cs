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

                //// Add a view engine as the first one in the list.
                //services.AddMvc(configuration)
                //    .SetupOptions<MvcOptions>(options =>
                //    {
                //        options.ViewEngines.Insert(0, typeof(TestViewEngine));
                //    });

                ////NOTE: An alternative to parsing the output would be to use a custom ActionResult that derives from
                //// ViewResult, that way there theoretically would be full control over the 'writer'
                //// https://github.com/aspnet/Mvc/blob/c17d33154f52fa8d8553484767683e0d3932fa11/src/Microsoft.AspNet.Mvc.Core/ActionResults/ViewResult.cs

                ////This replaces the default view type with our own

                var config = new Configuration();
                var describe = new ServiceDescriber(config);                
                //services.Add(describe.Instance<IMvcRazorHost>(new MvcRazorHost(typeof(CdfRazorView).FullName)));
                services.Add(describe.Transient<IRazorView, CdfRazorView>());
            });

            app.UseMvc();

            app.UseWelcomePage();
        }       
    }

    //NOTE: This is here PURELY to be used as a type argument above, it actually doesn't get used or invoked.
    // and for some reason if you try to force this view the @inherits directive doesn't seem to work
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
            using (var replacements = new PlaceholderReplacer(context.Writer/*, viewContext.HttpContext*/))
            {
                context.Writer = replacements.Writer;

                await base.RenderAsync(context);
            }
        }
    }
   
    //public abstract class CdfRazorView<TModel> : RazorPage<TModel>
    //{       

    //    public string GetMyString() {
            
    //        return "Blah!!";

    //    }

        // public override Task RenderAsync(ViewContext context)
        // {
            // using (var replacements = new PlaceholderReplacer(context.Writer/*, viewContext.HttpContext*/))
            // {
                // context.Writer = replacements.Writer;

                // return base.RenderAsync(context);
            // }

            
        // }
    //}

    //public abstract class CustomRazorView<TModel> : RazorPage<TModel>
    //{       

    //    public string EatMyShorts() {
            
    //        return "Blah!!";

    //    }

    //    // public override Task RenderAsync(ViewContext context)
    //    // {
    //        // return base.RenderAsync(context);            
    //    // }
    //}

    //public static class ViewEnginesExtensions
    //{
    //    /// <summary>
    //    /// Replaces the default razor view engine with the specified one
    //    /// </summary>
    //    /// <param name="engines"></param>
    //    /// <param name="replacement"></param>
    //    public static void ReplaceDefaultRazorEngine(this ViewEngineCollection engines, IViewEngine replacement)
    //    {
    //        engines.ReplaceEngine<RazorViewEngine>(replacement);
    //    }

    //    /// <summary>
    //    /// Replaces the engine matching 'T' with the specified one
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="engines"></param>
    //    /// <param name="replacement"></param>
    //    public static void ReplaceEngine<T>(this ViewEngineCollection engines, IViewEngine replacement)
    //        where T : IViewEngine
    //    {
    //        var engine = engines.SingleOrDefault(x => x.GetType() == typeof(T));
    //        if (engine != null)
    //        {
    //            engines.Remove(engine);
    //        }
    //        engines.Add(replacement);
    //    }
    //}

    public class PlaceholderReplacer : IDisposable
    {
        private readonly TextWriter _originalWriter;
        //private readonly HttpContext _httpContext;
        private readonly StringWriter _writer;

        public PlaceholderReplacer(TextWriter originalWriter /*, HttpContext httpContext*/)
        {
            _originalWriter = originalWriter;
            //_httpContext = httpContext;
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
            //var replaced = DependencyRenderer.GetInstance(_httpContext).ParseHtmlPlaceholders(output);

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
