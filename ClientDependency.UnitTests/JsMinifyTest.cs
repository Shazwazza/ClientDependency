using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClientDependency.Core.CompositeFiles;

namespace ClientDependency.UnitTests
{
    [TestClass]
    public class JsMinifyTest
    {
        [TestMethod]
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
                "\nvar Messaging={GetMessage:function(callback){$.ajax({type:\"POST\",url:\"/Services/MessageService.asmx/HelloWorld\",data:\"{}\",contentType:\"application/json; charset=utf-8\",dataType:\"json\",success:function(msg){callback.apply(this,[msg.d]);}});}\nvar blah=1;blah++;blah=blah + 2;var newBlah=++blah;newBlah +=234 +4;};",
                output);
        }

        [TestMethod]
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
    }
}
