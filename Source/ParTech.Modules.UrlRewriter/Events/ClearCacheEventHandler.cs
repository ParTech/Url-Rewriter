using System;
using Sitecore.Events;
using ParTech.Modules.UrlRewriter.Pipelines;

namespace ParTech.Modules.UrlRewriter.Events
{
    /// <summary>
    /// Handles cache clearing for the URL Rewriter.
    /// </summary>
    public class ClearCacheEventHandler
    {
        /// <summary>
        /// Called by the EventQueue when a ClearCacheEvent is processed.
        /// </summary>
        /// <param name="e"></param>
        public static void Run(ClearCacheEvent e)
        {
            // Raise the clearcache event so the cache is cleared on this instance.
            Event.RaiseEvent("urlrewriter:clearcache", new ClearCacheEventArgs(e));
        }

        /// <summary>
        /// Called when the 'urlrewriter:clearcache' event is raised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnClearCache(object sender, EventArgs e)
        {
            Logging.LogInfo("Clear cache event was raised.", this);

            UrlRewriteHandler.ClearCache();
        }
    }
}