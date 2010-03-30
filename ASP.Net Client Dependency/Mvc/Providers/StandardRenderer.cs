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

        protected override void RegisterJsFiles(List<IClientDependencyFile> jsDependencies)
        {
            if (jsDependencies.Count == 0)
                return;

            if (IsDebugMode)
            {
                foreach (IClientDependencyFile dependency in jsDependencies)
                {
                    ProcessSingleJsFile(dependency.FilePath);
                }
            }
            else
            {
                string js = ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript);
                ProcessSingleJsFile(js);
            }
        }

        protected override void RegisterCssFiles(List<IClientDependencyFile> cssDependencies)
        {
            if (cssDependencies.Count == 0)
                return;

            if (IsDebugMode)
            {
                foreach (IClientDependencyFile dependency in cssDependencies)
                {
                    ProcessSingleCssFile(dependency.FilePath);
                }
            }
            else
            {
                string css = ProcessCompositeList(cssDependencies, ClientDependencyType.Css);
                ProcessSingleCssFile(css);
            }
        }

        protected override void ProcessSingleJsFile(string js)
        {
            JsOutput.Append(string.Format(HtmlEmbedContants.ScriptEmbed, js));
        }

        protected override void ProcessSingleCssFile(string css)
        {
            CssOutput.Append(string.Format(HtmlEmbedContants.CssEmbed, css));
        }

    }
}
