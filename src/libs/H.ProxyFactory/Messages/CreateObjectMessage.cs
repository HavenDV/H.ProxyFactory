namespace H.ProxyFactory.Messages;

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
    public Guid? Guid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public CreateObjectMessage()
    {
        Text = "create_object";
    }
}
