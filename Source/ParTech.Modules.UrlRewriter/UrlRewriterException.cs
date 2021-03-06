﻿namespace ParTech.Modules.UrlRewriter
{
    using System;

    /// <summary>
    /// Represents an exception thrown by the Url Rewriter module.
    /// </summary>
    public class UrlRewriterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlRewriterException" /> class.
        /// </summary>
        public UrlRewriterException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlRewriterException" /> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stringFormatValues">Values to apply to the message using string.Format</param>
        public UrlRewriterException(string message, params object[] stringFormatValues)
            : base(string.Format(message, stringFormatValues))
        {
        }
    }
}