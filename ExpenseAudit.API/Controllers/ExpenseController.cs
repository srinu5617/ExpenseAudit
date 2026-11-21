namespace ExpenseAudit.API.Controllers
{
    using ExpenseAudit.API.Data;
    using ExpenseAudit.API.Models;
    using ExpenseAudit.API.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Globalization;

    [ApiController]
    [Route("api/expenses")]
    public class ExpenseController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorage;
        private readonly IReceiptAIService _aiService;
        private readonly IPolicyEngine _policyEngine;
        private readonly ExpenseDbContext _db;

        public ExpenseController(
            IBlobStorageService blobStorage,
            IReceiptAIService aiService,
            IPolicyEngine policyEngine,
            ExpenseDbContext db)
        {
            _blobStorage = blobStorage;
            _aiService = aiService;
            _policyEngine = policyEngine;
            _db = db;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> Upload(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a receipt.");

            var allowedTypes = new[]
            {
            "image/jpeg",
            "image/png",
            "application/pdf"
        };

            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Unsupported file type.");

            if (file.Length > 20 * 1024 * 1024)
                return BadRequest("Maximum file size is 20 MB.");

            // Step 1: Upload to Blob Storage

            var blobUrl = await _blobStorage.UploadAsync(
                file, cancellationToken);

            // Step 2: Read receipt bytes

            await using var memoryStream = new MemoryStream();

            await file.CopyToAsync(
                memoryStream, cancellationToken);

            var fileBytes = memoryStream.ToArray();

            // Step 3: Extract receipt data using AI

            ExtractedReceipt extracted;

            if (file.ContentType == "application/pdf")
            {
                return BadRequest(
                    "PDF page rendering is required before GPT-4o image extraction.");
            }

            extracted = await _aiService.ExtractAsync(
                fileBytes,
                file.ContentType,
                cancellationToken);

            // Step 4: Apply business policies

            var violations = await _policyEngine.EvaluateAsync(
                extracted);

            // Step 5: Save receipt

            var receipt = new ExpenseReceipt
            {
                FileName = Path.GetFileName(file.FileName),
                BlobUrl = blobUrl,
                Merchant = extracted.Merchant,
                TransactionDate = DateTime.TryParseExact(
                    extracted.TransactionDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var transactionDate)
                        ? transactionDate
                        : null,
                Amount = extracted.Amount,
                Category = extracted.Category,
                ConfidenceScore = extracted.ConfidenceScores.Count > 0
                    ? extracted.ConfidenceScores.Values.Average()
                    : 0,
                Status = violations.Any()
                    ? "Violation"
                    : "Approved",
                Violations = violations
            };

            _db.ExpenseReceipts.Add(receipt);

            await _db.SaveChangesAsync(cancellationToken);

            // Step 6: Return consolidated report

            return Ok(new
            {
                ReceiptId = receipt.Id,
                receipt.Merchant,
                receipt.TransactionDate,
                receipt.Amount,
                receipt.Category,
                receipt.ConfidenceScore,
                receipt.Status,
                Violations = violations.Select(v => new
                {
                    v.RuleId,
                    v.Description,
                    v.Severity,
                    v.CreatedAt
                })
            });
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
            CancellationToken cancellationToken)
        {
            var receipts = await _db.ExpenseReceipts
                .Include(x => x.Violations)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return Ok(receipts);
        }
    }
}
