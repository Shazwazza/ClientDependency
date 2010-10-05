using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ClientDependency.Core.Module;

namespace ClientDependency.Core.Mvc
{

    /// <summary>
    /// MvcFilter is required when using ClientDependency in MVC, without it, ClientDependency will not work.
    /// </summary>
    public class MvcFilter : IFilter
    {

        private MvcRogueFileFilter m_RogueFilter = new MvcRogueFileFilter();

        #region IFilter Members

        /// <summary>
        /// Sets the http context
        /// </summary>
        /// <param name="ctx"></param>
        public void SetHttpContext(HttpContextWrapper ctx)
        {
            CurrentContext = ctx;

            //set the context for the internal rogue filter
            m_RogueFilter.SetHttpContext(ctx);
        }

        public bool CanExecute()
        {
            return (CurrentContext.CurrentHandler is MvcHandler);
        }

        public bool ValidateCurrentHandler()
        {
            return (CurrentContext.CurrentHandler is MvcHandler);
        }

        /// <summary>
        /// Updates the html js/css templates rendered temporarily by the controls into real js/css html tags.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will also validate whether the rogue script handler should run and if so does.
        /// </remarks>
        public string UpdateOutputHtml(string html)
        {
            //first we need to check if this is MVC!
            if (CurrentContext.CurrentHandler is MvcHandler)
            {
                //parse the html output with the renderer
                var r = DependencyRenderer.Instance(CurrentContext);
                if (r != null)
                {
                    var output  = r.ParseHtmlPlaceholders(html);

                    //get the rogue filter going
                    if (m_RogueFilter.CanExecute())
                    {
                        output = m_RogueFilter.UpdateOutputHtml(output);
                    }

                    return output;
                }                
            }
            return html;            
        }

        public HttpContextBase CurrentContext { get; private set; }
      

        #endregion

    }
}
