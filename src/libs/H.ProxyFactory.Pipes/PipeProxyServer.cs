namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PipeProxyServer : RemoteProxyServer
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public PipeProxyServer() : base(new PipeConnection(false))
        {
        }

        #endregion
    }
}
