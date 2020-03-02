using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;

namespace IvrLib
{
    public class IvrInlinePolicies : Dictionary<string, PolicyDocument>
    {
        public IvrInlinePolicies(IvrStackProps props)
        {
            Add("IvrPolicy", new PolicyDocument(new PolicyDocumentProps {
                Statements = new PolicyStatement[] {
                    // Role is needed for allowing tools to use EC2 provided credentials
                    // see https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-role.html
                    new PolicyStatement().Allow().WithActions("sts:AssumeRole")
                        .WithResources($"arn:aws:iam::{props.Env.Account}:role/IvrStack*"),
                    new PolicyStatement().Allow().WithActions("s3:GetBucketLocation")
                        .WithResources(),
                    new PolicyStatement().Allow().WithActions("s3:ListBucket")
                        .WithResources(props.S3BucketResources("apps", "config", "install", "prompts", "prompts.update", "tools", "userjobs")),
                    new PolicyStatement().Allow().WithActions("s3:GetObject")
                        .WithResources(props.S3ObjectResources("apps", "config", "install", "logs", "prompts", "prompts.update", "sessions", "segments", "tools", "userjobs")),
                    new PolicyStatement().Allow().WithActions("s3:PutObject")
                        .WithResources(props.S3ObjectResources("logs", "sessions", "segments", "tools")),
                    new PolicyStatement().Allow().WithActions("s3:DeleteObject")
                        .WithResources(props.S3ObjectResources("userjobs")),
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