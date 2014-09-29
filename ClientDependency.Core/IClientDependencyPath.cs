namespace ClientDependency
{
	public interface IClientDependencyPath
	{

		string Name { get; set; }
		string Path { get; set; }
		bool ForceBundle { get; set; }

	}
}
