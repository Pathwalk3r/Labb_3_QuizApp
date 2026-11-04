namespace QuizApp.ViewModels;

internal class ConfigurationViewModel : ViewModelBase
{
    private readonly MainWindowViewModel? _mainWindowViewModel;

    public QuestionPackViewModel? ActivePack { get => _mainWindowViewModel?.ActivePack; }

    public void RaiseActivePackChanged() => RaisePropertyChanged(nameof(ActivePack));

    public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
}
