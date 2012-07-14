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

        //[TestMethod()]
        //public void Get_Composite_File_Url()
        //{
        //    var files = "/VirtualFolderTest/Pages/relative.css;/VirtualFolderTest/Css/Site.css;/VirtualFolderTest/Css/ColorScheme.css;/VirtualFolderTest/Css/Controls.css;/VirtualFolderTest/Css/CustomControl.css;/VirtualFolderTest/Css/Content.css;";
        //    var encodedFiles = files.EncodeTo64Url();
        //    var provider = new CompositeFileProcessingProvider();
        //    var ctxFactory = new FakeHttpContextFactory("~/somesite/hello");
        //    var url = provider.GetCompositeFileUrl(encodedFiles, ClientDependencyType.Css, ctxFactory.Context, CompositeUrlType.Base64Paths, 
        //        "/DependencyHandler.axd", 123);

        //    Assert.AreEqual("", url);
        //}

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
