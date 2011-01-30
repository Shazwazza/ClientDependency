using ClientDependency.Core.CompositeFiles.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ClientDependency.UnitTests
{
  
    [TestClass()]
    public class CompositeFileProcessingProviderTest
    {

        /// <summary>
        ///A test for CombineFiles
        ///</summary>
        [TestMethod()]
        public void CompositeFiles_Combine_Files()
        {
            //Arrange

            var provider = new CompositeFileProcessingProvider();

            //Act

            //provider.CombineFiles(

            //Assert

            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CompressBytes
        ///</summary>
        [TestMethod()]
        public void CompositeFiles_Compress_Bytes()
        {
          
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SaveCompositeFile
        ///</summary>
        [TestMethod()]
        public void CompositeFiles_Save_Composite_File()
        {
            
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
