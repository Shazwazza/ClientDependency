using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClientDependency.Core;

namespace ClientDependency.UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class CssTransformTest
    {

       

        [TestMethod]
        public void CssTransform_Parse_Urls()
        {
            var css = @"
#css3cool {
    background: url(""/images/big_smiley_face.png"") no-repeat center center, url(""/images/red_hot_flames.png"") repeat-x left top #f00;
    @font-face {
        font-family: 'Graublau Web';
        src: url('GraublauWeb.eot');
        src: local('☺'),
        url(""GraublauWeb.woff"") format(""woff""),
        url(""GraublauWeb.otf"") format(""opentype""),
        url(""GraublauWeb.svg#grablau"") format(""svg"");
    }
}
.my-background:transparent url(../images/something or other/image.gif) repeat-y 0 0;
";

            var output = CssFileUrlFormatter.TransformCssFile(css, new Uri("http://MySite/MySubFolder"));

            var expected = @"
#css3cool {
    background: url(""/images/big_smiley_face.png"") no-repeat center center, url(""/images/red_hot_flames.png"") repeat-x left top #f00;
    @font-face {
        font-family: 'Graublau Web';
        src: url(""/GraublauWeb.eot"");
        src: local('☺'),
        url(""/GraublauWeb.woff"") format(""woff""),
        url(""/GraublauWeb.otf"") format(""opentype""),
        url(""/GraublauWeb.svg#grablau"") format(""svg"");
    }
}
.my-background:transparent url(""/images/something%20or%20other/image.gif"") repeat-y 0 0;
";

            Assert.AreEqual(expected, output);
        }
    }
}
