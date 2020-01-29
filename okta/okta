#!/bin/bash

# Helper script for multiple account scenario for the tool:
#     https://github.com/oktadeveloper/okta-aws-cli-assume-role
# You need to create ~/.okta/accounts/<account-name>/config.properties for each account
# The format is the same as of ~/.okta/config.properties created by first run of okta-aws tool

ACCOUNT_NAME=$1

AWS_DIR=~/.aws
OKTA_DIR=~/.okta
ACCOUNTS_DIR=${OKTA_DIR}/accounts

function xokta-aws {
    ${OKTA_DIR}/bin/withokta "aws --profile $1" "$@"
}

function print_accounts {
        echo Existing accounts \(in ${ACCOUNTS_DIR}/\) are:
        find ${ACCOUNTS_DIR}/ -mindepth 1 -maxdepth 1 -type d -printf "  %P\n"
}

if [ -z "${ACCOUNT_NAME}" ]
then
        echo Account name is required.
        print_accounts
elif [ ! -d "${ACCOUNTS_DIR}/${ACCOUNT_NAME}" ]
then
        echo Invalid account name ${ACCOUNT_NAME}.
        print_accounts
else
        rm -f ${OKTA_DIR}/.current-session
        rm -f ${OKTA_DIR}/profiles
        cp ${ACCOUNTS_DIR}/${ACCOUNT_NAME}/*.* ${OKTA_DIR}/

        OKTACI=$(xokta-aws default sts get-caller-identity | grep "arn" | cut -d ":" -f 2-)

        REGION=$(cat ${AWS_DIR}/credentials | grep region | cut -d "=" -f 2)
        EXPIRY=$(cat ${OKTA_DIR}/.current-session | grep _EXPIRY | cut -d "=" -f 2)

        echo "  Assumed:" ${OKTACI//\"/}
        echo "  Expires:" $(date -d "${EXPIRY//\\:/:}")
        echo "  Region: " ${REGION}
fi
