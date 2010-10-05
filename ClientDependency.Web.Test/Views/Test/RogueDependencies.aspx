<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ClientDependency.Web.Models.TestModel>" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Rogue Dependency Test
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%--Rogue style registration--%>
    <link rel="Stylesheet" type="text/css" href="../Css/OverrideStyles.css" />

    <%--Load the jquery ui from cdn and make sure it isn't replaced--%>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.0/jquery-ui.min.js"></script>
    
    <%--Load first Rogue Script--%>
    <script type="text/javascript" src="../Js/RogueScript1.js"></script>

    <% Html.RequiresCss("Content.css", "Styles"); %>

    <div class="mainContent">
		<h2>
			<%= Html.Encode(this.ViewData.Model.Heading)%></h2>
		<div>
			<%= this.ViewData.Model.BodyContent %></div>
	</div>
</asp:Content>
