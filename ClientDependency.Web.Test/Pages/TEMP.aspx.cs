using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core.Web.Test.Controls;

namespace ClientDependency.Core.Web.Test.Pages
{
	public partial class TEMP : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{

			// add a ScriptManager to the form if not present
			if (ScriptManager.GetCurrent(Page) == null)
			{
				var mgr = new ScriptManager();
				mgr.EnablePartialRendering = true;
				Page.Form.Controls.Add(mgr);
			}

			MyPlaceholder.Controls.Add(new DynamicPanel());

			
		}

		
	}
}
