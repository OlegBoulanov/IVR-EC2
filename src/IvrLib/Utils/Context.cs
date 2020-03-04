using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace IvrLib.Utils
{
    public class Context : Dictionary<string, string>
    {
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
        public static Context FromJsonFiles(params string [] paths)
        {
            return  paths.Aggregate(new Context(), (c, p) =>
            {
                if(!string.IsNullOrWhiteSpace(p))
                {
                    if(!Path.HasExtension(p)) p = Path.ChangeExtension(p, ".json");
                    if(File.Exists(p)) c.WithJsonFile(p);
                }
                return c;
            });
        }
    }
}