<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<% Html.RequiresJs("/Js/jquery-1.3.2.min.js"); %>

<%--Demonstrates the use of using the PathNameAlias--%>
<% Html.RequiresCss("Controls.css", "Styles"); %>

<div class="control white bg-complement-2">
    <a href="/" class="f-primary-4 bg-complement-2">Return to landing page</a>
</div>
<div class="control header bg-primary-3 white">
	This is a header
	<ul >
		<li><%= Html.ActionLink("Default Renderer", "Default", null, new { Class = "white" }) %></li>
		<li><%= Html.ActionLink("Rogue script detection test", "RogueDependencies", null, new { Class = "white" })%></li>
	</ul>
</div>