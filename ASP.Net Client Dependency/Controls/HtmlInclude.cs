using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
 
namespace ClientDependency.Core.Controls
{
    [ToolboxData("<{0}:HtmlInclude runat=\"server\"></{0}:HtmlInclude>")]
    public class HtmlInclude : Literal
    {
        public const int DefaultPriority = 100;
        public const string TagPattern = @"<{0}((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>";
        public const string AttributePattern = @"{0}(\s*=\s*(?:""(?<val>.*?)""|'(?<val>.*?)'|(?<val>[^'"">\s]+)))";

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
            RegisterCssIncludes(innerHtml, loader);
            RegisterJsIncludes(innerHtml, loader);
        }

        private void RegisterCssIncludes(string innerHtml, ClientDependencyLoader loader)
        {
            var tagPattern = string.Format(TagPattern, "link");
            var typeAttributePattern = string.Format(AttributePattern, "type");
            var srcAttributePattern = string.Format(AttributePattern, "href");

            var count = 0;
            foreach (Match match in Regex.Matches(innerHtml, tagPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                var typeMatch = Regex.Match(match.Value, typeAttributePattern,
                                            RegexOptions.Compiled | RegexOptions.IgnoreCase |
                                            RegexOptions.CultureInvariant);

                if (typeMatch.Success && typeMatch.Groups["val"].Value == "text/css")
                {
                    var srcMatch = Regex.Match(match.Value, srcAttributePattern,
                                            RegexOptions.Compiled | RegexOptions.IgnoreCase |
                                            RegexOptions.CultureInvariant);

                    if (srcMatch.Success)
                    {
                        loader.RegisterDependency(DefaultPriority + count,
                            srcMatch.Groups["val"].Value,
                            ClientDependencyType.Css);

                        count++;
                    }
                }
            }
        }

        private void RegisterJsIncludes(string innerHtml, ClientDependencyLoader loader)
        {
            var tagPattern = string.Format(TagPattern, "script");
            var typeAttributePattern = string.Format(AttributePattern, "type");
            var srcAttributePattern = string.Format(AttributePattern, "src");

            var count = 0;
            foreach (Match match in Regex.Matches(innerHtml, tagPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                var typeMatch = Regex.Match(match.Value, typeAttributePattern,
                                            RegexOptions.Compiled | RegexOptions.IgnoreCase |
                                            RegexOptions.CultureInvariant);

                if (typeMatch.Success && typeMatch.Groups["val"].Value == "text/javascript")
                {
                    var srcMatch = Regex.Match(match.Value, srcAttributePattern,
                                            RegexOptions.Compiled | RegexOptions.IgnoreCase |
                                            RegexOptions.CultureInvariant);

                    if (srcMatch.Success)
                    {
                        loader.RegisterDependency(DefaultPriority + count,
                            srcMatch.Groups["val"].Value,
                            ClientDependencyType.Javascript);

                        count++;
                    }
                }
            }
        }
    }
}
