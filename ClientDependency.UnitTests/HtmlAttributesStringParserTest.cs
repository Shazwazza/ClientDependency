using System.Linq;
using ClientDependency.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ClientDependency.UnitTests
{
    
    
    [TestClass()]
    public class HtmlAttributesStringParserTest
    {
        [TestMethod()]
        public void Parse_Delimited_String_With_Comma()
        {
            const string attributes = "media:'print, projection'";
            var destination = new Dictionary<string, string>();
            
            HtmlAttributesStringParser.ParseIntoDictionary(attributes, destination);
            
            Assert.AreEqual(1, destination.Count);
            Assert.AreEqual("print, projection", destination.Last().Value);
            Assert.AreEqual("media", destination.Last().Key);

        }

        [TestMethod()]
        public void Parse_Normal_String()
        {
            const string attributes = "media:print";
            var destination = new Dictionary<string, string>();

            HtmlAttributesStringParser.ParseIntoDictionary(attributes, destination);

            Assert.AreEqual(1, destination.Count);
            Assert.AreEqual("print", destination.Last().Value);
            Assert.AreEqual("media", destination.Last().Key);

        }
    }
}
