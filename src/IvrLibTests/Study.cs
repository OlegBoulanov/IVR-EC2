
using System;
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
    }
}