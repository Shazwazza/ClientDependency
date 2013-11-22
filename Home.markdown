---
title: ClientDependency documentation
---

![ClientDependency Framework](http://shazwazza.com/Content/Downloads/ClientDependencyLogo.png)

#### Table of contents

[Overview](#overview)<br/>
[Quick Start](#quick-start)<br/>
&nbsp;&nbsp;&nbsp;&nbsp;[Debugging](#debugging)<br/>
&nbsp;&nbsp;&nbsp;&nbsp;[MVC](#mvc)<br/>
&nbsp;&nbsp;&nbsp;&nbsp;[Webforms](#webforms)<br/>
&nbsp;&nbsp;&nbsp;&nbsp;[Pre-defined bundles](#pre-defined-bundles)<br/>
[Path aliases](wiki/Paths)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[Configuration](wiki/Configuration)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[Prioritizing](wiki/Priorities)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[Versioning](wiki/Versioning)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[Composite files, debugging, urls & grouping ](wiki/Composite-Files)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[Html tag attributes](wiki/Html-Attributes)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[Forced providers](wiki/Forced-Providers)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[More on MVC - setup & runtime registration](wiki/Mvc)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[More on Webforms & runtime registration](wiki/Webforms)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>
[Rogue Files - script/link tag detection](wiki/Rogue-Files)&nbsp;<span class="mini-icon mini-icon-link"></span><br/>

<a name="overview"></a>
# Overview

The [main page](https://github.com/Shandem/ClientDependency#what-is-clientdependency-framework-cdf-) has a breif overview of what CDF is, it's feature set and links to releases

<a name="quick-start"></a>
# Quick start

To get started quickly, install CDF via Nuget. If you are using MVC be sure to install the MVC package as well.

	PM> Install-Package ClientDependency

	PM> Install-Package ClientDependency-Mvc


<a name="debugging"></a>
## Debugging

When you're creating a website, you don't want CDF to be combining, caching & compressing all of your files because you need to be able to debug your JavaScript and CSS in the browser so it is important to note that setting `<compilation debug="true">` will disable all combining, caching, compressing, rogue script detection, etc... !!

When you deploy your website or want to test the composite files created with CDF you need to change this to `<compilation debug="false">`

<a name="mvc"></a>
## MVC

##### Make your view dependent on any CSS/JavaScript file

```aspx-cs
@{Html.RequiresCss("~/Css/ColorScheme.css");}
```

```aspx-cs
@{Html.RequiresJs("~/Js/jquery.js");}
```

##### Make your view dependent on an entire CSS/JavaScript folder

```aspx-cs
@{Html.RequiresCssFolder("~/Css/Widgets/");}
```

```aspx-cs
@{Html.RequiresJsFolder("~/Js/Controls/");}
```

##### Support for chaining calls to any CDF HtmlHelper method

```aspx-cs
@{
Html.RequiresJs("~/Js/jquery.js")
	 .RequiresJs("~/Js/jquery.validation.js")
	 .RequiresJs("~/Js/myCoffeeLib.coffee");
}
```

##### Rendering CSS/JavaScript in your page

```aspx-cs
@Html.RenderJsHere()
```

```aspx-cs
@Html.RenderCssHere()
```

<a name="webforms"></a>
## Webforms

In webforms, you'll need to register a control prefix on your webforms page:

	<%@ Register TagPrefix="CD" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

##### Make your page/control dependent by using normal html markup

One of th easiest ways to get up and running with CDF with Webforms is to just wrap your normal css/javascript html declarations in the HtmlInclude control:

```aspx-cs
<CD:HtmlInclude runat="server">
	
	<link type='text/css' href='/css/site.less'/>
	<link type='text/css' href='/css/controls.css'/>

	<script type='text/javascript' src='/js/jquery.js'></script>
	<script type='text/javascript' src='/js/jquery.ui.js'></script>
	<script type='text/javascript' src='/js/myTypeScript.ts'></script>

</CD:HtmlInclude>
```

##### Make your page/control dependent on any CSS/JavaScript file

```aspx-cs
<CD:CssInclude runat="server" FilePath="~/CSS/ColorScheme.css" /> 
```

```aspx-cs
<CD:JsInclude runat="server" FilePath="~/Js/jquery.js" />
```

##### Make your page/control dependent on an entire CSS/JavaScript folder

```aspx-cs
<CD:CssFolderInclude runat="server" FolderVirtualPath="~/Css/Widgets/" /> 
```

```aspx-cs
<CD:JsFolderInclude runat="server" FolderVirtualPath="~/Js/Controls/" />
```

##### Make your composite controls dependent on any CSS/JavaScript file:

```c#
[ClientDependency(ClientDependencyType.Css, "~/Css/CustomControl.css")] 
public class MyControl : Control { ... }
```

```c#
[ClientDependency(ClientDependencyType.JavaScript, "~/Js/MyControl.js")] 
public class MyControl : Control { ... }
```

##### Rendering CSS/JavaScript in your page

You need to have a loader on your page:

```aspx-cs
<CD:ClientDependencyLoader runat="server" id="Loader" />
```

*NOTE: The loader control should normally be defined in your markup before any other CDF controls are declared.*

Then you'll need to define a place holder for where you want the JavaScript and CSS rendered 

```aspx-cs
<%--This will ensure the CSS is rendered at the location of this placeholder--%>
<asp:PlaceHolder runat="server" ID="CssPlaceHolder"></asp:PlaceHolder>
```

```aspx-cs
<%--This will ensure the JS is rendered at the location of this placeholder--%>
<asp:PlaceHolder runat="server" ID="JavaScriptPlaceHolder"></asp:PlaceHolder>
```

***NOTE**: the Ids of the controls must be how they are defined above, otherwise you can change the expected ids in the CDF configuration. Also in Webforms there are various providers that allow you to change the behavior of the ClientDependencyLoader but the PlaceHolderProvider is the default and is recommended.*

<a name="pre-defined-bundles"></a>
## Pre-defined bundles

Creating a pre-defined bundle on application startup. This example creates a bundle in the global.asax:

```c#
public class MyApplication : HttpApplication
{
    protected void Application_Start()
    {        
        CreateBundles();
    }

	public static void CreateBundles()
    {
        BundleManager.CreateCssBundle("MyControl", 
            new CssFile("~/Css/Controls/MyControl/css1.css"),
            new CssFile("~/Css/Controls/MyControl/css2.css"),
            new CssFile("~/Css/Controls/MyControl/css3.css"));

		BundleManager.CreateCssBundle("JQuery", 
            new CssFile("~/Js/jquery.js"),
            new CssFile("~/Js/jquery.validation.js"));
    }
}
```

##### Referencing a bundle in MVC

```aspx-cs
@{Html.RequiresCssBundle("MyControl");}
```

```aspx-cs
@{Html.RequiresJsBundle("JQuery");}
```

##### Referencing a bundle in Webforms

*Coming soon...*