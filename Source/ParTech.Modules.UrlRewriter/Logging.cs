namespace ParTech.Modules.UrlRewriter
{
    using Sitecore.Diagnostics;

    /// <summary>
    /// Provides methods for writing messages to the Sitecore log.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Format to use for log messages.
        /// </summary>
        private static string logMessageFormat = "ParTech.Modules.UrlRewriter: {0}";

        /// <summary>
        /// Log an error message to the Sitecore log.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="owner"></param>
        public static void LogError(string message, object owner)
        {
            Log.Error(string.Format(logMessageFormat, message), owner);
        }

        /// <summary>
        /// Log an info message to the Sitecore log.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="owner"></param>
        public static void LogInfo(string message, object owner)
        {
            Log.Info(string.Format(logMessageFormat, message), owner);
        }
    }
}