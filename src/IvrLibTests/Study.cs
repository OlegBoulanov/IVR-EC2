
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

using IvrLib;

namespace IvrLibTests
{
    public class Study
    {
        [Test]
        public void Collect()
        {
            IEnumerable<string> list = new List<string>() { "one", };
            Assert.IsFalse(0 == list?.Count());
            Assert.IsTrue(0 < list?.Count());
            list = new List<string>();
            Assert.IsTrue(0 == list?.Count());
            Assert.IsFalse(0 < list?.Count());
            list = null;
            Assert.IsFalse(0 == list?.Count());
            Assert.IsFalse(0 < list?.Count());
            Assert.IsTrue(0 == (list?.Count() ?? 0));
        }
        [Test]
        public void InvalidFilePathChars()
        {
            var invalidPathChars = "|\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\t\n\v\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f";
            var invalidFileChars = "\"<>" + invalidPathChars + ":*?\\/";
            Assert.AreEqual(invalidPathChars, new string(Path.GetInvalidPathChars()));
            Assert.AreEqual(invalidFileChars, new string(Path.GetInvalidFileNameChars()));

            Assert.AreEqual("Host_10_0_0_4", $"Host_10.0.0.4".AsCloudFormationId());
            Assert.AreEqual("Host_10.0.0.4", $"Host_10.0.0.4".AsWindowsFolder());
            Assert.AreEqual("Host-10-0-0-4", $"Host_10.0.0.4".AsWindowsComputerName());
        }

    }
}