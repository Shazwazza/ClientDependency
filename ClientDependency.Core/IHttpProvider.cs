using System.Web;

namespace ClientDependency
{
    /// <summary>
    /// A provider that requires initialization under an Http context.
    /// The Http initialization will happen after the standard provider initialization.
    /// </summary>
    public interface IHttpProvider
    {

        void Initialize(HttpContextBase http);

    }
}
