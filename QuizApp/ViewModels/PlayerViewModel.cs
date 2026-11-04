namespace QuizApp.ViewModels;

internal class PlayerViewModel : ViewModelBase
{
	private readonly MainWindowViewModel? _mainWindowViewModel;

	public PlayerViewModel(MainWindowViewModel? mainWindowViewModel)
	{
		_mainWindowViewModel = mainWindowViewModel;
	}

}
