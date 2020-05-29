
using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

using IvrLib.Utils;

namespace IvrLibTests
{
    public class PortSpecTests
    {
        void ParseAndCheck(string s) { Assert.AreEqual(s, PortSpec.Parse(s).ToString()); }
        [Test]
        public void Parse()
        {
            ParseAndCheck("SIP 5060");
            ParseAndCheck("SIPS 5060-5061");
            ParseAndCheck("RTP 5062-5600");
            //PortRangeSpec.Parse("XYZ:");
            Assert.Throws(typeof(FormatException), () => { PortSpec.Parse("XYZ"); });
            Assert.Throws(typeof(FormatException), () => { PortSpec.Parse("X Y Z"); });
            Assert.Throws(typeof(FormatException), () => { PortSpec.Parse("X Y-Z-W"); });
            Assert.Throws(typeof(FormatException), () => { PortSpec.Parse("XYZ "); });
        }
    }
}