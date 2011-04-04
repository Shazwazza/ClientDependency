using System;
using System.Collections.Generic;
using System.Text;

namespace ClientDependency.Core
{


    public static class Constants
    {
        /// <summary>
        /// If a priority is not set, the default will be 100.
        /// </summary>
        /// <remarks>
        /// This will generally mean that if a developer doesn't specify a priority it will come after all other dependencies that 
        /// have unless the priority is explicitly set above 100.
        /// </remarks>
        public const int DefaultPriority = 100;
    }

	/// <summary>
	/// This attribute is used for data types that uses client assets like Javascript and CSS for liveediting.
	/// The Live Editing feature in umbraco will look for this attribute and preload all dependencies to the page
	/// to ensure that all client events and assets gets loaded
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ClientDependencyAttribute : Attribute, IClientDependencyFile
	{
        public ClientDependencyAttribute()
        {
            Priority = Constants.DefaultPriority;
			PathNameAlias = "";
        }

       

		/// <summary>
		/// Gets or sets the priority.
		/// </summary>
		/// <value>The priority.</value>
		public int Priority { get; set; }

		/// <summary>
		/// This can be empty and will use default provider
		/// </summary>
		public string ForceProvider { get; set; }

		/// <summary>
		/// Gets or sets the file path.
		/// </summary>
		/// <value>The file path.</value>
		public string FilePath { get; set; }

		/// <summary>
		/// The path alias to be pre-pended to the file path if specified.
		/// The alias is specified in in the ClientDependencyHelper constructor.
		/// If the alias specified does not exist in the ClientDependencyHelper
		/// path collection, an exception is thrown.
		/// </summary>
		public string PathNameAlias { get; set; }

		/// <summary>
		/// Gets or sets the type of the dependency.
		/// </summary>
		/// <value>The type of the dependency.</value>
		public ClientDependencyType DependencyType { get; set; }

		public ClientDependencyAttribute(ClientDependencyType dependencyType, string fullFilePath)
            : this(Constants.DefaultPriority, dependencyType, fullFilePath, string.Empty)
		{ }

		public ClientDependencyAttribute(ClientDependencyType dependencyType, string fileName, string pathNameAlias)
            : this(Constants.DefaultPriority, dependencyType, fileName, pathNameAlias)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
		/// </summary>
		/// <param name="priority">The priority.</param>
		/// <param name="dependencyType">Type of the dependency.</param>
		/// <param name="fullFilePath">The file path to the dependency.</param>
		public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string fullFilePath)
			: this(priority, dependencyType, fullFilePath, string.Empty)
		{ }


	    /// <summary>
	    /// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
	    /// </summary>
	    /// <param name="priority">The priority.</param>
	    /// <param name="dependencyType">Type of the dependency.</param>
	    /// <param name="fileName"></param>
	    /// <param name="pathNameAlias"></param>
	    public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string fileName, string pathNameAlias)
		{
			if (String.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("fileName");

			Priority = priority;


			FilePath = fileName;
			PathNameAlias = pathNameAlias;

			DependencyType = dependencyType;
		}
        
	}

	
}
