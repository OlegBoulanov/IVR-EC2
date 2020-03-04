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

namespace IvrLib.Utils
{
    public static class ConstructNodeExtensions
    {
        // Resolve context variable
        // May return null, or throw help message ArgumentException, if set
        public static string Resolve(this ConstructNode node, Context ctx, string name, string envar = null, string help = null)
        {
            var v = node.TryGetContext(name) as string                       // 1. CDK implemented context: command line args -c "name=value", or ~/cdk.json and/or ./cdk.json
                ?? (ctx.TryGetValue(name, out var vv) ? vv : null)           // 2. provided context object
                ?? System.Environment.GetEnvironmentVariable(envar ?? name); // 3. System environment variable
            if (null == v && null != help) throw new ArgumentException($"Context variable '{name}' is not defined{(string.IsNullOrWhiteSpace(help)?"":$", {help}")}");
            return v;
        }
    }
}