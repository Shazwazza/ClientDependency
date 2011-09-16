<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<% Html.RequiresJs("/Js/jquery-1.3.2.min.js", 1); %>

<%--Demonstrates the use of using the PathNameAlias--%>
<% Html.RequiresCss("Controls.css", "Styles"); %>

<div class="control white bg-complement-2">
    <a href="/" class="f-primary-4 bg-complement-2">Return to landing page</a>
</div>
<div class="control header bg-primary-3 white">
	This is a header
	<ul >
		<li><%= Html.ActionLink("Default Renderer", "Default", null, new { @class = "white" }) %></li>
		<li><%= Html.ActionLink("Rogue script detection test", "RogueDependencies", null, new { @class = "white" })%></li>
        <li><%= Html.ActionLink("Remote dependencies test", "RemoteDependencies", null, new { @class = "white" })%></li>
        <li><%= Html.ActionLink("Html attributes test", "HtmlAttributes", null, new { @class = "white" })%></li>
        <li><%= Html.ActionLink("Dynamic path registration test", "DynamicPathRegistration", null, new { @class = "white" })%></li>
	</ul>
</div>