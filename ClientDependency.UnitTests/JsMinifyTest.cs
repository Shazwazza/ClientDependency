using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ClientDependency.Core.CompositeFiles;

namespace ClientDependency.UnitTests
{
    [TestFixture]
    public class JsMinifyTest
    {
        [Test]
        public void JsMinify_Minify()
        {
            //Arrange

            var script =
                @"var Messaging = {
    GetMessage: function(callback) {
        $.ajax({
            type: ""POST"",
            url: ""/Services/MessageService.asmx/HelloWorld"",
            data: ""{}"",
            contentType: ""application/json; charset=utf-8"",
            dataType: ""json"",
            success: function(msg) {
                callback.apply(this, [msg.d]);
            }
        });
    }
    var blah = 1;
    blah++;
    blah = blah + 2;
    var newBlah = ++blah;
    newBlah += 234 +4;
};";

            var minifier = new JSMin();

            //Act

            var output = minifier.Minify(script);

            Assert.AreEqual(
                "\nvar Messaging={GetMessage:function(callback){$.ajax({type:\"POST\",url:\"/Services/MessageService.asmx/HelloWorld\",data:\"{}\",contentType:\"application/json; charset=utf-8\",dataType:\"json\",success:function(msg){callback.apply(this,[msg.d]);}});}\nvar blah=1;blah++;blah=blah+2;var newBlah=++blah;newBlah+=234+4;};",
                output);
        }

        [Test]
        public void JsMinify_Minify_With_Unary_Operator()
        {
            //see: http://clientdependency.codeplex.com/workitem/13162

            //Arrange

            var script = 
@"var c = {};
var c.name = 0;
var i = 1;
c.name=i+ +new Date;
alert(c.name);";

            var minifier = new JSMin();
            
            //Act

            var output = minifier.Minify(script);

            //Assert

            Assert.AreEqual("\nvar c={};var c.name=0;var i=1;c.name=i+ +new Date;alert(c.name);", output);
        }

        [Test]
        public void JsMinify_Backslash_Line_Escapes()
        {
            var script = @"function Test() {
jQuery(this).append('<div>\
		<div>\
			<a href=""http://google.com"" /></a>\
		</div>\
	</div>');
}";

            var minifier = new JSMin();

            //Act

            var output = minifier.Minify(script);

            //Assert

            Assert.AreEqual("\nfunction Test(){jQuery(this).append('<div>\\\n\n  <div>\\\n\n   <a href=\"http://google.com\" /></a>\\\n\n  </div>\\\n\n </div>');}", output);

        }

        [Test]
        public void JsMinify_TypeScript_Enum()
        {
            var script = @"$(""#TenderListType"").val(1 /* Calendar */.toString());";

            var minifier = new JSMin();
            var output = minifier.Minify(script);
            Assert.AreEqual("\n$(\"#TenderListType\").val(1..toString());", output);
        }
    }
}
