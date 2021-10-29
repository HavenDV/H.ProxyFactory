using System.Reflection;
using H.Utilities.Args;
using H.Utilities.Extensions;
using H.Utilities.Messages;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoteProxyFactory : IAsyncDisposable
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public List<string> LoadedAssemblies { get; } = new ();

        private IConnection Connection { get; }
        private EmptyProxyFactory EmptyProxyFactory { get; } = new ();
        private Dictionary<object, Guid> GuidDictionary { get; } = new ();
        private Dictionary<Guid, object> ObjectsDictionary { get; } = new ();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<MethodEventArgs>? MethodCalled;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<MethodEventArgs>? MethodCompleted;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventEventArgs>? EventRaised;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventEventArgs>? EventCompleted;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<Exception>? ExceptionOccurred;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string>? MessageReceived;

        private void OnExceptionOccurred(Exception exception)
        {
            ExceptionOccurred?.Invoke(this, exception);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public RemoteProxyFactory(IConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Connection.MessageReceived += async (_, message) => await OnMessageReceived(message);
            Connection.ExceptionOccurred += (_, exception) => OnExceptionOccurred(exception);

            EmptyProxyFactory.AsyncMethodCalled += async (sender, args) =>
            {
                if (sender == null)
                {
                    return;
                }

                MethodCalled?.Invoke(sender, args);

                if (args.IsCanceled)
                {
                    return;
                }

                try
                {
                    args.ReturnObject = await RunMethodAsync(args.MethodInfo, sender, args.Arguments.ToArray(), args.CancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    args.Exception = exception;
                }

                MethodCompleted?.Invoke(sender, args);
            };
            EmptyProxyFactory.EventRaised += (_, args) => EventRaised?.Invoke(this, args);
            EmptyProxyFactory.EventCompleted += (_, args) => EventCompleted?.Invoke(this, args);
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
            await Connection.InitializeAsync(name, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public async Task<IList<string>> GetTypesAsync(CancellationToken cancellationToken = default)
        {
            await Connection.SendMessageAsync(new GetTypesMessage(), cancellationToken)
                .ConfigureAwait(false);

            var value = await Connection.ReceiveAsync<object?>("GetTypes", cancellationToken);
            
            return value switch
            {
                string[] typeNames => typeNames,
                Exception exception => throw exception,
                _ => throw new InvalidOperationException($"Invalid return type: {value?.GetType()}")
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public async Task LoadAssemblyAsync(string path, CancellationToken cancellationToken = default)
        {
            path = path ?? throw new ArgumentNullException(nameof(path));

            await Connection.SendMessageAsync(new LoadAssemblyMessage
            {
                Path = path,
            }, cancellationToken).ConfigureAwait(false);

            LoadedAssemblies.Add(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> CreateInstanceAsync<T>(string typeName, CancellationToken cancellationToken = default)
            where T : class
        {
            typeName = typeName ?? throw new ArgumentNullException(nameof(typeName));

            var instance = EmptyProxyFactory.CreateInstance<T>();
            var guid = Guid.NewGuid();
            ObjectsDictionary.Add(guid, instance);
            GuidDictionary.Add(instance, guid);

            await Connection.SendMessageAsync(new CreateObjectMessage
            {
                Guid = guid,
                TypeName = typeName,
            }, cancellationToken).ConfigureAwait(false);

            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBase"></typeparam>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TInterface> CreateInstanceAsync<TBase, TInterface>(CancellationToken cancellationToken = default)
            where TBase : class
            where TInterface : class
        {
            return await CreateInstanceAsync<TInterface>(typeof(TBase).FullName, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            await Connection.SendMessageAsync(new Message
            {
                Text = message,
            }, cancellationToken).ConfigureAwait(false);
        }

        private async Task<object?> RunMethodAsync(MethodInfo methodInfo, object instance, object?[] args, CancellationToken cancellationToken = default)
        {
            var token = args.FirstOrDefault(arg => arg is CancellationToken) as CancellationToken? 
                        ?? cancellationToken;
            var message = new RunMethodMessage
            {
                ObjectGuid = GuidDictionary[instance],
                MethodGuid = Guid.NewGuid(),
                MethodName = methodInfo.Name,
            };
            await Connection.SendMessageAsync(message, token).ConfigureAwait(false);

            using var registration = token.Register(async () =>
            {
                using var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                try
                {
                    await Connection.SendMessageAsync(new CancelMethodMessage
                    {
                        ObjectGuid = message.ObjectGuid,
                        MethodGuid = message.MethodGuid,
                        MethodName = message.MethodName,
                    }, source.Token);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    OnExceptionOccurred(e);
                }
            });

            await Task.WhenAll(args
                .Select(async (arg, i) =>
                {
                    if (arg?.GetType() == typeof(CancellationToken))
                    {
                        return;
                    }

                    await Connection.SendAsync($"{message.ConnectionPrefix}{i}", arg, token);
                }));

            var value = await Connection.ReceiveAsync<object?>($"{message.ConnectionPrefix}out", token);
            if (value is CreateObjectMessage createObjectMessage)
            {
                var guid = createObjectMessage.Guid ?? throw new InvalidOperationException("Guid is null");
                
                return GetReturnObject(methodInfo.ReturnType,
                    actualType =>
                    {
                        var obj = EmptyProxyFactory.CreateInstance(actualType);
                        
                        ObjectsDictionary.Add(guid, obj);
                        GuidDictionary.Add(obj, guid);

                        return obj;
                    });
            }
            if (value is Exception exception)
            {
                throw exception;
            }

            return GetReturnObject(methodInfo.ReturnType, _ => value);
        }

        private static object? GetReturnObject(Type type, Func<Type, object?> func)
        {
            if (type == typeof(Task))
            {
                return Task.CompletedTask;
            }
            if (type.BaseType == typeof(Task))
            {
                var taskType = type.GenericTypeArguments.FirstOrDefault()
                               ?? throw new InvalidOperationException("Task type is null");
                
                return typeof(Task).GetMethodInfo(nameof(Task.FromResult))
                    .MakeGenericMethod(taskType)
                    .Invoke(null, new[] { func(taskType) });
            }
            
            return func(type);
        }

        private async Task OnMessageReceived(Message message)
        {
            try
            {
                message = message ?? throw new ArgumentNullException(nameof(message));
                message.Text = message.Text ?? throw new ArgumentNullException($"{nameof(message.Text)}");

                MessageReceived?.Invoke(this, message.Text);

                switch (message)
                {
                    case ExceptionMessage o:
                        var exception = o.Exception ?? throw new ArgumentNullException(nameof(o.Exception));
                        OnExceptionOccurred(exception);
                        break;

                    case RaiseEventMessage o:
                        var objectGuid = o.ObjectGuid ?? throw new ArgumentNullException(nameof(o.ObjectGuid));
                        var eventName = o.EventName ?? throw new ArgumentNullException(nameof(o.EventName));
                        var connectionName = o.ConnectionName ?? throw new ArgumentNullException(nameof(o.ConnectionName));
                        await RaiseEventAsync(objectGuid, eventName, connectionName);
                        break;
                }
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private async Task RaiseEventAsync(Guid objectGuid, string eventName, string connectionName, CancellationToken cancellationToken = default)
        {
            var args = await Connection.ReceiveAsync<object?[]>(connectionName, cancellationToken);
            var instance = ObjectsDictionary[objectGuid];

            instance.RaiseEvent(eventName, args);
        }

        #endregion
    }
}
