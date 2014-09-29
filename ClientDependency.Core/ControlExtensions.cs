using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace ClientDependency
{
    public static class ControlExtensions
    {

        public static IEnumerable<Control> FlattenChildren(this Control control)
        {
            var children = control.Controls.Cast<Control>().ToArray();
            return children.SelectMany(FlattenChildren).Concat(children);
        }

    }
}
