using System;

namespace H.Utilities.Messages
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MethodMessage : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public string? MethodName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? ObjectGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? MethodGuid { get; set; }
    }
}
