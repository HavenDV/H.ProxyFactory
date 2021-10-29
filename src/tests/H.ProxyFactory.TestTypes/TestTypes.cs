namespace H.ProxyFactory.TestTypes;

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
    IEventsClass GetEventClass();
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

    public IEventsClass GetEventClass()
    {
        return new EventsClass();
    }
}

#endregion

#region Async methods

public interface IAsyncMethodsClass
{
    Task NoValueTaskAsync(CancellationToken cancellationToken = default);
    Task<int> ValueTypeResultTaskWithResultEquals1Async(CancellationToken cancellationToken = default);
    Task InvalidOperationExceptionAfterSecondTaskAsync(CancellationToken cancellationToken = default);
    Task<IEventsClass> GetEventClassAsync(CancellationToken cancellationToken = default);
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

    public Task<IEventsClass> GetEventClassAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEventsClass>(new EventsClass());
    }
}

#endregion

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
