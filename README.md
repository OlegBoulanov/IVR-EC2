# IVR-EC2

AWS CDK study:
- build/execute both on Linux and Windows
- separate public structure and private settings (context)

Prerequisites:
- AWS Account and valid credentials
- [ CDK_DEFAULT_ACCOUNT/CDK_DEFAULT_REGION ]
- Context values resolved in that order:
    ~/cdk.json
    ./cdk.json
    -c ctx=file.json
    -c name=value ...

Lifecycle (run in project directory):
- cdk synth (will create cdk.context.json, if does not exist)
- cdk deploy -v
- cdk destroy -v

References

- AWS CDK github: https://github.com/aws/aws-cdk
- Guide: https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html
