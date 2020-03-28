# IVR-EC2

AWS CDK study:
- build/execute both on Linux and Windows
- separate public structure and private settings (context)

There is no private information in this project, it's rather a set of Windows/VoIP recipes

Prerequisites:
- AWS CDK installed
- AWS Account and valid credentials (in ~/.aws/credentials file)
- IVR Site schema file (*.yaml)

Site Deployment example:
- git clone https://github.com/OlegBoulanov/IVR-EC2
- cd IVR-EC2
- cdk deploy  -c region=us-east-1  -c schema=~/Projects/IVR/siteOne.yaml

Lifecycle (run in project directory):
- cdk synth (will create cdk.context.json, if does not exist)
- cdk deploy -v
- cdk destroy -v

References

- AWS CDK github: https://github.com/aws/aws-cdk
- Guide: https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html
