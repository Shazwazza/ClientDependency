using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ClientDependency.Core.Controls;
using ClientDependency.Core.Config;
using System.Web;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public class LoaderControlProvider : WebFormsFileRegistrationProvider
	{
		
        public const string DefaultName = "LoaderControlProvider";
        string _dependenciesWebSite;

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			// Assign the provider a default name if it doesn't have one
			if (string.IsNullOrEmpty(name))
				name = DefaultName;

			RegisterDependenciesWebSite(config);
			base.Initialize(name, config);
		}

        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http)
		{
			if (!jsDependencies.Any())
				return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
			{
				foreach (var dependency in jsDependencies)
				{
                    sb.Append(RenderSingleJsFile(dependency.FilePath));
				}
			}
			else
			{
                var comp = ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript, http);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleJsFile(s));
                }                    
			}

            return sb.ToString();
		}

        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http)
		{
            if (!cssDependencies.Any())
                return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
			{
				foreach (var dependency in cssDependencies)
				{
                    sb.Append(RenderSingleCssFile(dependency.FilePath));
				}
			}
			else
			{
                var comp = ProcessCompositeList(cssDependencies, ClientDependencyType.Css, http);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleCssFile(s));
                }    
			}

            return sb.ToString();
		}

        protected override string RenderSingleJsFile(string js)
		{
            return string.Format(HtmlEmbedContants.ScriptEmbedWithSource, MapToDependenciesWebSite(js));
		}

        protected override string RenderSingleCssFile(string css)
		{
            return string.Format(HtmlEmbedContants.CssEmbedWithSource, MapToDependenciesWebSite(css));
		}

        /// <summary>
        /// Registers the dependencies as controls of the LoaderControl controls collection
        /// </summary>
        /// <param name="http"></param>
        /// <param name="js"></param>
        /// <param name="css"></param>
        /// <remarks>
        /// For some reason ampersands that aren't html escaped are not compliant to HTML standards when they exist in 'link' or 'script' tags in URLs,
        /// we need to replace the ampersands with &amp; . This is only required for this one w3c compliancy, the URL itself is a valid URL.
        /// </remarks>
        protected override void RegisterDependencies(HttpContextBase http, string js, string css)
        {
            AddToControl(http, css.Replace("&", "&amp;"));
            AddToControl(http, js.Replace("&", "&amp;"));
        }

        private static void AddToControl(HttpContextBase http, string literal)
		{          
			var dCtl = new LiteralControl(literal);
          	ClientDependencyLoader.GetInstance(http).Controls.Add(dCtl);           
		}

		void RegisterDependenciesWebSite(System.Collections.Specialized.NameValueCollection config)
		{
			_dependenciesWebSite = config["website"];
			if (!string.IsNullOrEmpty(_dependenciesWebSite))
				_dependenciesWebSite = _dependenciesWebSite.TrimEnd('/');
		}

		string MapToDependenciesWebSite(string url)
		{
			if (url.StartsWith("http://"))
				return url;

			// make sure the url begins with a /
			string slashedUrl = (url[0] != '/' ? "/" : string.Empty) + url;

			if (!string.IsNullOrEmpty(_dependenciesWebSite))
			{
				return _dependenciesWebSite + slashedUrl;
			}
			else
			{
				// if no dependencies website is configured then serve content ourselves
				// which means, map the url to the current application path

				System.Web.HttpContext context = System.Web.HttpContext.Current;
				System.Web.HttpRequest request = context.Request;
				string applicationPath = request.ApplicationPath;

				// request.ApplicationPath is '/<vdir>' so it can be either '/' or '/foo'
				if (applicationPath == "/")
					return url;
				else
					return applicationPath + slashedUrl;
			}
		}
	}
}
