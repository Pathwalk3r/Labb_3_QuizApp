using QuizApp.Command;

namespace QuizApp.ViewModels;

internal class QuizResultViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public string ResultText { get; }

    public RelayCommand PlayAgainCommand { get; }
    public RelayCommand ReturnToMenuCommand { get; }

    public QuizResultViewModel(
        int correctAnswers,
        int totalQuestions,
        MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        ResultText = $"You got {correctAnswers} out of {totalQuestions} questions correct.";

        PlayAgainCommand = new RelayCommand(_ =>
            _mainWindowViewModel.ShowPlayerCommand.Execute(null));

        ReturnToMenuCommand = new RelayCommand(_ =>
            _mainWindowViewModel.ShowStartCommand.Execute(null));
    }
}
