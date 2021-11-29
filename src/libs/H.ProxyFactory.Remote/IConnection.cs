using H.Ipc.Messages;

namespace H.ProxyFactory;

/// <summary>
/// Defines the connection used for RemoteProxyFactory and RemoteProxyServer
/// </summary>
public interface IConnection : IAsyncDisposable
{
    #region Events

    /// <summary>
    /// When a new internal message is received
    /// </summary>
    event EventHandler<Message>? MessageReceived;

    /// <summary>
    /// When a exception is occurred
    /// </summary>
    event EventHandler<Exception>? ExceptionOccurred;

    #endregion

    #region Public methods

    /// <summary>
    /// Initializes the connection
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitializeAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends internal message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendMessageAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends data of a certain type to the other side
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync<T>(string name, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives data of a certain type from the other side
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T> ReceiveAsync<T>(string name, CancellationToken cancellationToken = default);

    #endregion
}
