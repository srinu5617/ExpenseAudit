using ExpenseAudit.API.Models;
using System.Text.Json;

namespace ExpenseAudit.API.Services
{
    public class PolicyEngine : IPolicyEngine
    {
        private readonly IConfiguration _configuration;

        public PolicyEngine(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<AuditViolation>> EvaluateAsync(
            ExtractedReceipt receipt)
        {
            var path = _configuration["Policy:FilePath"]!;

            var json = await File.ReadAllTextAsync(path);

            using var document = JsonDocument.Parse(json);

            var rules = document.RootElement
                .GetProperty("rules");

            var violations = new List<AuditViolation>();

            foreach (var rule in rules.EnumerateArray())
            {
                if (!rule.GetProperty("enabled").GetBoolean())
                    continue;

                var ruleId = rule.GetProperty("ruleId").GetString()!;

                var name = rule.GetProperty("name").GetString()!;

                var severity = rule.GetProperty("severity").GetString()!;

                string? description = null;

                switch (ruleId)
                {
                    case "EXP001":

                        var maxAmount =
                            rule.GetProperty("maxAmount").GetDecimal();

                        if (string.Equals(
                            receipt.Category,
                            "Meal",
                            StringComparison.OrdinalIgnoreCase)
                            && receipt.Amount > maxAmount)
                        {
                            description =
                                $"Meal amount {receipt.Amount} exceeds limit {maxAmount}";
                        }

                        break;

                    case "EXP002":

                        if (receipt.ContainsAlcohol)
                        {
                            description =
                                "Alcohol expense is not permitted";
                        }

                        break;

                    case "EXP003":

                        if (string.IsNullOrWhiteSpace(receipt.Merchant))
                        {
                            description = "Merchant information is missing";
                        }

                        break;

                    case "EXP004":

                        if (receipt.Amount == null)
                        {
                            description = "Receipt amount is missing";
                        }

                        break;
                }

                if (description != null)
                {
                    violations.Add(new AuditViolation
                    {
                        RuleId = ruleId,
                        Description = description,
                        Severity = severity
                    });
                }
            }

            return violations;
        }
    }
}



