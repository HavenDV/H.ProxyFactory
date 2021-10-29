using H.ProxyFactory;
using System.Reflection;

namespace H.ProxyFactory.Args
{
    /// <summary>
    /// 
    /// </summary>
    public class MethodEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public List<object?> Arguments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public EmptyProxyFactory ProxyFactory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object? ReturnObject { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// 
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCanceled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="methodInfo"></param>
        /// <param name="proxyFactory"></param>
        public MethodEventArgs(List<object?> arguments, MethodInfo methodInfo, EmptyProxyFactory proxyFactory)
        {
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            ProxyFactory = proxyFactory ?? throw new ArgumentNullException(nameof(proxyFactory));
        }
    }
}
