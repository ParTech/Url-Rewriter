using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ParTech.Modules.UrlRewriter.Models;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Sites;

namespace ParTech.Modules.UrlRewriter.Pipelines
{
    /// <summary>
    /// Pipeline processor that processes URL rewriter rules.
    /// </summary>
    public class UrlRewriteHandler : HttpRequestProcessor
    {
        #region Cache objects
        /// <summary>
        /// Cache for <see cref="UrlRewriteRule"/> objects.
        /// </summary>
        private static List<UrlRewriteRule> urlRewriteRulesCache = new List<UrlRewriteRule>();

        /// <summary>
        /// Cache for <see cref="HostNameRewriteRule"/> objects.
        /// </summary>
        private static List<HostNameRewriteRule> hostNameRewriteRulesCache = new List<HostNameRewriteRule>();

        /// <summary>
        /// Indicates whether the rewrite rules have been loaded from Sitecore.
        /// </summary>
        private static bool rewriteRulesLoaded = false;
        #endregion

        /// <summary>
        /// Executes the pipeline processor.
        /// </summary>
        /// <param name="args"></param>
        public override void Process(HttpRequestArgs args)
        {
            bool isGetRequest = args.Context.Request.HttpMethod.Equals("get", StringComparison.InvariantCultureIgnoreCase);

            // Only rewrite GET requests and ignore all requests to the Sitecore Client.
            if (this.IsSitecoreClientRequest() || !isGetRequest)
            {
                return;
            }
            
            // Load the rewrite rules from Sitecore into the cache.
            this.LoadRewriteRules(args);

            // Rewrite URL's that contain trailing slashes if configuration allows it.
            this.RewriteTrailingSlash(args);

            // Try to tewrite the request URL based on URL rewrite rules.
            this.RewriteUrl(args);

            // Try to rewrite the request URL based on Hostname rewrite rules.
            this.RewriteHostName(args);
        }

        /// <summary>
        /// Clears the cache so the rewrite rules will be reloaded on the next request.
        /// </summary>
        public static void ClearCache()
        {
            urlRewriteRulesCache.Clear();
            hostNameRewriteRulesCache.Clear();

            rewriteRulesLoaded = false;

            Logging.LogInfo("Cleared rewriter rules cache.", typeof(UrlRewriteHandler));
        }

        #region Rules loading methods
        /// <summary>
        /// Load the rewrite rules from Sitecore.
        /// </summary>
        /// <param name="args">HttpRequest pipeline arguments.</param>
        private void LoadRewriteRules(HttpRequestArgs args)
        {
            if (rewriteRulesLoaded)
            {
                // Rules are cached and only loaded once when the pipeline processor is called for the first time.
                // Skip this method if the rules have already been loaded before.
                return;
            }

            // Verify that we can access the context database.
            if (Sitecore.Context.Database == null)
            {
                Logging.LogError("Cannot load URL rewrite rules because the Sitecore context database is not set.", this);
                return;
            }

            // Load the rules folder item from Sitecore and verify that it exists.
            Item rulesFolder = Sitecore.Context.Database.GetItem(Settings.RulesFolderId);

            if (rulesFolder == null)
            {
                Logging.LogError(string.Format("Cannot load URL rewrite rules folder with ID '{0}' from Sitecore. Verify that it exists.", Settings.RulesFolderId), this);
                return;
            }

            // Load the rewrite entries and add them to the cache.
            rulesFolder.Axes.GetDescendants()
                .ToList()
                .ForEach(this.AddRewriteRule);

            // Remember that the rewrite rules are loaded so we don't load them again during the lifecycle of the application.
            rewriteRulesLoaded = true;

            Logging.LogInfo(string.Format("Cached {0} URL rewrite rules and {1} hostname rewrite rules.", urlRewriteRulesCache.Count, hostNameRewriteRulesCache.Count), this);
        }

        /// <summary>
        /// Add a rewrite rule from Sitecore to the cache.
        /// </summary>
        /// <param name="rewriteRuleItem"></param>
        private void AddRewriteRule(Item rewriteRuleItem)
        {
            // Convert the rewrite rule item to a model object and add to the cache.
            if (rewriteRuleItem.TemplateID.Equals(ItemIds.Templates.UrlRewriteRule))
            {
                // Add a URL rewrite rule.
                var rule = new UrlRewriteRule(rewriteRuleItem);

                if (rule.Validate())
                {
                    urlRewriteRulesCache.Add(rule);
                }
            }
            else if (rewriteRuleItem.TemplateID.Equals(ItemIds.Templates.HostNameRewriteRule))
            {
                // Add a hostname rewrite rule.
                var rule = new HostNameRewriteRule(rewriteRuleItem);

                if (rule.Validate())
                {
                    hostNameRewriteRulesCache.Add(rule);
                }
            }
        }
        #endregion

        #region Rewrite methods
        /// <summary>
        /// If configuration allows it and the request URL ends with a slash, 
        /// the URL is rewritten to one without trailing slash.
        /// </summary>
        /// <param name="args">HttpRequest pipeline arguments.</param>
        private void RewriteTrailingSlash(HttpRequestArgs args)
        {
            // Only rewrite the URL if configuration allows it.
            if (!Settings.RemoveTrailingSlash)
            {
                return;
            }

            // Get the request URL and check for a trailing slash.
            Uri requestUrl = args.Context.Request.Url;

            if (requestUrl.AbsolutePath == "/" || !requestUrl.AbsolutePath.EndsWith("/"))
            {
                // The root document was requested or no trailing slash was found.
                return;
            }

            // 301-redirect to the same URL, but without trailing slash in the path
            string domain = requestUrl.GetComponents(UriComponents.Scheme | UriComponents.Host, UriFormat.Unescaped);
            string path = requestUrl.AbsolutePath.TrimEnd('/');
            string query = requestUrl.Query;
            string targetUrl = string.Concat(domain, path, query);

            if (Settings.LogRewrites)
            {
                Logging.LogInfo(string.Format("Removed trailing slash from '{0}'.", requestUrl), this);
            }

            // Return a permanent redirect to the target URL.
            this.Redirect(targetUrl, args.Context);
        }

        /// <summary>
        /// Rewrite the hostname if it matches any of the hostname rewrite rules.
        /// The requested path and querystring is kept intact, only the hostname is rewritten.
        /// </summary>
        /// <param name="args">HttpRequest pipeline arguments.</param>
        private void RewriteHostName(HttpRequestArgs args)
        {
            if (!hostNameRewriteRulesCache.Any())
            {
                return;
            }

            // Extract the hostname from the request URL.
            Uri requestUrl = args.Context.Request.Url;
            string hostName = requestUrl.Host;
            
            // Check if there is a hostname rewrite rule that matches the requested hostname.
            HostNameRewriteRule rule = hostNameRewriteRulesCache
                .FirstOrDefault(x => x.SourceHostName.Equals(hostName, StringComparison.InvariantCultureIgnoreCase));

            if (rule == null)
            {
                // No matching rewrite rule was found.
                return;
            }

            // Set the target URL with the new hostname and the original path and query.
            string scheme = requestUrl.Scheme;
            string path = requestUrl.AbsolutePath;
            string query = requestUrl.Query;

            string targetUrl = string.Concat(scheme, "://", rule.TargetHostName, path, query);

            if (Settings.LogRewrites)
            {
                // Write an entry to the Sitecore log informing about the rewrite.
                Logging.LogInfo(string.Format("Hostname rewrite rule '{0}' caused the requested URL '{1}' to be rewritten to '{2}'", rule.ItemId, requestUrl.AbsoluteUri, targetUrl), this);
            }

            // Return a permanent redirect to the target URL.
            this.Redirect(targetUrl, args.Context);
        }

        /// <summary>
        /// Rewrite the URL if it matches any of the URL rewrite rules.
        /// </summary>
        /// <param name="args">HttpRequest pipeline arguments.</param>
        private void RewriteUrl(HttpRequestArgs args)
        {
            if (!urlRewriteRulesCache.Any())
            {
                return;
            }

            // Prepare flags to retrieve the URL strings from Uri objects.
            var componentsWithoutQuery = UriComponents.Scheme | UriComponents.Host | UriComponents.Path;
            var componentsWithQuery = componentsWithoutQuery | UriComponents.Query;

            Uri requestUrl = args.Context.Request.Url;

            // If we found a matching URL rewrite rule for the request URL including its querystring,
            // we will rewrite to the exact target URL and dispose the request querystring.
            // Otherwise, if we found a match for the request URL without its querystring,
            // we will rewrite the URL and preserve the querystring from the request.
            bool preserveQueryString = false;

            // Use the request URL including the querystring to find a matching URL rewrite rule.
            UrlRewriteRule rule = urlRewriteRulesCache.FirstOrDefault(x => this.EqualUrl(x.GetSourceUrl(requestUrl), requestUrl, componentsWithQuery));

            if (rule == null)
            {
                // No match was found, try to find a match for the URL without querystring.
                rule = urlRewriteRulesCache.FirstOrDefault(x => this.EqualUrl(x.GetSourceUrl(requestUrl), requestUrl, componentsWithoutQuery));

                preserveQueryString = rule != null;
            }

            if (rule == null)
            {
                // No matching rewrite rule was found.
                return;
            }

            // Set the target URL with or without the original request's querystring.
            string targetUrl = preserveQueryString
                ? string.Concat(rule.GetTargetUrl(requestUrl).GetComponents(componentsWithoutQuery, UriFormat.Unescaped), requestUrl.Query)
                : rule.GetTargetUrl(requestUrl).GetComponents(componentsWithQuery, UriFormat.Unescaped);

            if (Settings.LogRewrites)
            {
                // Write an entry to the Sitecore log informing about the rewrite.
                Logging.LogInfo(string.Format("URL rewrite rule '{0}' caused the requested URL '{1}' to be rewritten to '{2}'", rule.ItemId, requestUrl.AbsoluteUri, targetUrl), this);
            }

            // Return a permanent redirect to the target URL.
            this.Redirect(targetUrl, args.Context);
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Compares the components of two URL's and returns true if they are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        private bool EqualUrl(Uri a, Uri b, UriComponents components)
        {
            string urlA = a.GetComponents(components, UriFormat.Unescaped);
            string urlB = b.GetComponents(components, UriFormat.Unescaped);

            return urlA.Equals(urlB, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Redirect to the URL using HTTP status code 301 (permanent redirect).
        /// </summary>
        /// <param name="url"></param>
        private void Redirect(string url, HttpContext httpContext)
        {
            if (httpContext == null)
            {
                Logging.LogError("Cannot redirect because the HttpContext was not set.", this);
                return;
            }

            // Return a 301 redirect.
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = (int)HttpStatusCode.MovedPermanently;
            httpContext.Response.RedirectLocation = url;
            httpContext.Response.End();
        }

        /// <summary>
        /// Indicates whether the current request is a request to the Sitecore Client.
        /// </summary>
        /// <returns></returns>
        private bool IsSitecoreClientRequest()
        {
            return false;
        }
        #endregion
    }
}