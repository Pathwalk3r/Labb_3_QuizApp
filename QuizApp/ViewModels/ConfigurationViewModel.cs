using QuizApp.Command;
using QuizApp.Dialogs;
using QuizApp.Models;
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
        set => SetProperty(ref _selectedQuestion, value);
    }

    public RelayCommand AddQuestionCommand { get; }
    public RelayCommand RemoveQuestionCommand { get; }
    public RelayCommand EditPackCommand { get; }
    public RelayCommand SavePackCommand { get; }

    public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        AddQuestionCommand = new RelayCommand(_ => AddQuestion());
        RemoveQuestionCommand = new RelayCommand(_ => RemoveQuestion());
        EditPackCommand = new RelayCommand(_ => EditPack());
        SavePackCommand = new RelayCommand(_ => SavePack());
    }

    public void RaiseActivePackChanged()
    {
        RaisePropertyChanged(nameof(ActivePack));
        SelectedQuestion = null;
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

    private void SavePack()
    {
        if (_mainWindowViewModel is null || ActivePack is null)
            return;

        try
        {
            _mainWindowViewModel.SaveActivePack();
            _mainWindowViewModel.MenuViewModel?.RefreshSavedPacks();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save the question pack: {ex.Message}");
        }
    }
}
