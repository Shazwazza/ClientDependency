using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core.Controls;

[assembly: WebResource("ClientDependency.Web.Test.Pages.embedded.css", "text/css")]

namespace ClientDependency.Web.Test.Pages
{

    public partial class EmbeddedResourceTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            var embeddedCssPath = Page.ClientScript.GetWebResourceUrl(typeof(EmbeddedResourceTest), "ClientDependency.Web.Test.Pages.embedded.css");

            //embed the web resource! sweeet.
            ClientDependencyLoader.Instance.RegisterDependency(embeddedCssPath, Core.ClientDependencyType.Css);

        }
    }
}