using System;
using System.Threading;
using System.Threading.Tasks;
using H.Pipes;
using H.Pipes.Extensions;
using H.Utilities.Messages;

namespace H.Utilities
{
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
                var client = new SingleConnectionPipeClient<Message>(name);
                client.MessageReceived += (sender, args) => OnMessageReceived(args.Message);
                client.ExceptionOccurred += (sender, args) => OnExceptionOccurred(args.Exception);

                await client.ConnectAsync(cancellationToken).ConfigureAwait(false);

                InternalConnection = client;
            }
            else
            {
                var server = new SingleConnectionPipeServer<Message>(name);
                server.MessageReceived += (sender, args) => OnMessageReceived(args.Message);
                server.ExceptionOccurred += (sender, args) => OnExceptionOccurred(args.Exception);

                await server.StartAsync(cancellationToken: cancellationToken);

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
            await using var client = new SingleConnectionPipeClient<object?>(name);
            client.ExceptionOccurred += (_, eventArgs) =>
            {
                OnExceptionOccurred(eventArgs.Exception);
            };

            await client.ConnectAsync(cancellationToken);

            await client.WriteAsync(value, cancellationToken);
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
            await using var server = new SingleConnectionPipeServer<T>(name);
            server.ExceptionOccurred += (_, eventArgs) =>
            {
                OnExceptionOccurred(eventArgs.Exception);
            };

            var args = await server.WaitMessageAsync(
                async token => await server.StartAsync(cancellationToken: token),
                cancellationToken);

            return args.Message;
        }

        /// <summary>
        /// 
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (InternalConnection != null)
            {
                await InternalConnection.DisposeAsync();
            }
        }

        #endregion
    }
}
