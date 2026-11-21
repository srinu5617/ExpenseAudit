namespace ExpenseAudit.API.Models
{
    public class ExpenseReceipt
    {
        public int Id { get; set; }

        public string FileName { get; set; } = "";

        public string BlobUrl { get; set; } = "";

        public string Merchant { get; set; } = "";

        public DateTime? TransactionDate { get; set; }

        public decimal? Amount { get; set; }

        public string Category { get; set; } = "";

        public decimal ConfidenceScore { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AuditViolation> Violations { get; set; }
            = new List<AuditViolation>();
    }
}
