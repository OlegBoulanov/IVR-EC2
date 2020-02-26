
using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

using IvrLib;

namespace IvrLibTests
{
    public class PortHelperTests
    {
        void ParseAndCheck(string s) { Assert.AreEqual(s, PortHelpers.Parse(s).ToString()); }
        [Test]
        public void ParseTest()
        {
            ParseAndCheck("ALL TRAFFIC");
            ParseAndCheck("ALL PORTS");
            ParseAndCheck("UDP ALL PORTS");
            ParseAndCheck("ICMP Type 8");
            Assert.Throws(typeof(NotImplementedException), () => ParseAndCheck("ICMP Type 9"));
            ParseAndCheck("2000-3000");
            ParseAndCheck("2000");
            ParseAndCheck("UDP 2000");
            ParseAndCheck("UDP 2000-3000");
            Assert.AreEqual("UDP 100", PortHelpers.Parse("UDP 300", 100).ToString());
            Assert.AreEqual("UDP 100-200", PortHelpers.Parse("UDP 300", 100, 200).ToString());
            Assert.AreEqual("UDP 100-200", PortHelpers.Parse("UDP 300-400", 100, 200).ToString());
            Assert.AreEqual("UDP 100", PortHelpers.Parse("UDP 300-400", 100, 100).ToString());
            Assert.AreEqual("UDP 100-400", PortHelpers.Parse("UDP 300-400", 100).ToString());
            Assert.AreEqual("UDP 300-500", PortHelpers.Parse("UDP 300-400", 0, 500).ToString());
            Assert.AreEqual("100", PortHelpers.Parse("300", 100).ToString());
        }
    }
}