using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.ProxyFactory.Pipes.UnitTests;

public static class BaseTests
{
    public static async Task BaseInstanceTestAsync<T>(string typeName, Func<T, CancellationToken, Task> func, CancellationToken cancellationToken)
        where T : class
    {
        var receivedException = (Exception?)null;
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await using var factory = new PipeProxyFactory();
        factory.MethodCalled += (_, args) => args.CancellationToken = cancellationToken;
        factory.ExceptionOccurred += (_, exception) =>
        {
            Console.WriteLine($"factory.ExceptionOccurred: {exception}");
            receivedException = exception;

                // ReSharper disable once AccessToDisposedClosure
                try
            {
                cancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        };
        await using var server = new PipeProxyServer();
        server.ExceptionOccurred += (_, exception) =>
        {
            Console.WriteLine($"target.ExceptionOccurred: {exception}");
            receivedException = exception;

                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
        };

        var random = new Random();
        var name = nameof(PipeProxyFactoryTests) + random.Next();
        await server.InitializeAsync(name, cancellationTokenSource.Token);
        await factory.InitializeAsync(name, cancellationTokenSource.Token);

        var instance = await factory.CreateInstanceAsync<T>(typeName, cancellationTokenSource.Token);

        await func(instance, cancellationToken);

        if (receivedException != null)
        {
            Assert.Fail(receivedException.ToString());
        }
    }
}
