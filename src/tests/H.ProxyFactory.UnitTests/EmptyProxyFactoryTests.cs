using H.ProxyFactory.Extensions;
using H.ProxyFactory.TestTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.ProxyFactory.UnitTests
{
    [TestClass]
    public class EmptyProxyFactoryTests
    {
        [TestMethod]
        public async Task AbstractTest()
        {
            var factory = CreateFactory();
            var instance = factory.CreateInstance<AbstractClass>();

            Assert.AreEqual(0, instance.Test1("hello"));
            instance.Test2();
            await instance.Test3Async();
            Assert.AreEqual(0, await instance.Test4Async());

            Assert.AreEqual(0, instance.Property1);
            instance.Property1 = 5;
            Assert.AreEqual(0, instance.Property1);
            Assert.AreEqual(0, instance.Property2);

            CollectionAssert.AreEqual(null, instance.Test5());
        }

        [TestMethod]
        public async Task InterfaceMethodsTest()
        {
            var factory = CreateFactory();
            factory.MethodCalled += (sender, args) =>
            {
                args.ReturnObject = args.MethodInfo.Name switch
                {
                    nameof(IInterface.Test1) => 3,
                    nameof(IInterface.Test4Async) => Task.FromResult(4),
                    nameof(IInterface.Test5) => Array.Empty<string>(),
                    _ => args.ReturnObject,
                };
            };
            var instance = factory.CreateInstance<IInterface>();

            Assert.AreEqual(3, instance.Test1("hello"));
            instance.Test2();
            await instance.Test3Async();
            Assert.AreEqual(4, await instance.Test4Async());
            CollectionAssert.AreEqual(Array.Empty<string>(), instance.Test5());
        }

        [TestMethod]
        public void InterfacePropertiesTest()
        {
            var factory = CreateFactory();
            factory.MethodCalled += (sender, args) =>
            {
                args.ReturnObject = args.MethodInfo.Name switch
                {
                    "get_" + nameof(IInterface.Property1) => 11,
                    _ => args.ReturnObject,
                };
            };
            var instance = factory.CreateInstance<IInterface>();

            Assert.AreEqual(11, instance.Property1);
            instance.Property1 = 5;
            Assert.AreEqual(11, instance.Property1);
            Assert.AreEqual(0, instance.Property2);
        }

        [TestMethod]
        public void InterfaceEventsTest()
        {
            var factory = CreateFactory();
            factory.EventRaised += (sender, args) =>
            {
                args.IsCanceled = args.EventInfo.Name switch
                {
                    nameof(IInterface.Event1) => true,
                    _ => false,
                };
            };
            var instance = factory.CreateInstance<IInterface>();

            instance.Event1 += (sender, args) => Console.WriteLine($"Event1: {args}"); // will be canceled
            instance.Event2 += (sender, value) => Console.WriteLine($"Event2: {value}");
            instance.Event3 += value => Console.WriteLine($"Event3: {value}");
            instance.RaiseEvent(nameof(IInterface.Event1), new object?[] { instance, EventArgs.Empty });
            instance.RaiseEvent(nameof(IInterface.Event3), new object?[] { "hello" });

            // it's empty methods
            instance.RaiseEvent1();
            instance.RaiseEvent2();
            instance.RaiseEvent3();
        }

        [TestMethod]
        public async Task CommonClassTest()
        {
            var factory = CreateFactory();
            factory.MethodCalled += (sender, args) =>
            {
                args.ReturnObject = args.MethodInfo.Name switch
                {
                    nameof(IInterface.Test1) => 3,
                    nameof(IInterface.Test4Async) => Task.FromResult(44),
                    _ => args.ReturnObject,
                };
            };
            var instance = factory.CreateInstance<CommonClass>();

            Assert.AreEqual(1, instance.Test1("hello"));
            instance.Test2();
            await instance.Test3Async();
            Assert.AreEqual(4, await instance.Test4Async());

            Assert.AreEqual(0, instance.Property1);
            instance.Property1 = 5;
            Assert.AreEqual(5, instance.Property1);
            Assert.AreEqual(0, instance.Property2);

            CollectionAssert.AreEqual(new[] { "1", "2" }, instance.Test5());
        }

        private static EmptyProxyFactory CreateFactory()
        {
            var factory = new EmptyProxyFactory();
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
