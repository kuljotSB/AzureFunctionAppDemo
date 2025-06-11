using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks; // Added for async/await

namespace Company.Function;

public class QueueTriggeredFunctionKuljot
{
    private readonly ILogger<QueueTriggeredFunctionKuljot> _logger;

    public QueueTriggeredFunctionKuljot(ILogger<QueueTriggeredFunctionKuljot> logger)
    {
        _logger = logger;
    }

    [Function(nameof(QueueTriggeredFunctionKuljot))]
    public async Task Run([QueueTrigger("function-input-queue", Connection = "TARGET_STORAGE_ACCOUNT")] QueueMessage message)
    {
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", message.MessageText);

        // Get the connection string and container name from environment variables
        string connectionString = Environment.GetEnvironmentVariable("TARGET_STORAGE_ACCOUNT");
        string outputContainerName = Environment.GetEnvironmentVariable("OUTPUT_CONTAINER_NAME");

        // Create a BlobServiceClient object 
        var blobServiceClient = new BlobServiceClient(connectionString);

        // create a BlobContainerClient object for the container
        var containerClient = blobServiceClient.GetBlobContainerClient(outputContainerName);

        // Ensure the container exists
        await containerClient.CreateIfNotExistsAsync();

        // read message content from the input queue and write it to a blob in form of a text file
        string blobName = $"{Guid.NewGuid()}.txt";
        var blobClient = containerClient.GetBlobClient(blobName);

        // Write text to a MemoryStream instead of a local file and then upload it to the blob
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(message.MessageText)))
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }
        
        // Log the blob name for reference
        _logger.LogInformation("Wrote message to blob storage: {blobName}", blobName);
    }
}