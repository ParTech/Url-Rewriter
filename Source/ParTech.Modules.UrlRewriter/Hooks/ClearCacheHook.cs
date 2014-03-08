using System;
using Sitecore.Eventing;
using Sitecore.Events.Hooks;
using ParTech.Modules.UrlRewriter.Events;

namespace ParTech.Modules.UrlRewriter.Hooks
{
    /// <summary>
    /// Hook that subscribes the CacheFlushEvent to the EventQueue.
    /// </summary>
    public class ClearCacheHook : IHook
    {
        /// <summary>
        /// Initializes the ClearCache hook.
        /// </summary>
        public void Initialize()
        {
            EventManager.Subscribe<ClearCacheEvent>(new Action<ClearCacheEvent>(ClearCacheEventHandler.Run));
        }
    }
}