using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core
{
    public static class StringExtensions
    {

        /// <summary>
        /// checks if the string ends with one of the strings specified. This ignores case.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static bool EndsWithOneOf(this string str, string[] ext)
        {
            var upper = str.ToUpper();
            bool isExt = false;
            foreach (var e in ext)
            {
                if (upper.EndsWith(e.ToUpper()))
                {
                    isExt = true;
                    break;
                }
            }
            return isExt;
        }

    }
}
