using System;
using Sitecore.Events;

namespace ParTech.Modules.UrlRewriter.Events
{
    /// <summary>
    /// EventArgs for ClearCacheEvent
    /// </summary>
    public class ClearCacheEventArgs : EventArgs, IPassNativeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClearCacheEventArgs" /> class.
        /// </summary>
        /// <param name="e">/param>
        public ClearCacheEventArgs(ClearCacheEvent e)
        {
        }
    }
}