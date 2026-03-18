using System.Text.Json;
using Azure.Storage.Blobs;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace GloboTicket.Services.IdentityServer;

public class BlobSigningKeyStore(BlobServiceClient blobServiceClient) : ISigningKeyStore
{
    private const string ContainerName = "identity-signing-keys";

    private BlobContainerClient GetContainer()
    {
        var container = blobServiceClient.GetBlobContainerClient(ContainerName);
        container.CreateIfNotExists();
        return container;
    }

    public async Task<IEnumerable<SerializedKey>> LoadKeysAsync()
    {
        var container = GetContainer();
        var keys = new List<SerializedKey>();

        await foreach (var blob in container.GetBlobsAsync())
        {
            var client = container.GetBlobClient(blob.Name);
            var response = await client.DownloadContentAsync();
            var key = JsonSerializer.Deserialize<SerializedKey>(response.Value.Content.ToString());
            if (key is not null) keys.Add(key);
        }

        return keys;
    }

    public async Task StoreKeyAsync(SerializedKey key)
    {
        var container = GetContainer();
        var client = container.GetBlobClient($"{key.Id}.json");
        var json = JsonSerializer.Serialize(key);
        await client.UploadAsync(BinaryData.FromString(json), overwrite: true);
    }

    public async Task DeleteKeyAsync(string id)
    {
        var container = GetContainer();
        var client = container.GetBlobClient($"{id}.json");
        await client.DeleteIfExistsAsync();
    }
    
}