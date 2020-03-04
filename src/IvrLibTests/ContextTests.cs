
using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

using IvrLib.Utils;

namespace IvrLibTests
{
    public class ContextTests
    {
        const string testContext1 = "{ \"context\": { \"one\": \"ctx1-1\", \"two\": \"ctx1-2\" }}";
        const string testContext2 = "{ \"context\": { \"one\": \"ctx2-1\", \"two\": \"\", \"three\": \"ctx2-3\" }}";
        [Test]
        public void Collect()
        {
            var ctxs = new Context().WithJson(testContext1);

            Assert.AreEqual("ctx1-1", ctxs["one"]);
            Assert.AreEqual("ctx1-2", ctxs["two"]);
            Assert.IsFalse(ctxs.ContainsKey("three"));

            ctxs = new Context().WithJson(testContext2);
            Assert.AreEqual("ctx2-1", ctxs["one"]);
            Assert.AreEqual(string.Empty, ctxs["two"]);
            Assert.AreEqual("ctx2-3", ctxs["three"]);

            ctxs = new Context().WithJson(testContext1).WithJson(testContext2);
            Assert.AreEqual("ctx2-1", ctxs["one"]);
            Assert.AreEqual(string.Empty, ctxs["two"]);
            Assert.AreEqual("ctx2-3", ctxs["three"]);
        }
        [Test]
        public void TestStrings()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.AreEqual(Environment.GetEnvironmentVariable("USERPROFILE"), OSAgnostic.Home);
                Assert.AreEqual(Environment.GetEnvironmentVariable("USERNAME"), OSAgnostic.User);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Assert.AreEqual("/home/olegb", OSAgnostic.Home);
                Assert.AreEqual("olegb", OSAgnostic.User);
            }
        }
    }
}