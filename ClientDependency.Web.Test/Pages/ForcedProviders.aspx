<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="ForcedProviders.aspx.cs" Inherits="ClientDependency.Core.Web.Test.Pages.FocedProviders" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
    
    <!-- Force page header provider //-->
    <CD:CssInclude ID="CssInclude2" runat="server" FilePath="OverrideStyles.css" PathNameAlias="Styles" ForceProvider="PageHeaderProvider" />    
    
    <!-- Force lazy load provider //-->
	<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/SomeLazyLoadScript.js" ForceProvider="LazyLoadProvider"  />
	<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Test.css" PathNameAlias="Styles" ForceProvider="LazyLoadProvider" />

	<div class="mainContent">
		<h2>
			Some dependencies are being forced to use certain providers here!</h2>
		<p>
		    On this page there are 3 providers being run: LoaderControlProvider (default), PageHeaderProvider and the LazyLoadProvider.
		    If you have a look at the html source, you can see where the scripts and stylesheets are being loaded.
		    You can also turn debug on/off for various providers and you'll see that it renders composite script/css paths or just standard ones for each of the locations.
		</p>
		<p class="lazyLoaded">
			
		</p>
		<p>
			Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut sed lorem viverra magna molestie vestibulum ac non risus. Sed sed leo quam, eu eleifend neque. Curabitur ultricies metus a lectus aliquam tempus. Integer in metus a nibh tincidunt fermentum. In nec purus vitae nunc rhoncus eleifend. Etiam placerat consectetur enim ac pharetra. Etiam et nisi orci, lobortis eleifend erat. Praesent ac metus metus, id luctus neque. Mauris dictum ultricies nisi vel sodales. Nunc vestibulum quam vel eros egestas dapibus. Ut aliquet turpis metus. Curabitur libero ligula, ullamcorper in volutpat non, gravida ac erat. Praesent sed nibh at tortor mattis commodo vitae vitae tortor. </p>
	</div>
</asp:Content>