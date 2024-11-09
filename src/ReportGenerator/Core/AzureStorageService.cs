using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using ReportGenerator.Common.Exceptions;
using Microsoft.Extensions.Options;

namespace ReportGenerator.Core;

public class AzureStorageService(IOptions<StorageOptions> options)
{
    private readonly StorageOptions _options = options.Value;

    public async Task<BlobClient> UploadBlobAsync(string containerName, Stream stream, string blobName)
    {
        var container = GetBlobContainerClient(containerName);
        await CreateContainerIfNotExists(container);

        BlobClient blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(stream);

        return blob;
    }

    public async Task<MemoryStream> DownloadBlobAsync(string containerName, string blobName)
    {
        var container = GetBlobContainerClient(containerName);
        await CreateContainerIfNotExists(container);

        BlobClient blob = container.GetBlobClient(blobName);

        MemoryStream memoryStream = new MemoryStream();
        await blob.DownloadToAsync(memoryStream);

        return memoryStream;
    }

    public async Task<Stream> OpenWriteAsync(string containerName, string blobName)
    {
        var container = GetBlobContainerClient(containerName);
        await CreateContainerIfNotExists(container);

        BlockBlobClient blob = container.GetBlockBlobClient(blobName);

        return await blob.OpenWriteAsync(true);
    }

    public async Task<Stream> OpenReadAsync(string containerName, string blobName)
    {
        var container = GetBlobContainerClient(containerName);
        await CreateContainerIfNotExists(container);

        BlockBlobClient blob = container.GetBlockBlobClient(blobName);

        return await blob.OpenReadAsync();
    }

    public async Task<AsyncPageable<BlobItem>> GetBlobsAsync(string containerName, string? prefix = null)
    {
        var container = GetBlobContainerClient(containerName);
        var containerExists = await container.ExistsAsync();

        if (!containerExists.Value)
        {
            throw new NotFoundException($"Unable to find a container with the name '{containerName}'");
        }

        return container.GetBlobsAsync(prefix: prefix);
    }

    public string GetSasTokenForBlobUri(Uri blobUri)
    {
        StorageSharedKeyCredential storageSharedKeyCredential =
            new(_options.StorageAccountName, _options.StorageAccountKey);

        var blobClient = new BlobClient(blobUri, storageSharedKeyCredential);

        var token = CreateServiceSASBlob(blobClient);

        if (token is null)
        {
            throw new ArgumentException("Unable to generate token for blob");
        }

        return token.ToString();
    }

    private static async Task CreateContainerIfNotExists(BlobContainerClient container)
    {
        var exists = await container.ExistsAsync();

        if (!exists.Value)
        {
            await container.CreateIfNotExistsAsync();
            await container.SetAccessPolicyAsync(PublicAccessType.Blob);
        }
    }

    private BlobContainerClient GetBlobContainerClient(string containerName)
    {
        // TODO: Move towards using DefaultAzureCredential.
        BlobContainerClient client = new(_options.StorageAccountConnectionString, containerName);

        return client;
    }

    private static Uri? CreateServiceSASBlob(BlobClient blobClient)
    {
        // Check if BlobContainerClient object has been authorized with Shared Key.
        if (blobClient.CanGenerateSasUri)
        {
            // Create a SAS token that's valid for two hours.
            BlobSasBuilder sasBuilder =
                new()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(2)
                };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri;
        }
        else
        {
            // Client object is not authorized via Shared Key.
            return null;
        }
    }
}
