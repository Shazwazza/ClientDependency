using System;
using System.Collections.Generic;
using System.Text;

namespace ClientDependency.Core
{
	public interface IClientDependencyFile
	{
		string FilePath { get; set; }
		ClientDependencyType DependencyType { get; }
		int Priority { get; set; }
        string PathNameAlias { get; set; }
		string ForceProvider { get; set; }

        /// <summary>
        /// Used to store additional attributes in the HTML markup for the item
        /// </summary>
        /// <remarks>
        /// Mostly used for CSS Media, but could be for anything
        /// </remarks>
        IEnumerable<KeyValuePair<string, string>> AdditionalAttributes { get; }
	}
}
