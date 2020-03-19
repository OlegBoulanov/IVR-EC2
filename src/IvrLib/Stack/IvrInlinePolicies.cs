
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.IAM;

using IvrLib.Security;

namespace IvrLib
{
    public class IvrInlinePolicies : Dictionary<string, PolicyDocument>
    {
        public IvrInlinePolicies(string account, string stackId, IvrSiteSchema schema)
        {
            Add("IvrPolicy", new PolicyDocument(new PolicyDocumentProps {
                Statements = new PolicyStatement[] {
                    // Role is needed for allowing tools to use EC2 provided credentials
                    // see https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-role.html
                    new PolicyStatement().Allow().WithActions("sts:AssumeRole")
                        .WithResources($"arn:aws:iam::{account}:role/{stackId}*"),    // allow roles defined in this stack
                    new PolicyStatement().Allow().WithActions("ec2:StartInstances", "ec2:StopInstances", "ec2:DescribeInstances")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("s3:GetBucketLocation")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("s3:ListBucket")
                        .WithResources(schema.S3BucketResources(schema.S3Buckets.ListBucket.ToArray())),
                    new PolicyStatement().Allow().WithActions("s3:GetObject")
                        .WithResources(schema.S3ObjectResources(schema.S3Buckets.GetObject.ToArray())),
                    new PolicyStatement().Allow().WithActions("s3:PutObject")
                        .WithResources(schema.S3ObjectResources(schema.S3Buckets.PutObject.ToArray())),
                    new PolicyStatement().Allow().WithActions("s3:DeleteObject")
                        .WithResources(schema.S3ObjectResources(schema.S3Buckets.DeleteObject.ToArray())),
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
                    },
                }));
        }
    }
}