using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
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
        
        #region Private members
        
        private bool? m_Runnable = null;
        private string m_MatchScript = "<script(?:(?:.*(?<src>(?<=src=\")[^\"]*(?=\"))[^>]*)|[^>]*)>(?<content>(?:(?:\n|.)(?!(?:\n|.)<script))*)</script>";
        private string m_MatchLink = "<link\\s+[^>]*(href\\s*=\\s*(['\"])(?<href>.*?)\\2)";

        private RogueFileCompressionElement m_FoundPath = null;

        #endregion

        #region IFilter Members

        public void SetHttpContext(HttpContextWrapper ctx)
        {
            CurrentContext = ctx;
            m_FoundPath = GetSupportedPath();
        }

        /// <summary>
        /// This filter can only execute when it's a Page or MvcHandler
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public bool ValidateCurrentHandler()
        {
            //don't filter if we're in debug mode
            if (ConfigurationHelper.IsCompilationDebug)
                return false;

            IHttpHandler handler = CurrentContext.CurrentHandler as Page;
            if (handler != null)
            {
                return true;
            }
            return false;
        }
       
        /// <summary>
        /// Returns true when this filter should be applied
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public bool CanExecute()
        {
            if (!m_Runnable.HasValue)
            {
                m_Runnable = (m_FoundPath != null);
            }
            return m_Runnable.Value;

        }

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

        private RogueFileCompressionElement GetSupportedPath()
        {
            var rogueFiles = ClientDependencySettings.Instance
                .ConfigSection
                .CompositeFileElement
                .RogueFileCompression;

            foreach (var m in rogueFiles.Cast<RogueFileCompressionElement>())
            {
                //if it is only "*" then convert it to proper regex
                var reg = m.FilePath == "*" ? ".*" : m.FilePath;
                var matched = Regex.IsMatch(CurrentContext.Request.RawUrl, reg, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (matched)
                {
                    bool isGood = true;
                    //if we have a match, make sure there are no exclusions
                    foreach (var e in m.ExcludePaths.Cast<RogueFileCompressionExcludeElement>())
                    {
                        var excluded = Regex.IsMatch(CurrentContext.Request.RawUrl, e.FilePath, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        if (excluded)
                        {
                            isGood = false;
                            break;
                        }
                    }

                    if (isGood) return m;
                }
            }
            return null;
        }

        /// <summary>
        /// Replaces all src attribute values for a script tag with their corresponding 
        /// URLs as a composite script.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ReplaceScripts(string html)
        {
            //check if we should be processing!            
            if (CanExecute() && m_FoundPath.CompressJs)
            {
                return ReplaceContent(html, "src", m_FoundPath.JsRequestExtension.Split(','), ClientDependencyType.Javascript, m_MatchScript);
            }            
            return html;
        }

        /// <summary>
        /// Replaces all href attribute values for a link tag with their corresponding 
        /// URLs as a composite style.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ReplaceStyles(string html)
        {
            //check if we should be processing!            
            if (CanExecute() && m_FoundPath.CompressCss)
            {
                return ReplaceContent(html, "href", m_FoundPath.CssRequestExtension.Split(','), ClientDependencyType.Css, m_MatchLink);
            }
            return html;
        }

        private string ReplaceContent(string html, string namedGroup, string[] extensions, ClientDependencyType type, string regex)
        {
            html = Regex.Replace(html, regex,
                (m) =>
                {
                    var grp = m.Groups[namedGroup];

                    //if there is no namedGroup group name or it doesn't end with a js/css extension or it's already using the composite handler,
                    //the return the existing string.
                    if (grp == null
                        || string.IsNullOrEmpty(grp.ToString())
                        || !grp.ToString().EndsWithOneOf(extensions)
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

                    var dependency = new BasicFile(type) { FilePath = grp.ToString() };
                    var resolved = BaseFileRegistrationProvider.GetCompositeFileUrl(dependency.ResolveFilePath(), type);
                    return m.ToString().Replace(grp.ToString(),
                        resolved);
                },
                RegexOptions.Compiled);

            return html;
        }


        #endregion

    }
}
