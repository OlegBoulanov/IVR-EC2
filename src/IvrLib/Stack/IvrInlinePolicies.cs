
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.IAM;

using IvrLib.Security;
using IvrLib.Utils;

namespace IvrLib
{
    public class IvrInlinePolicies : Dictionary<string, PolicyDocument>
    {
        public IvrInlinePolicies(string account, string stackId, IvrSiteSchema schema)
        {
            var statements = new List<PolicyStatement> {
                    // Role is needed for allowing tools to use EC2 provided credentials
                    // see https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-role.html
                    new PolicyStatement().Allow().WithActions("sts:AssumeRole")
                        .WithResources($"arn:aws:iam::{account}:role/{stackId}*"),    // allow roles defined in this stack
                    new PolicyStatement().Allow().WithActions("ec2:StartInstances", "ec2:StopInstances", "ec2:DescribeInstances")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("s3:GetBucketLocation")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("sqs:DeleteMessage", "sqs:GetQueueAttributes", "sqs:GetQueueUrl", "sqs:ReceiveMessage", "sqs:SendMessage")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("cloudwatch:GetMetricData", "cloudwatch:GetMetricStatistics", "cloudwatch:ListMetrics", "cloudwatch:PutMetricData")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("sns:Publish")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("ses:SendEmail")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("events:PutEvents")
                        .WithResources(),
            };
            //
            AllowCsv(statements, schema, schema.S3Buckets.ListBucket, "s3:ListBucket");
            AllowCsv(statements, schema, schema.S3Buckets.GetObject, "s3:GetObject");
            AllowCsv(statements, schema, schema.S3Buckets.PutObject, "s3:PutObject");
            AllowCsv(statements, schema, schema.S3Buckets.DeleteObject, "s3:DeleteObject");
            //
            Add("IvrPolicy", new PolicyDocument(new PolicyDocumentProps { Statements = statements.ToArray(), }));
        }
        void AllowCsv(IList<PolicyStatement> statements, IvrSiteSchema schema, IEnumerable<string> objects, params string [] actions)
        {
            if (null != objects && 0 < objects.Count())
            {
                statements.Add(new PolicyStatement().Allow().WithActions(actions).WithResources(schema.S3BucketResources(objects.SelectMany(x => x.Csv()).ToArray())));
            }
        }
    }
}