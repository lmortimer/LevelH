## Overview
F# web app hosted on Lambda to view the Hashigo Zake tap list.

`/tap`

* `GET` - Returns the beer currently on tap.
* `POST` - Updates the beer list.

## Deployment
Requires

    dotnet tool install -g Amazon.Lambda.Tools

Deploy to Lambda with

    dotnet lambda deploy-serverless LambdaFetch

## Testing
Test through the Lambda console with the following JSON, replacing `GET` with `POST` when appropriate.

```{
  "body": "",
  "resource": "tap",
  "path": "/tap",
  "httpMethod": "GET",
  "isBase64Encoded": true,
  "queryStringParameters": {},
  "stageVariables": {},
  "requestContext": {
    "accountId": "123456789012",
    "resourceId": "123456",
    "stage": "Prod",
    "requestId": "c6af9ac6-7b61-11e6-9a41-93e8deadbeef",
    "requestTime": "09/Apr/2015:12:34:56 +0000",
    "requestTimeEpoch": 1428582896000,
    "identity": {
      "cognitoIdentityPoolId": null,
      "accountId": null,
      "cognitoIdentityId": null,
      "caller": null,
      "accessKey": null,
      "sourceIp": "127.0.0.1",
      "cognitoAuthenticationType": null,
      "cognitoAuthenticationProvider": null,
      "userArn": null,
      "userAgent": "Custom User Agent String",
      "user": null
    },
    "path": "/tap",
    "resourcePath": "tap",
    "httpMethod": "GET",
    "apiId": "FOO",
    "protocol": "HTTP/1.1"
  }
}```