using QuizApp.Command;
using QuizApp.Models;
using QuizApp.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;

namespace QuizApp.ViewModels;

internal sealed record SavedPackFile(string Name, string FilePath);

class MenuViewModel : ViewModelBase
{

    public MainWindowViewModel? MainWindowViewModel { get; set; }

    public QuestionPackViewModel? ActivePack =>
        MainWindowViewModel?.ActivePack;

    public ObservableCollection<SavedPackFile> SavedPacks { get; } = new();

    public RelayCommand CreatePackCommand { get; }
    public RelayCommand SelectPackCommand { get; }
    public RelayCommand ExitApplicationCommand { get; }

    public MenuViewModel(MainWindowViewModel? mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;

        CreatePackCommand = new RelayCommand(_ => CreatePack());
        SelectPackCommand = new RelayCommand(SelectPack);
        ExitApplicationCommand = new RelayCommand(_ => Application.Current.Shutdown());

        RefreshSavedPacks();
    }

    public void RaiseActivePackChanged() =>
        RaisePropertyChanged(nameof(ActivePack));

    public void RefreshSavedPacks()
    {
        SavedPacks.Clear();

        if (MainWindowViewModel is null)
            return;

        foreach (var file in MainWindowViewModel.GetSavedPackFiles())
            SavedPacks.Add(file);
    }

    private void CreatePack()
    {
        if (MainWindowViewModel is null)
            return;

        var dialog = new QuestionPackCreatorWindow
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() != true)
            return;

        var details = dialog.ViewModel;
        var pack = new QuestionPack(
            details.Name.Trim(),
            details.SelectedDifficulty,
            details.TimeLimitInSeconds);

        MainWindowViewModel.ActivePack = new QuestionPackViewModel(pack);
    }

    private void SelectPack(object? parameter)
    {
        if (MainWindowViewModel is null ||
            parameter is not SavedPackFile file)
            return;

        try
        {
            var pack = MainWindowViewModel.LoadQuestionPack(file.FilePath);
            MainWindowViewModel.ActivePack = new QuestionPackViewModel(pack);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not load the question pack: {ex.Message}");
            RefreshSavedPacks();
        }
    }
}
