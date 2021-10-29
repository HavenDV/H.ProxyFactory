using System.Linq.Expressions;

namespace H.Utilities.Extensions
{
    /// <summary>
    /// Extensions that work with <see langword="event"/> <br/>
    /// <![CDATA[Version: 1.0.0.0]]> <br/>
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Subscribes to an event by name and calls the delegate after the event occurs
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public static void SubscribeToEvent(this object instance, string eventName, Action<string, object?[]> action)
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

            var parameters = parameterTypes.Select(Expression.Parameter).ToArray();
            var handlerExpression = Expression.Lambda(eventInfo.EventHandlerType,
                Expression.Call(
                    Expression.Constant(action.Target),
                    action.Method,
                    Expression.Constant(eventInfo.Name),
                    Expression.NewArrayInit(typeof(object),
                        parameters.Select(
                            parameter => Expression.Convert(parameter, typeof(object))))),
                parameters);

            eventInfo.AddEventHandler(instance, handlerExpression.Compile());
        }
    }
}
