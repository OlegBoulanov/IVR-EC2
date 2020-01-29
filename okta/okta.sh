#!/bin/bash

# Helper script for multiple account scenario for the tool:
#     https://github.com/oktadeveloper/okta-aws-cli-assume-role
# You need to create ~/.okta/accounts/<account-name>/config.properties for each account
# The format is the same as of ~/.okta/config.properties created by first run of okta-aws tool

ACCOUNT_NAME=$1
PROFILE=default

AWS_DIR=~/.aws
OKTA_DIR=~/.okta
ACCOUNTS_DIR=${OKTA_DIR}/accounts

TS_NOW=$(date "+%s")

function xokta-aws {
        # use the tool to ask for user credentials and update .aws/credentials
        ${OKTA_DIR}/bin/withokta "aws --profile $1" "$@"
}

function print_region {
        REGION=$(cat ${AWS_DIR}/credentials | grep region | cut -d "=" -f 2)
        echo "  Region: " ${REGION}
}

function print_current_info {
        echo "Current session:"
        EXPIRY=$(cat ${OKTA_DIR}/.current-session | grep _EXPIRY | cut -d "=" -f 2)
        EXPIRE=$(date -d "${EXPIRY//\\:/:}")
        TS_EXP=$(date -d "${EXPIRE}" "+%s")
        #echo "    Now:" ${TS_NOW}
        #echo "    Exp:" ${TS_EXP}
        if [ $TS_NOW -lt $TS_EXP ]
        then
                echo "  Expires:" ${EXPIRE}
        else
                echo "  Expired." ${EXPIRE}
        fi
        print_region
}

function print_accounts {
        echo Existing accounts \(in ${ACCOUNTS_DIR}/\) are:
        find ${ACCOUNTS_DIR}/ -mindepth 1 -maxdepth 1 -type d -printf "  %P\n"
        print_current_info
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
        rm -f ${AWS_DIR}/credentials
        rm -f ${OKTA_DIR}/profiles
        rm -f ${OKTA_DIR}/.current-session
        cp ${ACCOUNTS_DIR}/${ACCOUNT_NAME}/*.* ${OKTA_DIR}/

        OKTACI=$(xokta-aws ${PROFILE} sts get-caller-identity | grep "arn" | cut -d ":" -f 2-)
        echo "  Assumed:" ${OKTACI//\"/}

        print_current_info
fi

