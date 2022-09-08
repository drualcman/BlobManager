using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Reflection.Metadata;

public class Program
{
    private const string blobServiceEnpoint = "https://mediastoredrualcman.blob.core.windows.net/";
    private const string storageAccountName = "mediastoredrualcman";
    private const string storageAccountKey = "WAQ7nVKG5NXzze2RuAUrtoBrMWkYAxJaADrvGsbpQj3xxZBKR+UwxXQLKjBp9BLdfH+vpOlDJRN6+ASt8nBkWw==";


    public static async Task Main()
    {
        StorageSharedKeyCredential accountCredentials = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);

        BlobServiceClient serviceClient = new BlobServiceClient(new Uri(blobServiceEnpoint), accountCredentials);

        AccountInfo info = await serviceClient.GetAccountInfoAsync();
        await Console.Out.WriteLineAsync("Conexion establecida a la cuenta de Azure Storage");
        await Console.Out.WriteLineAsync($"Nombre de la cuenta: {storageAccountName}");
        await Console.Out.WriteLineAsync($"Tipo de cuenta: {info.AccountKind}");
        await Console.Out.WriteLineAsync($"SKU de la cuenta: {info.SkuName}");

        await EnumerateContainerAsync(serviceClient);
        string existingContainerName = "raster-graphics";
        await EnumerateBlobsAsync(serviceClient, existingContainerName);

        BlobContainerClient containerClient = await GetContainerAsync(serviceClient, "vector-graphics");
        string uploadedBlobName = "graph.svg";
        BlobClient blobClient = await GetBlobAsync(containerClient, uploadedBlobName);
        await Console.Out.WriteLineAsync($"Url del blob: {blobClient.Uri}");

        string newBlobName = @"C:\Azure Dev Ops\Mod01\Images\burger.jpg";
        await PutBlobAsync(containerClient, newBlobName);
    }

    private static async Task EnumerateContainerAsync(BlobServiceClient client)
    {
        await foreach(BlobContainerItem container in client.GetBlobContainersAsync())
        {
            await Console.Out.WriteAsync($"Container: {container.Name}");
        }
    }

    private static async Task EnumerateBlobsAsync(BlobServiceClient client, string containerName)
    {
        BlobContainerClient container = client.GetBlobContainerClient(containerName);

        await Console.Out.WriteAsync($"\nBuscando blobs en: {containerName}");

        await foreach(BlobItem blob in container.GetBlobsAsync())
        {
            await Console.Out.WriteAsync($"\n\tBlob existente: {blob.Name}");
        }
    }

    private static async Task<BlobContainerClient> GetContainerAsync(BlobServiceClient client, string containerName)
    {
        BlobContainerClient container = client.GetBlobContainerClient(containerName);

        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        await Console.Out.WriteLineAsync($"\nNuevo contenedor: {container.Name}");

        return container;
    }

    private static async Task<BlobClient> GetBlobAsync(BlobContainerClient client, string blobName)
    {
        BlobClient blob = client.GetBlobClient(blobName);
        await Console.Out.WriteLineAsync($"\nBlob encontrado: {blob.Name}");
        return blob;
    }

    private static async Task PutBlobAsync(BlobContainerClient client, string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        BlobClient blob = client.GetBlobClient(fileName);
        using FileStream uploadFileStream = File.OpenRead(filePath);
        try
        {
            await blob.UploadAsync(uploadFileStream, true);            
            await Console.Out.WriteLineAsync($"Url del nuevo blob: {blob.Uri}");
        }
        catch(Exception ex)
        {               
            await Console.Out.WriteLineAsync(ex.Message);
        }
        uploadFileStream.Close();
    }

}