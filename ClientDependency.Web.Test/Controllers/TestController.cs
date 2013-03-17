using System.Web.Mvc;
using ClientDependency.Core.Mvc;
using ClientDependency.Web.Test.Models;

namespace ClientDependency.Web.Test.Controllers
{

    //NOTE: Yes i realize this text shouldn't be hard coded here, but it's an example website!

    [HandleError]
    public class TestController : Controller
    {

        public ActionResult ImportsCss()
        {
            var model = new TestModel()
            {
                Heading = "This tests @import statement in css",
                BodyContent = @"<p>The Imports.css file is used here which also imports the Content.css and the OverrideStyles.css</p>"
            };

            return View(model);

        }

        public ActionResult Default()
        {

            var model = new TestModel()
            {
                Heading = "Using the default provider specified in the web.config",
                BodyContent = @"<p>
Nothing fancy here, just rendering the script and style blocks using the default renderer. 
This library ships with the <b>StandardRenderer</b> set to the default renderer. 
The default renderer will render out standard script/style html blocks wherever you specify using
the HtmlHelper:</p>
<p>
&lt;%= Html.RenderCssHere(new BasicPath(""Styles"", ""/Css"")) %&gt;
</p>
<p>
To make a page/view/etc... dependent on a script or file just use this HtmlHelper methods:
</p>
<p>
<% Html.RequiresCss(""Site.css"", ""Styles""); %><br/>
&lt;% Html.RequiresJs(""/Js/jquery-1.3.2.min.js""); %&gt;
</p>
"
            };

            return View(model);
        }

        public ActionResult RogueDependencies()
        {
            var model = new TestModel()
            {
                Heading = "Replacing rogue scripts/styles with composite files",
                BodyContent = @"<p>
This page demonstrates the replacement of 'Rogue' script/style (link) tags that exist in the raw html of the page. These scripts get replaced with the compression handler URL and are handled then just like other scripts that are being rendered via a client dependency object. 
</p>
<p>
The term 'Rogue' refers to a script/styles that hasn't been registered on the page with the ClientDependency framework and instead is registered as raw script/style tags. 
</p>
<p>
IMPORTANT: Please note that although Rogue scripts/styles get replaced with compressed scripts/styles using this framework, it still means that there will be more requests when they are not properly registered because rogue scripts are not combined into one request! 
</p>
"
            };

            return View(model);
        }

        public ActionResult HtmlAttributes()
        {
            var model = new TestModel()
            {
                Heading = "Some dependencies are have custom html attributes",
                BodyContent = @"<p>
On this page we have 2 dependencies registered with custom html attributes:
</p>
<ul>
<li>A print style sheet with a custom media='print' attribute</li>
<li>A js dependency with a type='text/jquery' which could be used as a jquery template</li>
</ul>
"
            };

            return View(model);
        }

        public ActionResult DynamicPathRegistration()
        {
            var model = new TestModel()
            {
                Heading = "Dynamic path registration",
                BodyContent = @"<p>
In the MVC Action for this page, we've dynamically added a path registration and have dynamically added a 2nd one in the view.
</p>
<p>
There are a few extension methods to acheive this, the direct way is to get an instance of the DependencyRenderer by calling the extension method
GetLoader() on either the ControllerContext, ViewContext or HttpContextBase, then you can just use the AddPath methods. <br/>
Otherwise, if you are working in a view, you can use the HtmlHelper methods: RegisterPathAlias
</p>
"
            };

            ControllerContext.GetLoader().AddPath("NewJsPath", "~/Js/TestPath");

            return View(model);
        }

        public ActionResult RemoteDependencies()
        {
            var model = new TestModel()
            {
                Heading = "Some dependencies are from remote servers",
                BodyContent = @"<p>
On this page, we've got the jquery library loaded from our local server with a priority of '1', but we've got the jquery UI registered with a file path from the Google CDN with a priority of '3'
</p>
<p>
In the source of this page, ClientDependency has split the registrations for JavaScript so that everthing found before the jQuery UI lib is compressed, combined, etc.. then the jQuery UI lib is registered for downloading from the CDN, then everything after is again compressed, combined, etc...
</p>
"
            };

            return View(model);
        }

        
    }
}
