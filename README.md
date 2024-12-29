# SocialMediaApp

This application is a social media app similar to Bluesky and such.

What this project requires to run is the Azure Cosmos DB emulator, as Cosmos DB is used as the database. You can get the emulator at <https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=windows%2Ccsharp&pivots=api-nosql> or downloading it directly from <https://aka.ms/cosmosdb-emulator>.

The project should create the database and required containers automatically. If it for some reason doesn't work, the database and containers are as follow:

Database:

```SocialApp```

Containers:

```AccountRoles``` with partition key path ```/partitionKey```

```leaseContainer``` with partition key path ```/partitionKey```

```LinkedRoles``` with partition key path ```/roleId```

```Posts``` with partition key path ```/partitionKey```

```UserAccounts``` with partition key path ```/partitionKey```


This app supports images and it works via Azure Blob storage. As this is also ran locally, you need Azurite.
<https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=npm%2Cblob-storage> has detailed instructions,
but you can install it via the following command 

```npm install -g azurite```

and then run Azurite with the command

```azurite --silent --location c:\azurite --debug c:\azurite\debug.log```

You need to have an existing Blob Container called ```profile-pictures```.

Azurite needs to be running to see the images, and in the Microsoft Azure Storage Explorer, you need to set the public access level of ```profile-pictures``` to at least Blobs.

And finally, the project requires following style of User Secrets:

```{
  "AppSettings:Token": "<token for password hashing>",
  "CosmosDb:ConnectionString": "<your-azure-cosmos-db-emulator-connection-string>",
  "CosmosDb:DatabaseName": "SocialApp",
  "StorageConnection": "UseDevelopmentStorage=true",
  "StorageConnection:blobServiceUri": "UseDevelopmentStorage=true",
  "StorageConnection:queueServiceUri": "UseDevelopmentStorage=true",
  "StorageConnection:tableServiceUri": "UseDevelopmentStorage=true",
  "BlobStorage": {
    "DefaultConnection": "<your-connection-string>",
    "ProfileContainerName": "profile-pictures",
    "PostContainerName": "post-images"
  }
}