namespace H.Ipc.Messages;

/// <summary>
/// 
/// </summary>
[Serializable]
public class RaiseEventMessage : Message
{
    /// <summary>
    /// 
    /// </summary>
    public Guid? ObjectGuid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? EventName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Guid? EventGuid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public RaiseEventMessage()
    {
        Text = "raise_event";
    }

    /// <summary>
    /// 
    /// </summary>
    public string ConnectionName => $"H.Containers.Process_{ObjectGuid}_{EventName}_Event_{EventGuid}";
}
