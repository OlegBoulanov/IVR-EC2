using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

using Amazon.CDK;

namespace IvrLib.Utils
{
    public class Context : Dictionary<string, string>
    {
        public ConstructNode Node { get; protected set; }
        public Context(ConstructNode node)
        {
            Node = node;
        }
        public Context WithJson(string json, string selector = null)
        {
            var document = JsonDocument.Parse(Encoding.UTF8.GetBytes(json));
            foreach (var ec in document.RootElement.EnumerateObject())
            {
                if (ec.NameEquals(selector ?? "context"))
                {
                    foreach (var e in ec.Value.EnumerateObject())
                    {
                        this[e.Name] = e.Value.ToString();
                    }
                }
            }
            return this;
        }
        public Context WithJsonFile(string path, string selector = null)
        {
            return WithJson(new StreamReader(path).ReadToEnd(), selector);
        }
        public static Context FromJsonFiles(ConstructNode node, params string [] paths)
        {
            return  paths.Aggregate(new Context(node), (c, p) =>
            {
                if(!string.IsNullOrWhiteSpace(p))
                {
                    if(!Path.HasExtension(p)) p = Path.ChangeExtension(p, ".json");
                    if(File.Exists(p)) c.WithJsonFile(p);
                }
                return c;
            });
        }
        public string Resolve(string name, string envar = null, string help = null)
        {
            var v = Node.TryGetContext(name) as string                          // 1. CDK implemented context: command line args -c "name=value", or ~/cdk.json and/or ./cdk.json
                ?? (this.TryGetValue(name, out var vv) ? vv : null)             // 2. provided context object
                ?? System.Environment.GetEnvironmentVariable(envar ?? name);    // 3. System environment variable
            if (null == v && null != help) throw new ArgumentException($"Context variable '{name}' is not defined{(string.IsNullOrWhiteSpace(help) ? "" : $", {help}")}");
            return v;
        }
    }
}
