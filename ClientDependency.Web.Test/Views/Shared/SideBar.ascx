<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<%--<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/jquery-1.3.2.min.js" />--%>


<!-- Demonstrates the use of using the PathNameAlias //-->
<%--<CD:CssInclude ID="CssInclude2" runat="server" FilePath="Controls.css" PathNameAlias="Styles" />--%>

<div class="control sidebar bg-primary-3 white">
	This is a side bar
	<% Html.RenderPartial("CustomControl"); %>
</div>