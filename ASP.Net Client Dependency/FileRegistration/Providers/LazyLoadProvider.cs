using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

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

		protected override void RegisterJsFiles(List<IClientDependencyFile> jsDependencies)
		{
			if (jsDependencies.Count == 0)
				return;

			RegisterDependencyLoader();

			if (IsDebugMode)
			{
				foreach (IClientDependencyFile dependency in jsDependencies)
				{
					ProcessSingleJsFile(string.Format("'{0}','{1}'", dependency.FilePath, string.Empty));
				}
			}
			else
			{
				string js = ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript);				

				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Processed composite list: {0}", js));

				ProcessSingleJsFile(string.Format("'{0}','{1}'", js, string.Empty));
			}
		}

		protected override void ProcessSingleJsFile(string js)
		{
            StringBuilder strClientLoader = new StringBuilder("CDLazyLoader");
			DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", js));
			strClientLoader.AppendFormat(".AddJs({0})", js);
			strClientLoader.Append(';');
			RegisterScript(strClientLoader.ToString());
		}

		protected override void RegisterCssFiles(List<IClientDependencyFile> cssDependencies)
		{
			if (cssDependencies.Count == 0)
				return;

			RegisterDependencyLoader();

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

				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Processed composite list: {0}", css));

				ProcessSingleCssFile(css);
			}

			
		}

		protected override void ProcessSingleCssFile(string css)
		{
            StringBuilder strClientLoader = new StringBuilder("CDLazyLoader");
			DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", css));
			strClientLoader.AppendFormat(".AddCss('{0}')", css);
			strClientLoader.Append(';');
			RegisterScript(strClientLoader.ToString());			
		}

		/// <summary>
		/// register loader script
		/// </summary>
		private void RegisterDependencyLoader()
		{
			if (!HttpContext.Current.Items.Contains(DependencyLoaderResourceName))
			{
				RegisterScriptFile(DependencyLoaderResourceName);
				HttpContext.Current.Items[DependencyLoaderResourceName] = true;
			}
		}

		private void RegisterScriptFile(string scriptPath)
		{
			DependantControl.Page.ClientScript.RegisterClientScriptResource(typeof(LazyLoadProvider), scriptPath);
		}

		private void RegisterScript(string strScript)
		{
			ScriptManager mgr = ScriptManager.GetCurrent(DependantControl.Page);

			if (mgr == null)
			{
				DependantControl.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), strScript.GetHashCode().ToString(), strScript, true);
			}
			else
			{
				ScriptManager.RegisterClientScriptBlock(DependantControl, this.GetType(), strScript.GetHashCode().ToString(), strScript, true);
			}
		}

	}

}
