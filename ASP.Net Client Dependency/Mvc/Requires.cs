using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ClientDependency.Core.Mvc
{
    public class RenderDependencies
    {
        public static string Here()
        {
            return Here(new Dictionary<string, string>());
        }

        public static string Here(Dictionary<string, string> paths)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var p in paths)
            {
                sb.Append(p.Key + "=\"" + p.Value + "\",");
            }
            var s = sb.ToString().TrimEnd(',');
            return string.Format("<!--[cd:{0}]//-->", s);
        }
    }

    public class Requires
    {

        

        public static string Js(string filePath, string pathNameAlias, int priority)
        {
            return string.Format("<!--[js:src=\"{0}\",alias=\"{1}\",priority=\"{2}\"]//-->", filePath, pathNameAlias, priority);
        }
        public static string Js(string filePath, string pathNameAlias)
        {
            return string.Format("<!--[js:src=\"{0}\",alias=\"{1}\"]//-->", filePath, pathNameAlias);
        }
        public static string Js(string filePath)
        {
            return string.Format("<!--[js:src=\"{0}\"]//-->", filePath);
        }

        public static string Css(string filePath, string pathNameAlias, int priority)
        {
            return string.Format("<!--[css:src=\"{0}\",alias=\"{1}\",priority=\"{2}\"]//-->", filePath, pathNameAlias, priority);
        }
        public static string Css(string filePath, string pathNameAlias)
        {
            return string.Format("<!--[css:src=\"{0}\",alias=\"{1}\"]//-->", filePath, pathNameAlias);
        }
        public static string Css(string filePath)
        {
            return string.Format("<!--[css:src=\"{0}\"]//-->", filePath);
        }        

    }

    public static class RequiresJsExtensions
    {
        public static string RequiresJs(this ViewMasterPage ctl, string filePath)
        {
            //ctl.ViewContext.

            return string.Format("<!--[js:src=\"{0}\"]//-->", filePath);
        }

        //private RequiresJs() { }
        //static RequiresJs() 
        //{
        //    m_Instance = new RequiresJs();
        //}
        //public static RequiresJs Instance 
        //{
        //    get
        //    {
        //        return m_Instance;
        //    }
        //}        
        //private readonly static RequiresJs m_Instance;


        //public RequiresJs Add(string path)
        //{
        //    return this;
        //}
    }

    public class RequiresCss
    {
        private RequiresCss() { }
        static RequiresCss()
        {
            m_Instance = new RequiresCss();
        }
        public static RequiresCss Instance
        {
            get
            {
                return m_Instance;
            }
        }
        private readonly static RequiresCss m_Instance;


        public RequiresCss Add(string path)
        {
            return this;
        }
    }

    
}
