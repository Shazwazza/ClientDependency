using System.Web;
using ClientDependency.Module;

namespace ClientDependency.Mvc
{
    /// <summary>
    /// Rogue file filter for ASP.Net MVC
    /// </summary>
    internal class MvcRogueFileFilter : RogueFileFilter
    {

        /// <summary>
        /// Overridden to check for MVC Handler
        /// </summary>
        /// <returns></returns>
        public override bool ValidateCurrentHandler()
        {
            //don't filter if we're in debug mode
            if (CurrentContext.IsDebuggingEnabled)
                return false;

            IHttpHandler handler = CurrentContext.CurrentHandler as MvcHandler;
            if (handler != null)
            {
                return true;
            }
            return false;
        }

    }    
}
