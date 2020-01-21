using System;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;

namespace Ivr
{
    public class IvrPolicy : ManagedPolicy
    {
        public static PolicyStatement Allow(string sid = default) { return new PolicyStatement { Sid = sid }.Allow(); }
        internal IvrPolicy(Construct scope, string id = "IvrPolicy") : base(scope, id)
        {
            this.WithStatements(

                Allow("SidStsAssumeRole")
                .WithActions("sts:AssumeRole", "sts:GetFederationToken")
                .WithResources("arn:aws:iam:::role/CallHost-EC2"),

                Allow("SidS3GetBucketLocation")
                .WithActions("s3:GetBucketLocation")
                .WithResources("arn:aws:s3:::*"),

                Allow("SidS3ListBucket")
                .WithActions("s3:ListBucket")
                .WithResources("arn:aws:s3:::logs.elizacorp.com"),

                Allow("SidS3GetPutObject")
                .WithActions("s3:GetObject", "s3:PutObject")
                .WithResources("arn:aws:s3:::logs.elizacorp.com/*"),

                Allow("SidSnsListTopicsAndPublish")
                .WithActions("sns:ListTopics", "sns:Publish")
                .WithResources("*")

            );
        }
    }
}
