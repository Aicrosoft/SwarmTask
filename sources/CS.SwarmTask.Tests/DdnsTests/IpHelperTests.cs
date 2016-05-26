using CS.SwarmTask.Jobs.Ddns;
using NUnit.Framework;

namespace CS.SwarmTask.Tests.DdnsTests
{
    [TestFixture]
    public class IpHelperTests
    {

        [Test]
        public void GetIpTest()
        {
            var helper = new IpHelper("");
            var ip = helper.GetIp();
            Assert.IsNotEmpty(ip);
            Assert.IsTrue(helper.IpChanged);
        }
    }
}