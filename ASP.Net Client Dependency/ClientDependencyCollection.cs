using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core
{

	/// <summary>
	/// Wraps a HashSet object for the client dependency file type and declares a equality operator
	/// </summary>
	public class ClientDependencyCollection : HashSet<IClientDependencyFile>
	{

		public ClientDependencyCollection() : base(new ClientDependencyComparer()) { }

		internal class ClientDependencyComparer : IEqualityComparer<IClientDependencyFile>
		{			
			#region IEqualityComparer<IClientDependencyFile> Members

			/// <summary>
			/// If the lowercased combination of the file path, dependency type and path name aliases are the same, 
			/// then they are the same dependency.
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			public bool Equals(IClientDependencyFile x, IClientDependencyFile y)
			{
				return (x.FilePath.ToUpper().Trim() + x.DependencyType.ToString().ToUpper() + x.PathNameAlias.ToUpper().Trim() ==
					y.FilePath.ToUpper().Trim() + y.DependencyType.ToString().ToUpper() + y.PathNameAlias.ToUpper().Trim());
			}

			public int GetHashCode(IClientDependencyFile obj)
			{
                return (obj.FilePath.ToUpper().Trim() + obj.DependencyType.ToString().ToUpper() + obj.PathNameAlias.ToUpper().Trim())
					.GetHashCode();
			}

			#endregion
		}

	}

	
}
