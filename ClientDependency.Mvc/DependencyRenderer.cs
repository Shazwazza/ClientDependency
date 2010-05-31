using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Linq;
using ClientDependency.Core.Config;
using ClientDependency.Core.FileRegistration.Providers;
using System.Web.Mvc;
using ClientDependency.Core.Controls;
using System.IO;
using System.Text.RegularExpressions;

namespace ClientDependency.Core.Mvc
{
    public class DependencyRenderer : BaseLoader
    {

        /// <summary>
        /// Constructor based on MvcHandler 
        /// </summary>
        /// <param name="handler"></param>
        private DependencyRenderer(HttpContextBase ctx)
            :base(ContextKey)
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

        #region Static methods

        /// <summary>
        /// used for locking
        /// </summary>
        private static readonly object m_Locker = new object();

        /// <summary>
        /// Singleton per request instance.
        /// </summary>
        /// <exception cref="NullReferenceException">
        /// If no MvcDependencyLoader control exists on the context, an exception is thrown.
        /// </exception>
        public static DependencyRenderer Instance(HttpContextBase ctx)
        {
            if (!ctx.Items.Contains(ContextKey))
                return null;
            return ctx.Items[ContextKey] as DependencyRenderer;
        }

        /// <summary>
        /// Checks if a loader already exists, if it does, it returns it, otherwise it will
        /// create a new one in the control specified.
        /// isNew will be true if a loader was created, otherwise false if it already existed.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        internal static DependencyRenderer TryCreate(HttpContextBase ctx, out bool isNew)
        {
            if (DependencyRenderer.Instance(ctx) == null)
            {
                lock (m_Locker)
                {
                    //double check
                    if (DependencyRenderer.Instance(ctx) == null)
                    {
                        DependencyRenderer loader = new DependencyRenderer(ctx);
                        isNew = true;
                        return loader;
                    }                    
                }
                
            }
           
            isNew = false;
            return DependencyRenderer.Instance(ctx);       

        }

        #endregion

        #region Internal Methods
        
        /// <summary>
        /// This replaces the HTML placeholders that we're rendered into the html
        /// markup before the module calls this method to update the placeholders with 
        /// the real dependencies.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        internal string ParseHtmlPlaceholders(string html)
        {
            GenerateOutput();

            html = Regex.Replace(html, JsMarkupRegex,
                (m) =>
                {
                    var grp = m.Groups["renderer"];
                    if (grp != null)
                    {                        
                        return m_Output
                            .Where(x => x.Name == grp.ToString())
                            .Single()
                            .OutputJs;
                    }
                    return m.ToString();
                }, RegexOptions.Compiled);

            html = Regex.Replace(html, CssMarkupRegex,
                (m) =>
                {
                    var grp = m.Groups["renderer"];
                    if (grp != null)
                    {
                        return m_Output
                            .Where(x => x.Name == grp.ToString())
                            .Single()
                            .OutputCss;
                    }
                    return m.ToString();
                }, RegexOptions.Compiled);
            
            return html;
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
            m_Paths.UnionWith(paths);

            return string.Format("<!--[{0}:Name=\"{1}\"]//-->"
                , type.ToString()
                , rendererName);
        } 

        #endregion


        private List<RendererOutput> m_Output = new List<RendererOutput>();


        private void GenerateOutput()
        {
            //Loop through each object and
            //get the output for both js and css from each provider in the list
            //based on each list items dependencies.
            m_Dependencies
                .ToList()
                .ForEach(x =>
                {
                    var renderer = ((BaseRenderer)x.Provider);
                    string js, css;
                    renderer.RegisterDependencies(x.Dependencies, m_Paths, out js, out css);

                    //store the output in a new output object
                    m_Output.Add(new RendererOutput()
                    {
                        Name = x.Provider.Name,
                        OutputCss = css,
                        OutputJs = js
                    });
                });                       

        }

        private class RendererOutput
        {
            public string Name { get; set; }
            public string OutputJs { get; set; }
            public string OutputCss { get; set; }
        }

        
    }
}
