using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.FileRegistration.Providers;

namespace ClientDependency.Core.Mvc.Providers
{
    public class StandardRenderer : BaseRenderer
    {

        public const string DefaultName = "StandardRenderer";

        protected override string RenderJsDependencies(List<IClientDependencyFile> jsDependencies)
        {
            if (jsDependencies.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (IsDebugMode)
            {
                foreach (IClientDependencyFile dependency in jsDependencies)
                {
                    sb.Append(RenderSingleJsFile(dependency.FilePath));
                }
            }
            else
            {
                sb.Append(RenderSingleJsFile(ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript)));
            }

            return sb.ToString();
        }

        protected override string RenderCssDependencies(List<IClientDependencyFile> cssDependencies)
        {
            if (cssDependencies.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (IsDebugMode)
            {
                foreach (IClientDependencyFile dependency in cssDependencies)
                {
                    sb.Append(RenderSingleCssFile(dependency.FilePath));
                }
            }
            else
            {
                sb.Append(RenderSingleCssFile(ProcessCompositeList(cssDependencies, ClientDependencyType.Css)));                
            }

            return sb.ToString();
        }

        protected override string RenderSingleJsFile(string js)
        {
            return string.Format(HtmlEmbedContants.ScriptEmbed, js);
        }

        protected override string RenderSingleCssFile(string css)
        {
            return string.Format(HtmlEmbedContants.CssEmbed, css);
        }

    }
}
