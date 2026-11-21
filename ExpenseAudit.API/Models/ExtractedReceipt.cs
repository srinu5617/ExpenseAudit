namespace ExpenseAudit.API.Models
{
    public class ExtractedReceipt
    {
        public string Merchant { get; set; } = "";

        public string TransactionDate { get; set; } = "";

        public decimal? Amount { get; set; }

        public string Category { get; set; } = "";

        public bool ContainsAlcohol { get; set; }

        public Dictionary<string, decimal> ConfidenceScores
        { get; set; } = new();
    }
}
