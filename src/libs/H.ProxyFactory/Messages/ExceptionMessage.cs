﻿using System;

namespace H.Utilities.Messages
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ExceptionMessage : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ExceptionMessage()
        {
            Text = "exception";
        }
    }
}
