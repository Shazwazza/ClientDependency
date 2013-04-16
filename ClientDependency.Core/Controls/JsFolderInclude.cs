namespace ClientDependency.Core.Controls
{
    /// <summary>
    /// A control used to specify a Js folder dependency
    /// </summary>
    public class JsFolderInclude : DependencyFolderInclude
    {
        private const string SearchPattern = "*.css";

        public JsFolderInclude()
            : base()
        {
            FileSearchPattern = SearchPattern;
        }

        public JsFolderInclude(string folderVirtualPath)
            : base(folderVirtualPath)
        {
            FileSearchPattern = SearchPattern;
        }

        protected override ClientDependencyType DependencyType
        {
            get { return ClientDependencyType.Javascript; }
        }

    }
}