
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using YamlDotNet;
using YamlDotNet.Serialization;

using NUnit.Framework;

using IvrLib;
using IvrLib.Utils;

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
        public void InvalidPathCharsStudy()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            { 
                var ipc = System.IO.Path.GetInvalidPathChars();
                Assert.AreEqual(33, ipc.Length);
                Assert.AreEqual('|', ipc[0]);
                Assert.AreEqual('\0', ipc[1]);
                var ifc = System.IO.Path.GetInvalidFileNameChars();
                Assert.AreEqual(44, ifc.Length);
                Assert.AreEqual('\"', ifc[0]);
                Assert.AreEqual('<', ifc[1]);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var ipc = System.IO.Path.GetInvalidPathChars();
                Assert.AreEqual(1, ipc.Length);
                Assert.AreEqual('\0', ipc[0]);
                var ifc = System.IO.Path.GetInvalidFileNameChars();
                Assert.AreEqual(2, ifc.Length);
                Assert.AreEqual('\0', ifc[0]);
                Assert.AreEqual('/', ifc[1]);
            }
        }
        [Test]
        public void InvalidFileNameAndPathChars()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var invalidPathChars = "|\u0000\u0001\u0002\u0003\u0004\u0005\u0006\a\b\t\n\v\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f";
                Assert.AreEqual(invalidPathChars, Path.GetInvalidPathChars());
                var invalidFileChars = "\"<>" + invalidPathChars + ":*?\\/";
                Assert.AreEqual(invalidFileChars, Path.GetInvalidFileNameChars());
            } 
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var invalidPathChars = "\0";
                Assert.AreEqual(invalidPathChars, Path.GetInvalidPathChars());
                var invalidFileChars = "\0/";
                Assert.AreEqual(invalidFileChars, Path.GetInvalidFileNameChars());
            }
        }        
        [Test]
        public void AsSpecificName()
        {
            Assert.AreEqual("Host_10_0_0_4", $"Host_10.0.0.4".AsCloudFormationId());
            Assert.AreEqual("Host_10.0.0.4", $"Host_10.0.0.4".AsWindowsFolder());
            Assert.AreEqual("Host-10-0-0-4", $"Host_10.0.0.4".AsWindowsComputerName());
        }
        [Test]
        public void YamlTest()
        {
            var ys = new SerializerBuilder().Build();
            var s = ys.Serialize(new { Field1 = "one", Field2 = "two",  });
            Console.WriteLine(s);
            Assert.AreEqual($"Field1: one{Environment.NewLine}Field2: two{Environment.NewLine}", s);
            var x = new { Name = "None", Count = 4, };
            s = ys.Serialize(new { Field1 = "one", Field2 = "two", Az1 = x, Az2 = x, });
            Console.WriteLine(s);
            Assert.AreEqual($"Field1: one{Environment.NewLine}Field2: two{Environment.NewLine}Az1: &o0{Environment.NewLine}  Name: None{Environment.NewLine}  Count: 4{Environment.NewLine}Az2: *o0{Environment.NewLine}", s);

            var site = new IvrSiteSchema {
                Domain = "my.site.domain.net",
                InstallFrom = "https://raw.githubusercontent.com/OlegBoulanov/s3i/",
                MaxAzs = 2,
                SipProviders = new List<string> { "Twilio", },
                IngressPorts = new List<PortSpec> { PortSpec.Parse("SIP 5060"), PortSpec.Parse("RTP 5064-6000"), },
                RdpProps = new RdpProps {
                    UserName = "RdpUser",
                    Password = "P4$$word!",
                    Cidrs = new List<string> { "1.2.3.4/32", }
                },
                HostGroups = new List<HostGroup> {
                    new HostGroup {
                        GroupName = "Mixed/Public",
                        UseElasticIP = true,
                        Subdomains = new List<string> { "sip", "workers", },
                        HostCount = 2,
                        InstallFrom = "develop/Examples/Config.ini",
                    },
                    new HostGroup {
                        GroupName = "Workers/Private",
                        UseElasticIP = false,
                        Subdomains = new List<string> { "workers", },
                        HostCount = 16,
                        InstallFrom = "develop/Examples/Config.ini",
                    },
                },
            };
            s = ys.Serialize(site);
            Console.WriteLine(s);
            var yd = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            var site2 = yd.Deserialize<IvrSiteSchema>(s);
            Assert.AreEqual(2, site2.HostGroups.Count());

            Console.WriteLine("******");
            using(var sr = new StreamReader($"{OSAgnostic.Home}/Projects/CdkTest-1.yaml")) {
                var site1 = yd.Deserialize<IvrSiteSchema>(sr.ReadToEnd());
                Console.WriteLine(ys.Serialize(site1));
            }
        }
    }
}