# Okta/AWS .aws/credentials update script

Helper script for multiple account scenario for the tool: https://github.com/oktadeveloper/okta-aws-cli-assume-role

You need to create <b>~/.okta/accounts/&lt;account-name&gt;/config.properties</b> for each account

The format is the same as of ~/.okta/config.properties created by first run of <b>okta-aws</b> tool

Format example:
```
    #OktaAWSCLI
    OKTA_ORG=<company>.okta.com
    OKTA_AWS_APP_URL=https://<company>.okta.com/home/amazon_aws/<path>
    OKTA_USERNAME=<username>
    ;OKTA_BROWSER_AUTH=true
    OKTA_AWS_PROFILE=default
    OKTA_AWS_REGION=us-east-1
    OKTA_STS_DURATION=3600
    OKTA_AWS_ROLE_TO_ASSUME=arn:aws:iam::<account>:role/<ROLE>
    OKTA_MFA_CHOICE=OKTA.push
```
Running the script:
```
    ./okta.sh <account-name>
```
Credentials are written to <b>~/.aws/credentials</b>[&lt;default&gt;]