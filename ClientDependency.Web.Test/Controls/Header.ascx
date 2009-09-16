<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header.ascx.cs" Inherits="ClientDependency.Core.Web.Test.Controls.Header" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/jquery-1.3.2.min.js" />

<!-- Demonstrates the use of using the PathNameAlias //-->
<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Controls.css" PathNameAlias="Styles" />

<div class="control header">
	This is a header
	<ul>
		<li><a href="/Pages/Default.aspx">Default Provider</a></li>
		<li><a href="/Pages/LazyLoadProviderTest.aspx">Lazy Load Provider with dynamic registration</a></li>
		<li><a href="/Pages/ForcedProviders.aspx">Default Provider with a Forced Lazy Load provider dependency</a></li>
	</ul>
</div>