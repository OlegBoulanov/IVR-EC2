# IVR-EC2

AWS CDK experiments:
- build/execute both on Linux and Windows
- 

Prerequisites:
- AWS Account and valid credentials
- [ CDK_DEFAULT_ACCOUNT/CDK_DEFAULT_REGION ]
- RDP_PUBLIC_IP - set it to your IP (like 73.121.98.115) to allow RDP access to created instance

Lifecycle (run in project directory):
- cdk synth (will create cdk.context.json, if does not exist)
- cdk deploy -v
- cdk destroy -v

References

- AWS CDK github: https://github.com/aws/aws-cdk
- Guide: https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html
