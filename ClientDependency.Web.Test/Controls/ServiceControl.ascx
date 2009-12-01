<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ServiceControl.ascx.cs" Inherits="ClientDependency.Core.Web.Test.Controls.ServiceControl" %>


<asp:ScriptManagerProxy runat="server">
	<Services>
		<asp:ServiceReference Path="~/Services/TestService.asmx" />
	</Services>
</asp:ScriptManagerProxy>

<div style="background-color:Blue;width:300px;height:300px;display:block;">

</div>

<script type="text/javascript">
	function doThis() {
		alert("starting");
		var svc = ClientDependency.Core.Web.Test.Services.TestService;
		svc.HelloWorld(function(sender, e) {
			alert(e.d);
		});
	}	
</script>