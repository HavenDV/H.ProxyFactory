﻿namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PipeProxyFactory : RemoteProxyFactory
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public PipeProxyFactory() : base(new PipeConnection(true))
        {
        }

        #endregion
    }
}
