using System;

namespace H.Utilities.Messages
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class LoadAssemblyMessage : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LoadAssemblyMessage()
        {
            Text = "load_assembly";
        }
    }
}
