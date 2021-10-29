using H.Utilities.Args;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class DirectProxyFactory
    {
        #region Properties

        private EmptyProxyFactory EmptyProxyFactory { get; } = new EmptyProxyFactory();
        private Dictionary<object, object> Dictionary { get; } = new Dictionary<object, object>();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public virtual event EventHandler<MethodEventArgs>? MethodCalled;

        /// <summary>
        /// 
        /// </summary>
        public virtual event EventHandler<MethodEventArgs>? MethodCompleted;

        /// <summary>
        /// 
        /// </summary>
        public virtual event EventHandler<EventEventArgs>? EventRaised;

        /// <summary>
        /// 
        /// </summary>
        public virtual event EventHandler<EventEventArgs>? EventCompleted;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public DirectProxyFactory()
        {
            EmptyProxyFactory.MethodCalled += (sender, args) =>
            {
                if (sender == null ||
                    !Dictionary.TryGetValue(sender, out var obj))
                {
                    return;
                }

                MethodCalled?.Invoke(sender, args);

                if (args.IsCanceled)
                {
                    return;
                }

                var method = obj.GetType().GetMethod(args.MethodInfo.Name, args.MethodInfo
                    .GetParameters()
                    .Select(i => i.ParameterType)
                    .ToArray())
                    ?? throw new InvalidOperationException($"Method not found: {args.MethodInfo}");

                args.ReturnObject = method.Invoke(obj, args.Arguments.ToArray());

                MethodCompleted?.Invoke(sender, args);
            };
            EmptyProxyFactory.EventRaised += (sender, args) => EventRaised?.Invoke(this, args);
            EmptyProxyFactory.EventCompleted += (sender, args) => EventCompleted?.Invoke(this, args);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="internalInstance"></param>
        /// <returns></returns>
        public T CreateInstance<T>(T internalInstance) where T : class
        {
            var instance = EmptyProxyFactory.CreateInstance<T>();

            Dictionary.Add(instance, internalInstance);

            foreach (var eventInfo in internalInstance.GetType().GetEvents())
            {
                // ReSharper disable once ConstantNullCoalescingCondition
                var eventHandlerType = eventInfo.EventHandlerType
                                       ?? throw new InvalidOperationException("Event Handler Type not found");
                var method = instance.GetType().GetMethod($"On{eventInfo.Name}")
                             ?? throw new ArgumentException($"On{eventInfo.Name} method is not found");
                var @delegate = Delegate.CreateDelegate(eventHandlerType, instance, method, true);

                eventInfo.AddEventHandler(internalInstance, @delegate);
            }

            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public void DeleteInstance(object instance)
        {
            if (Dictionary.ContainsKey(instance))
            {
                Dictionary.Remove(instance);
            }
        }

        #endregion
    }
}
