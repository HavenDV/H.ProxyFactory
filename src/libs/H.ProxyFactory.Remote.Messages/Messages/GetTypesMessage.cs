﻿namespace H.ProxyFactory.Remote.Messages;

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
