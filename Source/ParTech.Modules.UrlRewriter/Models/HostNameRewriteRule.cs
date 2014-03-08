using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace ParTech.Modules.UrlRewriter.Models
{
    /// <summary>
    /// Represents a domain rewriting rule.
    /// </summary>
    public class HostNameRewriteRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostNameRewriteRule" /> class.
        /// </summary>
        /// <param name="item">Sitecore item based on a HostName Rewrite Rule template.</param>
        public HostNameRewriteRule(Item item)
        {
            this.ItemId = item.ID;

            this.SourceHostName = item[ItemIds.Fields.SourceHostName];
            this.TargetHostName = item[ItemIds.Fields.TargetHostName];
        }

        /// <summary>
        /// Gets or sets the Sitecore ID of the item containing the data for this model.
        /// </summary>
        public ID ItemId { get; set; }

        /// <summary>
        /// Gets the source hostname; the hostname that needs to be rewritten.
        /// </summary>
        /// <remarks>
        /// Expected format is a hostname without protocol prefix or slashes.
        /// e.g. www.mydomain.com
        /// </remarks>
        public string SourceHostName { get; private set; }

        /// <summary>
        /// Gets the target hostname; the hostname to redirect to.
        /// </summary>
        /// <remarks>
        /// Expected format is a hostname without protocol prefix or slashes.
        /// e.g. www.mydomain.com
        /// </remarks>
        public string TargetHostName { get; private set; }

        /// <summary>
        /// Validate the values of this rule and write an error to the Sitecore log if it's invalid.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            // Ensure that the field values are not empty.
            if (string.IsNullOrEmpty(this.SourceHostName))
            {
                Logging.LogError(string.Format("Hostname rewrite rule with ID '{0}' is invalid because the Source Hostname field is empty.", this.ItemId), this);
                return false;
            }

            if (string.IsNullOrEmpty(this.TargetHostName))
            {
                Logging.LogError(string.Format("Hostname rewrite rule with ID '{0}' is invalid because the Target Hostname field is empty.", this.ItemId), this);
                return false;
            }

            // Ensure the values are valid hostnames or IP addresses.
            string validIpRegEx = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            string validHostNameRegEx = @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$";

            if (!(Regex.IsMatch(this.SourceHostName, validIpRegEx) || Regex.IsMatch(this.SourceHostName, validHostNameRegEx)))
            {
                Logging.LogError(string.Format("Hostname rewrite rule with ID '{0}' is invalid because the Source Hostname field does not contain a hostname or IP", this.ItemId), this);
                return false;
            }

            if (!(Regex.IsMatch(this.TargetHostName, validIpRegEx) || Regex.IsMatch(this.TargetHostName, validHostNameRegEx)))
            {
                Logging.LogError(string.Format("Hostname rewrite rule with ID '{0}' is invalid because the Target Hostname field does not contain a hostname or IP", this.ItemId), this);
                return false;
            }

            return true;
        }
    }
}
