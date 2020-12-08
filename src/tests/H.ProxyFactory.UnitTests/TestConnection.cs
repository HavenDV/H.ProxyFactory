using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using H.Utilities.Messages;

namespace H.Utilities.Tests
{
    internal class TestConnection : IConnection
    {
        #region Properties

        public ConcurrentQueue<Message> IncomingMessagesQueue { get; }
        public ConcurrentQueue<Message> OutgoingMessagesQueue { get; }
        public ConcurrentDictionary<string, object?> Dictionary { get; }

        private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        #endregion

        #region Events

        public event EventHandler<Message>? MessageReceived;

#pragma warning disable 0067
        public event EventHandler<Exception>? ExceptionOccurred;
#pragma warning restore 0067

        private void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, message);
        }

        #endregion

        #region Constructors

        public TestConnection(
            ConcurrentQueue<Message> incomingMessagesQueue,
            ConcurrentQueue<Message> outgoingMessagesQueue,
            ConcurrentDictionary<string, object?> dictionary)
        {
            IncomingMessagesQueue = incomingMessagesQueue;
            OutgoingMessagesQueue = outgoingMessagesQueue;
            Dictionary = dictionary;
        }

        #endregion

        #region Public methods

        public Task InitializeAsync(string name, CancellationToken cancellationToken = default)
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (IncomingMessagesQueue.TryDequeue(out var message))
                        {
                            OnMessageReceived(message);
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }, CancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            OutgoingMessagesQueue.Enqueue(message);

            return Task.CompletedTask;
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
            while (!Dictionary.TryAdd(name, value))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
            }
        }

        public async Task<T> ReceiveAsync<T>(string name, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                if (Dictionary.TryGetValue(name, out var value))
                {
                    return value != null ? (T)value : default!;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
            }
        }

        public async ValueTask DisposeAsync()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
            
            await Task.CompletedTask;
        }

        #endregion
    }
}
