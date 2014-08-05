using System.Web;
using dotless.Core.Input;

namespace ClientDependency.Less
{
    class CdfFileReader : dotless.Core.Input.IFileReader
    {
        public CdfFileReader()
        {
            _innerReader = new FileReader(
                new CdfPathResolver(new HttpContextWrapper(HttpContext.Current), (string)HttpContext.Current.Items["Cdf_LessWriter_origUrl"]));
        }

        private readonly FileReader _innerReader;

        public byte[] GetBinaryFileContents(string fileName)
        {
            return _innerReader.GetBinaryFileContents(fileName);
        }

        public string GetFileContents(string fileName)
        {
            return _innerReader.GetFileContents(fileName);
        }

        public bool DoesFileExist(string fileName)
        {
            return _innerReader.DoesFileExist(fileName);
        }

        public bool UseCacheDependencies
        {
            get { return false; }
        }
    }
}