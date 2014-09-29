using System.Web;

namespace ClientDependency.Module
{
    public interface IFilter
    {
        void SetHttpContext(HttpContextBase ctx);
        string UpdateOutputHtml(string html);
        HttpContextBase CurrentContext { get; }
        bool CanExecute();
        bool ValidateCurrentHandler();
    }
}
