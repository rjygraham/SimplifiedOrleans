# SimplifiedOrleans

A Simplified Visual Studio project structure demonstrating a simple IoT scenario using Microsoft Orleans for application code and Cosmos DB as a globally distributed backing store.

## Setup

This solution can run locally for development and debugging or in Azure for POC purposes. In either setup, you will need Cosmos DB for storing information. While working locally, you can use an actual Cosmos DB account or the [Azure Cosmos DB emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21).

Regardless of where the Cosmos DB is hosted, you will need a database and the following containers:
- `events` with partition key of `/pk`
- `clusters` with partition key of `/ClusterId`


### Running on Localhost

1. Open Solution in Visual Studio.
2. Right-click on the `SimplifiedOrleans` project, select `Manage User Secrets`
3. Add the following to the `secrets.json` file:
```json
{
  "ApplicationInsights:InstrumentationKey": "<your app insights instrumentation key>",
  "CosmosDb:AccountEndpoint": "https://your-cosmosdb-account.documents.azure.com:443/",
  "CosmosDb:AccountKey": "<your Cosmos DB account key>",
  "CosmosDb:DB": "orleans",
  "CosmosDb:Collection": "clusters",
  "CosmosDb:DatabaseId": "orleans",
  "CosmosDb:ContainerId": "events"
}
```

> NOTE: If using Cosmos DB Emulator, use the emulator AccountEndpoint and AccountKey values.

### Running in Azure

1. Create AKS cluster, Azure Container Registry, and Azure Cosmos DB.

2. Publish the SimplifiedOrleans application container image:
```sh
az acr build -r <acr-name> -t "<acr-name>.azurecr.io/simplifiedorleans:{{.Run.ID}}" -f .\src\SimplifiedOrleans\Dockerfile .\src
```

3. Deploy Orleans AKS role bindings

```sh
# Switch kubectl contexts to your regional AKS cluster, substituting your actual values for name, env, and region
kubectl config use-context <target-aks-context>

helm install orleansrolebindings .\charts\orleansrolebindings\
```

4. Deploy Secrets
> NOTE: This is a worst practice, in a production environment these secrets would be pulled from something akin to Azure Key Vault.

```sh
# Switch kubectl contexts to your regional AKS cluster, substituting your actual values for name, env, and region
kubectl config use-context <target-aks-context>

helm install simplifiedorleanssecrets .\charts\simplifiedorleanssecrets\ --set-string appInsightsInstrumentationKey=<appInsightsInstrumentationKey>,cosmosDbAccountEndpoint=<https://accountname.documents.azure.com:443/>,cosmosDbAccountKey=<cosmosDbAccountKey>,cosmosDbDatabaseId=<cosmosDbDatabaseId>,cosmosDbClusteringContainerId=<cosmosDbClusteringContainerId>,cosmosDbEventsContainerId=<cosmosDbEventsContainerId>
```

5. Deploy the silo host

```sh
# Switch kubectl contexts to your regional AKS cluster, substituting your actual values for name, env, and region
kubectl config use-context <target-aks-context>

helm install simplifiedorleans .\charts\simplifiedorleans\ --set-string orleans.clusterId=<region>,image.repository=<acr-name.azurecr.io/simplifiedorleans>,image.tag=<image tag>
```
## Usage

After setup is complete the following HTTP requests can be used to test the application.
> NOTE: For simplicity, this setup uses HTTP instead of HTTPS which is susceptible to main-in-the-middle attacks. In a production system, all communications should happen over HTTPS with TLS 1.2 or greater.

1. Create a customer (Customer Id will be included in the response and is require for "activating" a sensor)

```sh
POST http://XX.XX.XX.XX/customer
content-type: application/json

{
    "name": "Acme Corp"
}
```

2. Activate a sensor:

```sh
POST http://XX.XX.XX.XX/sensor/1ecc5d44-264d-4045-b0b7-693be3c3f4bd
content-type: application/json

{
    "customerId": "96d6ac0d-44a8-4d88-87f9-1d8f747170cf",
    "readingWindow": "00:02:00"
}
```

3. Verify the sensor was associated with customer:

```sh
GET http://XX.XX.XX.XX/customer/96d6ac0d-44a8-4d88-87f9-1d8f747170cf/sensors
```

4. Send readings for the sensor:

```sh
GET http://XX.XX.XX.XX/sensor?id=1ecc5d44-264d-4045-b0b7-693be3c3f4bd&ts=1622137307&type=A&scanType=Enter&value=102
```

5. Query sensor readings:

Current reading:
```sh
GET http://XX.XX.XX.XX/sensor/1ecc5d44-264d-4045-b0b7-693be3c3f4bd/readings/current
```

All readings since point in time (within readingWindow):
```sh
GET http://XX.XX.XX.XX/sensor/1ecc5d44-264d-4045-b0b7-693be3c3f4bd/readings/1622137100
```
6. Deprovision a sensor:

```sh
DELETE http://XX.XX.XX.XX/sensor/1ecc5d44-264d-4045-b0b7-693be3c3f4bd
```

## License

The MIT License (MIT)

Copyright © 2020 Ryan Graham

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.