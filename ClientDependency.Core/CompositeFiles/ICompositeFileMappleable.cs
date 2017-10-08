using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.CompositeFiles
{
    /// <summary>
	/// Structure and base methods of the XML stored in the map file
	/// </summary>
	public interface ICompositeFileMappleable
    {
        string FileKey { get;  }
        string CompositeFileName { get;  }
        string CompressionType { get;  }
        int Version { get;  }
        IEnumerable<string> DependentFiles { get;  }
        
        /// <summary>
        /// If for some reason the file doesn't exist any more or we cannot read the file, this should return false.
        /// </summary>
        bool HasFileBytes { get; }

        /// <summary>
        /// Returns the file's bytes
        /// </summary>
        byte[] GetCompositeFileBytes();
    }
}
