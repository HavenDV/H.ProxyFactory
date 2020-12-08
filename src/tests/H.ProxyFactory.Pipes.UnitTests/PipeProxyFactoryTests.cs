using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.ProxyFactory.Pipes.UnitTests.Extensions;
using H.ProxyFactory.TestTypes;
using H.Recorders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.ProxyFactory.Pipes.UnitTests
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
                async (instance, cancellationToken) =>
                {
                    Assert.AreEqual(123, instance.Echo(123));
                    Assert.AreEqual("Hello 123", instance.HelloName("123"));
                    CollectionAssert.AreEqual(new [] { 1, 2, 3 }, instance.IntegerCollection123().ToArray());
                    CollectionAssert.AreEqual(new[] { "1", "2", "3" }, instance.StringCollection123().ToArray());

                    var eventClass = instance.GetEventClass();
                    Assert.IsNotNull(eventClass);

                    eventClass.Event1 += (_, value) =>
                    {
                        Console.WriteLine($"Hello, I'm the Event1. My value is {value}");
                    };
                    
                    var event1Value = await eventClass.WaitEventAsync<int>(_ =>
                    {
                        eventClass.RaiseEvent1();

                        return Task.CompletedTask;
                    }, nameof(eventClass.Event1), cancellationToken);
                    Assert.AreEqual(777, event1Value);
                },
                cancellationTokenSource.Token);
        }
        
        [TestMethod]
        public async Task RecorderTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            await BaseTests.BaseInstanceTestAsync<IRecorder>(
                GetFullName(typeof(NAudioRecorder)),
                async (instance, cancellationToken) =>
                {
                    instance.RawDataReceived += (_, args) =>
                    {
                        Console.WriteLine(
                            $"{nameof(instance.RawDataReceived)}: {args.RawData?.Count ?? 0}, {args.WavData?.Count ?? 0}");
                    };
                    await instance.InitializeAsync(cancellationToken);

                    if (!NAudioRecorder.GetAvailableDevices().Any())
                    {
                        return;
                    }
                    
                    await instance.StartAsync(cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                    await instance.StopAsync(cancellationToken);
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
                    instance.Event1 += (_, value) =>
                    {
                        Console.WriteLine($"Hello, I'm the Event1. My value is {value}");
                    };
                    instance.Event3 += (value) =>
                    {
                        Console.WriteLine($"Hello, I'm the Event3. My value is {value}");
                    };

                    var event1Value = await instance.WaitEventAsync<int>(_ =>
                    {
                        instance.RaiseEvent1();

                        return Task.CompletedTask;
                    }, nameof(instance.Event1), cancellationToken);
                    Assert.AreEqual(777, event1Value);

                    var event2Values = await instance.WaitEventAsync(_ =>
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
