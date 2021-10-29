namespace H.ProxyFactory.Extensions;

/// <summary>
/// 
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="eventName"></param>
    /// <param name="args"></param>
    public static void RaiseEvent(this object obj, string eventName, object?[] args)
    {
        obj = obj ?? throw new ArgumentNullException(nameof(obj));
        eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        args = args ?? throw new ArgumentNullException(nameof(args));

        var method = obj.GetType().GetMethod($"On{eventName}")
                     ?? throw new ArgumentException($"On{eventName} method is not found");

        method.Invoke(obj, args);
    }
}
