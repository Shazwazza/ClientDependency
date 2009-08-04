<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Sidebar.ascx.cs" Inherits="ClientDependency.Core.Web.Test.Controls.Sidebar" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/jquery-1.3.2.min.js" />

<!-- Demonstrates the use of using the PathNameAlias //-->
<CD:CssInclude ID="CssInclude2" runat="server" FilePath="Controls.css" PathNameAlias="Styles" />

<div class="control sidebar">
	This is a side bar
</div>