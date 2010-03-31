<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ClientDependency.Web.Models.TestModel>" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <% Html.RequiresCss("Content.css", "Styles"); %>

    <div class="mainContent">
		<h2>
			<%= Html.Encode(this.ViewData.Model.Heading)%></h2>
		<p>
			<%= this.ViewData.Model.BodyContent %></p>
	</div>   
</asp:Content>
