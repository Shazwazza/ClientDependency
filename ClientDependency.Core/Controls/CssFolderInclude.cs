namespace ClientDependency.Core.Controls
{
    /// <summary>
    /// A control used to specify a Css folder dependency
    /// </summary>
    public class CssFolderInclude : DependencyFolderInclude
    {
        public CssFolderInclude() : base()
        {            
        }

        public CssFolderInclude(string folderVirtualPath) : base(folderVirtualPath)
        {                        
        }

        protected override ClientDependencyType DependencyType
        {
            get { return ClientDependencyType.Css; }
        }

        protected override string FileSearchPattern
        {
            get { return "*.css"; }
        }
    }
}