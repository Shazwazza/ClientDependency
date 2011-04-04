using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using ClientDependency.Core.Controls;

namespace ClientDependency.Core.Mvc
{
    /// <summary>
    /// HtmlHelper extensions
    /// </summary>
    public static class HtmlHelperExtensions
    {

        #region RequiresJsResource Extensions
        public static HtmlHelper RequiresJsResource(this HtmlHelper html, Type resourceType, string resourcePath, int priority)
        {
            var page = new Page();
            var resourceUrl = page.ClientScript.GetWebResourceUrl(resourceType, resourcePath);
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new NullReferenceException("Could not find embedded resource " + resourcePath + " in assembly " + resourceType.Assembly.FullName);
            }

            html.ViewContext.GetLoader().RegisterDependency(priority, resourceUrl, ClientDependencyType.Javascript);
            return html;
        }

        public static HtmlHelper RequiresJsResource(this HtmlHelper html, Type resourceType, string resourcePath)
        {
            return html.RequiresJsResource(resourceType, resourcePath, Constants.DefaultPriority);
        } 
        #endregion

        #region RequiresCssResource Extensions
        public static HtmlHelper RequiresCssResource(this HtmlHelper html, Type resourceType, string resourcePath, int priority)
        {
            var page = new Page();
            var resourceUrl = page.ClientScript.GetWebResourceUrl(resourceType, resourcePath);
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new NullReferenceException("Could not find embedded resource " + resourcePath + " in assembly " + resourceType.Assembly.FullName);
            }

            html.ViewContext.GetLoader().RegisterDependency(priority, resourceUrl, ClientDependencyType.Css);
            return html;
        }

        public static HtmlHelper RequiresCssResource(this HtmlHelper html, Type resourceType, string resourcePath)
        {
            return html.RequiresCssResource(resourceType, resourcePath, Constants.DefaultPriority);
        } 
        #endregion

        #region RequiresJs Extensions
        public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, ClientDependencyType.Javascript);
            return html;
        }
        public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, string pathNameAlias)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, pathNameAlias, ClientDependencyType.Javascript);
            return html;
        }
        public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, string pathNameAlias, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, pathNameAlias, ClientDependencyType.Javascript);
            return html;
        }
        public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, ClientDependencyType.Javascript);
            return html;
        } 
        #endregion

        #region RequiresCss Extensions
        public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, ClientDependencyType.Css);
            return html;
        }
        public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, string pathNameAlias)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, pathNameAlias, ClientDependencyType.Css);
            return html;
        }
        public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, string pathNameAlias, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, pathNameAlias, ClientDependencyType.Css);
            return html;
        }
        public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, ClientDependencyType.Css);
            return html;
        } 
        #endregion

        #region RenderJsHere Extensions
        public static string RenderJsHere(this HtmlHelper html)
        {
            return html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Javascript, new List<IClientDependencyPath>());
        }
        public static string RenderJsHere(this HtmlHelper html, IClientDependencyPath path)
        {
            return html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Javascript, new List<IClientDependencyPath>() { path });
        }
        public static string RenderJsHere(this HtmlHelper html, IEnumerable<IClientDependencyPath> paths)
        {
            return html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Javascript, paths);
        }
        public static string RenderJsHere(this HtmlHelper html, string rendererName, IEnumerable<IClientDependencyPath> paths)
        {
            return html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Javascript, rendererName, paths);
        } 
        #endregion
        
        #region RenderCssHere Extensions
        public static string RenderCssHere(this HtmlHelper html)
        {
            return html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, new List<IClientDependencyPath>());
        }
        public static string RenderCssHere(this HtmlHelper html, IClientDependencyPath path)
        {
            return html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, new List<IClientDependencyPath>() { path });
        }
        public static string RenderCssHere(this HtmlHelper html, IEnumerable<IClientDependencyPath> paths)
        {
            return html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, paths);
        }
        public static string RenderCssHere(this HtmlHelper html, string rendererName, IEnumerable<IClientDependencyPath> paths)
        {
            return html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, rendererName, paths);
        } 
        #endregion
    }
}
