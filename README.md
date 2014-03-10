# URL Rewriter Module

## Description
This module enables administrator or editors to manage URL rewrite rules from within the Sitecore client.  
It supports rewriting of hostnames, relative URL's and absolute URL's.


## Screenshots
*Managing a URL rewrite rule in Sitecore*  
![alt text](https://raw.github.com/ParTech/Url-Rewriter/master/Screenshots/url-rewrite-rule_small.png "Managing a URL rewrite rule in Sitecore.")

*Managing a Hostname rewrite rule in Sitecore*  
![alt text](https://raw.github.com/ParTech/Url-Rewriter/master/Screenshots/hostname-rewrite-rule_small.png "Managing a Hostname rewrite rule in Sitecore.")


## Installation
The Sitecore package *[\Release\ParTech.Modules.UrlRewriter-1.0.0.zip](https://github.com/ParTech/Url-Rewriter/raw/master/Release/ParTech.Modules.UrlRewriter-1.0.0.zip)* contains:
- Binary (release build).
- Configuration include file.
- Core items that add a new command to the Publish ribbon in the Content Editor.
- Templates for rewrite rules and a default folder to store rules in.

Use the Sitecore Installation Wizard to install the package.  
After installation:
- The default folder to store rewrite rules in can be found under */sitecore/System/Modules/URL Rewriter rules*.
- A new button can be found in the Publish ribbon that is used to clear the cache for the module.

You will need to setup your own rewrite rules after the installation has succeeded.


## Usage
The rewrite rules are stored as Sitecore items in the */sitecore/System/Modules/URL Rewriter rules* folder.  
They are loaded into memory when Sitecore is started and will not be reloaded until the URL Rewriter cache is cleared (see *Clearing the cache*).  
You can manage two types of rewrite rules: *URL rewrite rules* and *Hostname rewrite rules*.  

### How to use URL rewrite rules  
URL rewrite rules allow you to rewrite the entire request URL.  
You must at least specify a **relative URL**, so `/` would be the minimum valid value.  
If you specify a hostname, you must specify an **absolute URL**, including the protocol prefix (e.g. `http://www.mydomain.com/`).  
The hostname from the current request will be used if there is no hostname specified in the target URL.  
The **querystring** of your request will be kept intact during the rewrite, unless you explicitly define one in the target URL.  

Examples:  
Source URL = `http://www.source.com/my-old-page.html`  
Target URL = `http://www.target.com/my-new-page.aspx`

In this case, a request to: `http://www.source.com/my-old-page.html`  
will be redirected to: `http://www.target.com/my-new-page.aspx`  
  
The querystring is kept intact, so a request to: `http://www.source.com/my-old-page.html?myquery=value`  
will be redirected to: `http://www.target.com/my-new-page.aspx?myquery=value`  
  
If a querystring was defined on the target URL, it will overwrite any existing querystring:  
  
Source URL = `http://www.source.com/my-old-page.html`  
Target URL = `http://www.target.com/my-new-page.aspx?my-explicit=querystring`  
  
In that case, a request to: `http://www.source.com/my-old-page.html?myquery=value`  
will be redirected to: `http://www.target.com/my-new-page.aspx?my-explicit=querystring`  

### How to use Hostname rewrite rules  
Hostname rewrite rules allow you to rewrite the hostname of a request, while keeping the rest of the URL intact.  
You must specify only the hostnames (or IP-addresses), no other values such as protocol prefix or path.  

Example:  
Source hostname = `www.sourcedomain.com`  
Target hostname = `www.mynewdomain.com`

In this case, a request to: `http://www.sourcedomain.com/my-path/my-document.html?my=querystring`  
will be redirected to: `http://www.mynewdomain.com/my-path/my-document.html?my=querystring`

### Clearing the cache  
The cache is populated during the first request after the Sitecore instance is started.  
If you make changes to rewrite rules, you need to clear the cache using the *Clear cache* button in the Publish ribbon, otherwise the changes will not be applied.  
Note that you need to have publishing rights in order to see this button.  
If you are using a multi-instance environment (i.e. you have separate Content Management and Content Delivery instances), the cache is cleared on all the instances (assuming that EventQueues are enabled).  

### Configuration  
All the configuration related to this module is stored in the */App_Config/Includes/ParTech.Modules.UrlRewriter.config* include file.  
The settings are commented in that file and don't need further explanation in this document.  


## Release notes
*1.0.0*
- Initial release


## References
Blog: http://www.partechit.nl/nl/blog/2014/03/sitecore-url-rewriter-module  
GitHub: https://github.com/ParTech/Url-Rewriter


## Author
This solution was brought to you and is supported by Ruud van Falier, ParTech IT

Twitter: @BrruuD / @ParTechIT   
E-mail: ruud@partechit.nl   
Web: http://www.partechit.nl
