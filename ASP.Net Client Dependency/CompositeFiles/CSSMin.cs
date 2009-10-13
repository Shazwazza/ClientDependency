using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientDependency.Core.CompositeFiles
{
    public class CssMin
    {
        public static string CompressCSS(string body)
        {
            body = Regex.Replace(body, "/\\*.+?\\*/", "", RegexOptions.Singleline);
            body = body.Replace("  ", string.Empty);
            body = body.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, string.Empty);
            body = body.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            body = body.Replace(Environment.NewLine, string.Empty);
            body = body.Replace("\\t", string.Empty);
            body = body.Replace(" {", "{");
            body = body.Replace(" :", ":");
            body = body.Replace(": ", ":");
            body = body.Replace(", ", ",");
            body = body.Replace("; ", ";");
            body = body.Replace(";}", "}");
            body = Regex.Replace(body, "/\\*[^\\*]*\\*+([^/\\*]*\\*+)*/", "$1");
            body = Regex.Replace(body, "(?<=[>])\\s{2,}(?=[<])|(?<=[>])\\s{2,}(?=&nbsp;)|(?<=&ndsp;)\\s{2,}(?=[<])", string.Empty);

            return body;

        }
    }
}
