using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.FileRegistration.Providers
{
	public class HtmlEmbedContants
	{
		public const string ScriptEmbedWithSource = "<script type=\"text/javascript\" src=\"{0}\"></script>";
		public const string CssEmbedWithSource = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
        
        public const string ScriptEmbedWithCode = "<script type=\"text/javascript\">{0}</script>";
        //public const string CssEmbed = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
	}
}
