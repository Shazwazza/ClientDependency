using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClientDependency.Core;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
    [TestFixture]
    public class CssImportStatementsTest
    {

        [Test]
        public void Can_Parse_Import_Statements()
        {
            var css = @"@import url('/css/typography.css');
@import url('/css/layout.css');
@import url('http://mysite/css/color.css');
@import url(/css/blah.css);
@import url('//mysite/css/style.css');

body { color: black; }
div {display: block;}";

            IEnumerable<string> importPaths;
            IEnumerable<string> externalPaths;
            var output = CssHelper.ParseImportStatements(css, out importPaths, out externalPaths);

            Assert.AreEqual(@"

body { color: black; }
div {display: block;}", output);

            Assert.AreEqual(2, externalPaths.Count());
            Assert.AreEqual("http://mysite/css/color.css", externalPaths.ElementAt(0));
            Assert.AreEqual("//mysite/css/style.css", externalPaths.ElementAt(1));

            Assert.AreEqual(3, importPaths.Count());
            Assert.AreEqual("/css/typography.css", importPaths.ElementAt(0));
            Assert.AreEqual("/css/layout.css", importPaths.ElementAt(1));
            Assert.AreEqual("/css/blah.css", importPaths.ElementAt(2));
        }
    }
}