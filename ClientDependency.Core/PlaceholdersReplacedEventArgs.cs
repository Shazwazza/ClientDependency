using System;
using System.Web;

namespace ClientDependency
{
    internal class PlaceholdersReplacedEventArgs : EventArgs
    {
        public HttpContext HttpContext { get; private set; }        
        public string ReplacedText { get; set; }

        public PlaceholdersReplacedEventArgs(HttpContext httpContext, string replacedText)
        {
            HttpContext = httpContext;
            ReplacedText = replacedText;
        }
    }
}