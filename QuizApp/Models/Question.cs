using System.Text.Json.Serialization;

namespace QuizApp.Models;

internal class Question
{
    [JsonConstructor]
    public Question(string query, string correctAnswer, string[] incorrectAnswer)
    {
        Query = query;
        CorrectAnswer = correctAnswer;
        IncorrectAnswer = incorrectAnswer;
    }

    public Question(
        string query,
        string correctAnswer,
        string incorrectAnswer1,
        string incorrectAnswer2,
        string incorrectAnswer3)
        : this(
            query,
            correctAnswer,
            [incorrectAnswer1, incorrectAnswer2, incorrectAnswer3])
    {
    }

    public string Query { get; set; }
    public string CorrectAnswer { get; set; }
    public string[] IncorrectAnswer { get; set; }
}
