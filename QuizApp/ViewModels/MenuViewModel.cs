using QuizApp.Command;
using System.Windows;

namespace QuizApp.ViewModels;

class MenuViewModel : ViewModelBase
{
    public MainWindowViewModel? MainWindowViewModel { get; set; }

    public RelayCommand ChangePackNameCommand { get; }
    public RelayCommand ExitApplicationCommand { get; }
    public QuestionPackViewModel? ActivePack { get => MainWindowViewModel?.ActivePack; }

    public void RaiseActivePackChanged() => RaisePropertyChanged(nameof(ActivePack));

    public MenuViewModel(MainWindowViewModel? mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;

        ChangePackNameCommand = new RelayCommand(ChangePackName);

        ExitApplicationCommand = new RelayCommand(ExitApplication);
    }

    private void ExitApplication(object? obj) => Application.Current.Shutdown();

    private void ChangePackName(object? obj)
    {
        ActivePack?.Name = "New Name";
    }
}
