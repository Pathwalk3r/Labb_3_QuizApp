namespace QuizApp.ViewModels;

internal class PlayerViewModel : ViewModelBase
{
	private readonly MainWindowViewModel? _mainWindowViewModel;

    public QuestionPackViewModel? ActivePack { get => _mainWindowViewModel?.ActivePack; }

    public PlayerViewModel(MainWindowViewModel? mainWindowViewModel)
	{
		_mainWindowViewModel = mainWindowViewModel;
	}

}
