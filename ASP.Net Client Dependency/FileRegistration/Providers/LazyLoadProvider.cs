using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public class LazyLoadProvider : WebFormsFileRegistrationProvider
	{

		public const string DefaultName = "LazyLoadProvider";

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			// Assign the provider a default name if it doesn't have one
			if (string.IsNullOrEmpty(name))
				name = DefaultName;

			base.Initialize(name, config);
		}

		/// <summary>Path to the dependency loader we need for adding control dependencies.</summary>
        protected const string DependencyLoaderResourceName = "ClientDependency.Core.Resources.LazyLoader.js";

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
                var comp = ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", s, string.Empty)));
                }   
			}

            return sb.ToString();
		}

        protected override string RenderSingleJsFile(string js)
		{
            StringBuilder strClientLoader = new StringBuilder("CDLazyLoader");
			strClientLoader.AppendFormat(".AddJs({0})", js);
			strClientLoader.Append(';');
            return strClientLoader.ToString();
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
            StringBuilder strClientLoader = new StringBuilder("CDLazyLoader");
			strClientLoader.AppendFormat(".AddCss('{0}')", css);
			strClientLoader.Append(';');
            return strClientLoader.ToString();
		}

        protected override void RegisterDependencies(Control dependantControl, string js, string css)
        {
            dependantControl.Page.ClientScript.RegisterClientScriptResource(typeof(LazyLoadProvider), DependencyLoaderResourceName);

            RegisterScript(js, dependantControl);
            RegisterScript(css, dependantControl);
        }		

        private void RegisterScript(string strScript, Control dependantControl)
		{
            ScriptManager mgr = ScriptManager.GetCurrent(dependantControl.Page);

			if (mgr == null)
			{
                if (dependantControl.Page.Form == null)
                    throw new InvalidOperationException("A form tag must be present on the page with a runat='server' attribute specified");
                dependantControl.Page.ClientScript.RegisterStartupScript(this.GetType(), strScript.GetHashCode().ToString(), strScript, true);
			}
			else
			{
                ScriptManager.RegisterStartupScript(dependantControl, this.GetType(), strScript.GetHashCode().ToString(), strScript, true);
			}
		}

	}

}
