using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace IvrLib.Utils
{
    public static class JsonElementExtensions
    {
        public static JsonElement? Select(this JsonElement element, params string[] path)
        {
            foreach(var p in path)
            {
                element = element.EnumerateObject().First(jp => jp.NameEquals(p)).Value;
            }
            return element;
        }
    }
}
