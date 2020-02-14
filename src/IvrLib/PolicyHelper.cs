using System;
using System.Collections.Generic;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public class PolicyHelper
    {
        public static IDictionary<string, PolicyDocument> Policy(string name, params PolicyStatement[] statements)
        {
            //return new Dictionary<string, PolicyDocument>(Statements(name, statements));
        }
        public static KeyValuePair<string, PolicyDocument> Statements(string name, params PolicyStatement[] statements)
        {
            return new KeyValuePair<string, PolicyDocument>(name, new PolicyDocument(new PolicyDocumentProps { Statements = statements }));
        }
    }
}