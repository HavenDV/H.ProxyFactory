using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Utilities.Tests
{
    public static class BaseTests
    {
        public static async Task BaseInstanceTestAsync<T>(string typeName, Func<T, CancellationToken, Task> func, CancellationToken cancellationToken)
            where T : class
        {
            var receivedException = (Exception?)null;
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await using var factory = new PipeProxyFactory();
            factory.MethodCalled += (sender, args) => args.CancellationToken = cancellationToken;
            factory.ExceptionOccurred += (sender, exception) =>
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
            server.ExceptionOccurred += (sender, exception) =>
            {
                Console.WriteLine($"target.ExceptionOccurred: {exception}");
                receivedException = exception;

                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
            };

            await server.InitializeAsync(nameof(PipeProxyFactoryTests), cancellationTokenSource.Token);
            await factory.InitializeAsync(nameof(PipeProxyFactoryTests), cancellationTokenSource.Token);

            var instance = await factory.CreateInstanceAsync<T>(typeName, cancellationTokenSource.Token);

            await func(instance, cancellationToken);

            if (receivedException != null)
            {
                Assert.Fail(receivedException.ToString());
            }
        }
    }
}
