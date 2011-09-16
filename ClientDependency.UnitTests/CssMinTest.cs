using ClientDependency.Core.CompositeFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientDependency.UnitTests
{
    [TestClass]
    public class CssMinTest
    {
        /// <summary>
        /// CSSs the transform_ ensure_ element_ with_ id_ selector.
        /// </summary>
        [TestMethod]
        public void CssMin_Ensure_Element_With_Id_Selector()
        {
            //refer to this: http://clientdependency.codeplex.com/workitem/13181

            var css = @"
ol#controls {display:block;    }

table {font-family: Arial;   }
";
            var output = CssMin.CompressCSS(css);

            Assert.AreEqual("ol#controls{display:block;}table{font-family:Arial;}", output);

        }

    }
}