<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ClientDependency.Web.Test.Models.TestModel>" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Rogue Dependency Test
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%--Load the jquery ui from cdn and make sure it isn't replaced--%>
    <% Html.RequiresJs("http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.0/jquery-ui.min.js", 3); %>

    <% Html.RequiresCss("Content.css", "Styles"); %>

    <div class="mainContent">
		<h2>
			<%= Html.Encode(this.ViewData.Model.Heading)%></h2>
		<div>
			<%= this.ViewData.Model.BodyContent %></div>
	</div>
</asp:Content>
