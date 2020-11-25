using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.ProxyFactory.IntegrationTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            using var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await Task.Delay(TimeSpan.FromSeconds(5), source.Token);
        }
    }
}
