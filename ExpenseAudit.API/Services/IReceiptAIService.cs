using ExpenseAudit.API.Models;

namespace ExpenseAudit.API.Services
{
    public interface IReceiptAIService
    {
        Task<ExtractedReceipt> ExtractAsync(
            byte[] imageBytes,
            string contentType,
            CancellationToken cancellationToken);
    }
}
