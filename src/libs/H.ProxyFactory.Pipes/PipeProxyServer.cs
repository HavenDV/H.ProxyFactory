namespace H.ProxyFactory;

/// <summary>
/// 
/// </summary>
public sealed class PipeProxyServer : RemoteProxyServer
{
    #region Constructors

    /// <summary>
    /// 
    /// </summary>
#pragma warning disable CA2000 // Dispose objects before losing scope
    public PipeProxyServer() : base(new PipeConnection(false))
#pragma warning restore CA2000 // Dispose objects before losing scope
    {
    }

    #endregion
}
