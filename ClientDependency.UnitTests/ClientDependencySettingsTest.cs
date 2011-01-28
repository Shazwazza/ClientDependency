using System.Reflection;
using ClientDependency.Core.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.FileRegistration.Providers;
using System.Collections.Generic;
using ClientDependency.Core.Logging;
using System.IO;
using Rhino.Mocks;
using System.Web;

namespace ClientDependency.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for ClientDependencySettingsTest and is intended
    ///to contain all ClientDependencySettingsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SettingsTest
    {
        private class FakeServer : HttpServerUtilityBase
        {
            public override string MapPath(string path)
            {
                return path;
            }
        }

        /// <summary>
        ///A test for CompositeFileHandlerPath
        ///</summary>
        [TestMethod()]
        public void Settings_CompositeFileHandlerPath()
        {
            //Arrange

            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            var binFolder = Path.GetDirectoryName(path);
            var ctx = MockRepository.GenerateStub<HttpContextBase>();
            ctx.Stub(x => x.Server).Return(new FakeServer());

            var configFile = new FileInfo(binFolder + "\\..\\..\\App.Config");
            var target = new ClientDependencySettings(configFile, ctx);
            
            //Act


            //Assert

            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CompositeFileProcessingProviderCollection
        ///</summary>
        [TestMethod()]
        public void CompositeFileProcessingProviderCollectionTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            CompositeFileProcessingProviderCollection actual;
            actual = target.CompositeFileProcessingProviderCollection;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DefaultCompositeFileProcessingProvider
        ///</summary>
        [TestMethod()]
        public void DefaultCompositeFileProcessingProviderTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            BaseCompositeFileProcessingProvider actual;
            actual = target.DefaultCompositeFileProcessingProvider;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DefaultFileRegistrationProvider
        ///</summary>
        [TestMethod()]
        public void DefaultFileRegistrationProviderTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            WebFormsFileRegistrationProvider actual;
            actual = target.DefaultFileRegistrationProvider;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DefaultMvcRenderer
        ///</summary>
        [TestMethod()]
        public void DefaultMvcRendererTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            BaseRenderer actual;
            actual = target.DefaultMvcRenderer;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for FileBasedDependencyExtensionList
        ///</summary>
        [TestMethod()]
        public void FileBasedDependencyExtensionListTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            List<string> expected = null; // TODO: Initialize to an appropriate value
            List<string> actual;
            target.FileBasedDependencyExtensionList = expected;
            actual = target.FileBasedDependencyExtensionList;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for FileRegistrationProviderCollection
        ///</summary>
        [TestMethod()]
        public void FileRegistrationProviderCollectionTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            FileRegistrationProviderCollection actual;
            actual = target.FileRegistrationProviderCollection;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Instance
        ///</summary>
        [TestMethod()]
        public void InstanceTest()
        {
            ClientDependencySettings actual;
            actual = ClientDependencySettings.Instance;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Logger
        ///</summary>
        [TestMethod()]
        public void LoggerTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            ILogger actual;
            actual = target.Logger;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for MvcRendererCollection
        ///</summary>
        [TestMethod()]
        public void MvcRendererCollectionTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            RendererCollection actual;
            actual = target.MvcRendererCollection;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Version
        ///</summary>
        [TestMethod()]
        public void VersionTest()
        {
            ClientDependencySettings target = new ClientDependencySettings(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.Version = expected;
            actual = target.Version;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
