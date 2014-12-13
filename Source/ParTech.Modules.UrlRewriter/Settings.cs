namespace ParTech.Modules.UrlRewriter
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using Sitecore.Collections;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Diagnostics;

    /// <summary>
    /// Provides access to the configuration for the URL Rewriter module.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed.")]
    public class Settings
    {
        /// <summary>
        /// Static accessor for the URL Rewriter settings.
        /// This will initialize the Settings class and load the settings from the Sitecore configuration XML.
        /// </summary>
        private static Settings settings = new Settings();

        #region Instance members

        /// <summary>
        /// Dictionary containing the URL Rewriter settings from the Sitecore configuration XML.
        /// </summary>
        private readonly SafeDictionary<string> settingValues = new SafeDictionary<string>();

        /// <summary>
        /// Name of the configuration node.
        /// </summary>
        private readonly string settingsNodeName = "ParTech.Modules.UrlRewriter";

        /// <summary>
        /// The settings node.
        /// </summary>
        private XmlNode settingsNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings" /> class.
        /// </summary>
        public Settings()
        {
            // Load settings from configuration XML into dictionary.
            foreach (XmlNode node in this.SettingsNode.SelectNodes("./*"))
            {
                this.settingValues.Add(node.Name.ToLower(), node.InnerText);
            }
        }

        /// <summary>
        /// Gets the settings configuration node from the Sitecore configuration XML.
        /// </summary>
        private XmlNode SettingsNode
        {
            get
            {
                if (this.settingsNode == null)
                {
                    // Load the URL Rewriter configuration node from the Sitecore configuration.
                    this.settingsNode = Factory.GetConfigNode(this.settingsNodeName);

                    if (this.settingsNode == null || !this.settingsNode.HasChildNodes)
                    {
                        throw new UrlRewriterException("Could not load configuration node '{0}' with URL Rewriter settings.", this.settingsNodeName);
                    }
                }

                return this.settingsNode;
            }
        }

        /// <summary>
        /// Gets a string setting from the URL Rewriter configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="required">Indicates whether to throw an exception if the setting could not be found.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <exception cref="UrlRewriterException">URL Rewriter setting '{0}' could not be found and is required.</exception>
        public string GetString(string name, bool required = true, string defaultValue = null)
        {
            Assert.ArgumentNotNull(name, "name");

            string value = this.settingValues[name.ToLower()];

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                {
                    throw new UrlRewriterException("URL Rewriter setting '{0}' could not be found and is required.", name);
                }

                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Gets a boolean setting from the URL Rewriter configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="required">Indicates whether to throw an exception if the setting could not be found or parsed.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <exception cref="UrlRewriterException">URL Rewriter setting '{0}' is required and has an invalid value (boolean expected).</exception>
        public bool GetBoolean(string name, bool required = true, bool defaultValue = false)
        {
            string value = this.GetString(name, required);
            bool result;

            if (!bool.TryParse(value, out result))
            {
                if (required)
                {
                    throw new UrlRewriterException("URL Rewriter setting '{0}' is required and has an invalid value (boolean expected).", name);
                }

                result = defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Gets a Sitecore ID setting from the URL Rewriter configuration.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="required">Indicates whether to throw an exception if the setting could not be found or parsed.</param>
        /// <returns></returns>
        public ID GetID(string name, bool required = true)
        {
            string value = this.GetString(name, required);

            if (string.IsNullOrWhiteSpace(value) && !required)
            {
                return ID.Null;
            }

            ID result;

            if (!ID.TryParse(value, out result) && required)
            {
                throw new UrlRewriterException("URL Rewriter setting '{0}' is required and has an invalid value (ID expected).", name);
            }

            return result;
        }

        #endregion

        #region Static setting properties

        /// <summary>
        /// Gets the configured ID of the Sitecore item that contains the rewrite rules.
        /// </summary>
        public static ID RulesFolderId
        {
            get { return settings.GetID("RulesFolderId"); }
        }

        /// <summary>
        /// Gets the rules table item identifier.
        /// </summary>
        public static ID RulesTableItemId
        {
            get { return settings.GetID("RulesTableItemId", false); }
        }

        /// <summary>
        /// Gets a value indicating whether trailing slashes must be removed from the request URL.
        /// </summary>
        public static bool RemoveTrailingSlash
        {
            get { return settings.GetBoolean("RemoveTrailingSlash"); }
        }

        /// <summary>
        /// Gets a value indicating whether to write an entry to the Sitecore log every time a URL is rewritten.
        /// </summary>
        public static bool LogRewrites
        {
            get { return settings.GetBoolean("LogRewrites", false, false); }
        }

        /// <summary>
        /// Gets a value indicating whether the URL Rewriter Module has been enabled.
        /// </summary>
        public static bool Enabled
        {
            get { return settings.GetBoolean("Enabled", false, false); }
        }

        /// <summary>
        /// Gets the name of the Core database.
        /// </summary>
        public static string CoreDatabase
        {
            get { return settings.GetString("CoreDatabase", false, "core"); }
        }

        /// <summary>
        /// Gets an array with site names for which URL rewriting must be skipped.
        /// </summary>
        public static string[] IgnoreForSites
        {
            get
            {
                return settings.GetString("IgnoreForSites", false, "shell,login")
                    .ToLower()
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        #endregion
    }
}