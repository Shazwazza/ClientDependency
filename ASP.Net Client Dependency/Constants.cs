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
}