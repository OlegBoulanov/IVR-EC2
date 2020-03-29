# IVR-EC2

AWS CDK study:
- build/execute CDK project on both Linux and Windows
- separate public structure and private settings (context)

There is no private information in this project, it's rather a collection of Windows/SIP Providers recipes from public sources

Prerequisites:
- AWS CDK installed
- AWS Account and valid credentials (in ~/.aws/credentials file)
- IVR Site schema file (*.yaml)

Site Deployment example:
- git clone https://github.com/OlegBoulanov/IVR-EC2
- cd IVR-EC2
- (current credentials in ~/.aws/credentials [profile-name])
- cdk deploy --profile "profile-name"  -c region=us-east-1  -c schema=~/Projects/site-schema.yaml


References

- AWS CDK github: https://github.com/aws/aws-cdk
- Guide: https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html
