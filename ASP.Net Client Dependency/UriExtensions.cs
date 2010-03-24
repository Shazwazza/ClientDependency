using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace ClientDependency.Core
{
    public static class UriExtensions
    {
        /// <summary>
        /// Checks if the url is a local/relative uri, if it is, it makes it absolute based on the 
        /// current request uri.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Uri MakeAbsoluteUri(this Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                string http = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                Uri absoluteUrl = new Uri(new Uri(http), uri);
                return absoluteUrl;
            }
            return uri;
        }

        /// <summary>
        /// Determines if the uri is a locally based file
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool IsLocalUri(this Uri uri)
        {
            IPAddress[] host;
            IPAddress[] local;
            bool isLocal = false;

            if (!uri.IsAbsoluteUri)
            {
                uri = uri.MakeAbsoluteUri();
            }

            host = Dns.GetHostAddresses(uri.Host);
            local = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress hostAddress in host)
            {
                if (IPAddress.IsLoopback(hostAddress))
                {
                    isLocal = true;
                    break;
                }
                else
                {
                    foreach (IPAddress localAddress in local)
                    {
                        if (hostAddress.Equals(localAddress))
                        {
                            isLocal = true;
                            break;
                        }
                    }

                    if (isLocal)
                    {
                        break;
                    }
                }
            }
            return isLocal;
        }
    }
}
