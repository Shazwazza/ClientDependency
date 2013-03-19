namespace ClientDependency.Core.Controls
{
    /// <summary>
    /// A control used to specify a Js folder dependency
    /// </summary>
    public class JsFolderInclude : DependencyFolderInclude
    {
        public JsFolderInclude()
            : base()
        {
        }

        public JsFolderInclude(string folderVirtualPath)
            : base(folderVirtualPath)
        {
        }

        protected override ClientDependencyType DependencyType
        {
            get { return ClientDependencyType.Javascript; }
        }

        protected override string FileSearchPattern
        {
            get { return "*.js"; }
        }
    }
}