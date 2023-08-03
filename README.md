# rotating-shop-items-grpc-plugin-server-csharp

```mermaid
flowchart LR
   subgraph AccelByte Gaming Services
   CL[gRPC Client]
   end
   subgraph gRPC Server Deployment
   SV["gRPC Server\n(YOU ARE HERE)"]
   DS[Dependency Services]
   CL --- DS
   end
   DS --- SV
```

`AccelByte Gaming Services` capabilities can be extended using custom functions implemented in a `gRPC server`. If configured, custom functions in the `gRPC server` will be called by `AccelByte Gaming Services` instead of the default function.

The `gRPC server` and the `gRPC client` can actually communicate directly. However, additional services are necessary to provide **security**, **reliability**, **scalability**, and **observability**. We call these services as `dependency services`. The [grpc-plugin-dependencies](https://github.com/AccelByte/grpc-plugin-dependencies) repository is provided as an example of what these `dependency services` may look like. It
contains a docker compose which consists of these `dependency services`.

> :warning: **grpc-plugin-dependencies is provided as example for local development purpose only:** The dependency services in the actual gRPC server deployment may not be exactly the same.

## Overview

This repository contains a `rotating shop items gRPC server app` written in `C#`. It provides a simple custom rotating shop items function for platform service in `AccelByte Gaming Services`.

This sample app also shows how this `gRPC server` can be instrumented for better observability.
It is configured by default to send metrics, traces, and logs to the observability `dependency services` in [grpc-plugin-dependencies](https://github.com/AccelByte/grpc-plugin-dependencies).

## Prerequisites

1. Windows 10 WSL2 or Linux Ubuntu 20.04 with the following tools installed.

   a. bash

   b. make

   c. docker v23.x

   d. docker-compose v2

   e. .net 6 sdk

   f. docker loki driver
    
      ```
      docker plugin install grafana/loki-docker-driver:latest --alias loki --grant-all-permissions
      ```
   g. [ngrok](https://ngrok.com/)

   h. [postman](https://www.postman.com/)

2. A local copy of [grpc-plugin-dependencies](https://github.com/AccelByte/grpc-plugin-dependencies) repository.

   ```
   git clone https://github.com/AccelByte/grpc-plugin-dependencies.git
   ```

3. Access to `AccelByte Gaming Services` demo environment.

    a. Base URL: https://demo.accelbyte.io.

    b. [Create a Game Namespace](https://docs.accelbyte.io/esg/uam/namespaces.html#tutorials) if you don't have one yet. Keep the `Namespace ID`.

    c. [Create an OAuth Client](https://docs.accelbyte.io/guides/access/iam-client.html) with `confidential` client type. Keep the `Client ID` and `Client Secret`.

## Setup

To be able to run this sample app, you will need to follow these setup steps.

1. Create a docker compose `.env` file by copying the content of [.env.template](.env.template) file.
2. Fill in the required environment variables in `.env` file as shown below.

   ```
   AB_BASE_URL=https://demo.accelbyte.io     # Base URL of AccelByte Gaming Services demo environment
   AB_CLIENT_ID='xxxxxxxxxx'                 # Client ID from the Prerequisites section
   AB_CLIENT_SECRET='xxxxxxxxxx'             # Client Secret from the Prerequisites section
   AB_NAMESPACE='xxxxxxxxxx'                 # Namespace ID from the Prerequisites section
   PLUGIN_GRPC_SERVER_AUTH_ENABLED=false     # Enable or disable access token and permission verification
   ```

   > :warning: **Keep PLUGIN_GRPC_SERVER_AUTH_ENABLED=false for now**: It is currently not
   supported by `AccelByte Gaming Services`, but it will be enabled later on to improve security. If it is
   enabled, the gRPC server will reject any calls from gRPC clients without proper authorization
   metadata.

For more options, create `src/AccelByte.PluginArch.ItemRotation.Demo.Server/appsettings.Development.json` and fill in the required configuration.

```json
{
  "DirectLogToLoki": false,
  "EnableAuthorization": false,                 // Enable or disable access token and permission check (env var: PLUGIN_GRPC_SERVER_AUTH_ENABLED)
  "RevocationListRefreshPeriod": 60,
  "AccelByte": {
    "BaseUrl": "https://demo.accelbyte.io",     // Base URL (env var: AB_BASE_URL)
    "ClientId": "xxxxxxxxxx",                   // Client ID (env var: AB_CLIENT_ID)    
    "ClientSecret": "xxxxxxxxxx",               // Client Secret (env var: AB_CLIENT_SECRET)
    "AppName": "MMV2GRPCSERVICE",
    "TraceIdVersion": "1",
    "Namespace": "xxxxxxxxxx",                  // Namespace ID (env var: AB_NAMESPACE)
    "EnableTraceId": true,
    "EnableUserAgentInfo": true,
    "ResourceName": "MMV2GRPCSERVICE"
  }
}
```
> :warning: **Environment variable values will override related configuration values in this file**.


## Building

To build this sample app, use the following command.

```
make build
```

## Running

To (build and) run this sample app in a container, use the following command.

```
docker-compose up --build
```

## Testing

### Functional Test in Local Development Environment

The custom functions in this sample app can be tested locally using `postman`.

1. Run the `dependency services` by following the `README.md` in the [grpc-plugin-dependencies](https://github.com/AccelByte/grpc-plugin-dependencies) repository.

   > :warning: **Make sure to start dependency services with mTLS disabled for now**: It is currently not supported by `AccelByte Gaming Services`, but it will be enabled later on to improve security. If it is enabled, the gRPC client calls without mTLS will be rejected.

2. Run this `gRPC server` sample app.

3. Open `postman`, create a new `gRPC request` (tutorial [here](https://blog.postman.com/postman-now-supports-grpc/)), and enter `localhost:10000` as server URL. 

   > :exclamation: We are essentially accessing the `gRPC server` through an `Envoy` proxy in `dependency services`.

4. In `postman`, continue by selecting `Section/GetRotationItems` method and invoke it with the sample message below.

   ```json
   {
      "namespace": "accelbyte",
      "userId": "c6354ec948604a1c9f5c026795e420d9",
      "sectionObject": {        
         "items": [
            {
                  "itemId": "7fcad276c5df4128b3f38564abd012c4",
                  "itemSku": "S1"
            },
            {
                  "itemId": "59ab1f45979e460295178deb609ec5d6",
                  "itemSku": "S2"
            },
            {
                  "itemId": "e51ae70222af4fba96ba8d7f631b8407",
                  "itemSku": "S3"
            },
            {
                  "itemId": "f790c28a58734212b594b0a161ffb297",
                  "itemSku": "S4"
            },
            {
                  "itemId": "f0f745e8dac14614a0c30470438ecfed",
                  "itemSku": "S5"
            },
            {
                  "itemId": "365ef7d7624b4f23b5d815ad1fd2f7cc",
                  "itemSku": "S6"
            },
            {
                  "itemId": "ce6d664c2c7f4c0fb488663814a33176",
                  "itemSku": "S7"
            },
            {
                  "itemId": "37eb332bf8e748f2a12f6ba19b4018df",
                  "itemSku": "S8"
            }            
         ],
         "sectionId": "c4d737f6f42c423e8690ff705ab75d9f",
         "sectionName": "example",
         "startDate": "1672519500",
         "endDate": "1675197900"
      }    
   }
   ```

5. If successful, you will see the item(s) in the response.

   ```json
   {
      "items": [
         {
            "itemId": "59ab1f45979e460295178deb609ec5d6",
            "itemSku": "S2"
         }
      ]
   }
   ```

6. Still in `postman`, continue by selecting `Section/Backfill` method and invoke it with the sample message below.

   ```json
   {
   "userId": "c6354ec948604a1c9f5c026795e420d9",
   "namespace": "accelbyte",
   "items": [
      {
         "itemId": "7fcad276c5df4128b3f38564abd012c4",
         "itemSku": "S1",
         "owned": true,
         "index": 1
      },
      {
         "itemId": "59ab1f45979e460295178deb609ec5d6",
         "itemSku": "S2",
         "owned": false,
         "index": 2
      },
      {
         "itemId": "e51ae70222af4fba96ba8d7f631b8407",
         "itemSku": "S3",
         "owned": false,
         "index": 3
      }
   ],
   "sectionName": "example",
   "sectionId": "9f5c026795e420d9c6354ec948604a1c"
   }
   ```

7. If successful, you will see the item(s) in the response. The `itemId` will changed accordingly.

   ```json
   {
      "backfilledItems": [
         {
               "itemId": "687d110a30dc401ea5f76cd8fafff8e5",
               "itemSku": "",
               "index": 1
         }
      ]
   }
   ```

### Integration Test with AccelByte Gaming Services

After passing functional test in local development environment, you may want to perform
integration test with `AccelByte Gaming Services`. Here, we are going to expose the `gRPC server`
in local development environment to the internet so that it can be called by
`AccelByte Gaming Services`. To do this without requiring public IP, we can use [ngrok](https://ngrok.com/)


1. Run the `dependency services` by following the `README.md` in the [grpc-plugin-dependencies](https://github.com/AccelByte/grpc-plugin-dependencies) repository.

   > :warning: **Make sure to start dependency services with mTLS disabled for now**: It is currently not supported by `AccelByte Gaming Services`, but it will be enabled later on to improve security. If it is enabled, the gRPC client calls without mTLS will be rejected.


2. Run this `gRPC server` sample app.

3. Sign-in/sign-up to [ngrok](https://ngrok.com/) and get your auth token in `ngrok` dashboard.

4. In [grpc-plugin-dependencies](https://github.com/AccelByte/grpc-plugin-dependencies) repository folder, run the following command to expose the `Envoy` proxy port connected to the `gRPC server` in local development environment to the internet. Take a note of the `ngrok` forwarding URL e.g. `tcp://0.tcp.ap.ngrok.io:xxxxx`.

   ```
   make ngrok NGROK_AUTHTOKEN=xxxxxxxxxxx    # Use your ngrok auth token
   ```

5. [Create an OAuth Client](https://docs.accelbyte.io/guides/access/iam-client.html) with `confidential` client type with the following permissions. Keep the `Client ID` and `Client Secret`. This is different from the Oauth Client from the Prerequisites section and it is required by CLI demo app [here](demo/cli/) in the next step to register the `gRPC Server` URL.

   - ADMIN:NAMESPACE:{namespace}:PLUGIN:CATALOG [READ, UPDATE, DELETE]
   - ADMIN:NAMESPACE:{namespace}:STORE [CREATE, READ, UPDATE, DELETE]
   - ADMIN:NAMESPACE:{namespace}:CATEGORY [CREATE]
   - ADMIN:NAMESPACE:{namespace}:CURRENCY [CREATE, DELETE]
   - ADMIN:NAMESPACE:{namespace}:ITEM [CREATE, DELETE]
   - ADMIN:NAMESPACE:{namespace}:USER:*:ENTITLEMENT [CREATE, UPDATE, DELETE]

   > :warning: **Oauth Client created in this step is different from the one from Prerequisites section:** It is required by CLI demo app in the next step to register the `gRPC Server` URL.

6. Create a user for testing. Keep the `Username` and `Password`.

7. Refer to [Demo CLI](src/AccelByte.PluginArch.ItemRotation.Demo.Client/README.md) readme on how to use the demo cli app to test the service.

> :warning: **Ngrok free plan has some limitations**: You may want to use paid plan if the traffic is high.

#### Deploy to AccelByte Gaming Services

After passing integration test against locally running sample app you may want to deploy the sample app to AGS (AccelByte Gaming Services).

1. Download and setup [extend-helper-cli](https://github.com/AccelByte/extend-helper-cli/)
2. Create new Extend App on Admin Portal, please refer to docs [here](https://docs.accelbyte.io/gaming-services/services/customization/override-ags-feature/getting-started-with-rotating-shop-items-customization/)
3. Do docker login using `extend-helper-cli`, please refer to its documentation
4. Build and push sample app docker image to AccelByte ECR using the following command inside sample app directory
   ```
   make imagex_push REPO_URL=xxxxxxxxxx.dkr.ecr.us-west-2.amazonaws.com/accelbyte/justice/development/extend/xxxxxxxxxx/xxxxxxxxxx IMAGE_TAG=v0.0.1
   ```
   > Note: the REPO_URL is obtained from step 2 in the app detail on the 'Repository Url' field

Please refer to [getting started docs](https://docs.accelbyte.io/gaming-services/services/customization/override-ags-feature/getting-started-with-rotating-shop-items-customization/) for more detailed steps on how to deploy sample app to AccelByte Gaming Service.
