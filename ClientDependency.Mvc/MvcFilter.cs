using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ClientDependency.Core.Module;

namespace ClientDependency.Core.Mvc
{
    public class MvcFilter : IFilter
    {

        #region IFilter Members

        public void SetHttpContext(HttpContextWrapper ctx)
        {
            CurrentContext = ctx;
        }

        public bool CanExecute()
        {
            return (CurrentContext.CurrentHandler is MvcHandler);
        }

        public bool ValidateCurrentHandler()
        {
            return (CurrentContext.CurrentHandler is MvcHandler);
        }

        public string UpdateOutputHtml(string html)
        {
            //first we need to check if this is MVC!
            if (CurrentContext.CurrentHandler is MvcHandler)
            {
                //parse the html output with the renderer
                var r = DependencyRenderer.Instance(CurrentContext);
                if (r != null)
                {
                    return r.ParseHtmlPlaceholders(html);
                }                
            }
            return html;            
        }

        public HttpContextBase CurrentContext { get; private set; }
      

        #endregion

    }
}
