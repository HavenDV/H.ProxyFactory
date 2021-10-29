namespace H.ProxyFactory;

/// <summary>
/// 
/// </summary>
public sealed class PipeProxyFactory : RemoteProxyFactory
{
    #region Constructors

    /// <summary>
    /// 
    /// </summary>
#pragma warning disable CA2000 // Dispose objects before losing scope
    public PipeProxyFactory() : base(new PipeConnection(true))
#pragma warning restore CA2000 // Dispose objects before losing scope
    {
    }

    #endregion
}
