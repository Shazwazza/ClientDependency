using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.Controls
{
	public class CssInclude : ClientDependencyInclude
	{

		public CssInclude()
		{
			DependencyType = ClientDependencyType.Css;
		}
		public CssInclude(IClientDependencyFile file)
			: base(file)
		{
			DependencyType = ClientDependencyType.Css;
		}
	}
}
