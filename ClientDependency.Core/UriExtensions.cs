using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Net;
using ClientDependency.Core.Config;

namespace ClientDependency.Core
{
    public class NetworkHelper
    {
        /// <summary>
        /// Generally used for unit tests to get access to the settings
        /// </summary>
        internal static Func<ClientDependencySection> GetConfigSection;

        /// <summary>
        /// Returns the current machine name
        /// </summary>
        /// <remarks>
        /// Tries to resolve the machine name, if it cannot it uses the config section
        /// </remarks>
        public static string MachineName
        {
            get
            {
                var section = GetConfigSection == null
                                  ? ClientDependencySettings.GetDefaultSection()
                                  : GetConfigSection();

                if (!string.IsNullOrEmpty(section.MachineName))
                {
                    //return the config specified machine name
                    return section.MachineName;
                }

                try
                {
                    return Environment.MachineName;
                }
                catch
                {
                    try
                    {
                        return System.Net.Dns.GetHostName();
                    }
                    catch
                    {
                        //if we get here it means we cannot access the machine name
                        throw new ApplicationException("Cannot resolve the current machine name eithe by Environment.MachineName or by Dns.GetHostname(). Because of either security restrictions applied to this server or network issues not being able to resolve the hostname you will need to specify an explicity host name in the ClientDependency config section");
                    }
                }
            }
        }
    }

    public static class UriExtensions
    {
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
