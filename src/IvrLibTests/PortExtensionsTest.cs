using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

using Amazon.CDK.AWS.EC2;

using IvrLib;

namespace IvrLibTests
{
    public class PortExtensionsTest
    {
        [Test]
        public void TestClone()
        {
            var port = Port.UdpRange(2000, 3000);
            Assert.AreEqual("UDP 2000-3000", port.ToString());
            Assert.AreEqual("UDP 2000-3000", port.Clone().ToString());
            Assert.AreEqual("UDP 2500", port.Clone(2500).ToString());
            Assert.AreEqual("UDP 2500-2700", port.Clone(2500, 2700).ToString());
        }
    }
}
