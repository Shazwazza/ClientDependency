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
        public void CssTransform_Ensure_Inline_Images_Retained()
        {
            //refer to this:http://clientdependency.codeplex.com/workitem/13173

            var css = @"ul li.expanded {
  background: transparent url(data:image/gif;base64,
  R0lGODlhCgAKAMQUAM/Q0vT09Ojr7s7Q0dLU1f7+/u/z9+Dj5tfX1+Dg4MrKyu
  fq7tfZ27m5uba2tsLCwvv7+7W1tbS0tP////P3+wAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAACH5BAEAABQALAAAAAAKAAoAAAU0ICWOZGkCkaRKEU
  AtTzHNU/Es1JDQUzKIggZkBmkIRgTEDEEgGRyBgMNQYigUDBPlcCCFAAA7) no-repeat 1px .35em;
}";

            var output = CssFileUrlFormatter.TransformCssFile(css, new Uri("http://MySite/MySubFolder"));

            Assert.AreEqual(css, output);
        }

        [TestMethod]
        public void CssTransform_Ensure_Query_Strings_Retained()
        {
            //refer to this:http://clientdependency.codeplex.com/workitem/13184

            var css = @"#test {display:block; background-image:url(""/media.ashx?arg=value"")   }";

            var output = CssFileUrlFormatter.TransformCssFile(css, new Uri("http://MySite/MySubFolder"));

            Assert.AreEqual(@"#test {display:block; background-image:url(""/media.ashx?arg=value"")   }", output);

        }

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
