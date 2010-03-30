using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Mvc;

namespace ClientDependency.Core
{
	public class BasicPath : IClientDependencyPath
	{
        public BasicPath() { }
        public BasicPath(string name, string path)
        {
            Name = name;
            Path = path;
        }

		public string Name { get; set; }
		public string Path { get; set; }
		public string ResolvedPath
		{
			get
			{
                //return (HttpContext.Current.CurrentHandler as Page).ResolveUrl(Path);
                return VirtualPathUtility.ToAbsolute(Path);
			}
		}
	}
}

