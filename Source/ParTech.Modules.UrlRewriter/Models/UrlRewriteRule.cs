namespace ParTech.Modules.UrlRewriter.Models
{
    using System;
    using Sitecore.Data;
    using Sitecore.Data.Items;

    /// <summary>
    /// Represents a URL rewriting rule.
    /// </summary>
    public class UrlRewriteRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlRewriteRule" /> class.
        /// </summary>
        /// <param name="item">Sitecore item based on a URL Rewrite Rule template.</param>
        public UrlRewriteRule(Item item)
        {
            this.ItemId = item.ID;

            this.SourceUrl = item[ItemIds.Fields.SourceUrl];
            this.TargetUrl = item[ItemIds.Fields.TargetUrl];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlRewriteRule"/> class.
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="targetUrl">The target URL.</param>
        public UrlRewriteRule(string sourceUrl, string targetUrl)
        {
            this.SourceUrl = sourceUrl;
            this.TargetUrl = targetUrl;
        }

        /// <summary>
        /// Gets or sets the Sitecore ID of the item containing the data for this model.
        /// </summary>
        public ID ItemId { get; set; }

        /// <summary>
        /// Gets or sets the source URL as string.
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// Gets or sets the target URL as string.
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// Gets the source URL; the URL that needs to be rewritten.
        /// </summary>
        /// <param name="requestUrl">The request URL that is being rewritten.</param>
        /// <returns></returns>
        public Uri GetSourceUri(Uri requestUrl)
        {
            string absoluteSourceUrl = this.GetAbsoluteUrl(this.SourceUrl, requestUrl);

            if (!Uri.IsWellFormedUriString(absoluteSourceUrl, UriKind.Absolute))
            {
                Logging.LogError(string.Format("Source URL '{0}' defined in rewrite rule item '{1}' is not well formed.", absoluteSourceUrl, this.ItemId), this);
                return default(Uri);
            }

            return new Uri(absoluteSourceUrl);
        }

        /// <summary>
        /// Gets the target URL; the URL where to redirect to.
        /// </summary>
        /// <param name="requestUrl">The request URL that is being rewritten.</param>
        /// <returns></returns>
        public Uri GetTargetUri(Uri requestUrl)
        {
            string absoluteTargetUrl = this.GetAbsoluteUrl(this.TargetUrl, requestUrl);

            if (!Uri.IsWellFormedUriString(absoluteTargetUrl, UriKind.Absolute))
            {
                Logging.LogError(string.Format("Target URL '{0}' defined in rewrite rule item '{1}' is not well formed.", absoluteTargetUrl, this.ItemId), this);
                return default(Uri);
            }

            return new Uri(absoluteTargetUrl);
        }

        /// <summary>
        /// Validate the values of this rule and write an error to the Sitecore log if it's invalid.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            // Ensure that the field values are not empty.
            if (string.IsNullOrEmpty(this.SourceUrl))
            {
                Logging.LogError(string.Format("URL rewrite rule with ID '{0}' is invalid because the Source URL field is empty.", this.ItemId), this);
                return false;
            }

            if (string.IsNullOrEmpty(this.TargetUrl))
            {
                Logging.LogError(string.Format("URL rewrite rule with ID '{0}' is invalid because the Target URL field is empty.", this.ItemId), this);
                return false;
            }

            // Ensure that the URL's are well formed and can be parsed to Uri objects.
            try
            {
                var test = new Uri(this.SourceUrl);
            }
            catch
            {
                return false;
            }

            try
            {
                var test = new Uri(this.TargetUrl);
            }
            catch
            {
                return false;
            }

            /*
            if (!Uri.IsWellFormedUriString(this.SourceUrl, UriKind.RelativeOrAbsolute))
            {
                Logging.LogError(string.Format("URL rewrite rule with ID '{0}' is invalid because the Source URL field contains an invalid URL.", this.ItemId), this);
                return false;
            }

            if (!Uri.IsWellFormedUriString(this.TargetUrl, UriKind.RelativeOrAbsolute))
            {
                Logging.LogError(string.Format("URL rewrite rule with ID '{0}' is invalid because the Target URL field contains an invalid URL.", this.ItemId), this);
                return false;
            }*/

            return true;
        }

        /// <summary>
        /// Ensures that the relativeOrAbsoluteUrl is returned as an absolute URL.
        /// If the input is a relative URL, the scheme and hostname from the requestUrl will be used in the resulting absolute URL.
        /// </summary>
        /// <param name="relativeOrAbsoluteUrl"></param>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        private string GetAbsoluteUrl(string relativeOrAbsoluteUrl, Uri requestUrl)
        {
            string baseUrl = string.Empty;

            if (!Uri.IsWellFormedUriString(relativeOrAbsoluteUrl, UriKind.Absolute))
            {
                // The input URL is not an absolute URL, so we need to add a scheme and hostname to it.
                // Use the scheme and host from the request URL to get the absolute URL.
                baseUrl = requestUrl.GetComponents(UriComponents.Scheme | UriComponents.Host, UriFormat.Unescaped);

                // Ensure the relative target URL starts with a slash.
                relativeOrAbsoluteUrl = relativeOrAbsoluteUrl.TrimStart('/').Insert(0, "/");
            }

            return string.Concat(baseUrl, relativeOrAbsoluteUrl);
        }
    }
}