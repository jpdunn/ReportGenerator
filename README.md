# Report Generator
A deployable microservice (Azure Container App) responsible for generating reports.

This service listens to an Azure Service Bus Queue and processes report generation messages using a background service which will:
* Generate a Stimulsoft report based on the template (not included)
* Upload the report to the specified Azure Blob Storage container
* Publish a new Azure Service Bus message to the specified topic

If any errors are encountered the message will be dead-lettered to avoid continual retries.

## Setup

## Technologies
This repo uses the following technologies:
* NodaTime
* Serilog
* Azure Service Bus
* Azure Blob Storage
* Stimulsoft
 

Please familiarise yourself with these technologies before committing to the repository.


## Deployment
This repo is a boilerplate starting point for report generation. A few things have been specifically left out to ensure maximum reusability for multiple cases:
* Azure pipelines - You will need to implement your own `azure-pipelines.yml` in accordance with your requirements.
* Docker config - In most cases a basic docker (or docker-compose) will be all that's needed for this service.
* Azure Container Apps deployment - This service is originally intended to be deployed using Azure Container Apps but can be deployed using other technologies if required. If you wish to deploy it using Azure Container Apps you will need to use your own configuration.
