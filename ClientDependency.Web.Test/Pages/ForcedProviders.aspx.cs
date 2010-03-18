using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core;

namespace ClientDependency.Web.Test.Pages
{
	public partial class FocedProviders : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            //dynamically register the dependency
            ClientDependencyLoader.Instance.RegisterDependency("Content.css", "Styles", ClientDependencyType.Css);

        }
    }
}
