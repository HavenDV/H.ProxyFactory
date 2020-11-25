using System;

namespace H.Utilities.Messages
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class GetTypesMessage : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public GetTypesMessage()
        {
            Text = "get_types";
        }
    }
}
