using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;

namespace ParTech.Modules.UrlRewriter
{
    /// <summary>
    /// References to Sitecore item ID's that belong to the Url Rewriter module.
    /// </summary>
    public static class ItemIds
    {
        /// <summary>
        /// References to Sitecore templates that belong to the Url Rewriter module.
        /// </summary>
        public static class Templates
        {
            /// <summary>
            /// Gets the ID for the Hostname Rewrite Rule template.
            /// </summary>
            public static readonly ID HostNameRewriteRule = new ID("{C0884868-FE60-4AD3-91DD-A5DB6DC1FA16}");

            /// <summary>
            /// Gets the ID for the URL Rewrite Rule template.
            /// </summary>
            public static readonly ID UrlRewriteRule = new ID("{24DB2387-14C3-46E3-B9B0-362E0D787424}");

            /// <summary>
            /// Gets the ID for the Rewrite Rules Folder template.
            /// </summary>
            public static readonly ID RewriteRulesFolder = new ID("{CBF22343-A180-46E9-B20D-329C9382E10C}");
        }

        /// <summary>
        /// References to Sitecore fields that belong to the Url Rewriter module.
        /// </summary>
        public static class Fields
        {
            /// <summary>
            /// Gets the ID for the Source URL field.
            /// </summary>
            public static readonly ID SourceUrl = new ID("{646F3B6F-1F33-4E01-B31B-1D2DDDC7F8EF}");

            /// <summary>
            /// Gets the ID for the Target URL field.
            /// </summary>
            public static readonly ID TargetUrl = new ID("{8AFF68DF-A577-41A0-B66F-533B44E08005}");

            /// <summary>
            /// Gets the ID for the Source Hostname field.
            /// </summary>
            public static readonly ID SourceHostName = new ID("{052C77CB-05B7-421A-B97C-CCD7DE89AB89}");

            /// <summary>
            /// Gets the ID for the Target Hostname field.
            /// </summary>
            public static readonly ID TargetHostName = new ID("{BE421AF4-DA73-4F12-89B5-2FBEFF6B7633}");
        }
    }
}