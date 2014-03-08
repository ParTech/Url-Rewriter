using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;

namespace ParTech.Modules.UrlRewriter
{
    /// <summary>
    /// Provides access to the configuration for the URL Rewriter module.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Static accessor for the URL Rewriter settings.
        /// This will initialize the Settings class and load the settings from the Sitecore configuration XML.
        /// </summary>
        private static readonly Settings settings = new Settings();

        #region Instance members
        /// <summary>
        /// Name of the configuration node.
        /// </summary>
        private readonly string settingsNodeName = "ParTech.Modules.UrlRewriter";

        /// <summary>
        /// Dictionary containing the URL Rewriter settings from the Sitecore configuration XML.
        /// </summary>
        private readonly SafeDictionary<string> settingValues = new SafeDictionary<string>();

        private XmlNode settingsNode = null;

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
        /// Gets a string setting from the URL Rewriter configuration.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="required">Indicates whether to throw an exception if the setting could not be found.</param>
        /// <returns></returns>
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
        /// <param name="name"></param>
        /// <param name="required">Indicates whether to throw an exception if the setting could not be found or parsed.</param>
        /// <returns></returns>
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
        public static ID RulesFolderId { get { return settings.GetID("RulesFolderId"); }  }

        /// <summary>
        /// Gets a value indicating whether trailing slashes must be removed from the request URL.
        /// </summary>
        public static bool RemoveTrailingSlash { get { return settings.GetBoolean("RemoveTrailingSlash"); } }

        /// <summary>
        /// Gets a value indicating whether to write an entry to the Sitecore log every time a URL is rewritten.
        /// </summary>
        public static bool LogRewrites { get { return settings.GetBoolean("LogRewrites", false, false); } }

        /// <summary>
        /// Gets a value indicating whether the URL Rewriter Module has been enabled.
        /// </summary>
        public static bool Enabled { get { return settings.GetBoolean("Enabled", false, false); } }

        /// <summary>
        /// Gets the name of the Core database.
        /// </summary>
        public static string CoreDatabase { get { return settings.GetString("CoreDatabase", false, "core"); } }

        /// <summary>
        /// Gets an array with site names for which URL rewriting must be skipped.
        /// </summary>
        public static string[] IgnoreForSites 
        { 
            get 
            { 
                return settings.GetString("IgnoreForSites", false, "shell,login")
                    .ToLower()
                    .Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries); 
            } 
        }
        #endregion
    }
}