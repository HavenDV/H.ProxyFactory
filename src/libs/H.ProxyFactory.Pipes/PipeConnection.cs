using H.Pipes;
using H.Pipes.Extensions;
using H.Ipc.Messages;

namespace H.ProxyFactory;

/// <summary>
/// 
/// </summary>
public class PipeConnection : IConnection
{
    #region Properties

    private bool IsFactory { get; }
    private IPipeConnection<Message>? InternalConnection { get; set; }

    #endregion

    #region Events

    /// <summary>
    /// 
    /// </summary>
    public event EventHandler<Message>? MessageReceived;

    /// <summary>
    /// 
    /// </summary>
    public event EventHandler<Exception>? ExceptionOccurred;

    private void OnExceptionOccurred(Exception exception)
    {
        ExceptionOccurred?.Invoke(this, exception);
    }

    private void OnMessageReceived(Message message)
    {
        MessageReceived?.Invoke(this, message);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isFactory"></param>
    public PipeConnection(bool isFactory)
    {
        IsFactory = isFactory;
    }

    #endregion

    #region Private methods

    private static IPipeClient<T> CreateClient<T>(string name)
    {
        return new SingleConnectionPipeClient<T>(name);
    }

    private static IPipeServer<T> CreateServer<T>(string name)
    {
        return new SingleConnectionPipeServer<T>(name);
    }

    #endregion


    #region Public methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InitializeAsync(string name, CancellationToken cancellationToken = default)
    {
        if (IsFactory)
        {
            var client = CreateClient<Message>(name);
            client.MessageReceived += (_, args) =>
            {
                if (args.Message == null)
                {
                    OnExceptionOccurred(new InvalidOperationException("Received null message from server."));
                    return;
                }

                OnMessageReceived(args.Message);
            };
            client.ExceptionOccurred += (_, args) => OnExceptionOccurred(args.Exception);

            await client.ConnectAsync(cancellationToken).ConfigureAwait(false);

            InternalConnection = client;
        }
        else
        {
            var server = CreateServer<Message>(name);
            server.MessageReceived += (_, args) =>
            {
                if (args.Message == null)
                {
                    OnExceptionOccurred(new InvalidOperationException("Received null message from client."));
                    return;
                }

                OnMessageReceived(args.Message);
            };
            server.ExceptionOccurred += (_, args) => OnExceptionOccurred(args.Exception);

            await server.StartAsync(cancellationToken).ConfigureAwait(false);

            InternalConnection = server;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        InternalConnection = InternalConnection ?? throw new InvalidOperationException("InternalConnection is null");

        await InternalConnection.WriteAsync(message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SendAsync<T>(string name, T value, CancellationToken cancellationToken = default)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var client = CreateClient<object?>(name);
#pragma warning restore CA2000 // Dispose objects before losing scope
        await using (client.ConfigureAwait(false))
        {
            client.ExceptionOccurred += (_, eventArgs) =>
            {
                OnExceptionOccurred(eventArgs.Exception);
            };

            await client.ConnectAsync(cancellationToken).ConfigureAwait(false);

            await client.WriteAsync(value, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T> ReceiveAsync<T>(string name, CancellationToken cancellationToken = default)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var server = CreateServer<T>(name);
#pragma warning restore CA2000 // Dispose objects before losing scope
        await using (server.ConfigureAwait(false))
        {

            server.ExceptionOccurred += (_, eventArgs) =>
            {
                OnExceptionOccurred(eventArgs.Exception);
            };

            var args = await server.WaitMessageAsync(
                async token => await server.StartAsync(token).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            return args.Message;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (InternalConnection != null)
        {
            await InternalConnection.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion
}
