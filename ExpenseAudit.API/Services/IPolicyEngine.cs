using ExpenseAudit.API.Models;

namespace ExpenseAudit.API.Services
{
    public interface IPolicyEngine
    {
        Task<List<AuditViolation>> EvaluateAsync(
            ExtractedReceipt receipt);
    }
}
