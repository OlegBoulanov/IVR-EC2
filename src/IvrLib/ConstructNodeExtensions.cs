using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS;
using Amazon.CDK.Assets;
using Amazon.CDK.CXAPI;
using Amazon.JSII.Runtime.Deputy;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace IvrLib
{
    public static class ConstructNodeExtensions
    {
        public static ConstructNode WithJsonFiles(this ConstructNode node, IEnumerable<string> paths)
        {
            // collect extra files first
            var context = paths.Aggregate(new Context(), (c, p) => c.WithJsonFile(p));
            // override from existing standard context
            foreach(var k in context.Keys)
            {
                var v = node.TryGetContext(k)?.ToString();
                if(!string.IsNullOrWhiteSpace(v)) context[k] = v;
            }
            // and push back to standard
            foreach(var kv in context) node.SetContext(kv.Key, kv.Value);
            return node;
        }
    }    
}