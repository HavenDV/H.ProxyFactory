using System;

namespace H.Utilities.Messages
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class RunMethodMessage : MethodMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public RunMethodMessage()
        {
            Text = "run_method";
        }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionPrefix => $"H.Containers.Process_{ObjectGuid}_{MethodName}_{MethodGuid}_";
    }
}
