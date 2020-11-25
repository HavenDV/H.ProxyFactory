using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Utilities.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Utilities.Tests
{
    [TestClass]
    public class PipeProxyFactoryTests
    {
        [TestMethod]
        public async Task MethodsTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            await BaseTests.BaseInstanceTestAsync<IMethodsClass>(
                GetFullName(typeof(MethodsClass)),
                (instance, cancellationToken) =>
                {
                    Assert.AreEqual(123, instance.Echo(123));
                    Assert.AreEqual("Hello 123", instance.HelloName("123"));
                    CollectionAssert.AreEqual(new [] { 1, 2, 3 }, instance.IntegerCollection123().ToArray());
                    CollectionAssert.AreEqual(new[] { "1", "2", "3" }, instance.StringCollection123().ToArray());

                    return Task.CompletedTask;
                },
                cancellationTokenSource.Token);
        }

        [TestMethod]
        public async Task EventsTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            await BaseTests.BaseInstanceTestAsync<IEventsClass>(
                GetFullName(typeof(EventsClass)),
                async (instance, cancellationToken) =>
                {
                    instance.Event1 += (sender, value) =>
                    {
                        Console.WriteLine($"Hello, I'm the Event1. My value is {value}");
                    };
                    instance.Event3 += (value) =>
                    {
                        Console.WriteLine($"Hello, I'm the Event3. My value is {value}");
                    };

                    var event1Value = await instance.WaitEventAsync<int>(token =>
                    {
                        instance.RaiseEvent1();

                        return Task.CompletedTask;
                    }, nameof(instance.Event1), cancellationToken);
                    Assert.AreEqual(777, event1Value);

                    var event2Values = await instance.WaitEventAsync(token =>
                    {
                        instance.RaiseEvent3();

                        return Task.CompletedTask;
                    }, nameof(instance.Event3), cancellationToken);
                    Assert.IsNotNull(event2Values);
                    Assert.AreEqual(1, event2Values.Length);
                    Assert.AreEqual("555", event2Values[0]);
                },
                cancellationTokenSource.Token);
        }

        [TestMethod]
        public async Task AsyncMethodsTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            await BaseTests.BaseInstanceTestAsync<IAsyncMethodsClass>(
                GetFullName(typeof(AsyncMethodsClass)),
                async (instance, cancellationToken) =>
                {
                    await instance.NoValueTaskAsync(cancellationToken);

                    Assert.AreEqual(1, await instance.ValueTypeResultTaskWithResultEquals1Async(cancellationToken));

                    await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                        await instance.InvalidOperationExceptionAfterSecondTaskAsync(cancellationToken));
                },
                cancellationTokenSource.Token);
        }

        private static string GetFullName(Type type)
        {
            return type.FullName ??
                   throw new InvalidOperationException("Type full name is null");
        }
    }
}
