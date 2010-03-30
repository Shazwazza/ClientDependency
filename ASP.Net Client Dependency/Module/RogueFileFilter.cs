using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using ClientDependency.Core;
using System.Net;

namespace ClientDependency.Core.Module
{

    /// <summary>
    /// Used as an http response filter to modify the contents of the output html.
    /// This filter is used to intercept js and css rogue registrations on the html page.
    /// </summary>
    public class RogueFileFilter : IFilter
    {
        public RogueFileFilter(HttpContextBase ctx)
        {
            CurrentContext = ctx;
        }

        #region Private members
        
        private string m_MatchScript = "<script(?:(?:.*(?<src>(?<=src=\")[^\"]*(?=\"))[^>]*)|[^>]*)>(?<content>(?:(?:\n|.)(?!(?:\n|.)<script))*)</script>";
        private string m_MatchLink = "<link\\s+[^>]*(href\\s*=\\s*(['\"])(?<href>.*?)\\2)";

        #endregion

        #region IFilter Members

        /// <summary>
        /// Replaces any rogue script tag's with calls to the compression handler instead 
        /// of just the script.
        /// </summary>
        public string UpdateOutputHtml(string html)
        {
            html = ReplaceScripts(html);
            html = ReplaceStyles(html);
            return html;
        }

        public HttpContextBase CurrentContext { get; private set; }

        #endregion

        #region Private methods


        /// <summary>
        /// Replaces all src attribute values for a script tag with their corresponding 
        /// URLs as a composite script.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Need to add caching to the src match found and what we returned for it so this doesn't
        /// get processed every request.
        /// </remarks>
        private string ReplaceScripts(string html)
        {
            //check if we should be processing!
            if (ClientDependencySettings.Instance.ProcessRogueJSFiles)
            {
                return ReplaceContent(html, "src", ".js", ClientDependencyType.Javascript, m_MatchScript);
            }            
            return html;
        }

        /// <summary>
        /// Replaces all href attribute values for a link tag with their corresponding 
        /// URLs as a composite style.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Need to add caching to the src match found and what we returned for it so this doesn't
        /// get processed every request.
        /// </remarks>
        private string ReplaceStyles(string html)
        {
            //check if we should be processing!
            if (ClientDependencySettings.Instance.ProcessRogueCSSFiles)
            {
                return ReplaceContent(html, "href", ".css", ClientDependencyType.Css, m_MatchLink);
            }
            return html;
        }

        private string ReplaceContent(string html, string namedGroup, string extension, ClientDependencyType type, string regex)
        {
            html = Regex.Replace(html, regex,
                (m) =>
                {
                    var grp = m.Groups[namedGroup];

                    //if there is no namedGroup group name or it doesn't end with a js/css extension or it's already using the composite handler,
                    //the return the existing string.
                    if (grp == null
                        || string.IsNullOrEmpty(grp.ToString())
                        || !grp.ToString().ToUpper().EndsWith(extension.ToUpper())
                        || grp.ToString().StartsWith(ClientDependencySettings.Instance.CompositeFileHandlerPath))
                        return m.ToString();

                    //make sure that it's an internal request, though we can deal with external 
                    //requests, we'll leave that up to the developer to register an external request
                    //explicitly if they want to include in the composite scripts.
                    try
                    {
                        var url = new Uri(grp.ToString(), UriKind.RelativeOrAbsolute);
                        if (!url.IsLocalUri())
                            return m.ToString(); //not a local uri                       
                    }
                    catch (UriFormatException)
                    {
                        //malformed url, let's exit
                        return m.ToString();
                    }

                    return m.ToString().Replace(grp.ToString(),
                        BaseFileRegistrationProvider.GetCompositeFileUrl(grp.ToString(), type));
                },
                RegexOptions.Compiled);

            return html;
        }


        #endregion

    }
}
