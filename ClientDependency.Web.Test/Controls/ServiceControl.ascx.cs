using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClientDependency.Core.Web.Test.Controls
{
	public partial class ServiceControl : System.Web.UI.UserControl
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			//var svc = new ServiceReference("~/Services/TestService.asmx");
			//var mgr = ScriptManager.GetCurrent(this.Page);
			//if (!mgr.Services.Contains(svc))
			//    mgr.Services.Add(svc);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			ScriptManager.RegisterStartupScript(this, this.GetType(), "Blah", "setTimeout(function() {alert('starting');doThis();},5000);", true);
			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "asdf", @"
				function doThis() {
						alert(""starting"");
						var svc = ClientDependency.Core.Web.Test.Services.TestService;
						svc.HelloWorld(function(sender, e) {
							alert(e.d);
						});
					}", true);
		}
	}
}