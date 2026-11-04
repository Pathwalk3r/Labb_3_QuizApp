using QuizApp.Command;
using QuizApp.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace QuizApp.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    private object _currentView;

    public object CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            RaisePropertyChanged();
        }
    }
    public ObservableCollection<QuestionPackViewModel>? Packs { get; }

	private QuestionPackViewModel? _activePack;

    public QuestionPackViewModel? ActivePack
    {
        get => _activePack;
        set
        {
            if (SetProperty(ref _activePack, value))
            {
                ConfigurationViewModel?.RaiseActivePackChanged();
                MenuViewModel?.RaiseActivePackChanged();
            }
        }
    }

    //private enum AppView { Start, Player, Configuration }
    //private AppView _currentView = AppView.Start;

    //public Visibility StartVisibility => 
    //    _currentView == AppView.Start ? Visibility.Visible : Visibility.Collapsed;

    //public Visibility PlayerVisibility =>
    //    _currentView == AppView.Player ? Visibility.Visible : Visibility.Collapsed;

    //public Visibility ConfigurationVisibility =>
    //    _currentView == AppView.Configuration ? Visibility.Visible : Visibility.Collapsed;

    public PlayerViewModel? PlayerViewModel { get; }

    public ConfigurationViewModel? ConfigurationViewModel { get; }

    public MenuViewModel? MenuViewModel { get; }

    public RelayCommand ShowStartCommand { get; }

    public RelayCommand ShowPlayerCommand { get; }
    public RelayCommand ShowConfigurationCommand { get; }

    public MainWindowViewModel()
    {
		PlayerViewModel = new PlayerViewModel(this);
        ConfigurationViewModel = new ConfigurationViewModel(this);
        MenuViewModel = new MenuViewModel(this);

        //ShowStartCommand = new RelayCommand(_ => ShowView(AppView.Start));
        //ShowPlayerCommand = new RelayCommand(_ => ShowView(AppView.Player));
        //ShowConfigurationCommand = new RelayCommand(_ => ShowView(AppView.Configuration));

        var questionPack = new QuestionPack("Mina frågor!");
        questionPack.Questions.Add(new Question("Vad heter Sveriges huvudstad?", "Stockholm", "Oslo", "Göteborg", "London"));
        questionPack.Questions.Add(new Question("Vad är 2 + 3?", "5", "10", "8", "1"));

        ActivePack = new QuestionPackViewModel(questionPack);
    }

    //private void ShowView(AppView view)
    //{
    //    _currentView = view;
    //    RaisePropertyChanged(nameof(StartVisibility));
    //    RaisePropertyChanged(nameof(PlayerVisibility));
    //    RaisePropertyChanged(nameof(ConfigurationVisibility));
    //}
}
