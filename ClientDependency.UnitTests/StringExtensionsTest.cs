using ClientDependency.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientDependency.UnitTests
{

    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void Decode_From_64_Url()
        {
            var files = "/VirtualFolderTest/Pages/relative.css;/VirtualFolderTest/Css/Site.css;/VirtualFolderTest/Css/ColorScheme.css;/VirtualFolderTest/Css/Controls.css;/VirtualFolderTest/Css/CustomControl.css;/VirtualFolderTest/Css/Content.css;";
            var encodedFiles = files.EncodeTo64Url();

            var decoded = encodedFiles.DecodeFrom64Url();

            Assert.AreEqual(files, decoded);
        }
    }
}