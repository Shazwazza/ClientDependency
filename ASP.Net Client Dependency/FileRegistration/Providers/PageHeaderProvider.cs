using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Linq;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public class PageHeaderProvider : WebFormsFileRegistrationProvider
	{		

		public const string DefaultName = "PageHeaderProvider";
		

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

		protected override string RenderSingleJsFile(string js)
		{
            return string.Format(HtmlEmbedContants.ScriptEmbed, js);
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

		protected override string RenderSingleCssFile(string css)
		{
            return string.Format(HtmlEmbedContants.CssEmbed, css);
		}

        protected override void RegisterDependencies(Control dependantControl, string js, string css)
        {
            if (dependantControl.Page.Header == null)
                throw new NullReferenceException("PageHeaderProvider requires a runat='server' tag in the page's header tag");

            LiteralControl jsScriptBlock = new LiteralControl(js);
            LiteralControl cssStyleBlock = new LiteralControl(css);
            dependantControl.Page.Header.Controls.Add(cssStyleBlock);
            dependantControl.Page.Header.Controls.Add(jsScriptBlock);

        }

		
	}
}
