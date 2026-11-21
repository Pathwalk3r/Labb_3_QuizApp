using QuizApp.Command;
using QuizApp.Dialogs;
using QuizApp.Models;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace QuizApp.ViewModels;

internal class ConfigurationViewModel : ViewModelBase
{
    private readonly MainWindowViewModel? _mainWindowViewModel;

    public QuestionPackViewModel? ActivePack =>
        _mainWindowViewModel?.ActivePack;

    private Question? _selectedQuestion;

    public Question? SelectedQuestion
    {
        get => _selectedQuestion;
        set
        {
            if (SetProperty(ref _selectedQuestion, value))
                RemoveQuestionCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand AddQuestionCommand { get; }
    public RelayCommand RemoveQuestionCommand { get; }
    public RelayCommand EditPackCommand { get; }
    public RelayCommand SavePackCommand { get; }

    public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        AddQuestionCommand = new RelayCommand(
            _ => AddQuestion(),
            _ => ActivePack is not null);
        RemoveQuestionCommand = new RelayCommand(
            _ => RemoveQuestion(),
            _ => ActivePack is not null && SelectedQuestion is not null);
        EditPackCommand = new RelayCommand(
            _ => EditPack(),
            _ => ActivePack is not null);
        SavePackCommand = new RelayCommand(
            _ => SavePack(),
            _ => ActivePack is not null);
        ImportQuestionsCommand = new RelayCommand(
            _ => _ = ImportQuestionsAsync(),
            _ => !IsImporting);
    }

    private readonly OpenTriviaDbClient _openTriviaDbClient = new();

    private double _importCountSliderValue = 10;
    public double ImportCountSliderValue
    {
        get => _importCountSliderValue;
        set
        {
            if (SetProperty(ref _importCountSliderValue, value))
                RaisePropertyChanged(nameof(ImportCount));
        }
    }

    public int ImportCount => (int)Math.Round(ImportCountSliderValue);

    private bool _isImporting;
    public bool IsImporting
    {
        get => _isImporting;
        private set
        {
            if (SetProperty(ref _isImporting, value))
                ImportQuestionsCommand.RaiseCanExecuteChanged();
        }
    }

    private string _importStatus = "";
    public string ImportStatus
    {
        get => _importStatus;
        private set => SetProperty(ref _importStatus, value);
    }

    public RelayCommand ImportQuestionsCommand { get; }
    public void RaiseActivePackChanged()
    {
        RaisePropertyChanged(nameof(ActivePack));
        SelectedQuestion = null;

        AddQuestionCommand.RaiseCanExecuteChanged();
        RemoveQuestionCommand.RaiseCanExecuteChanged();
        EditPackCommand.RaiseCanExecuteChanged();
        SavePackCommand.RaiseCanExecuteChanged();
    }

    private void AddQuestion()
    {
        if (ActivePack is null)
            return;

        var question = new Question(
            "New question",
            "Correct answer",
            "Incorrect answer 1",
            "Incorrect answer 2",
            "Incorrect answer 3");

        ActivePack.Questions.Add(question);
        SelectedQuestion = question;
    }

    private void RemoveQuestion()
    {
        if (ActivePack is null || SelectedQuestion is null)
            return;

        ActivePack.Questions.Remove(SelectedQuestion);
        SelectedQuestion = ActivePack.Questions.FirstOrDefault();
    }

    private void EditPack()
    {
        if (ActivePack is null)
            return;

        var dialog = new QuestionPackCreatorWindow(ActivePack.ToModel())
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() != true)
            return;

        ActivePack.Name = dialog.ViewModel.Name.Trim();
        ActivePack.Difficulty = dialog.ViewModel.SelectedDifficulty;
        ActivePack.TimeLimitInSeconds = dialog.ViewModel.TimeLimitInSeconds;
    }

    private async void SavePack()
    {
        if (_mainWindowViewModel is null || ActivePack is null)
            return;

        try
        {
            await _mainWindowViewModel.SaveActivePackAsync();
            _mainWindowViewModel.MenuViewModel?.RefreshSavedPacks();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save the question pack: {ex.Message}");
        }
    }

    private async Task ImportQuestionsAsync()
    {
        if (_mainWindowViewModel is null)
            return;

        var dialog = new QuestionPackCreatorWindow
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() != true)
            return;

        var details = dialog.ViewModel;
        IsImporting = true;
        ImportStatus = "Importing questions...";

        try
        {
            var questions = await _openTriviaDbClient.GetQuestionsAsync(
                ImportCount,
                details.SelectedDifficulty);

            if (questions.Count == 0)
            {
                ImportStatus = "No questions were returned.";
                return;
            }

            var pack = new QuestionPack(
                details.Name.Trim(),
                details.SelectedDifficulty,
                details.TimeLimitInSeconds)
            {
                Questions = questions.ToList()
            };

            _mainWindowViewModel.ActivePack = new QuestionPackViewModel(pack);
            SelectedQuestion = ActivePack?.Questions.FirstOrDefault();
            ImportStatus = $"Created a new pack with {questions.Count} questions.";
        }
        catch (HttpRequestException ex)
        {
            ImportStatus = "Could not connect to Open Trivia DB.";
            MessageBox.Show($"Network error while importing questions:\n{ex.Message}");
        }
        catch (TaskCanceledException)
        {
            ImportStatus = "The request timed out.";
            MessageBox.Show("The Open Trivia DB request timed out. Please try again.");
        }
        catch (System.Text.Json.JsonException ex)
        {
            ImportStatus = "Open Trivia DB returned an invalid response.";
            MessageBox.Show($"Could not read the API response:\n{ex.Message}");
        }
        catch (Exception ex)
        {
            ImportStatus = "Question import failed.";
            MessageBox.Show($"Could not import questions:\n{ex.Message}");
        }
        finally
        {
            IsImporting = false;
        }
    }
}
