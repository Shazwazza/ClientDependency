namespace ClientDependency.CompositeFiles
{
	/// <summary>
	/// A simple class defining a Uri string and whether or not it is a local application file
	/// </summary>
	public class CompositeFileDefinition
	{
		public CompositeFileDefinition(string uri, bool isLocalFile)
		{
			IsLocalFile = isLocalFile;
			Uri = uri;
		}
		public bool IsLocalFile { get; set; }
		public string Uri { get; set; }

		public override bool Equals(object obj)
		{
			return (obj.GetType() == this.GetType()
				&& ((CompositeFileDefinition)obj).IsLocalFile.Equals(IsLocalFile)
				&& ((CompositeFileDefinition)obj).Uri.Equals(Uri));
		}

        /// <summary>
        /// overrides hash code to ensure that it is unique per machine
        /// </summary>
        /// <returns></returns>
		public override int GetHashCode()
		{
            return (NetworkHelper.MachineName + Uri).GetHashCode();
		}
	}
}
