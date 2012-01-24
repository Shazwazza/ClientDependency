using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Query.Dynamic;

namespace ClientDependency.Core
{
    internal static class HtmlAttributesStringParser
    {

        internal static void ParseIntoDictionary(string attributes, IDictionary<string, string> destination)
        {
            destination.Clear();

            if (string.IsNullOrEmpty(attributes))
                return;
            if (destination == null)
                return;
            
            var key = "";
            var val = "";
            var isKey = true;
            var isVal = false;
            var isValDelimited = false;
            for (var i = 0; i < attributes.Length; i++)
            {
                var c = attributes.ToCharArray()[i];
                if (c == ':')
                {
                    isKey = false;
                    isVal = true;
                    continue;
                }

                if (isKey)
                {
                    key += c;
                }

                if (isVal)
                {
                    if (c == '\'')
                    {
                        if (!isValDelimited)
                        {
                            isValDelimited = true;
                            continue;
                        }
                        else
                        {
                            isValDelimited = false;
                            if ((i == attributes.Length - 1))
                            {
                                //if it the end, add the value
                                destination.Add(key, val);
                            }
                            continue;
                        }
                    }
                    
                    if (c == ',' && !isValDelimited)
                    {
                        //we've reached a comma and the value is not longer delimited, this means we create a new key
                        isKey = true;
                        isVal = false;

                        //now we can add the current value to the dictionary
                        destination.Add(key, val);
                        continue;
                    }
                    
                    val += c;

                    if ((i == attributes.Length - 1))
                    {
                        //if it the end, add the value
                        destination.Add(key, val);
                    }
                }
            }

        }
    }
}