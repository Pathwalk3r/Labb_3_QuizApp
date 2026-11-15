using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuizApp.Models;

internal sealed class OpenTriviaDbClient
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<Question>> GetQuestionsAsync(
        int amount,
        Difficulty difficulty,
        CancellationToken cancellationToken = default)
    {
        if (amount is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(amount), "Choose between 1 and 50 questions.");

        string difficultyText = difficulty.ToString().ToLowerInvariant();
        string url =
            $"https://opentdb.com/api.php?amount={amount}&difficulty={difficultyText}&type=multiple";

        using var response = await HttpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<OpenTdbResponse>(json, JsonOptions)
            ?? throw new JsonException("Open Trivia DB returned an empty response.");

        if (result.ResponseCode != 0)
        {
            string message = result.ResponseCode switch
            {
                1 => "No questions were found for that difficulty and amount.",
                2 => "Open Trivia DB rejected the request parameters.",
                3 => "Open Trivia DB could not find the requested session token.",
                4 => "The Open Trivia DB session token has no questions remaining.",
                5 => "Open Trivia DB rate limit reached. Wait a few seconds and try again.",
                _ => $"Open Trivia DB returned error code {result.ResponseCode}."
            };

            throw new InvalidOperationException(message);
        }

        return result.Results.Select(item =>
        {
            if (item.IncorrectAnswers.Length != 3)
                throw new JsonException("A multiple-choice result did not contain three incorrect answers.");

            return new Question(
                WebUtility.HtmlDecode(item.Question),
                WebUtility.HtmlDecode(item.CorrectAnswer),
                item.IncorrectAnswers
                    .Select(WebUtility.HtmlDecode)
                    .ToArray());
        }).ToList();
    }

    private sealed class OpenTdbResponse
    {
        [JsonPropertyName("response_code")]
        public int ResponseCode { get; set; }

        [JsonPropertyName("results")]
        public List<OpenTdbQuestion> Results { get; set; } = [];
    }

    private sealed class OpenTdbQuestion
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = "";

        [JsonPropertyName("correct_answer")]
        public string CorrectAnswer { get; set; } = "";

        [JsonPropertyName("incorrect_answers")]
        public string[] IncorrectAnswers { get; set; } = [];
    }
}
