using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;

namespace ClientDependency.Core.Controls
{
    [ToolboxData("<{0}:HtmlInclude runat=\"server\"></{0}:HtmlInclude>")]
    public class HtmlInclude : Literal
    {
        public const int DefaultPriority = 100;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var isNew = false;
            var loader = ClientDependencyLoader.TryCreate(Page, out isNew);

            RegisterIncludes(Text, loader);

            Text = string.Empty;
        }

        private void RegisterIncludes(string innerHtml, ClientDependencyLoader loader)
        {
            var snippet = new HtmlDocument();
            snippet.LoadHtml(innerHtml);

            RegisterCssIncludes(snippet, loader);
            RegisterJsIncludes(snippet, loader);
        }

        private void RegisterCssIncludes(HtmlDocument snippet, ClientDependencyLoader loader)
        {
            var nodes = snippet.DocumentNode.SelectNodes(@"//link[@type='text/css' and @href]");
            if (nodes == null)
                return;

            var count = 0;
            foreach (var att in nodes.Select(node => node.Attributes["href"]))
            {
                loader.RegisterDependency(DefaultPriority + count,
                    att.Value,
                    ClientDependencyType.Css);

                count++;
            }
        }

        private void RegisterJsIncludes(HtmlDocument snippet, ClientDependencyLoader loader)
        {
            var nodes = snippet.DocumentNode.SelectNodes(@"//script[@type='text/javascript' and @src]");
            if(nodes == null)
                return;

            var count = 0;
            foreach (var att in nodes.Select(node => node.Attributes["src"]))
            {
                loader.RegisterDependency(DefaultPriority + count,
                    att.Value,
                    ClientDependencyType.Javascript);

                count++;
            }
        }
    }
}
