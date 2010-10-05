﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected override string RenderJsDependencies(List<IClientDependencyFile> jsDependencies)
        {
            if (jsDependencies.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (ConfigurationHelper.IsCompilationDebug || !EnableCompositeFiles)
            {
                foreach (IClientDependencyFile dependency in jsDependencies)
                {
                    sb.Append(RenderSingleJsFile(dependency.FilePath));
                }
            }
            else
            {
                var comp = ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleJsFile(s));
                }                
            }

            return sb.ToString();
        }

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
            return string.Format(HtmlEmbedContants.ScriptEmbedWithSource, js);
        }

        protected override string RenderSingleCssFile(string css)
        {
            return string.Format(HtmlEmbedContants.CssEmbedWithSource, css);
        }

    }
}