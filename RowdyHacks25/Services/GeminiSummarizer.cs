using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RowdyHacks25.Models;

namespace RowdyHacks25.Services
{
    public interface IGeminiSummarizer
    {
        bool IsConfigured { get; }
        Task<string> SummarizeAsync(Bounty bounty, CancellationToken ct = default);
    }

    public sealed class GeminiSummarizer : IGeminiSummarizer
    {
        private readonly HttpClient _http;
        private readonly string? _apiKey;

        // Matches the REST path segment for v1beta
        private const string ModelPath = "v1beta/models/gemini-2.0-flash:generateContent";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        public GeminiSummarizer(HttpClient http, IConfiguration config)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Do NOT throw here; allow app to run without the key
            _apiKey = config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        }

        public async Task<string> SummarizeAsync(Bounty b, CancellationToken ct = default)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Gemini API key is not configured.");

            var prompt = BuildPrompt(b);
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, ModelPath);
            req.Headers.Add("X-goog-api-key", _apiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                using var resp = await _http.SendAsync(req, ct);
                var json = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Gemini API error {(int)resp.StatusCode}: {json}");

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Attempt to read candidates[0].content.parts[0].text
                if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    throw new InvalidOperationException("Gemini response missing candidates.");

                var content = candidates[0].GetProperty("content");
                var parts = content.GetProperty("parts");
                if (parts.GetArrayLength() == 0)
                    throw new InvalidOperationException("Gemini response missing content parts.");

                var text = parts[0].GetProperty("text").GetString();
                return text?.Trim() ?? "";
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to call Gemini API: {ex.Message}", ex);
            }
        }

        private static string BuildPrompt(Bounty b)
        {
            var bio = string.IsNullOrWhiteSpace(b.Bio) ? "No additional biography provided." : b.Bio;
            return $@"
You are writing a short sci?fi bounty board summary. In 60–90 words, summarize the target using the given context.
Be concise, actionable, and neutral. Mention risk, last known planet, and reward scale without repeating exact numbers excessively.

Context:
- Target: {b.TargetName}
- Planet: {b.Planet}
- Danger Level: {b.DangerLevel}
- Status: {b.Status}
- Reward: {b.Reward}
- Posted By: {b.PostedBy}
- Biography: {bio}

Output only the summary paragraph.";
        }
    }
}