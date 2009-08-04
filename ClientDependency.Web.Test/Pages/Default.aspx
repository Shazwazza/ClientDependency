<%@ Page Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ClientDependency.Core.Web.Test.Pages._Default" %>

<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>
<asp:Content runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
	<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />
	<div class="mainContent">
		<p>
			This is my main content</p>
		<p>
			Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut sed lorem viverra magna molestie vestibulum ac non risus. Sed sed leo quam, eu eleifend neque. Curabitur ultricies metus a lectus aliquam tempus. Integer in metus a nibh tincidunt fermentum. In nec purus vitae nunc rhoncus eleifend. Etiam placerat consectetur enim ac pharetra. Etiam et nisi orci, lobortis eleifend erat. Praesent ac metus metus, id luctus neque. Mauris dictum ultricies nisi vel sodales. Nunc vestibulum quam vel eros egestas dapibus. Ut aliquet turpis metus. Curabitur libero ligula, ullamcorper in volutpat non, gravida ac erat. Praesent sed nibh at tortor mattis commodo vitae vitae tortor. </p>
	</div>
</asp:Content>
