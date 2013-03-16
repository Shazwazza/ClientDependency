using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
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

        /// <summary>
        /// Dynamically registers a path alias
        /// </summary>
        /// <param name="html"></param>
        /// <param name="pathAlias"></param>
        /// <param name="virtualPath"></param>
        public static void RegisterPathAlias(this HtmlHelper html, string pathAlias, string virtualPath)
        {
            html.ViewContext.GetLoader().AddPath(pathAlias, virtualPath);
        }

        /// <summary>
        /// Dynamically registers a path alias
        /// </summary>
        /// <param name="html"></param>
        /// <param name="path"></param>
        public static void RegisterPathAlias(this HtmlHelper html, IClientDependencyPath path)
        {
            html.ViewContext.GetLoader().AddPath(path);
        }


        #region RequiresJsResource Extensions
        public static HtmlHelper RequiresJsResource(this HtmlHelper html, Type resourceType, string resourcePath, int priority, int group)
        {
            var page = new Page();
            var resourceUrl = page.ClientScript.GetWebResourceUrl(resourceType, resourcePath);
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new NullReferenceException("Could not find embedded resource " + resourcePath + " in assembly " + resourceType.Assembly.FullName);
            }

            html.ViewContext.GetLoader().RegisterDependency(group, priority, resourceUrl, ClientDependencyType.Javascript);
            return html;
        }

		public static HtmlHelper RequiresJsResource(this HtmlHelper html, Type resourceType, string resourcePath, int priority)
		{
			return html.RequiresJsResource(resourceType, resourcePath, priority, Constants.DefaultGroup);
		}

        public static HtmlHelper RequiresJsResource(this HtmlHelper html, Type resourceType, string resourcePath)
        {
            return html.RequiresJsResource(resourceType, resourcePath, Constants.DefaultPriority);
        } 
        #endregion

        #region RequiresCssResource Extensions
        public static HtmlHelper RequiresCssResource(this HtmlHelper html, Type resourceType, string resourcePath, int priority, int group)
        {
            var page = new Page();
            var resourceUrl = page.ClientScript.GetWebResourceUrl(resourceType, resourcePath);
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new NullReferenceException("Could not find embedded resource " + resourcePath + " in assembly " + resourceType.Assembly.FullName);
            }

            html.ViewContext.GetLoader().RegisterDependency(group, priority, resourceUrl, ClientDependencyType.Css);
            return html;
        }

		public static HtmlHelper RequiresCssResource(this HtmlHelper html, Type resourceType, string resourcePath, int priority)
		{
			return html.RequiresCssResource(resourceType, resourcePath, priority, Constants.DefaultGroup);
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
		public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, string pathNameAlias, int priority, int group)
		{
			html.ViewContext.GetLoader().RegisterDependency(group, priority, filePath, pathNameAlias, ClientDependencyType.Javascript);
			return html;
		}
		public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, ClientDependencyType.Javascript);
            return html;
        }
		public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, int priority, int group)
		{
			html.ViewContext.GetLoader().RegisterDependency(group, priority, filePath, ClientDependencyType.Javascript);
			return html;
		}


        public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, object htmlAttributes)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, ClientDependencyType.Javascript, htmlAttributes);
            return html;
        }
        public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, string pathNameAlias, object htmlAttributes)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, pathNameAlias, ClientDependencyType.Javascript, htmlAttributes);
            return html;
        }
        public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, string pathNameAlias, int priority, object htmlAttributes)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, pathNameAlias, ClientDependencyType.Javascript, htmlAttributes);
            return html;
        }
		public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, string pathNameAlias, int priority, int group, object htmlAttributes)
		{
			html.ViewContext.GetLoader().RegisterDependency(group, priority, filePath, pathNameAlias, ClientDependencyType.Javascript, htmlAttributes);
			return html;
		}
		public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, int priority, object htmlAttributes)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, ClientDependencyType.Javascript, htmlAttributes);
            return html;
        }
		public static HtmlHelper RequiresJs(this HtmlHelper html, string filePath, int priority, int group, object htmlAttributes)
		{
			html.ViewContext.GetLoader().RegisterDependency(group, priority, filePath, ClientDependencyType.Javascript, htmlAttributes);
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
		public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, string pathNameAlias, int priority, int group)
		{
			html.ViewContext.GetLoader().RegisterDependency(group, priority, filePath, pathNameAlias, ClientDependencyType.Css);
			return html;
		}
		public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, ClientDependencyType.Css);
            return html;
        }
		public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, int priority, int group)
		{
			html.ViewContext.GetLoader().RegisterDependency(group, priority, filePath, ClientDependencyType.Css);
			return html;
		}

        public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, object htmlAttributes)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, ClientDependencyType.Css, htmlAttributes);
            return html;
        }
        public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, string pathNameAlias, object htmlAttributes)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, pathNameAlias, ClientDependencyType.Css, htmlAttributes);
            return html;
        }
        public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, string pathNameAlias, int priority, object htmlAttributes)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, pathNameAlias, ClientDependencyType.Css, htmlAttributes);
            return html;
        }
		public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, string pathNameAlias, int priority, int group, object htmlAttributes)
		{
			html.ViewContext.GetLoader().RegisterDependency(group, priority, filePath, pathNameAlias, ClientDependencyType.Css, htmlAttributes);
			return html;
		}
		public static HtmlHelper RequiresCss(this HtmlHelper html, string filePath, int priority, int group, object htmlAttributes)
        {
            html.ViewContext.GetLoader().RegisterDependency(group, priority, filePath, ClientDependencyType.Css, htmlAttributes);
            return html;
        } 
        #endregion

        #region RenderJsHere Extensions
        public static IHtmlString RenderJsHere(this HtmlHelper html)
        {
            return new HtmlString(
                html.ViewContext.GetLoader().RenderPlaceholder(
                    ClientDependencyType.Javascript, new List<IClientDependencyPath>()));
        }
        public static IHtmlString RenderJsHere(this HtmlHelper html, params IClientDependencyPath[] path)
        {
            return new HtmlString(
                html.ViewContext.GetLoader().RenderPlaceholder(
                    ClientDependencyType.Javascript, path));
        }
        public static IHtmlString RenderJsHere(this HtmlHelper html, IEnumerable<IClientDependencyPath> paths)
        {
            return new HtmlString(
                html.ViewContext.GetLoader().RenderPlaceholder(
                    ClientDependencyType.Javascript, paths));
        }
        public static IHtmlString RenderJsHere(this HtmlHelper html, string rendererName, IEnumerable<IClientDependencyPath> paths)
        {
            return new HtmlString(
                html.ViewContext.GetLoader().RenderPlaceholder(
                    ClientDependencyType.Javascript, rendererName, paths));
        } 
        #endregion
        
        #region RenderCssHere Extensions
        public static IHtmlString RenderCssHere(this HtmlHelper html)
        {
            return new HtmlString(html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, new List<IClientDependencyPath>()));
        }
        public static IHtmlString RenderCssHere(this HtmlHelper html, params IClientDependencyPath[] path)
        {
            return new HtmlString(html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, path));
        }
        public static IHtmlString RenderCssHere(this HtmlHelper html, IEnumerable<IClientDependencyPath> paths)
        {
            return new HtmlString(html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, paths));
        }
        public static IHtmlString RenderCssHere(this HtmlHelper html, string rendererName, IEnumerable<IClientDependencyPath> paths)
        {
            return new HtmlString(html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, rendererName, paths));
        }
        public static IHtmlString RenderCssHere(this HtmlHelper html, string rendererName, params IClientDependencyPath[] paths)
        {
            return new HtmlString(html.ViewContext.GetLoader().RenderPlaceholder(
                ClientDependencyType.Css, rendererName, paths));
        } 
        #endregion
    }
}
