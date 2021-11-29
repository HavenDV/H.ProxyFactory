namespace H.Ipc.Messages;

/// <summary>
/// 
/// </summary>
[Serializable]
public class CreateObjectMessage : RunMethodMessage
{
    /// <summary>
    /// 
    /// </summary>
    public string? TypeName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public CreateObjectMessage()
    {
        Text = "create_object";
    }
}
