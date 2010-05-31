using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using System.Web.UI;
using System.Web;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public class LazyLoadRenderer : BaseRenderer
    {
        public const string DefaultName = "LazyLoadRenderer";

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
                name = DefaultName;

            base.Initialize(name, config);
        }

        /// <summary>Path to the dependency loader we need for adding control dependencies.</summary>
        protected const string DependencyLoaderResourceName = "ClientDependency.Core.Resources.LazyLoader.js";
        

        private static readonly object m_Locker = new object();
        

        /// <summary>
        /// This is silly to have to do this but MS don't give you a way in MVC to do this
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        private string GetWebResourceUrl(Type type, string resourceId)
        {
            if (type == null)
                type = this.GetType();

            Page page = new Page();
            return page.ClientScript.GetWebResourceUrl(type, resourceId);
        }

        /// <summary>
        /// This will check if the lazy loader script has been registered yet (it does this by storing a flah in the HttpContext.Items)
        /// if it hasn't then it will add the script registration to the StringBuilder
        /// </summary>
        /// <param name="sb"></param>
        private void RegisterLazyLoadScript(StringBuilder sb)
        {
            if (HttpContext.Current.Items["LazyLoaderLoaded"] == null || (bool)HttpContext.Current.Items["LazyLoaderLoaded"] == false)
            {
                lock (m_Locker)
                {
                    if (HttpContext.Current.Items["LazyLoaderLoaded"] == null || (bool)HttpContext.Current.Items["LazyLoaderLoaded"] == false)
                    {
                        HttpContext.Current.Items["LazyLoaderLoaded"] = true;

                        var url = GetWebResourceUrl(typeof(LazyLoadProvider), DependencyLoaderResourceName);
                        sb.Append(string.Format(HtmlEmbedContants.ScriptEmbedWithSource, url));   
                    }
                }
            }
        }

        protected override string RenderJsDependencies(List<IClientDependencyFile> jsDependencies)
        {
            if (jsDependencies.Count == 0)
				return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (ConfigurationHelper.IsCompilationDebug || !EnableCompositeFiles)
			{
				foreach (IClientDependencyFile dependency in jsDependencies)
				{
                    sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", dependency.FilePath, string.Empty)));
				}
			}
			else
			{

                RegisterLazyLoadScript(sb);

                var comp = ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", s, string.Empty)));
                }   
			}

            return sb.ToString();
        }

        /// <summary>
        /// Registers the Css dependencies. 
        /// </summary>
        /// <param name="cssDependencies"></param>
        /// <returns></returns>
        protected override string RenderCssDependencies(List<IClientDependencyFile> cssDependencies)
        {
            if (cssDependencies.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (ConfigurationHelper.IsCompilationDebug || !EnableCompositeFiles)
            {
                foreach (IClientDependencyFile dependency in cssDependencies)
                {
                    sb.Append(RenderSingleCssFile(dependency.FilePath));
                }
            }
            else
            {

                RegisterLazyLoadScript(sb);

                var comp = ProcessCompositeList(cssDependencies, ClientDependencyType.Css);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleCssFile(s));
                }
            }

            return sb.ToString();
        }

        protected override string RenderSingleJsFile(string js)
        {
            StringBuilder strClientLoader = new StringBuilder("CDLazyLoader");
            strClientLoader.AppendFormat(".AddJs({0})", js);
            strClientLoader.Append(';');

            return string.Format(HtmlEmbedContants.ScriptEmbedWithCode, strClientLoader.ToString());
        }

        protected override string RenderSingleCssFile(string css)
        {
            StringBuilder strClientLoader = new StringBuilder("CDLazyLoader");
            strClientLoader.AppendFormat(".AddCss('{0}')", css);
            strClientLoader.Append(';');

            return string.Format(HtmlEmbedContants.ScriptEmbedWithCode, strClientLoader);
        }
    }
}
