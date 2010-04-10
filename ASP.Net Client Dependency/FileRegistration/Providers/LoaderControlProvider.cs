using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ClientDependency.Core.Controls;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public class LoaderControlProvider : WebFormsFileRegistrationProvider
	{
		
        public const string DefaultName = "LoaderControlProvider";

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

            if (ConfigurationHelper.IsCompilationDebug)
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

            if (ConfigurationHelper.IsCompilationDebug)
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

        protected override void RegisterDependencies(Control dependantControl, string js, string css)
        {
            AddToControl(dependantControl, css);
            AddToControl(dependantControl, js);
        }

		private void AddToControl(Control dependantControl, string literal)
		{
          
			LiteralControl dCtl = new LiteralControl(literal);
          	ClientDependencyLoader.Instance.Controls.Add(dCtl);           
		}
	}
}
