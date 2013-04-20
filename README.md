
![ClientDependency Framework](http://shazwazza.com/Content/Downloads/ClientDependencyLogo.png)

## What is *ClientDependency Framework (CDF)* ?

CDF is a framework for managing dependencies for JavaScript and CSS files in your web application (client files). It allows for each individual component in your web application to declare what CSS and JavaScript files they require instead of relying on a single master page to include all dependencies for all modules. 

This hugely simplifies collaborative development since any developer who is working on a component doesn't need to worry about what CSS/JavaScript is being included on the main page. CDF will automagically wire everything up, ensure that your dependencies are ordered correctly, that there are no duplicates and render your CSS and JavaScript html tags properly on to the rendering page.

## Out of the box you get

* MVC support - Any view engine
* Webforms support
* Runtime dependency resolution
* Pre-defined bundling
* Combining, compressing & minifying output
* Support for external/CDN files
* OutputCaching of the combined files
* Persisting the combined composite files for increased performance when applications restart or when the Cache expires
* Versioning the files ... great for ensuring your clients' browser cache is cleared!
* Prioritizing dependencies
* Pre-defined file paths - great for theming!
* Detecting script and styles that are not explicitly registered with CDF and have the output compressed
* Provider Model so you can choose how you would like your JS and CSS files rendered, combined, compressed & minified
* MIME type compression output for things like JSON services, or anything you want
* Control over how composite file URLs are structured if you need a custom format
* Medium trust compatible
* *COMING SOON:* native .Less, SASS & CoffeeScript support

## Nuget

	PM> Install-Package ClientDependency

	PM> Install-Package ClientDependency-Mvc

## [Documentation](https://github.com/Shandem/ClientDependency/wiki)

Click the link to see the documentation on how to get started and more advanced techniques

## [Releases](https://github.com/Shandem/ClientDependency/wiki/All-Releases)

Shows information about all CDF releases