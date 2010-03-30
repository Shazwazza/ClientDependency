<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<% Html.RequiresJs("/Js/jquery-1.3.2.min.js"); %>

<%--Demonstrates the use of using the PathNameAlias--%>
<% Html.RequiresCss("/Css/Controls.css", "Styles"); %>

<div class="control white bg-complement-2">
    <a href="/" class="f-primary-4 bg-complement-2">Return to landing page</a>
</div>
<div class="control header bg-primary-3 white">
	This is a header
	<ul >
		<li><a class="white" href="/Pages/Default.aspx">Default Provider</a></li>
		<li><a class="white" href="/Pages/LazyLoadProviderTest.aspx">Lazy Load Provider with dynamic registration</a></li>
		<li><a class="white" href="/Pages/ForcedProviders.aspx">Default Provider with a Forced Lazy Load provider dependency</a></li>
		<li><a class="white" href="/Pages/RogueScriptDetectionTest.aspx">Rogue script detection test</a></li>
	</ul>
</div>