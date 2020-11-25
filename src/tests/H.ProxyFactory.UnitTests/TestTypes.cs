using System;
using System.Threading;
using System.Threading.Tasks;

namespace H.Utilities.Tests
{

    public delegate void TextDelegate(string text);

    public interface IBaseInterface
    {
        int Property1 { get; set; }

        event EventHandler Event1;

        int Test1(string test);
        Task<int> Test4Async(CancellationToken cancellationToken = default);
        void RaiseEvent1();
    }

    public interface IInterface : IBaseInterface
    {
        int Property2 { get; }

        event EventHandler<int> Event2;
        event TextDelegate Event3;

        void Test2();
        Task Test3Async(CancellationToken cancellationToken = default);
        void RaiseEvent2();
        void RaiseEvent3();
        string[] Test5();
    }

    public abstract class AbstractClass
    {
        public abstract int Property1 { get; set; }
        public abstract int Property2 { get; }

        public abstract int Test1(string test);
        public abstract void Test2();
        public abstract Task Test3Async(CancellationToken cancellationToken = default);
        public abstract Task<int> Test4Async(CancellationToken cancellationToken = default);
        public abstract void RaiseEvent1();
        public abstract string[] Test5();
    }

    public class CommonClass : IInterface
    {
        public int Property1 { get; set; } = 1;
        public int Property2 { get; } = 2;

        public event EventHandler? Event1;
        public event EventHandler<int>? Event2;
        public event TextDelegate? Event3;

        public int Test1(string test)
        {
            return 1;
        }

        public void Test2()
        {
            Console.WriteLine("Test2 is completed");
        }

        public async Task Test3Async(CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            Console.WriteLine("Test3Async is completed");
        }

        public async Task<int> Test4Async(CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);

            return 4;
        }

        public void RaiseEvent1()
        {
            Event1?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseEvent2()
        {
            Event2?.Invoke(this, 777);
        }

        public void RaiseEvent3()
        {
            Event3?.Invoke("555");
        }

        public string[] Test5()
        {
            return new[] { "1", "2" };
        }
    }

    public interface ISimpleEventClass
    {
        event EventHandler<int> Event1;
        event TextDelegate? Event3;

        void RaiseEvent1();
        void RaiseEvent3();
        int Method1(int input);
        string Method2(string input);
    }

    public class SimpleEventClass : ISimpleEventClass
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

        public int Method1(int input)
        {
            return 321 + input;
        }

        public string Method2(string input)
        {
            return $"Hello, input = {input}";
        }
    }
}
