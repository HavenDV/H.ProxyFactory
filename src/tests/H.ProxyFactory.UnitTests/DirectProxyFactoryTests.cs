using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Utilities.Tests
{
    [TestClass]
    public class DirectProxyFactoryTests
    {
        [TestMethod]
        public async Task CommonClassWithInterfaceMethodsTest()
        {
            var factory = CreateFactory();
            factory.MethodCalled += (sender, args) =>
            {
                args.IsCanceled = args.MethodInfo.Name switch
                {
                    nameof(IInterface.Test2) => true,
                    nameof(IInterface.Test3Async) => true,
                    _ => false,
                };
            };
            var instance = factory.CreateInstance<IInterface>(new CommonClass());

            Assert.AreEqual(1, instance.Test1("hello"));
            instance.Test2();
            await instance.Test3Async();
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(
                async () => await instance.Test4Async(tokenSource.Token));
        }

        [TestMethod]
        public void CommonClassWithInterfacePropertiesTest()
        {
            var factory = CreateFactory();
            var instance = factory.CreateInstance<IInterface>(new CommonClass());

            Assert.AreEqual(1, instance.Property1);
            instance.Property1 = 5;
            Assert.AreEqual(5, instance.Property1);
            Assert.AreEqual(2, instance.Property2);
        }

        [TestMethod]
        public void CommonClassWithInterfaceEventsTest()
        {
            var factory = CreateFactory();
            var instance = factory.CreateInstance<IInterface>(new CommonClass());

            instance.Event1 += (sender, args) => Console.WriteLine($"Event1: {args}");
            instance.Event2 += (sender, value) => Console.WriteLine($"Event2: {value}");
            instance.Event3 += (value) => Console.WriteLine($"Event3: {value}");
            instance.RaiseEvent1();
            instance.RaiseEvent2();
            instance.RaiseEvent3();
        }

        [TestMethod]
        public void CommonClassWithInterfaceEventsCancelTest()
        {
            var factory = CreateFactory();
            var instance = factory.CreateInstance<IInterface>(new CommonClass());

            instance.Event1 += (sender, args) => Assert.Fail("Event should not happen");

            // Cancel test
            factory.EventRaised += (sender, args) =>
            {
                args.IsCanceled = args.EventInfo.Name switch
                {
                    nameof(IInterface.Event1) => true,
                    _ => false,
                };
            };
            instance.RaiseEvent1();
        }

        private static DirectProxyFactory CreateFactory()
        {
            var factory = new DirectProxyFactory();
            factory.MethodCalled += (sender, args) =>
            {
                Console.WriteLine($"MethodCalled: {args.MethodInfo}");

                if (args.Arguments.Any())
                {
                    Console.WriteLine("Arguments:");
                }
                for (var i = 0; i < args.Arguments.Count; i++)
                {
                    Console.WriteLine($"{i}: \"{args.Arguments[i]?.ToString() ?? "null"}\"");
                }
            };
            factory.MethodCompleted += (sender, args) =>
            {
                Console.WriteLine($"MethodCompleted: {args.MethodInfo}");
            };
            factory.EventRaised += (sender, args) =>
            {
                Console.WriteLine($"EventRaised: {args.EventInfo}");
            };
            factory.EventCompleted += (sender, args) =>
            {
                Console.WriteLine($"EventCompleted: {args.EventInfo}");
            };

            return factory;
        }
    }
}
