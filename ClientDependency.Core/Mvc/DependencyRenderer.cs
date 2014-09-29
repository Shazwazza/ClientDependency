using System;
using System.Collections.Generic;
using System.Web;
using ClientDependency.Config;
using ClientDependency.FileRegistration;

namespace ClientDependency.Mvc
{
    /// <summary>
    /// This is the class that controls rendering dependencies for MVC
    /// </summary>
    public class DependencyRenderer : BaseLoader
    {
        /// <summary>
        /// Constructor based on MvcHandler 
        /// </summary>
        /// <param name="ctx"></param>
        private DependencyRenderer(HttpContext ctx)
            : base(ctx)
        {
            //by default the provider is the default provider 
            Provider = ClientDependencySettings.Instance.DefaultMvcRenderer;
            if (ctx.Items.Contains(ContextKey))
                throw new InvalidOperationException("Only one ClientDependencyLoader may exist in a context");
            ctx.Items[ContextKey] = this;
        }


        #region Constants
        public const string ContextKey = "MvcLoader";
        private const string JsMarkupRegex = "<!--\\[Javascript:Name=\"(?<renderer>.*?)\"\\]//-->";
        private const string CssMarkupRegex = "<!--\\[Css:Name=\"(?<renderer>.*?)\"\\]//-->";
        #endregion
        
        #region Internal Methods
        
        /// <summary>
        /// This replaces the HTML placeholders that we're rendered into the html
        /// markup before the module calls this method to update the placeholders with 
        /// the real dependencies.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string ParseHtmlPlaceholders(string html)
        {
            GenerateOutput();

            return PlaceholderParser.ParseHtmlPlaceholders(CurrentContext, html, JsMarkupRegex, CssMarkupRegex, _output.ToArray());
        }

        /// <summary>
        /// Renders the HTML markup placeholder with the default provider name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        internal string RenderPlaceholder(ClientDependencyType type, IEnumerable<IClientDependencyPath> paths)
        {
            return RenderPlaceholder(type, Provider.Name, paths);
        }

        /// <summary>
        /// Renders the HTML markup placeholder with the provider specified by rendererName
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rendererName"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        internal string RenderPlaceholder(ClientDependencyType type, string rendererName, IEnumerable<IClientDependencyPath> paths)
        {
            Paths.UnionWith(paths);

            return string.Format("<!--[{0}:Name=\"{1}\"]//-->"
                , type
                , rendererName);
        }

        #endregion


        private readonly List<RendererOutput> _output = new List<RendererOutput>();

        /// <summary>
        /// Loop through each object and
        /// get the output for both js and css from each provider in the list
        /// based on each list items dependencies.
        /// </summary>
        /// <remarks>
        /// For some reason ampersands that aren't html escaped are not compliant to HTML standards when they exist in 'link' or 'script' tags in URLs,
        /// we need to replace the ampersands with &amp; . This is only required for this one w3c compliancy, the URL itself is a valid URL.
        /// </remarks>
        private void GenerateOutput()
        {
            foreach (var x in Dependencies)
            {
                var renderer = ((BaseRenderer)x.Provider);
                string js, css;
                renderer.RegisterDependencies(x.Dependencies, Paths, out js, out css, CurrentContext);

                //store the output in a new output object
                _output.Add(new RendererOutput()
                {
                    Name = x.Provider.Name,
                    OutputCss = css,
                    OutputJs = js
                });
            }
        }

        


    }
}
