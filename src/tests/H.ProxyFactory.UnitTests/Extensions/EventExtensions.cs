using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace H.Utilities.Tests.Extensions
{
    /// <summary>
    /// Extensions that work with <see langword="event"/> <br/>
    /// <![CDATA[Version: 1.0.0.2]]> <br/>
    /// </summary>
    public static class EventExtensions
    {
        private class WaitObject<T>
        {
            public TaskCompletionSource<T>? Source { get; set; }

            // ReSharper disable once UnusedParameter.Local
            public void HandleEvent(object sender, T e)
            {
                Source?.TrySetResult(e);
            }
        }

        /// <summary>
        /// Asynchronously expects <see langword="event"/> until they occur or until canceled <br/>
        /// <![CDATA[Version: 1.0.0.2]]> <br/>
        /// <![CDATA[Dependency: WaitObject]]> <br/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="eventName"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T">EventArgs type</typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public static async Task<T> WaitEventAsync<T>(this object value, string eventName, CancellationToken cancellationToken = default)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            var eventInfo = value.GetType().GetEvent(eventName)
                            ?? throw new ArgumentException($"Event \"{eventName}\" is not found");

            var taskCompletionSource = new TaskCompletionSource<T>();
            using var registration = cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());

            var waitObject = new WaitObject<T>
            {
                Source = taskCompletionSource,
            };
            var method = waitObject.GetType().GetMethod(nameof(WaitObject<int>.HandleEvent))
                         ?? throw new InvalidOperationException("HandleEvent method is not found");
            // ReSharper disable once ConstantNullCoalescingCondition
            var eventHandlerType = eventInfo.EventHandlerType
                                   ?? throw new InvalidOperationException("Event Handler Type is null");
            var delegateObject = Delegate.CreateDelegate(eventHandlerType, waitObject, method, true);

            try
            {
                eventInfo.AddEventHandler(value, delegateObject);

                return await taskCompletionSource.Task.ConfigureAwait(false);
            }
            finally
            {
                eventInfo.RemoveEventHandler(value, delegateObject);
            }
        }

        /// <summary>
        /// Asynchronously expects <see langword="event"/> until they occur or until canceled <br/>
        /// <![CDATA[Version: 1.0.0.2]]> <br/>
        /// <![CDATA[Dependency: WaitEventAsync(this object value, string eventName, CancellationToken cancellationToken = default)]]> <br/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="func"></param>
        /// <param name="eventName"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T">EventArgs type</typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public static async Task<T> WaitEventAsync<T>(this object value, Func<CancellationToken, Task> func, string eventName, CancellationToken cancellationToken = default)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            func = func ?? throw new ArgumentNullException(nameof(func));
            eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));

            var task = value.WaitEventAsync<T>(eventName, cancellationToken);

            await func(cancellationToken).ConfigureAwait(false);

            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously expects <see langword="event"/> until they occur or until canceled <br/>
        /// <![CDATA[Version: 1.0.0.2]]> <br/>
        /// <![CDATA[Dependency: WaitObject]]> <br/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public static async Task<object?[]> WaitEventAsync(this object instance, string eventName, CancellationToken cancellationToken = default)
        {
            var eventInfo = instance.GetType().GetEvent(eventName)
                            ?? throw new InvalidOperationException("Event info is not found");
            // ReSharper disable once ConstantNullCoalescingCondition
            var handlerType = eventInfo.EventHandlerType
                              ?? throw new InvalidOperationException("Event Handler Type is not found");
            var methodInfo = handlerType.GetMethod("Invoke")
                             ?? throw new InvalidOperationException("Invoke method is not found");
            var parameterTypes = methodInfo
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray();

            var taskCompletionSource = new TaskCompletionSource<object?[]>();
            var parameters = parameterTypes.Select(Expression.Parameter).ToArray();
            var handlerExpression = Expression.Lambda(eventInfo.EventHandlerType,
                Expression.Call(
                    Expression.Constant(taskCompletionSource), 
                    typeof(TaskCompletionSource<object?[]>)
                        .GetMethod(
                            nameof(TaskCompletionSource<object?[]>.SetResult),
                            new []{typeof(object?[])}) ?? 
                    throw new InvalidOperationException("SetResult method is not found"),
                    Expression.NewArrayInit(typeof(object),
                        parameters.Select(
                            parameter => Expression.Convert(parameter, typeof(object))))),
                parameters);
            var @delegate = handlerExpression.Compile();

            using var registration = cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());

            try
            {
                eventInfo.AddEventHandler(instance, @delegate);

                return await taskCompletionSource.Task.ConfigureAwait(false);
            }
            finally
            {
                eventInfo.RemoveEventHandler(instance, @delegate);
            }
        }

        /// <summary>
        /// Asynchronously expects <see langword="event"/> until they occur or until canceled <br/>
        /// <![CDATA[Version: 1.0.0.2]]> <br/>
        /// <![CDATA[Dependency: WaitEventAsync(this object instance, string eventName, CancellationToken cancellationToken = default)]]> <br/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="func"></param>
        /// <param name="eventName"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public static async Task<object?[]> WaitEventAsync(this object value, Func<CancellationToken, Task> func, string eventName, CancellationToken cancellationToken = default)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            func = func ?? throw new ArgumentNullException(nameof(func));
            eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));

            var task = value.WaitEventAsync(eventName, cancellationToken);

            await func(cancellationToken).ConfigureAwait(false);

            return await task.ConfigureAwait(false);
        }
    }
}
