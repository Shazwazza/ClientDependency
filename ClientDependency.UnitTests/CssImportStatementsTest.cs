using System.Collections.Generic;
using System.Linq;
using ClientDependency.Core;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
    [TestFixture]
    public class CssImportStatementsTest
    {
        [Test]
        public void Retain_External_Imports()
        {
            var cssWithImport = @"@import url(""//fonts.googleapis.com/css?subset=latin,cyrillic-ext,latin-ext,cyrillic&family=Open+Sans+Condensed:300|Open+Sans:400,600,400italic,600italic|Merriweather:400,300,300italic,400italic,700,700italic|Roboto+Slab:400,300"");
@import url(""//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css"");";

            IEnumerable<string> importPaths;
            var output = CssHelper.ParseImportStatements(cssWithImport, out importPaths);

            Assert.AreEqual(output, cssWithImport);
        }

        [Test]
        public void Can_Parse_Import_Statements()
        {
            var css = @"@import url('/css/typography.css');
@import url('/css/layout.css');
@import url('http://mysite/css/color.css');
@import url(/css/blah.css);

body { color: black; }
div {display: block;}";

            IEnumerable<string> importPaths;
            var output = CssHelper.ParseImportStatements(css, out importPaths);

            Assert.AreEqual(@"@import url('http://mysite/css/color.css');


body { color: black; }
div {display: block;}", output);

            Assert.AreEqual(3, importPaths.Count());
            Assert.AreEqual("/css/typography.css", importPaths.ElementAt(0));
            Assert.AreEqual("/css/layout.css", importPaths.ElementAt(1));
            //Assert.AreEqual("http://mysite/css/color.css", importPaths.ElementAt(2));
            Assert.AreEqual("/css/blah.css", importPaths.ElementAt(2));
        }
    }
}