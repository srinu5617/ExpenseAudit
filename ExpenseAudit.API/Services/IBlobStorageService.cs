namespace ExpenseAudit.API.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(
            IFormFile file,
            CancellationToken cancellationToken);
    }
}
