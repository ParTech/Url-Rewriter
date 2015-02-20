namespace ParTech.Modules.UrlRewriter.Pipelines
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using ParTech.Modules.UrlRewriter.Models;
    using Sitecore;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Pipelines.HttpRequest;

    /// <summary>
    /// Pipeline processor that processes URL rewriter rules.
    /// </summary>
    public class UrlRewriteHandler : HttpRequestProcessor
    {
        #region Cache objects

        /// <summary>
        /// Cache for <see cref="UrlRewriteRule"/> objects.
        /// </summary>
        private static Dictionary<string, UrlRewriteRule> urlRewriteRulesCache = new Dictionary<string, UrlRewriteRule>();

        /// <summary>
        /// Cache for <see cref="HostNameRewriteRule"/> objects.
        /// </summary>
        private static List<HostNameRewriteRule> hostNameRewriteRulesCache = new List<HostNameRewriteRule>();

        /// <summary>
        /// Indicates whether the rewrite rules have been loaded from Sitecore.
        /// </summary>
        private static bool rewriteRulesLoaded;

        /// <summary>
        /// The locking object.
        /// </summary>
        private static object locking = new object();
        
        #endregion

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

        /// <summary>
        /// Executes the pipeline processor.
        /// </summary>
        /// <param name="args"></param>
        public override void Process(HttpRequestArgs args)
        {
            // Ignore requests that are not GET requests,
            // have the context database set to Core or point to ignored sites
            if (this.IgnoreRequest(args.Context))
            {
                return;
            }

            // Load the rewrite rules from Sitecore into the cache.
            this.LoadRewriteRules();

            // Rewrite URL's that contain trailing slashes if configuration allows it.
            this.RewriteTrailingSlash(args);

            // Try to tewrite the request URL based on URL rewrite rules.
            this.RewriteUrl(args);

            // Try to rewrite the request URL based on Hostname rewrite rules.
            this.RewriteHostName(args);
        }

        #region Rules loading methods

        /// <summary>
        /// Load the rewrite rules from Sitecore.
        /// </summary>
        private void LoadRewriteRules()
        {
            lock (locking)
            {
                if (rewriteRulesLoaded && urlRewriteRulesCache != null && hostNameRewriteRulesCache != null)
                {
                    // Rules are cached and only loaded once when the pipeline processor is called for the first time.
                    // Skip this method if the rules have already been loaded before.
                    return;
                }

                // Ensure the cache objects are never null.
                urlRewriteRulesCache = new Dictionary<string, UrlRewriteRule>();
                hostNameRewriteRulesCache = new List<HostNameRewriteRule>();

                // Verify that we can access the context database.
                if (Context.Database == null)
                {
                    Logging.LogError("Cannot load URL rewrite rules because the Sitecore context database is not set.", this);
                    return;
                }

                // Load the rules folder item from Sitecore and verify that it exists.
                Item rulesFolder = Context.Database.GetItem(Settings.RulesFolderId);

                if (rulesFolder == null)
                {
                    Logging.LogError(string.Format("Cannot load URL rewrite rules folder with ID '{0}' from Sitecore. Verify that it exists.", Settings.RulesFolderId), this);
                    return;
                }

                // Load the rewrite entries and add them to the cache.
                rulesFolder.Axes.GetDescendants()
                    .ToList()
                    .ForEach(this.AddRewriteRule);

                // Load the rules from the raw data item with rules table.
                Item rulesTable = Context.Database.GetItem(Settings.RulesTableItemId);

                if (rulesTable == null)
                {
                    Logging.LogInfo(string.Format("Rules table item with ID '{0}' does not exist.", Settings.RulesTableItemId), this);
                }
                else
                {
                    this.AddRewriteRulesTable(rulesTable[new ID("{4AA2FCD6-B2D7-452F-B4FB-35CB4A35A1B3}")]);
                }

                Logging.LogInfo(string.Format("Cached {0} URL rewrite rules and {1} hostname rewrite rules.", urlRewriteRulesCache.Count, hostNameRewriteRulesCache.Count), this);

                // Remember that the rewrite rules are loaded so we don't load them again during the lifecycle of the application.
                rewriteRulesLoaded = true;
            }
        }

        /// <summary>
        /// Adds the rewrite rules table.
        /// </summary>
        /// <param name="data">The data.</param>
        private void AddRewriteRulesTable(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return;
            }

            string[] lines = data.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { "|>|" }, StringSplitOptions.None);
                string sourceUrl = parts.First();
                string targetUrl = parts.Last();

                if (sourceUrl == targetUrl)
                {
                    continue;
                }

                // Add a URL rewrite rule.
                var rule = new UrlRewriteRule(sourceUrl, targetUrl);

                if (rule.Validate())
                {
                    sourceUrl = sourceUrl.ToLower();

                    if (!urlRewriteRulesCache.ContainsKey(sourceUrl))
                    {
                        urlRewriteRulesCache.Add(sourceUrl, rule);
                    }
                }
                else
                {
                    Logging.LogError(string.Concat("Ignored invalid source URL: ", sourceUrl), this);
                }
            }
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
                    if (!urlRewriteRulesCache.ContainsKey(rule.SourceUrl))
                    {
                        urlRewriteRulesCache.Add(rule.SourceUrl.ToLower(), rule);
                    }
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

            Uri requestUri = args.Context.Request.Url;
            string rawUrl = string.Concat(requestUri.Scheme, "://", requestUri.Host, args.Context.Request.RawUrl);

            // The request URL might already be rewritten by Sitecore to remove the language code.
            // We need the original request URL.
            if (!requestUri.PathAndQuery.Equals(args.Context.Request.RawUrl, StringComparison.OrdinalIgnoreCase))
            {
                requestUri = new Uri(rawUrl);
            }

            // Get request URL and query as string.
            string requestUrl = requestUri.ToString().ToLower();

            string[] querySplit = requestUrl.Split('?');
            string requestUrlWithoutQuery = querySplit.First();
            string requestQuery = querySplit.Length > 1
                ? querySplit[1]
                : null;

            // If we found a matching URL rewrite rule for the request URL including its querystring,
            // we will rewrite to the exact target URL and dispose the request querystring.
            // Otherwise, if we found a match for the request URL without its querystring,
            // we will rewrite the URL and preserve the querystring from the request.
            bool preserveQueryString = false;

            // Use the request URL including the querystring to find a matching URL rewrite rule.
            UrlRewriteRule rule = null;

            if (urlRewriteRulesCache.ContainsKey(requestUrl))
            {
                rule = urlRewriteRulesCache[requestUrl];
            }

            if (rule == null)
            {
                // No match was found, try to find a match for the URL without querystring.
                if (urlRewriteRulesCache.ContainsKey(requestUrlWithoutQuery))
                {
                    rule = urlRewriteRulesCache[requestUrlWithoutQuery];
                }

                preserveQueryString = rule != null;
            }

            if (rule == null)
            {
                // No matching rewrite rule was found.
                return;
            }

            // Set the target URL with or without the original request's querystring.
            string targetUrl = preserveQueryString
                ? string.Concat(rule.TargetUrl, "?", requestQuery)
                : rule.TargetUrl;

            // Return a permanent redirect to the target URL.
            this.Redirect(targetUrl, args.Context);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Redirect to the URL using HTTP status code 301 (permanent redirect).
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpContext">The HTTP context.</param>
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
        /// Indicates whether the current request must be ignored by the URL Rewriter module.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns></returns>
        private bool IgnoreRequest(HttpContext httpContext)
        {
            // Only GET request can be rewritten.
            bool getRequest = httpContext.Request.HttpMethod.Equals("get", StringComparison.InvariantCultureIgnoreCase);

            // Check if the context database is set to Core.
            bool coreDatabase = Context.Database != null
                && Context.Database.Name.Equals(Settings.CoreDatabase, StringComparison.InvariantCultureIgnoreCase);

            // CHeck if the context site is in the list of ignored sites.
            bool ignoredSite = Settings.IgnoreForSites.Contains(Context.GetSiteName().ToLower());

            string rawUrl = httpContext.Request.RawUrl;
            bool ignorePages = rawUrl.StartsWith("/mvc/", StringComparison.OrdinalIgnoreCase) || rawUrl.StartsWith("/~/media/", StringComparison.OrdinalIgnoreCase);

            return !getRequest || coreDatabase || ignoredSite || ignorePages;
        }

        #endregion
    }
}