using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ClientDependency.Core.Controls;

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

                DependantControl.Page.Trace.Write("ClientDependency", string.Format("Processed composite list: {0}", js));

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
				
				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Processed composite list: {0}", css));				
				
				ProcessSingleCssFile(css);
			}
		}

		protected override void ProcessSingleJsFile(string js)
		{
			DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", js));
			AddToControl(string.Format(HtmlEmbedContants.ScriptEmbed, js));
		}

		protected override void ProcessSingleCssFile(string css)
		{
			DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", css));
			AddToControl(string.Format(HtmlEmbedContants.CssEmbed, css));
		}

		private void AddToControl(string literal)
		{
			List<int> indexes = new List<int>();
			Type iDependency = typeof(IClientDependencyFile);
			foreach (Control ctl in ClientDependencyLoader.Instance.Controls)
			{
				if (ctl.ID != null && ctl.ID.StartsWith("CD_"))
					indexes.Add(DependantControl.Page.Header.Controls.IndexOf(ctl));
			}
			//now that we have all of the indexes of the client dependencies, we need to insert
			//the next one after the largest index
			int newIndex = indexes.Count == 0 ? 0 : indexes.Max() + 1;
			LiteralControl dCtl = new LiteralControl(literal);
			dCtl.ID = "CD_" + newIndex.ToString();
            if (newIndex >= ClientDependencyLoader.Instance.Controls.Count)
				ClientDependencyLoader.Instance.Controls.Add(dCtl);
			else
				ClientDependencyLoader.Instance.Controls.AddAt(newIndex, dCtl);


		}
	}
}
