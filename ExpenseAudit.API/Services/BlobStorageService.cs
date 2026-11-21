namespace ExpenseAudit.API.Services
{
    using Azure.Storage.Blobs;

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration config)
        {
            var connectionString =
                config["AzureBlob:ConnectionString"];

            var containerName =
                config["AzureBlob:ContainerName"];

            var client = new BlobServiceClient(connectionString);

            _container = client.GetBlobContainerClient(containerName);

            _container.CreateIfNotExists();
        }

        public async Task<string> UploadAsync(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var fileName =
                $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            var blob = _container.GetBlobClient(fileName);

            await using var stream = file.OpenReadStream();

            await blob.UploadAsync(
                stream,
                overwrite: false,
                cancellationToken);

            return blob.Uri.ToString();
        }
    }
}
