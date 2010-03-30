using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ClientDependency.Core.Module
{
    internal interface IFilter
    {
        string UpdateOutputHtml(string html);
        HttpContextBase CurrentContext { get; }
    }
}
