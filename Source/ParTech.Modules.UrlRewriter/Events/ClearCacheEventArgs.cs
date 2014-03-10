namespace ParTech.Modules.UrlRewriter.Events
{
    using System;
    using Sitecore.Events;

    /// <summary>
    /// EventArgs for ClearCacheEvent
    /// </summary>
    public class ClearCacheEventArgs : EventArgs, IPassNativeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClearCacheEventArgs"/> class.
        /// </summary>
        /// <param name="e">The event.</param>
        public ClearCacheEventArgs(ClearCacheEvent e)
        {
        }
    }
}