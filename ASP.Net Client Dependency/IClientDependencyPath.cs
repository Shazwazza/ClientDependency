using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency
{
	public interface IClientDependencyPath
	{

		string Name { get; set; }
		string Path { get; set; }
		string ResolvedPath { get; }
	}
}
