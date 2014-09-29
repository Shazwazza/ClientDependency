using System;

namespace ClientDependency
{
	public class BasicPath : IClientDependencyPath
	{
        public BasicPath() { }
        public BasicPath(string name, string path)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            Name = name;
            Path = path;
        }

		public string Name { get; set; }
		public string Path { get; set; }
		public bool ForceBundle { get; set; }

	    protected bool Equals(BasicPath other)
	    {
	        return string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase)
	               && string.Equals(Path, other.Path, StringComparison.InvariantCultureIgnoreCase);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != this.GetType()) return false;
	        return Equals((BasicPath) obj);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            return (Name.GetHashCode()*397) ^ Path.GetHashCode();
	        }
	    }
	}
}

