using System;
using System.Collections.Generic;
using System.Linq;
using ClientDependency.Core.CompositeFiles;

#if !Net35
using System.Collections.Concurrent;
#endif

namespace ClientDependency.Core
{
    /// <summary>
    /// Defines the file writers for file extensions or for explicit file paths
    /// </summary>
    public class FileWriters
    {

#if !Net35
        private static readonly ConcurrentDictionary<string, IFileWriter> ExtensionWriters = new ConcurrentDictionary<string, IFileWriter>();
        private static readonly ConcurrentDictionary<string, IFileWriter> PathWriters = new ConcurrentDictionary<string, IFileWriter>();
        private static readonly ConcurrentDictionary<string, IVirtualFileWriter> VirtualExtensionWriters = new ConcurrentDictionary<string, IVirtualFileWriter>();
        private static readonly ConcurrentDictionary<string, IVirtualFileWriter> VirtualPathWriters = new ConcurrentDictionary<string, IVirtualFileWriter>();
#else
        private static readonly Dictionary<string, IFileWriter> ExtensionWriters = new Dictionary<string, IFileWriter>();
        private static readonly Dictionary<string, IFileWriter> PathWriters = new Dictionary<string, IFileWriter>();
        private static readonly Dictionary<string, IVirtualFileWriter> VirtualExtensionWriters = new Dictionary<string, IVirtualFileWriter>();
        private static readonly Dictionary<string, IVirtualFileWriter> VirtualPathWriters = new Dictionary<string, IVirtualFileWriter>();

        private static readonly object DictionaryLocker = new object();
#endif

        
        private static readonly IFileWriter DefaultFileWriter = new DefaultFileWriter();

        /// <summary>
        /// Returns the default writer
        /// </summary>
        /// <returns></returns>
        public static IFileWriter GetDefault()
        {
            return DefaultFileWriter;
        }

        /// <summary>
        /// returns all extensions that have been registered
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<string> GetRegisteredExtensions()
        {
            return ExtensionWriters.Select(x => x.Key.ToUpper()).Distinct()
                .Union(VirtualExtensionWriters.Select(x => x.Key.ToUpper()).Distinct());
        }

        /// <summary>
        /// This will add or update a writer for a specific file extension
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <param name="?"></param>
        /// <param name="writer"></param>
        public static void AddWriterForExtension(string fileExtension, IVirtualFileWriter writer)
        {
            if (fileExtension == null) throw new ArgumentNullException("fileExtension");
            if (writer == null) throw new ArgumentNullException("writer");

            if (!fileExtension.StartsWith("."))
            {
                throw new FormatException("A file extension must begin with a '.'");
            }

#if !Net35
            VirtualExtensionWriters.AddOrUpdate(fileExtension.ToUpper(), s => writer, (s, fileWriter) => writer);
#else
            lock (DictionaryLocker)
            {
                VirtualExtensionWriters[fileExtension.ToUpper()] = writer;
            }
#endif

            
        }

        /// <summary>
        /// Returns the writer for the file extension, if none is found then the null will be returned
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public static IVirtualFileWriter GetVirtualWriterForExtension(string fileExtension)
        {
            if (fileExtension == null) throw new ArgumentNullException("fileExtension");

            IVirtualFileWriter writer;
            return VirtualExtensionWriters.TryGetValue(fileExtension.ToUpper(), out writer) 
                ? writer 
                : null;
        }

        /// <summary>
        /// This will add or update a writer for a specific file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="writer"></param>
        public static void AddWriterForFile(string filePath, IVirtualFileWriter writer)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            if (writer == null) throw new ArgumentNullException("writer");

            if (!filePath.StartsWith("/"))
            {
                throw new FormatException("A file path must begin with a '/'");
            }

#if !Net35
            VirtualPathWriters.AddOrUpdate(filePath.ToUpper(), s => writer, (s, fileWriter) => writer);
#else
            lock (DictionaryLocker)
            {
                VirtualPathWriters[filePath.ToUpper()] = writer;
            }
#endif
            
        }

        /// <summary>
        /// Returns the writer for the file path, if none is found then the null will be returned
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static IVirtualFileWriter GetVirtualWriterForFile(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            IVirtualFileWriter writer;
            return VirtualPathWriters.TryGetValue(filePath.ToUpper(), out writer)
                ? writer
                : null;
        }

        /// <summary>
        /// This will add or update a writer for a specific file extension
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <param name="?"></param>
        /// <param name="writer"></param>
        public static void AddWriterForExtension(string fileExtension, IFileWriter writer)
        {
            if (fileExtension == null) throw new ArgumentNullException("fileExtension");
            if (writer == null) throw new ArgumentNullException("writer");

            if (!fileExtension.StartsWith("."))
            {
                throw new FormatException("A file extension must begin with a '.'");
            }

#if !Net35
            ExtensionWriters.AddOrUpdate(fileExtension.ToUpper(), s => writer, (s, fileWriter) => writer);
#else
            lock (DictionaryLocker)
            {
                ExtensionWriters[fileExtension.ToUpper()] = writer;
            }
#endif
            
        }

        /// <summary>
        /// Returns the writer for the file extension, if none is found then the default writer will be returned
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public static IFileWriter GetWriterForExtension(string fileExtension)
        {
            if (fileExtension == null) throw new ArgumentNullException("fileExtension");

            IFileWriter writer;
            return ExtensionWriters.TryGetValue(fileExtension.ToUpper(), out writer)
                ? writer
                : DefaultFileWriter;
        }

        /// <summary>
        /// This will add or update a writer for a specific file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="writer"></param>
        public static void AddWriterForFile(string filePath, IFileWriter writer)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            if (writer == null) throw new ArgumentNullException("writer");

            if (!filePath.StartsWith("/"))
            {
                throw new FormatException("A file path must begin with a '/'");
            }

#if !Net35
            PathWriters.AddOrUpdate(filePath.ToUpper(), s => writer, (s, fileWriter) => writer);
#else
            lock (DictionaryLocker)
            {
                PathWriters[filePath.ToUpper()] = writer;
            }
#endif
            
        }

        /// <summary>
        /// Returns the writer for the file path, if none is found then the default writer will be returned
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static IFileWriter GetWriterForFile(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            IFileWriter writer;
            return PathWriters.TryGetValue(filePath.ToUpper(), out writer)
                ? writer
                : DefaultFileWriter;
        }
    }
}
