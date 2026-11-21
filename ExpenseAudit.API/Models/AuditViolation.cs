namespace ExpenseAudit.API.Models
{
    public class AuditViolation
    {
        public int Id { get; set; }

        public int ExpenseReceiptId { get; set; }

        public string RuleId { get; set; } = "";

        public string Description { get; set; } = "";

        public string Severity { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ExpenseReceipt? ExpenseReceipt { get; set; }
    }
}
