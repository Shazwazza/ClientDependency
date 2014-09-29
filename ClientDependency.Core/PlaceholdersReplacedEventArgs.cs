using System;
using System.Web;

namespace ClientDependency
{
    internal class PlaceholdersReplacedEventArgs : EventArgs
    {
        public HttpContextBase HttpContext { get; private set; }        
        public string ReplacedText { get; set; }

        public PlaceholdersReplacedEventArgs(HttpContextBase httpContext, string replacedText)
        {
            HttpContext = httpContext;
            ReplacedText = replacedText;
        }
    }
}