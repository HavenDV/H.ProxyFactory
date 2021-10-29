using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using H.Utilities.Extensions;
using H.Utilities.Messages;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoteProxyServer : IAsyncDisposable
    {
        #region Properties

        private IConnection Connection { get; }

        private List<Assembly> Assemblies { get; } = AppDomain.CurrentDomain.GetAssemblies().ToList();
        private List<Assembly> LoadedAssemblies { get; } = new ();
        private Dictionary<Guid, object> ObjectsDictionary { get; } = new ();

        #endregion

        #region Events

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

        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public RemoteProxyServer(IConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Connection.MessageReceived += async (_, message) =>
            {
                await OnMessageReceivedAsync(message);
            };
            Connection.ExceptionOccurred += (_, exception) =>
            {
                OnExceptionOccurred(exception);
            };
        }

        #endregion

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factoryException"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendExceptionAsync(Exception factoryException, CancellationToken cancellationToken = default)
        {
            try
            {
                await Connection.SendMessageAsync(new ExceptionMessage
                {
                    Exception = CreateSerializableException(factoryException),
                }, cancellationToken);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private async Task OnEventOccurredAsync(
            Guid objectGuid, string eventName, Guid eventGuid, object?[] args,
            CancellationToken cancellationToken = default)
        {
            var message = new RaiseEventMessage
            {
                ObjectGuid = objectGuid,
                EventName = eventName,
                EventGuid = eventGuid,
            };
            await Connection.SendMessageAsync(message, cancellationToken);

            await Connection.SendAsync(message.ConnectionName, args, cancellationToken);
        }

        private async Task OnMessageReceivedAsync(Message message)
        {
            try
            {
                message = message ?? throw new ArgumentNullException(nameof(message));
                message.Text = message.Text ?? throw new ArgumentNullException($"{nameof(message.Text)}");

                OnMessageReceived(message.Text);

                switch (message)
                {
                    case LoadAssemblyMessage o:
                        var path = o.Path ?? throw new ArgumentNullException(nameof(o.Path));
                        LoadAssembly(path);
                        break;

                    case GetTypesMessage _:
                        await GetTypesAsync();
                        break;

                    case CreateObjectMessage o:
                        var guid = o.Guid ?? throw new ArgumentNullException(nameof(o.Guid));
                        var typeName = o.TypeName ?? throw new ArgumentNullException(nameof(o.TypeName));
                        CreateObject(guid, typeName);
                        break;

                    case RunMethodMessage o:
                        var objectGuid = o.ObjectGuid ?? throw new ArgumentNullException(nameof(o.ObjectGuid));
                        var methodName = o.MethodName ?? throw new ArgumentNullException(nameof(o.MethodName));
                        var methodGuid = o.MethodGuid ?? throw new ArgumentNullException(nameof(o.MethodGuid));
                        var connectionPrefix = o.ConnectionPrefix ?? throw new ArgumentNullException(nameof(o.ConnectionPrefix));
                        await RunMethodAsync(objectGuid, methodName, methodGuid, connectionPrefix);
                        break;
                }
            }
            catch (Exception exception)
            {
                await SendExceptionAsync(exception);
            }
        }


        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void LoadAssembly(string path)
        {
            var assembly = Assembly.LoadFrom(path);

            Assemblies.Add(assembly);
            LoadedAssemblies.Add(assembly);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetTypesAsync(CancellationToken cancellationToken = default)
        {
            object? value;
            try
            {
                value = LoadedAssemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .Select(type => type.FullName ?? string.Empty)
                    .ToArray();
            }
            catch (Exception exception)
            {
                value = CreateSerializableException(exception);
            }

            try
            {
                await Connection.SendAsync("GetTypes", value, cancellationToken);
            }
            catch (Exception exception)
            {
                value = CreateSerializableException(exception);

                await Connection.SendAsync("GetTypes", value, cancellationToken);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="typeName"></param>
        public void CreateObject(Guid guid, string typeName)
        {
            ////throw new Exception(string.Join(" ", assembly.GetTypes().Select(i => $"{i.FullName}")));

            var assembly = Assemblies.FirstOrDefault(i =>
                               i.GetTypes().Any(type => type.FullName == typeName))
                           ?? throw new InvalidOperationException($"Assembly with type \"{typeName}\" is not loaded");
            var instance = assembly.CreateInstance(typeName) ?? throw new InvalidOperationException("Instance is null");
            
            AddObject(guid, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="instance"></param>
        public void AddObject(Guid guid, object instance)
        {
            foreach (var eventInfo in instance.GetType().GetEvents())
            {
                instance.SubscribeToEvent(eventInfo.Name, async (name, args) =>
                {
                    try
                    {
                        if (args.ElementAtOrDefault(0) == instance)
                        {
                            args[0] = null;
                        }

                        await OnEventOccurredAsync(guid, name, Guid.NewGuid(), args);
                    }
                    catch (Exception exception)
                    {
                        await SendExceptionAsync(exception);
                    }
                });
            }

            ObjectsDictionary.Add(guid, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectGuid"></param>
        /// <param name="methodName"></param>
        /// <param name="methodGuid"></param>
        /// <param name="connectionPrefix"></param>
        /// <returns></returns>
        public async Task RunMethodAsync(Guid objectGuid, string methodName, Guid methodGuid, string connectionPrefix)
        {
            var instance = ObjectsDictionary[objectGuid];
            var methodInfo = instance.GetType().GetMethod(methodName)
                             ?? throw new InvalidOperationException($"Method is not found: {methodName}");

            using var cancellationTokenSource = new CancellationTokenSource();
            var args = await Task.WhenAll(methodInfo.GetParameters()
                .Select(async (parameter, i) =>
                {
                    if (parameter.ParameterType == typeof(CancellationToken))
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        return cancellationTokenSource.Token;
                    }

                    // ReSharper disable once AccessToDisposedClosure
                    return await Connection.ReceiveAsync<object?>($"{connectionPrefix}{i}", cancellationTokenSource.Token);
                }));

            object? value;
            try
            {
                value = methodInfo.Invoke(instance, args.ToArray());
            }
            catch (Exception exception)
            {
                value = CreateSerializableException(exception);
            }

            if (value is ICollection)
            {
                var type = value.GetType();
                if (!type.IsArray)
                {
                    var elementType = type.GetInterfaces()
                        .FirstOrDefault(i => 
                            i.Name.StartsWith(nameof(ICollection)) && 
                            i.GenericTypeArguments.Any())?
                        .GenericTypeArguments
                        .FirstOrDefault();
                    value = typeof(Enumerable)
                        .GetMethod(nameof(Enumerable.ToArray), BindingFlags.Static | BindingFlags.Public)?
                        .MakeGenericMethod(elementType)
                        .Invoke(null, new []{ value });
                }
            }
            if (value is Task task)
            {
                try
                {
                    await task;

                    var type = value.GetType();
                    var taskTypeName = type.BaseType?.GenericTypeArguments?.FirstOrDefault()?.FullName 
                        ?? type.GenericTypeArguments?.FirstOrDefault()?.FullName;
                    if (taskTypeName == "System.Threading.Tasks.VoidTaskResult")
                    {
                        value = null;
                    }
                    else
                    {
                        value = value
                            .GetType()
                            .GetProperty(nameof(Task<int>.Result), BindingFlags.Public | BindingFlags.Instance)?
                            .GetValue(value);
                    }
                }
                catch (Exception exception)
                {
                    value = CreateSerializableException(exception);
                }
            }

            try
            {
                await Connection.SendAsync($"{connectionPrefix}out", value, cancellationTokenSource.Token);
            }
            catch (SerializationException) when (value != null)
            {
                var guid = Guid.NewGuid();
                AddObject(guid, value);
                
                await Connection.SendAsync($"{connectionPrefix}out", new CreateObjectMessage
                {
                    Guid = guid,
                }, cancellationTokenSource.Token);
            }
            catch (Exception exception)
            {
                value = CreateSerializableException(exception);

                await Connection.SendAsync($"{connectionPrefix}out", value, cancellationTokenSource.Token);
            }
        }

        private static Exception CreateSerializableException(Exception exception)
        {
            if (exception.GetType().IsSerializable)
            {
                return exception;
            }

            return new Exception($"{exception}");
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            foreach (var pair in ObjectsDictionary)
            {
                var instance = pair.Value;
                if (instance == null)
                {
                    continue;
                }

                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                if (instance is IAsyncDisposable asyncDisposable)
                {
                    asyncDisposable.DisposeAsync().AsTask().Wait();
                }
            }

            ObjectsDictionary.Clear();

            await Connection.DisposeAsync();
        }

        #endregion
    }
}
