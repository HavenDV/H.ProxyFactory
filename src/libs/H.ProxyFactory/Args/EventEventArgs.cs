﻿using System.Reflection;

namespace H.ProxyFactory.Args;

/// <summary>
/// 
/// </summary>
public class EventEventArgs : EventArgs
{
    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyCollection<object?> Arguments { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public EventInfo EventInfo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public EmptyProxyFactory ProxyFactory { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsCanceled { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="eventInfo"></param>
    /// <param name="proxyFactory"></param>
    public EventEventArgs(IReadOnlyCollection<object?> arguments, EventInfo eventInfo, EmptyProxyFactory proxyFactory)
    {
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        EventInfo = eventInfo ?? throw new ArgumentNullException(nameof(eventInfo));
        ProxyFactory = proxyFactory ?? throw new ArgumentNullException(nameof(proxyFactory));
    }
}
