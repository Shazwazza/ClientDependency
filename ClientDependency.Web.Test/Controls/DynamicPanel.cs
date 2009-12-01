using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClientDependency.Core.Web.Test.Controls
{
	public class DynamicPanel : UpdatePanel
	{

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			EnsureChildControls();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			var btn = new Button();
			btn.Text = "Submit";
			btn.ID = "SubmitButton";
			btn.Click += new EventHandler(btn_Click);
			ContentTemplateContainer.Controls.Add(btn);
		}

		void btn_Click(object sender, EventArgs e)
		{
			EnsureChildControls();
			var ctl = Page.LoadControl("~/Controls/ServiceControl.ascx");
			ctl.ID = "MyNewControl";
			ContentTemplateContainer.Controls.Add(ctl);
		}

	}
}
