using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ClientDependency.Core.Controls;

namespace ClientDependency.Core.Mvc
{
    public static class HtmlHelperExtensions
    {

        public static void RequiresJs(this HtmlHelper html, string filePath)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, ClientDependencyType.Javascript);
        }
        public static void RequiresJs(this HtmlHelper html, string filePath, string pathNameAlias)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, pathNameAlias, ClientDependencyType.Javascript);
        }
        public static void RequiresJs(this HtmlHelper html, string filePath, string pathNameAlias, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, pathNameAlias, ClientDependencyType.Javascript);
        }
        public static void RequiresJs(this HtmlHelper html, string filePath, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, ClientDependencyType.Javascript);
        }

        public static void RequiresCss(this HtmlHelper html, string filePath)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, ClientDependencyType.Css);
        }
        public static void RequiresCss(this HtmlHelper html, string filePath, string pathNameAlias)
        {
            html.ViewContext.GetLoader().RegisterDependency(filePath, pathNameAlias, ClientDependencyType.Css);
        }
        public static void RequiresCss(this HtmlHelper html, string filePath, string pathNameAlias, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, pathNameAlias, ClientDependencyType.Css);
        }
        public static void RequiresCss(this HtmlHelper html, string filePath, int priority)
        {
            html.ViewContext.GetLoader().RegisterDependency(priority, filePath, ClientDependencyType.Css);
        }

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
    }
}
