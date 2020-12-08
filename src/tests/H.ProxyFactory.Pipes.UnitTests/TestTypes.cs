using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace H.ProxyFactory.Pipes.UnitTests
{
    #region Events

    public delegate void TextDelegate(string text);

    public interface IEventsClass
    {
        event EventHandler<int> Event1;
        event TextDelegate? Event3;

        void RaiseEvent1();
        void RaiseEvent3();
    }

    public class EventsClass : IEventsClass
    {
        public event EventHandler<int>? Event1;
        public event TextDelegate? Event3;

        public void RaiseEvent1()
        {
            Event1?.Invoke(this, 777);
        }

        public void RaiseEvent3()
        {
            Event3?.Invoke("555");
        }
    }

    #endregion

    #region Methods

    public interface IMethodsClass
    {
        int Echo(int input);
        string HelloName(string input);
        ICollection<int> IntegerCollection123();
        ICollection<string> StringCollection123();
    }

    public class MethodsClass : IMethodsClass
    {
        public int Echo(int input)
        {
            return input;
        }

        public string HelloName(string name)
        {
            return $"Hello {name}";
        }

        public ICollection<int> IntegerCollection123()
        {
            return new[] { 1, 2, 3 };
        }

        public ICollection<string> StringCollection123()
        {
            return new Dictionary<string, object?>
            {
                { "1", null },
                { "2", null },
                { "3", null },
            }.Keys;
        }
    }

    #endregion

    #region Async methods

    public interface IAsyncMethodsClass
    {
        Task NoValueTaskAsync(CancellationToken cancellationToken = default);
        Task<int> ValueTypeResultTaskWithResultEquals1Async(CancellationToken cancellationToken = default);
        Task InvalidOperationExceptionAfterSecondTaskAsync(CancellationToken cancellationToken = default);
    }

    public class AsyncMethodsClass : IAsyncMethodsClass
    {
        public async Task NoValueTaskAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        public async Task<int> ValueTypeResultTaskWithResultEquals1Async(CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            return 1;
        }

        public async Task InvalidOperationExceptionAfterSecondTaskAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            throw new InvalidOperationException("Custom exception message");
        }
    }

    #endregion
}
