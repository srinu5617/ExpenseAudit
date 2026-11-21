namespace ExpenseAudit.API.Services
{
    using ExpenseAudit.API.Models;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;

    public class AzureOpenAIService : IReceiptAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AzureOpenAIService(
            HttpClient httpClient,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<ExtractedReceipt> ExtractAsync(
            byte[] imageBytes,
            string contentType,
            CancellationToken cancellationToken)
        {
            var endpoint = _config["AzureOpenAI:Endpoint"]!;
            var deployment = _config["AzureOpenAI:Deployment"]!;
            var apiVersion = _config["AzureOpenAI:ApiVersion"]!;
            var apiKey = _config["AzureOpenAI:ApiKey"]!;

            var url =
                $"{endpoint.TrimEnd('/')}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

            var base64 = Convert.ToBase64String(imageBytes);

            var imageUrl =
                $"data:{contentType};base64,{base64}";

            var prompt = """
        Analyze this expense receipt.

        Extract:
        1. Merchant
        2. TransactionDate in yyyy-MM-dd format
        3. Total Amount
        4. Expense Category
        5. Whether alcohol is present

        Return confidence scores between 0 and 1
        for merchant, transactionDate, amount and category.

        Return ONLY valid JSON.

        Expected JSON:
        {
          "merchant": "ABC Restaurant",
          "transactionDate": "2026-11-20",
          "amount": 85.50,
          "category": "Meal",
          "containsAlcohol": false,
          "confidenceScores": {
            "merchant": 0.98,
            "transactionDate": 0.95,
            "amount": 0.99,
            "category": 0.90
          }
        }
        """;

            var request = new
            {
                messages = new object[]
                {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = prompt
                        },
                        new
                        {
                            type = "image_url",
                            image_url = new
                            {
                                url = imageUrl,
                                detail = "high"
                            }
                        }
                    }
                }
                },
                temperature = 0,
                max_tokens = 1500,
                response_format = new
                {
                    type = "json_object"
                }
            };

            using var message = new HttpRequestMessage(
                HttpMethod.Post, url);

            message.Headers.Add("api-key", apiKey);

            message.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(
                message,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync(
                cancellationToken);

            using var document = JsonDocument.Parse(result);

            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return JsonSerializer.Deserialize<ExtractedReceipt>(
                content!,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }
    }
}
