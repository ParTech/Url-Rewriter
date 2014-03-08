using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Sitecore.Pipelines.GetContentEditorWarnings;

namespace ParTech.Modules.UrlRewriter.Pipelines
{
    /// <summary>
    /// Implements an instruction manual on rewrite rules by adding Content Editor warnings when editing rewrite rule items.
    /// </summary>
    public class ApplyHelpNotifications
    {
        private readonly string urlHelpHideStateCookieName = "url-rewrite-help-hide";
        private readonly string hostNameHelpHideStateCookieName = "hostname-rewrite-help-hide";

        /// <summary>
        /// Process the pipeline processor.
        /// </summary>
        /// <param name="args"></param>
        public void Process(GetContentEditorWarningsArgs args)
        {
            if (args.Item == null || (!args.Item.TemplateID.Equals(ItemIds.Templates.HostNameRewriteRule) && !args.Item.TemplateID.Equals(ItemIds.Templates.UrlRewriteRule)))
            {
                // Only apply the notifications to rewrite rule items.
                return;
            }

            if (args.Item.TemplateID.Equals(ItemIds.Templates.HostNameRewriteRule))
            {
                // Add a Content Editor warning with instructions on how to use hostname rewrite rules.
                args.Add("How to use Hostname rewrite rules", this.GetHostNameRuleHelp());
            }

            if (args.Item.TemplateID.Equals(ItemIds.Templates.UrlRewriteRule))
            {
                // Add a Content Editor warning with instructions on how to use url rewrite rules.
                args.Add("How to use URL rewrite rules", this.GetUrlRuleHelp());
            }
        }

        /// <summary>
        /// Gets the help text for hostname rewrite rule items.
        /// </summary>
        /// <returns></returns>
        private string GetHostNameRuleHelp()
        {
            // Check if user has hidden the help text.
            // Storing the hide/show state works by setting a JavaScript cookie.
            // We check for that cookie when generating the help HTML.
            bool hideHelpText = HttpContext.Current != null
                && HttpContext.Current.Request.Cookies.AllKeys.Contains(this.hostNameHelpHideStateCookieName);

            // Return the HTML for the help text.
            return @"
                <style type=""text/css"">
                    .urlrewrite-help span.url { color: #707070 } 
                    .urlrewrite-help span.url em { color: #222; font-style: normal } 
                    .urlrewrite-help a { color: #000; font-weight: bold; cursor: pointer }
                </style>
                <div id=""urlrewrite-help"" class=""urlrewrite-help"">
                    <div class=""content"" style=""" + (hideHelpText ? "display:none" : string.Empty) + @""">

                        Hostname rewrite rules allow you to rewrite the hostname of a request, while keeping the rest of the URL intact.<br />
                        You must specify only the hostnames (or IP-addresses), no other values such as protocol prefix or path.<br />
                        <br />
                        Example:<br />
                        <br />
                        Source hostname = '<span class=""url"">www.sourcedomain.com</span>'<br />
                        Target hostname = '<span class=""url"">www.mynewdomain.com</span>'<br />
                        <br />
                        In this case, a request to: <span class=""url"">http://<em>www.sourcedomain.com</em>/my-path/my-document.html?my=querystring</span><br />
                        will be redirected to: <span class=""url"">http://<em>www.mynewdomain.com</em>/my-path/my-document.html?my=querystring</span><br />
                        <br />

                        " + this.GetHideLink(this.hostNameHelpHideStateCookieName) + @"
                    </div>
                    " + this.GetShowLink(this.hostNameHelpHideStateCookieName, hideHelpText) + @"
                </div>".Replace("  ", string.Empty);
        }

        /// <summary>
        /// Gets the help text for URL rewrite rule items.
        /// </summary>
        /// <returns></returns>
        private string GetUrlRuleHelp()
        {
            // Check if user has hidden the help text.
            // Storing the hide/show state works by setting a JavaScript cookie.
            // We check for that cookie when generating the help HTML.
            bool hideHelpText = HttpContext.Current != null
                && HttpContext.Current.Request.Cookies.AllKeys.Contains(this.urlHelpHideStateCookieName);

            // Return the HTML for the help text.
            return @"
                <style type=""text/css"">
                    .urlrewrite-help span.url { color: #707070 } 
                    .urlrewrite-help span.url em { color: #222; font-style: normal } 
                    .urlrewrite-help a { color: #000; font-weight: bold; cursor: pointer }
                </style>
                <div id=""urlrewrite-help"" class=""urlrewrite-help"">
                    <div class=""content"" style=""" + (hideHelpText ? "display:none" : string.Empty) + @""">

                        URL rewrite rules allow you to rewrite the entire request URL.<br />
                        You must at least specify a <strong>relative URL</strong>, so <span class=""url"">/</span> would be the minimum valid value.<br />
                        If you specify a hostname, you must specify an <strong>absolute URL</strong>, including the protocol prefix (e.g. <span class=""url"">http://www.mydomain.com/</span>).<br />
                        The hostname from the current request will be used if there is no hostname specified in the target URL.<br />
                        The <strong>querystring</strong> of your request will be kept intact during the rewrite, unless you explicitly define one in the target URL.<br />
                        <br />
                        Examples:<br />
                        <br />
                        Source URL = '<span class=""url"">http://www.source.com/my-old-page.html</span>'<br />
                        Target URL = '<span class=""url"">http://www.target.com/my-new-page.aspx</span>'<br />
                        <br />
                        In this case, a request to: <span class=""url"">http://www.source.com<em>/my-old-page.html</em></span><br />
                        will be redirected to: <span class=""url"">http://www.target.com<em>/my-new-page.aspx</em></span><br />
                        <br />
                        The querystring is kept intact, so a request to: <span class=""url"">http://www.source.com<em>/my-old-page.html?myquery=value</em></span><br />
                        will be redirected to: <span class=""url"">http://www.target.com<em>/my-new-page.aspx?myquery=value</em></span><br />
                        <br />
                        If a querystring was defined on the target URL, it will overwrite any existing querystring:<br />
                        <br />
                        Source URL = '<span class=""url"">http://www.source.com/my-old-page.html</span>'<br />
                        Target URL = '<span class=""url"">http://www.target.com/my-new-page.aspx?my-explicit=querystring</span>'<br />
                        <br />
                        In that case, a request to: <span class=""url"">http://www.source.com/my-old-page.html<em>?myquery=value</em></span><br />
                        will be redirected to: <span class=""url"">http://www.target.com/my-new-page.aspx<em>?my-explicit=querystring</em></span><br />
                        <br />

                        " + this.GetHideLink(this.urlHelpHideStateCookieName) + @"
                    </div>
                    " + this.GetShowLink(this.urlHelpHideStateCookieName, hideHelpText) + @"
                </div>".Replace("  ", string.Empty);
        }

        /// <summary>
        /// Gets the HTML for the 'show instructions' link.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="currentHiddenState"></param>
        /// <returns></returns>
        private string GetShowLink(string cookieName, bool currentHiddenState)
        {
            return @"<a onclick=""
                document.cookie='" + cookieName + @"=; expires=Thu, 01 Jan 1970 00:00:00 GMT'; 
                this.parentNode.getElementsByTagName('div')[0].style.display='block'; 
                this.style.display='none'"" " + (!currentHiddenState ? @"style=""display:none""" : string.Empty) + @">
                    [Show instructions]</a>";
        }

        /// <summary>
        /// Gets the HTML for the 'hide instructions' link.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        private string GetHideLink(string cookieName)
        {
            return @"<a onclick=""
                document.cookie='" + cookieName + @"=1; expires=Sun, 28 Feb 2044 12:36:30 GMT'; 
                this.parentNode.style.display='none'; 
                var n = this.parentNode.parentNode.getElementsByTagName('a'); 
                n[n.length - 1].style.display='block'"">
                    [Hide instructions]</a>";
        }
    }
}
