namespace H.ProxyFactory.Messages;

/// <summary>
/// 
/// </summary>
[Serializable]
public class CancelMethodMessage : MethodMessage
{
    /// <summary>
    /// 
    /// </summary>
    public CancelMethodMessage()
    {
        Text = "cancel_method";
    }
}
