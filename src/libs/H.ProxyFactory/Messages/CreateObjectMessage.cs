namespace H.Utilities.Messages
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CreateObjectMessage : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public string? TypeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? Guid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CreateObjectMessage()
        {
            Text = "create_object";
        }
    }
}
