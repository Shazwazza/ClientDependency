using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Net;
using ClientDependency.Core.Config;

namespace ClientDependency.Core
{
    public static class UriExtensions
    {

        internal static string ToAbsolutePath(this Uri originalUri, string path)
        {
            var hashSplit = path.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);

            return string.Format(@"{0}{1}",
                                 path.StartsWith("http") ? path : new Uri(originalUri, path).PathAndQuery,
                                 hashSplit.Length > 1 ? ("#" + hashSplit[1]) : "");
        }

        /// <summary>
        /// Checks if the url is a local/relative uri, if it is, it makes it absolute based on the 
        /// current request uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        public static Uri MakeAbsoluteUri(this Uri uri, HttpContextBase http)
        {
            if (!uri.IsAbsoluteUri)
            {
                if (http.Request.Url != null)
                {
                    var left = http.Request.Url.GetLeftPart(UriPartial.Authority);
                    var absoluteUrl = new Uri(new Uri(left), uri);
                    return absoluteUrl;
                }
            }
            return uri;
        }

        /// <summary>
        /// Determines if the uri is a locally based file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        public static bool IsLocalUri(this Uri uri, HttpContextBase http)
        {
            var isLocal = false;

            if (!uri.IsAbsoluteUri)
            {
                uri = uri.MakeAbsoluteUri(http);
            }

            try
            {
                var host = Dns.GetHostAddresses(uri.Host);
                var local = Dns.GetHostAddresses(Dns.GetHostName());

                foreach (var hostAddress in host)
                {
                    if (IPAddress.IsLoopback(hostAddress))
                    {
                        isLocal = true;
                        break;
                    }
                    if (local.Contains(hostAddress))
                    {
                        isLocal = true;
                    }

                    if (isLocal)
                    {
                        break;
                    }
                }
                return isLocal;
            }
            catch (SocketException ex)
            {
                ClientDependencySettings.Instance.Logger.Error("SocketException occurred while checking for local address. Error: " + ex.Message, ex);
                
                //if DNS cannot be resolved, then we'll just return false
                return false;
            }
        }
    }
}
