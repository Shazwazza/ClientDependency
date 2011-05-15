using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public class StandardRenderer : BaseRenderer
    {

        public const string DefaultName = "StandardRenderer";

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
                name = DefaultName;

            base.Initialize(name, config);
        }

        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http)
        {
            if (!jsDependencies.Any())
                return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
            {
                foreach (var dependency in jsDependencies)
                {
                    sb.Append(RenderSingleJsFile(dependency.FilePath));
                }
            }
            else
            {
                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript, http);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleJsFile(s));
                }                
            }

            return sb.ToString();
        }

        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http)
        {
            if (!cssDependencies.Any())
                return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
            {
                foreach (var dependency in cssDependencies)
                {
                    sb.Append(RenderSingleCssFile(dependency.FilePath));
                }
            }
            else
            {
                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(cssDependencies, ClientDependencyType.Css, http);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleCssFile(s));
                }    
            }

            return sb.ToString();
        }

        protected override string RenderSingleJsFile(string js)
        {
            return string.Format(HtmlEmbedContants.ScriptEmbedWithSource, js);
        }

        protected override string RenderSingleCssFile(string css)
        {
            return string.Format(HtmlEmbedContants.CssEmbedWithSource, css);
        }

    }
}
